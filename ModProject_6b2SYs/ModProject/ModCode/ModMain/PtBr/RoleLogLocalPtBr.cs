using System.Collections.Generic;
using System.IO;
using System.Text;
using MelonLoader;
using Newtonsoft.Json;

namespace MOD_6b2SYs
{
    /// <summary>
    /// Troca o texto inglês (en) de RoleLogLocal (notícias do mês, diário de eventos, falas de NPC) pelo PT-BR
    /// de ModAssets/ptbr/RoleLogLocal.json (id -> texto), como o LocalTextPtBr faz com o LocalText.
    /// </summary>
    public static class RoleLogLocalPtBr
    {
        private const string ModSoleID = "6b2SYs";
        private static Dictionary<string, string> translations;

        /// <summary>Devolve false enquanto a config do jogo ainda não carregou.</summary>
        public static bool TryApply()
        {
            if (g.conf == null || g.conf.roleLogLocal == null || g.conf.roleLogLocal.allConfList == null
                || g.conf.roleLogLocal.allConfList.Count == 0)
            {
                return false;
            }

            string path = Path.Combine(g.mod.GetModPathRoot(ModSoleID), "ModAssets", "ptbr", "RoleLogLocal.json");
            if (!File.Exists(path))
            {
                MelonLogger.Msg("[PtBr] RoleLogLocal: arquivo nao encontrado (" + path + ")");
                return true;
            }
            var pt = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path, Encoding.UTF8));
            translations = pt;

            var list = g.conf.roleLogLocal.allConfList;
            int used = 0;
            for (int i = 0; i < list.Count; i++)
            {
                ConfRoleLogLocalItem item = list[i];
                string text;
                if (item != null && pt.TryGetValue(item.id.ToString(), out text))
                {
                    item.en = text;
                    used++;
                }
            }
            MelonLogger.Msg(string.Format("[PtBr] RoleLogLocal: {0} traduzidas de {1} ({2} no JSON)", used, list.Count, pt.Count));
            WriteMissing();
            return true;
        }

        /// <summary>Grava em ptbr_missing_RoleLogLocal.txt as linhas da tabela sem tradução neste momento.</summary>
        public static void WriteMissing()
        {
            if (translations == null)
            {
                return;
            }
            var missing = new StringBuilder();
            var list = g.conf.roleLogLocal.allConfList;
            for (int i = 0; i < list.Count; i++)
            {
                ConfRoleLogLocalItem item = list[i];
                if (item != null && !translations.ContainsKey(item.id.ToString()))
                {
                    missing.Append(item.id).Append('\t').Append(item.keyID).Append('\t').Append(OneLine(item.ch)).Append('\t')
                        .Append(OneLine(item.en)).Append('\n');
                }
            }
            File.WriteAllText(Path.Combine(g.mod.GetModPathRoot(ModSoleID), "ptbr_missing_RoleLogLocal.txt"), missing.ToString(),
                new UTF8Encoding(false));
        }

        private static string OneLine(string s)
        {
            return s == null ? "" : s.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
        }
    }
}
