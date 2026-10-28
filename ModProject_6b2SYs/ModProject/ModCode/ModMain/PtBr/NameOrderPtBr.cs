using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace MOD_6b2SYs
{
    /// <summary>
    /// Reordena os nomes que o jogo monta peça por peça na ordem inglesa (código nativo, sem formato no LocalText):
    /// "Supremo Horizonte Seita Estado Hua Feng Filial" -> "Seita do Horizonte Supremo Filial Estado Hua Feng",
    /// "HangXiu Cidade" -> "Cidade da Jornada do Cultivo". As peças vêm do LocalText traduzido (schoolNameFirst/Second
    /// 101+, school_name_last, schoolLastName); gênero e número dos substantivos estão nas tabelas abaixo. Os nomes de
    /// cidade vêm de ModAssets/ptbr/Cidades.json (pinyin -> "do Herói Sem Nome"; gerado por tools/ptbr/cidades.py).
    /// </summary>
    public static class NameOrderPtBr
    {
        private const string Lasts = "Seita|Salão|Pavilhão|Palácio|Templo|Aldeia da Montanha";
        // Região de filial: "Yun Mo", "Estado Hua Feng".
        private const string Branch = @"(?: (?<reg>(?:Estado )?\p{Lu}\p{Ll}+(?: \p{Lu}\p{Ll}+)?) Filial)?";

        private static readonly HashSet<string> Feminine = new HashSet<string> {
            "Alma", "Chama", "Chamas", "Colinas", "Corte", "Estrelas", "Fênix", "Geada", "Horas", "Lua", "Miragens",
            "Neve", "Nuvem", "Névoa", "Ondas", "Pausa", "Paz", "Poeiras", "Vila" };
        private static readonly HashSet<string> SingularEndingInS = new HashSet<string> {
            "Arco-íris", "Cosmos", "Lótus", "Quebra-cabeças" };
        private static readonly HashSet<string> Numerals = new HashSet<string> {
            "Um", "Dois", "Três", "Cinco", "Sete", "Nove", "Dez", "Cem", "Mil" };
        // Peças da primeira posição que são substantivos: viram complemento ("Alma do Dragão").
        private static readonly Dictionary<string, string> NounFirst = new Dictionary<string, string> {
            { "Dragão", "do Dragão" }, { "Tinta", "de Tinta" }, { "Corte", "do Corte" } };
        // Cores e comparativos não variam.
        private static readonly HashSet<string> Invariable = new HashSet<string> {
            "Rubi", "Cinza", "Violeta", "Carmesim", "Ciano", "Azul-celeste", "Maior", "Menor", "Superior", "Inferior" };

        // Cidade e Vila são femininas: "para HengZhi Vila" -> "para a Vila de Ananke".
        private static readonly Dictionary<string, string> Contraction = new Dictionary<string, string> {
            { "para", "para a" }, { "Para", "Para a" }, { "em", "na" }, { "Em", "Na" }, { "de", "da" }, { "De", "Da" } };
        private const string Before = @"(?:(?<pre>[Pp]ara|[Ee]m|[Dd]e) )?";

        private static Regex sect;
        private static Regex pinyin;
        private static Regex town;
        private static Regex townPhrase;
        private static Dictionary<string, string> towns = new Dictionary<string, string>();

        public static void Prepare(string localTextJson, string townsJson)
        {
            Prepare(ReadJson(localTextJson), ReadJson(townsJson));
        }

        private static Dictionary<string, string> ReadJson(string path)
        {
            return File.Exists(path)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path, Encoding.UTF8))
                : new Dictionary<string, string>();
        }

        /// <summary>Monta as regras a partir do LocalText traduzido (chave -> pt) e dos nomes de cidade
        /// (pinyin -> "do Herói Sem Nome"; nome com espaço -> nome inteiro).</summary>
        public static void Prepare(Dictionary<string, string> pt, Dictionary<string, string> townNames)
        {
            towns = townNames;
            var phrases = towns.Keys.Where(k => k.IndexOf(' ') >= 0).OrderByDescending(k => k.Length).Select(Regex.Escape);
            string alt = string.Join("|", phrases);
            // O jogo mostra "Cultivador Aliança" sem sufixo; se vier com um, ele some.
            townPhrase = alt.Length == 0 ? null : new Regex(
                @"(?<!\p{L})" + Before + "(?<name>" + alt + @")(?: (?:Cidade|Vila|Aldeia))?(?!\p{L})");
            string firsts = Alternation(pt, "schoolNameFirst");
            string seconds = Alternation(pt, "schoolNameSecond");
            sect = firsts.Length == 0 || seconds.Length == 0 ? null : new Regex(
                @"(?<!\p{L})(?<adj>" + firsts + ") (?<noun>" + seconds + ") (?<last>" + Lasts + ")" + Branch + @"(?!\p{L})");
            // Nomes em pinyin juntam duas sílabas ("HangXiu"); só a ordem muda.
            pinyin = new Regex(@"(?<!\p{L})(?<name>\p{Lu}\p{Ll}+\p{Lu}\p{Ll}+) (?<last>" + Lasts + ")" + Branch + @"(?!\p{L})");
            town = new Regex(@"(?<!\p{L})" + Before + @"(?<name>\p{Lu}\p{Ll}+\p{Lu}\p{Ll}+) (?<last>Cidade|Vila|Aldeia)(?!\p{L})");
        }

        /// <summary>Texto com os nomes reordenados, ou null se não havia nome para mudar.</summary>
        public static string Rewrite(string text)
        {
            if (sect == null || string.IsNullOrEmpty(text))
            {
                return null;
            }
            string s = sect.Replace(text, SectName);
            s = pinyin.Replace(s, m => m.Groups["last"].Value + " " + m.Groups["name"].Value + BranchOf(m));
            s = town.Replace(s, TownName);
            if (townPhrase != null)
            {
                s = townPhrase.Replace(s, m => Pre(m) + towns[m.Groups["name"].Value]);
            }
            return s == text ? null : s;
        }

        /// <summary>"HengZhi Vila" -> "Vila de Ananke"; pinyin fora da planilha só muda de ordem.</summary>
        private static string TownName(Match m)
        {
            string name = m.Groups["name"].Value, pt;
            return Pre(m) + m.Groups["last"].Value + " " + (towns.TryGetValue(name, out pt) ? pt : name);
        }

        private static string Pre(Match m)
        {
            return m.Groups["pre"].Success ? Contraction[m.Groups["pre"].Value] + " " : "";
        }

        /// <summary>As peças de sentido: a série "Final" inteira e, na outra, os números 101 em diante
        /// (os menores são sílabas em pinyin).</summary>
        private static string Alternation(Dictionary<string, string> pt, string prefix)
        {
            var parts = pt.Where(kv => kv.Key.StartsWith(prefix)
                    && (kv.Key.Substring(prefix.Length).StartsWith("Final") || Number(kv.Key.Substring(prefix.Length)) > 100))
                .Select(kv => kv.Value.Trim()).Where(v => v.Length > 0 && v.IndexOf('{') < 0)
                .Distinct().OrderByDescending(v => v.Length).Select(Regex.Escape);
            return string.Join("|", parts);
        }

        private static int Number(string s)
        {
            int n;
            return int.TryParse(s, out n) ? n : 0;
        }

        private static string SectName(Match m)
        {
            string adj = m.Groups["adj"].Value, noun = m.Groups["noun"].Value;
            bool fem = Feminine.Contains(noun) || noun.EndsWith("a") || noun.EndsWith("ade") || noun.EndsWith("ção");
            bool plural = noun.EndsWith("s") && !SingularEndingInS.Contains(noun);
            string prep = "d" + (fem ? "a" : "o") + (plural ? "s" : "");
            string core;
            if (Numerals.Contains(adj))
            {
                core = adj + " " + noun;
            }
            else if (NounFirst.ContainsKey(adj))
            {
                core = noun + " " + NounFirst[adj];
            }
            else
            {
                core = noun + " " + Agree(adj, fem, plural);
            }
            return m.Groups["last"].Value + " " + prep + " " + core + BranchOf(m);
        }

        private static string BranchOf(Match m)
        {
            return m.Groups["reg"].Success ? " Filial " + m.Groups["reg"].Value : "";
        }

        private static string Agree(string adj, bool fem, bool plural)
        {
            if (Invariable.Contains(adj))
            {
                return adj;
            }
            string a = adj;
            if (fem)
            {
                if (a.EndsWith("o"))
                {
                    a = a.Substring(0, a.Length - 1) + "a";
                }
                else if (a.EndsWith("or"))
                {
                    a += "a";
                }
            }
            if (!plural)
            {
                return a;
            }
            if (a.EndsWith("il"))
            {
                return a.Substring(0, a.Length - 2) + "is";
            }
            if (a.EndsWith("l"))
            {
                return a.Substring(0, a.Length - 1) + "is";
            }
            if (a.EndsWith("z") || a.EndsWith("r"))
            {
                return a + "es";
            }
            return a + "s";
        }
    }
}
