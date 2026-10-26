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
            return true;
        }
    }
}
