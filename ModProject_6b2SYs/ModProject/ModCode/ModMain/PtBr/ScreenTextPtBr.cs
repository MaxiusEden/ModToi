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
            NameOrderPtBr.Prepare(Path.Combine(root, "ModAssets", "ptbr", "LocalText.json"));

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
                    logged.Add(tab >= 0 ? line.Substring(0, tab) : line);
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
                // Nomes de seita e cidade montados na ordem inglesa pelo jogo.
                result = NameOrderPtBr.Rewrite(text);
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

        private static void LogUnknown(string t, string where)
        {
            if (logPath == null || logged.Count >= MaxLogged || !logged.Add(t))
            {
                return;
            }
            string line = t.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t") + "\t" + where + "\n";
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

        private static void Apply(UnityEngine.UI.Text t)
        {
            if (applying || t == null)
            {
                return;
            }
            string rep = Process(t.text, () => "Text " + PathOf(t));
            if (rep != null)
            {
                applying = true;
                try { t.text = rep; } finally { applying = false; }
            }
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
                }
            }
            var texts = UnityEngine.Object.FindObjectsOfType(Il2CppType.Of<UnityEngine.UI.Text>());
            for (int i = 0; i < texts.Length; i++)
            {
                var t = texts[i].TryCast<UnityEngine.UI.Text>();
                if (t != null && Changed(t.GetInstanceID(), t.text))
                {
                    Apply(t);
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
