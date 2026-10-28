using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace MOD_6b2SYs
{
    /// <summary>
    /// Remonta os nomes de técnica gerados. O jogo junta "peça 1 + peça 2 + nome-base" na ordem inglesa; com o en
    /// da BattleSkillPrefixName trocado pelo PT (TechniquePrefixPtBr) sai "do Imortal Maciço Água Espiritual",
    /// que vira "Água Espiritual Maciça do Imortal": o adjetivo concorda com o nome-base. As peças e o gênero dos
    /// nomes-base vêm de ModAssets/ptbr/Tecnicas.json (gerado por tools/ptbr/tecnicas.py).
    /// </summary>
    public static class TechniqueNamePtBr
    {
        public class Data
        {
            public Dictionary<string, string> peca1 = new Dictionary<string, string>();
            public Dictionary<string, List<string>> peca2 = new Dictionary<string, List<string>>();
            public Dictionary<string, string> bases = new Dictionary<string, string>();
        }

        public static Data Pieces { get; private set; }

        private static Dictionary<string, string> feminine;
        private static Regex whole;
        private static Regex inText;

        public static Data Load(string path)
        {
            Prepare(File.Exists(path)
                ? JsonConvert.DeserializeObject<Data>(File.ReadAllText(path, Encoding.UTF8))
                : new Data());
            return Pieces;
        }

        public static void Prepare(Data data)
        {
            Pieces = data;
            feminine = new Dictionary<string, string>();
            foreach (var p in data.peca2.Values)
            {
                feminine[p[0]] = p.Count > 1 && p[1].Length > 0 ? p[1] : p[0];
            }
            if (data.peca1.Count == 0 || feminine.Count == 0)
            {
                whole = inText = null;
                return;
            }
            string firsts = Alternation(data.peca1.Values);
            string seconds = Alternation(feminine.Keys);
            whole = new Regex("^(?<p1>" + firsts + ") (?<p2>" + seconds + ") (?<base>.+)$");
            // Em texto corrido só com um nome-base conhecido, para não engolir o resto da frase.
            inText = data.bases.Count == 0 ? null : new Regex(@"(?<!\p{L})(?<p1>" + firsts + ") (?<p2>" + seconds
                + ") (?<base>" + Alternation(data.bases.Keys) + @")(?!\p{L})");
        }

        private static string Alternation(IEnumerable<string> values)
        {
            return string.Join("|", values.Where(v => v.Length > 0).Distinct().OrderByDescending(v => v.Length)
                .Select(Regex.Escape));
        }

        /// <summary>O nome inteiro, como GetName devolve; null se não for um nome montado.</summary>
        public static string Rebuild(string name)
        {
            if (whole == null || string.IsNullOrEmpty(name))
            {
                return null;
            }
            Match m = whole.Match(name);
            return m.Success ? Build(m) : null;
        }

        /// <summary>Nomes montados dentro de um texto da tela; null se não havia nenhum.</summary>
        public static string Rewrite(string text)
        {
            if (inText == null || string.IsNullOrEmpty(text))
            {
                return null;
            }
            string s = inText.Replace(text, Build);
            return s == text ? null : s;
        }

        private static string Build(Match m)
        {
            string name = m.Groups["base"].Value;
            string kind;
            if (!Pieces.bases.TryGetValue(name, out kind))
            {
                kind = Guess(name);
            }
            string masc = m.Groups["p2"].Value;
            string adj = kind[0] == 'f' ? feminine[masc] : masc;
            if (kind[1] == 'p')
            {
                adj = Plural(adj);
            }
            return name + " " + adj + " " + m.Groups["p1"].Value;
        }

        /// <summary>Gênero e número pela primeira palavra, para nome-base fora da planilha.</summary>
        private static string Guess(string name)
        {
            string w = name.Split(' ')[0];
            bool plural = w.EndsWith("s") && w != "Lótus";
            string s = plural ? w.Substring(0, w.Length - 1) : w;
            bool fem = s.EndsWith("a") || s.EndsWith("ade") || s.EndsWith("ção") || s.EndsWith("são") || s.EndsWith("gem");
            return (fem ? "f" : "m") + (plural ? "p" : "s");
        }

        private static string Plural(string a)
        {
            if (a.IndexOf(' ') >= 0 || a.EndsWith("s") && !a.EndsWith("ês"))
            {
                return a;
            }
            if (a.EndsWith("ês"))
            {
                return a.Substring(0, a.Length - 2) + "eses";
            }
            if (a.EndsWith("il"))
            {
                return a.Substring(0, a.Length - 2) + "is";
            }
            if (a.EndsWith("l"))
            {
                return a.Substring(0, a.Length - 1) + "is";
            }
            if (a.EndsWith("r") || a.EndsWith("z"))
            {
                return a + "es";
            }
            if (a.EndsWith("m"))
            {
                return a.Substring(0, a.Length - 1) + "ns";
            }
            return a + "s";
        }
    }
}
