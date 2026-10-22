using System.Collections.Generic;
using System.IO;
using System.Text;
using MelonLoader;
using Newtonsoft.Json;

namespace MOD_6b2SYs
{
    /// <summary>
    /// Troca o texto inglês (en) de LocalText pelo PT-BR de ModAssets/ptbr/LocalText.json.
    /// </summary>
    public static class LocalTextPtBr
    {
        private const string ModSoleID = "6b2SYs";

        /// <summary>
        /// Devolve false enquanto a config do jogo ainda não carregou, para quem chama tentar de novo.
        /// </summary>
        public static bool TryApply()
        {
            if (g.conf == null || g.conf.localText == null || g.conf.localText.allConfList == null
                || g.conf.localText.allConfList.Count == 0)
            {
                return false;
            }

            string root = g.mod.GetModPathRoot(ModSoleID);
            string path = Path.Combine(root, "ModAssets", "ptbr", "LocalText.json");
            Dictionary<string, string> pt = File.Exists(path)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path, Encoding.UTF8))
                : new Dictionary<string, string>();

            var used = new HashSet<string>();
            var missing = new StringBuilder();
            var list = g.conf.localText.allConfList;
            int total = 0;
            for (int i = 0; i < list.Count; i++)
            {
                ConfLocalTextItem item = list[i];
                if (item == null)
                {
                    continue;
                }
                total++;
                string text;
                if (pt.TryGetValue(item.key, out text))
                {
                    item.en = text;
                    used.Add(item.key);
                }
                else
                {
                    missing.Append(item.key).Append('\t').Append(OneLine(item.en)).Append('\n');
                }
            }

            File.WriteAllText(Path.Combine(root, "ptbr_missing_LocalText.txt"), missing.ToString(), new UTF8Encoding(false));
            // Só ASCII: o Latest.log do MelonLoader 0.5.4 não é gravado em UTF-8 e estraga acentos.
            MelonLogger.Msg(string.Format("[PtBr] LocalText: {0} traduzidas, {1} sem traducao, {2} chaves do JSON inexistentes no jogo ({3})",
                used.Count, total - used.Count, pt.Count - used.Count, path));
            return true;
        }

        private static string OneLine(string s)
        {
            return s == null ? "" : s.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
        }
    }
}
