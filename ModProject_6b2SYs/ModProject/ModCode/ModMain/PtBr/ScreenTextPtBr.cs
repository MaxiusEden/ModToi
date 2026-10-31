using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using MelonLoader;
using Newtonsoft.Json;
using UnhollowerRuntimeLib;

namespace MOD_6b2SYs
{
    /// <summary>
    /// Troca na tela textos que não passam pelo LocalText (ModAssets/ptbr/Tela.json, texto exato -> PT-BR)
    /// e registra em ptbr_tela_desconhecidas.txt os textos exibidos que não existem no LocalText.
    /// </summary>
    public static class ScreenTextPtBr
    {
        private const string ModSoleID = "6b2SYs";
        private const float ScanInterval = 3f;
        private const int MaxLogged = 5000;
        private const int MaxCache = 20000;

        private static Dictionary<string, string> map = new Dictionary<string, string>();
        private static HashSet<string> known;
        private static HashSet<string> knownTemplates;
        private static readonly HashSet<string> logged = new HashSet<string>();
        private static readonly Dictionary<string, string> cache = new Dictionary<string, string>();
        private static readonly Dictionary<int, string> lastSeen = new Dictionary<int, string>();
        private static readonly Regex Placeholder = new Regex(@"\{[^{}]*\}|\d+(?:[.,]\d+)*");
        private static readonly Regex Letter = new Regex(@"\p{L}.*\p{L}");

        // Textos que o jogo monta juntando pedaços na ordem do inglês ou cortando uma palavra do nome.
        private static readonly KeyValuePair<Regex, string>[] Parts = {
            // Alquimia: o jogo tira "Receita" (refineElixir_text2) de "Receita de Elixir X" e sobra "de Elixir X".
            new KeyValuePair<Regex, string>(new Regex(@"^de (Elixir\b.*)$", RegexOptions.Singleline), "$1"),
            // Datas: ano + " Ano" + mês + " Mês" (common_nian, common_yue), às vezes sem espaço ("21 Ano6 Mês:").
            new KeyValuePair<Regex, string>(new Regex(@"^(\d+) Ano ?(\d+) Mês(:?)$"), "Ano $1 Mês $2$3"),
            // Talismãs, materiais de cada nível: sem "Talismã do", o nome e o material cabem na linha.
            new KeyValuePair<Regex, string>(new Regex(@"^(<color=#\w+>)Talismã d[aeo]s? "), "$1"),
            // Regras e características da seita (Salão do Conselho): o jogo numera com "1," (common_dian).
            new KeyValuePair<Regex, string>(new Regex(@"(?m)^(\d+),(?=\p{L})"), "$1. "),
            // Salão de Missões: "Recompensa: " + nome do item (item_name10011) + valor, e "Contribuição da Seita: N"
            // no quadro do discípulo; o rótulo é só "Contribuição".
            new KeyValuePair<Regex, string>(new Regex(@"Contribuição da Seita(?=(?:</color>)?\s*(?:[:+x×]|\d|<color))"),
                "Contribuição"),
            // Arena: o <g> do jogo vira um verde escuro, ilegível sobre o fundo dessa tela.
            new KeyValuePair<Regex, string>(new Regex(@"^<color=#057600>(O Pavilhão do Coração do Dao cresceu.*)$",
                RegexOptions.Singleline), "<color=#7CFF7C>$1"),
            // Dificuldade: "Pontos de Jade Bagua:9", sem o espaço que os outros atributos têm.
            new KeyValuePair<Regex, string>(new Regex(@"^Pontos de Jade Bagua:(?=\d)"), "Pontos de Jade Bagua: "),
            // Mod de reparo do save: resumo depois de carregar.
            new KeyValuePair<Regex, string>(new Regex(@"^删除(\d+)个无效月结日志$"), "Excluídos $1 registros mensais inválidos"),
            new KeyValuePair<Regex, string>(new Regex(@"(?<=\d)删除不存在的月结日志 "), ". Registro mensal excluído, código "),
            // Espírito do Artefato: preço de venda escrito em chinês no código do jogo.
            new KeyValuePair<Regex, string>(new Regex(@"^出售价格：(\d+)灵石$"), "Preço de venda: $1 Pedras Espirituais"),
            // Leilão: "Formação da Alma、Iluminação", reinos que o jogo junta com a vírgula chinesa.
            new KeyValuePair<Regex, string>(new Regex(@"(?<=\p{L})、(?=\p{L})"), ", "),
            // Leilão: "Itens leiloados（Clique para sacar）", com os parênteses chineses que o jogo junta.
            new KeyValuePair<Regex, string>(new Regex(@"^(.*\p{L})（(.+)）$", RegexOptions.Singleline), "$1 ($2)"),
        };

