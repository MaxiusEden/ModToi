using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MelonLoader;

namespace MOD_6b2SYs
{
    /// <summary>
    /// Troca o en da BattleSkillPrefixName (peças dos nomes de técnica, fora do LocalText) pelo PT de
    /// ModAssets/ptbr/Tecnicas.json e remonta na ordem portuguesa o nome que GetName devolve.
    /// </summary>
    public static class TechniquePrefixPtBr
    {
        private const string ModSoleID = "6b2SYs";

        /// <summary>Devolve false enquanto a config do jogo ainda não carregou.</summary>
        public static bool TryApply()
        {
            if (g.conf == null || g.conf.battleSkillPrefixName == null || g.conf.battleSkillPrefixName._allConfList == null
                || g.conf.battleSkillPrefixName._allConfList.Count == 0)
            {
                return false;
            }

            string path = Path.Combine(g.mod.GetModPathRoot(ModSoleID), "ModAssets", "ptbr", "Tecnicas.json");
            var data = TechniqueNamePtBr.Load(path);
            var list = g.conf.battleSkillPrefixName._allConfList;
            int used = 0;
            for (int i = 0; i < list.Count; i++)
            {
                ConfBattleSkillPrefixNameItem item = list[i];
                if (item == null)
                {
                    continue;
                }
                string id = item.id.ToString(), text;
                List<string> forms;
                if (item.index == 1 && data.peca1.TryGetValue(id, out text))
                {
                    item.en = text;
                    used++;
                }
                else if (item.index == 2 && data.peca2.TryGetValue(id, out forms))
                {
                    item.en = forms[0];
                    used++;
                }
            }
            MelonLogger.Msg(string.Format("[PtBr] Tecnicas: {0} pecas traduzidas de {1}, {2} nomes-base",
                used, list.Count, data.bases.Count));
            return true;
        }

        [HarmonyPatch(typeof(ConfBattleSkillPrefixName), nameof(ConfBattleSkillPrefixName.GetName))]
        private static class GetName
        {
            private static void Postfix(ref string __result)
            {
                try
                {
                    string name = TechniqueNamePtBr.Rebuild(__result);
                    if (name != null)
                    {
                        __result = name;
                    }
                }
                catch (Exception e)
                {
                    MelonLogger.Warning("[PtBr] Tecnicas (GetName): " + e.Message);
                }
            }
        }
    }
}