        // Propósito da seita: o jogo junta dois lemas (schoolSlogan*) por linha com "，" e "。" chineses.
        private static readonly Regex SloganLine = new Regex(@"(?m)^([^，。\n]+)，(\p{Lu})([^，。\n]*)。");

        // Prazos: "começará em 7 Mês" (número + common_yue/common_nian) vira "começará em 7 meses".
        private static readonly Regex TimeCount = new Regex(@"(?<=\b(?:em|cada)(?: \d+ Ano)? )(\d+) (Mês|Ano)\b");

        // Mensagens de outro mod, de reparo do save, escritas em chinês durante o carregamento.
        private static readonly Regex SaveCheck = new Regex(@"^(检测|存档修复完成|大鬼修复存档)");
        private static readonly KeyValuePair<string, string>[] SaveCheckTerms = {
            new KeyValuePair<string, string>("检测所有角色生平日志数据", "Verificando históricos dos personagens"),
            new KeyValuePair<string, string>("检测角色数据", "Verificando personagens"),
            new KeyValuePair<string, string>("检测建筑数据", "Verificando construções"),
            new KeyValuePair<string, string>("检测坊市", "Verificando mercados"),
            new KeyValuePair<string, string>("检测大地图副本", "Verificando masmorras do mapa"),
            new KeyValuePair<string, string>("检测大地图事件", "Verificando eventos do mapa"),
            new KeyValuePair<string, string>("检测壶妖数据", "Verificando diabretes"),
            new KeyValuePair<string, string>("检测丹方", "Verificando receitas de elixir"),
            new KeyValuePair<string, string>("检测信件数据", "Verificando cartas"),
            new KeyValuePair<string, string>("检测数据完成，累计修复", "Verificação concluída: "),
            new KeyValuePair<string, string>("存档修复完成，总共修补了", "Reparo do save concluído: "),
            new KeyValuePair<string, string>("处错误。", " erros corrigidos."),
            new KeyValuePair<string, string>("处错误", " erros corrigidos"),
            new KeyValuePair<string, string>("本次修复存档耗时", "Tempo do reparo: "),
            new KeyValuePair<string, string>("秒。", " s."),
            new KeyValuePair<string, string>("八荒大鬼为你的存档保驾护航！", "O mod Bahuang Dagui protege o seu save!"),
            new KeyValuePair<string, string>("大鬼修复存档MOD检测中", "Mod de reparo do save verificando"),
            new KeyValuePair<string, string>("修复:", "Corrigidos: "),
            new KeyValuePair<string, string>("境界", "Reino"),
            new KeyValuePair<string, string>("天骄", "Escolhidos"),
            new KeyValuePair<string, string>("性格", "Caráter"),
            new KeyValuePair<string, string>("坊市道具", "Itens"),
            new KeyValuePair<string, string>("道具", "Itens"),
            new KeyValuePair<string, string>("秘籍", "Manuais"),
            new KeyValuePair<string, string>("气运", "Destino"),
            new KeyValuePair<string, string>("任务", "Missões"),
            new KeyValuePair<string, string>("器灵", "Espíritos"),
            new KeyValuePair<string, string>("道号", "Títulos"),
            new KeyValuePair<string, string>("立绘", "Retratos"),
            new KeyValuePair<string, string>("技能", "Habilidades"),
            new KeyValuePair<string, string>("日志", "Históricos"),
            new KeyValuePair<string, string>("宗门", "Seitas"),
            new KeyValuePair<string, string>("建木", "Árvores-Cofre"),
            new KeyValuePair<string, string>("副本", "Masmorras"),
            new KeyValuePair<string, string>("壶妖", "Diabretes"),
            new KeyValuePair<string, string>("丹方", "Receitas"),
            new KeyValuePair<string, string>("事件", "Eventos"),
            new KeyValuePair<string, string>("信件", "Cartas"),
        };

        // Placa de andar do Pavilhão dos Manuais (school_dijiceng): caixa de 30 px, uma letra por linha.
        private static readonly Regex FloorSign = new Regex(@"^Piso (\d+)$");
        private const string FloorNumber = "PtBrPisoNum";
        private const float FloorSignMaxWidth = 50f;
        private const float FloorLettersDx = -7f;
        private const float FloorNumberDx = 11f;

        // Bandeira da seita (Salão do Conselho, Lista de Honra): uma palavra por linha, centradas, na maior fonte que
        // cabe na largura. Mais de 4 palavras também diminuem a fonte, para a altura ficar a de 4 linhas.
        private const string BannerName = "G:textSchoolName_En";
        private const string BannerParent = "LanguageGroup";
        private const int BannerMaxLines = 4;
        private const int BannerMinSize = 10;
        private static readonly Dictionary<int, int> bannerSize = new Dictionary<int, int>();

        private static string logPath;
        private static float nextScan;
        private static bool applying;

        /// <summary>Chamar depois que o LocalText foi aplicado (precisa da config carregada).</summary>
        public static void Prepare()
        {
            string root = g.mod.GetModPathRoot(ModSoleID);
            string path = Path.Combine(root, "ModAssets", "ptbr", "Tela.json");
            map = File.Exists(path)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path, Encoding.UTF8))
                : new Dictionary<string, string>();
            logPath = Path.Combine(root, "ptbr_tela_desconhecidas.txt");
            NameOrderPtBr.Prepare(Path.Combine(root, "ModAssets", "ptbr", "LocalText.json"),
                Path.Combine(root, "ModAssets", "ptbr", "Cidades.json"));

            known = new HashSet<string>();
            knownTemplates = new HashSet<string>();
            var list = g.conf.localText.allConfList;
            for (int i = 0; i < list.Count; i++)
            {
                ConfLocalTextItem item = list[i];
                if (item == null)
                {
                    continue;
                }
                AddKnown(item.en);
                AddKnown(item.ch);
            }
            foreach (var v in map.Values)
            {
                AddKnown(v);
            }
            cache.Clear();
            lastSeen.Clear();
            if (File.Exists(logPath))
            {
                foreach (var line in File.ReadAllLines(logPath, Encoding.UTF8))
                {
                    int tab = line.IndexOf('\t');
                    logged.Add(Template(tab >= 0 ? line.Substring(0, tab) : line));
                }
            }
            MelonLogger.Msg(string.Format("[PtBr] Tela: {0} trocas, {1} textos conhecidos, {2} desconhecidos ja registrados",
                map.Count, known.Count, logged.Count));
        }

        private static void AddKnown(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return;
            }
            string t = Clean(s);
            known.Add(t);
            knownTemplates.Add(Template(t));
        }

        private static string Clean(string s)
        {
            return s.Replace("\\n", "\n").Trim();
        }

        private static string Template(string s)
        {
            return Placeholder.Replace(s, "#");
        }

        /// <summary>Devolve a troca para o texto, ou null se não houver; registra desconhecidos.</summary>
        private static string Process(string text, Func<string> where)
        {
            if (known == null || string.IsNullOrEmpty(text))
            {
                return null;
            }
            string result;
            if (cache.TryGetValue(text, out result))
            {
                return result;
            }
            string t = text.Trim();
            if (!map.TryGetValue(t, out result))
            {
                result = RewriteParts(t);
                // Nomes de seita e cidade montados na ordem inglesa pelo jogo.
                result = NameOrderPtBr.Rewrite(result ?? text) ?? result;
                // Nomes de técnica que não passaram pelo GetName.
                result = TechniqueNamePtBr.Rewrite(result ?? text) ?? result;
                if (result == null && !known.Contains(t) && Letter.IsMatch(t) && !knownTemplates.Contains(Template(t)))
                {
                    LogUnknown(t, where());
                }
            }
            if (cache.Count > MaxCache)
            {
                cache.Clear();
            }
            cache[text] = result;
            return result;
        }

        private static string RewriteParts(string t)
        {
            if (SloganLine.IsMatch(t))
            {
                return SloganLine.Replace(t, m => m.Groups[1].Value + ", " + m.Groups[2].Value.ToLowerInvariant()
                    + m.Groups[3].Value + ".");
            }
            if (SaveCheck.IsMatch(t))
            {
                foreach (var term in SaveCheckTerms)
                {
                    t = t.Replace(term.Key, term.Value);
                }
                return t;
            }
            if (TimeCount.IsMatch(t))
            {
                return TimeCount.Replace(t, m => m.Groups[1].Value + " " + (m.Groups[1].Value == "1"
                    ? m.Groups[2].Value.ToLowerInvariant()
                    : m.Groups[2].Value == "Ano" ? "anos" : "meses"));
            }
            foreach (var part in Parts)
            {
                if (part.Key.IsMatch(t))
                {
                    return part.Key.Replace(t, part.Value);
                }
            }
            return null;
        }

        /// <summary>
        /// "Piso 3" na placa estreita vira "P/i/s/o" em coluna, com o número numa cópia do texto ao lado, centrado na
        /// altura. Devolve true se tratou o texto.
        /// </summary>
        private static bool ApplyFloorSign(UnityEngine.UI.Text t)
        {
            var m = FloorSign.Match(t.text ?? "");
            var rt = t.rectTransform;
            if (!m.Success || t.name == FloorNumber || rt.sizeDelta.x > FloorSignMaxWidth)
            {
                return false;
            }
            applying = true;
            try
            {
                t.text = "P\ni\ns\no";
                var numTr = rt.parent.Find(FloorNumber);
                if (numTr == null)
                {
                    var go = UnityEngine.Object.Instantiate(t.gameObject, rt.parent).Cast<UnityEngine.GameObject>();
                    go.name = FloorNumber;
                    numTr = go.transform;
                    numTr.Cast<UnityEngine.RectTransform>().anchoredPosition =
                        rt.anchoredPosition + new UnityEngine.Vector2(FloorNumberDx, 0f);
                    rt.anchoredPosition += new UnityEngine.Vector2(FloorLettersDx, 0f);
                }
                numTr.GetComponent<UnityEngine.UI.Text>().text = m.Groups[1].Value;
            }
            finally { applying = false; }
            return true;
        }

        private static void ApplyBanner(UnityEngine.UI.Text t)
        {
            var parent = t.transform.parent;
            if (t.name != BannerName || parent == null || parent.name != BannerParent)
            {
                return;
            }
            string[] words = (t.text ?? "").Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
            {
                return;
            }
            // A fonte do prefab: a cada nome novo o tamanho é recalculado a partir dela.
            int id = t.GetInstanceID();
            int size;
            if (!bannerSize.TryGetValue(id, out size))
            {
                size = t.fontSize;
                bannerSize[id] = size;
            }
            applying = true;
            try
            {
                t.text = string.Join("\n", words);
                t.horizontalOverflow = UnityEngine.HorizontalWrapMode.Overflow;
                t.alignment = (UnityEngine.TextAnchor)((int)t.alignment / 3 * 3 + 1);
                t.resizeTextForBestFit = false;
                t.fontSize = size;
                float scale = Math.Min(1f, (float)BannerMaxLines / words.Length);
                float width = t.preferredWidth;
                if (width > 0f)
                {
                    scale = Math.Min(scale, t.rectTransform.rect.width / width);
                }
                t.fontSize = Math.Max(BannerMinSize, (int)(size * scale));
            }
            finally { applying = false; }
        }

        private static void LogUnknown(string t, string where)
        {
            string escaped = t.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
            // Um registro por modelo: textos que só mudam nos números (contadores) não enchem o arquivo.
            if (logPath == null || logged.Count >= MaxLogged || !logged.Add(Template(escaped)))
            {
                return;
            }
            string line = escaped + "\t" + where + "\n";
            try
            {
                File.AppendAllText(logPath, line, new UTF8Encoding(false));
            }
            catch (Exception e)
            {
                MelonLogger.Warning("[PtBr] Tela: falha ao registrar desconhecido: " + e.Message);
                logPath = null;
            }
        }

        private static string PathOf(UnityEngine.Component c)
        {
            var parts = new List<string>();
            var tr = c.transform;
            for (int i = 0; tr != null && i < 6; i++)
            {
                parts.Insert(0, tr.name);
                tr = tr.parent;
            }
            return string.Join("/", parts.ToArray());
        }

        private static void Apply(TMPro.TMP_Text t)
        {
            if (applying || t == null)
            {
                return;
            }
            string rep = Process(t.text, () => "TMP " + PathOf(t));
            if (rep != null)
            {
                applying = true;
                try { t.text = rep; } finally { applying = false; }
            }
        }

        /// <summary>Troca o texto agora, sem esperar a próxima varredura geral.</summary>
        public static void Refresh(UnityEngine.UI.Text t)
        {
            Apply(t);
        }

        private static void Apply(UnityEngine.UI.Text t)
        {
            if (applying || t == null || ApplyFloorSign(t))
            {
                return;
            }
            string rep = Process(t.text, () => "Text " + PathOf(t));
            if (rep != null)
            {
                applying = true;
                try { t.text = rep; } finally { applying = false; }
            }
            ApplyBanner(t);
        }

        /// <summary>
        /// Varre os textos ativos: rótulos fixos de prefab chegam prontos do prefab e nunca passam por set_text.
        /// </summary>
        public static void Tick()
        {
            if (known == null || UnityEngine.Time.unscaledTime < nextScan)
            {
                return;
            }
            nextScan = UnityEngine.Time.unscaledTime + ScanInterval;
            var tmps = UnityEngine.Object.FindObjectsOfType(Il2CppType.Of<TMPro.TMP_Text>());
            for (int i = 0; i < tmps.Length; i++)
            {
                var t = tmps[i].TryCast<TMPro.TMP_Text>();
                if (t != null && Changed(t.GetInstanceID(), t.text))
                {
                    Apply(t);
                    // Guarda o texto já trocado: se o jogo voltar ao original, a próxima varredura troca de novo.
                    lastSeen[t.GetInstanceID()] = t.text;
                }
            }
            var texts = UnityEngine.Object.FindObjectsOfType(Il2CppType.Of<UnityEngine.UI.Text>());
            for (int i = 0; i < texts.Length; i++)
            {
                var t = texts[i].TryCast<UnityEngine.UI.Text>();
                if (t != null && Changed(t.GetInstanceID(), t.text))
                {
                    Apply(t);
                    lastSeen[t.GetInstanceID()] = t.text;
                }
            }
            if (lastSeen.Count > MaxCache)
            {
                lastSeen.Clear();
            }
        }

        private static bool Changed(int id, string text)
        {
            string old;
            if (lastSeen.TryGetValue(id, out old) && old == text)
            {
                return false;
            }
            lastSeen[id] = text;
            return true;
        }

        [HarmonyPatch(typeof(TMPro.TMP_Text), nameof(TMPro.TMP_Text.text), MethodType.Setter)]
        private static class TmpSetText
        {
            private static void Postfix(TMPro.TMP_Text __instance)
            {
                try { Apply(__instance); }
                catch (Exception e) { MelonLogger.Warning("[PtBr] Tela (TMP): " + e.Message); }
            }
        }

        /// <summary>
        /// O mapa-múndi escreve os nomes com TMP_Text.SetText(string, ...), que não passa pelo setter de text.
        /// </summary>
        [HarmonyPatch]
        private static class TmpSetTextMethod
        {
            private static IEnumerable<System.Reflection.MethodBase> TargetMethods()
            {
                foreach (var m in typeof(TMPro.TMP_Text).GetMethods())
                {
                    var ps = m.GetParameters();
                    if (m.Name == "SetText" && ps.Length > 0 && ps[0].ParameterType == typeof(string))
                    {
                        yield return m;
                    }
                }
            }

            private static void Postfix(TMPro.TMP_Text __instance)
            {
                try { Apply(__instance); }
                catch (Exception e) { MelonLogger.Warning("[PtBr] Tela (TMP SetText): " + e.Message); }
            }
        }

        /// <summary>
        /// Os nomes de cidade e seita no cenário. Trocar o texto do rótulo depois não muda o que aparece (o jogo já
        /// montou o desenho), então o nome é trocado na entrada; o Postfix só registra o resultado.
        /// </summary>
        [HarmonyPatch(typeof(GridBaseUI), nameof(GridBaseUI.SetBuildName))]
        private static class GridBuildName
        {
            private static readonly HashSet<string> seen = new HashSet<string>();

            private static void Prefix(ref string name)
            {
                try
                {
                    string rep = Process(name, () => "cenario");
                    if (rep != null)
                    {
                        name = rep;
                    }
                }
                catch (Exception e) { MelonLogger.Warning("[PtBr] Tela (cenario): " + e.Message); }
            }

            private static void Postfix(GridBaseUI __instance, string name)
            {
                try
                {
                    var go = __instance.goName;
                    if (go == null)
                    {
                        return;
                    }
                    var tmps = go.GetComponentsInChildren<TMPro.TMP_Text>(true);
                    string shown = null;
                    for (int i = 0; i < tmps.Length; i++)
                    {
                        Apply(tmps[i]);
                        lastSeen[tmps[i].GetInstanceID()] = shown = tmps[i].text;
                    }
                    if (name != null && seen.Count < 500 && seen.Add(name))
                    {
                        MelonLogger.Msg(string.Format("[PtBr] Cenario: \"{0}\" -> \"{1}\" ({2} textos)", name, shown, tmps.Length));
                    }
                }
                catch (Exception e) { MelonLogger.Warning("[PtBr] Tela (cenario): " + e.Message); }
            }
        }

        [HarmonyPatch(typeof(UnityEngine.UI.Text), nameof(UnityEngine.UI.Text.text), MethodType.Setter)]
        private static class UiSetText
        {
            private static void Postfix(UnityEngine.UI.Text __instance)
            {
                try { Apply(__instance); }
                catch (Exception e) { MelonLogger.Warning("[PtBr] Tela (Text): " + e.Message); }
            }
        }
    }
}
