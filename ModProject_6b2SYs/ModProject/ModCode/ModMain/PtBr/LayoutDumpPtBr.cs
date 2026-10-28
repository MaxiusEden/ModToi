using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MelonLoader;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace MOD_6b2SYs
{
    /// <summary>
    /// Diagnóstico de layout: com F9, grava em ptbr_layout.txt todos os textos visíveis com caminho, posição e
    /// tamanho, e as imagens com o nome do sprite, para achar o objeto que precisa ser movido numa tela.
    /// </summary>
    public static class LayoutDumpPtBr
    {
        private const string ModSoleID = "6b2SYs";
        private static int count;

        public static void Tick()
        {
            if (Input.GetKeyDown(KeyCode.F9))
            {
                Dump();
            }
        }

        private static void Dump()
        {
            var sb = new StringBuilder();
            count++;
            sb.Append("=== F9 #").Append(count).Append(' ').Append(DateTime.Now.ToString("HH:mm:ss"))
                .Append(" tela ").Append(Screen.width).Append('x').Append(Screen.height).Append('\n');
            sb.Append("tipo\tcaminho\ttexto\tanchoredPos\tsizeDelta\tpivot\tanchorMin\tanchorMax\tfonte\tquebra\taltura pref.\tlayout do pai\n");
            int n = 0;
            var tmps = UnityEngine.Object.FindObjectsOfType(Il2CppType.Of<TMPro.TMP_Text>());
            for (int i = 0; i < tmps.Length; i++)
            {
                var t = tmps[i].TryCast<TMPro.TMP_Text>();
                if (t != null && t.isActiveAndEnabled && !string.IsNullOrEmpty(t.text))
                {
                    Line(sb, "TMP", t, t.text, t.fontSize, t.enableWordWrapping ? "quebra" : "sem quebra", t.preferredHeight);
                    n++;
                }
            }
            var texts = UnityEngine.Object.FindObjectsOfType(Il2CppType.Of<UnityEngine.UI.Text>());
            for (int i = 0; i < texts.Length; i++)
            {
                var t = texts[i].TryCast<UnityEngine.UI.Text>();
                if (t != null && t.isActiveAndEnabled && !string.IsNullOrEmpty(t.text))
                {
                    Line(sb, "Text", t, t.text, t.fontSize, t.horizontalOverflow + "/" + t.verticalOverflow, t.preferredHeight);
                    n++;
                }
            }
            // Imagens (ícones, caixas de marcar, placas com texto desenhado): o "texto" é o nome do sprite.
            var images = UnityEngine.Object.FindObjectsOfType(Il2CppType.Of<UnityEngine.UI.Image>());
            for (int i = 0; i < images.Length; i++)
            {
                var img = images[i].TryCast<UnityEngine.UI.Image>();
                if (img != null && img.isActiveAndEnabled)
                {
                    Line(sb, "Img", img, img.sprite == null ? "-" : img.sprite.name, 0f, "", 0f);
                }
            }
            string path = Path.Combine(g.mod.GetModPathRoot(ModSoleID), "ptbr_layout.txt");
            File.AppendAllText(path, sb.ToString(), new UTF8Encoding(false));
            MelonLogger.Msg(string.Format("[PtBr] Layout: F9 #{0}, {1} textos gravados em {2}", count, n, path));
        }

        private static void Line(StringBuilder sb, string kind, Component c, string text, float fontSize, string wrap,
            float preferredHeight)
        {
            var rt = c.transform.TryCast<RectTransform>();
            string clean = text.Replace("\r", "").Replace("\n", "\\n").Replace("\t", " ");
            if (clean.Length > 80)
            {
                clean = clean.Substring(0, 80) + "…";
            }
            var parent = c.transform.parent;
            string layout = "";
            if (parent != null)
            {
                if (parent.GetComponent(Il2CppType.Of<UnityEngine.UI.LayoutGroup>()) != null)
                {
                    layout += "LayoutGroup ";
                }
                if (c.GetComponent(Il2CppType.Of<UnityEngine.UI.ContentSizeFitter>()) != null)
                {
                    layout += "ContentSizeFitter";
                }
            }
            sb.Append(kind).Append('\t').Append(FullPath(c.transform)).Append('\t').Append(clean).Append('\t');
            if (rt != null)
            {
                sb.Append(V(rt.anchoredPosition)).Append('\t').Append(V(rt.sizeDelta)).Append('\t').Append(V(rt.pivot))
                    .Append('\t').Append(V(rt.anchorMin)).Append('\t').Append(V(rt.anchorMax));
            }
            else
            {
                sb.Append("\t\t\t\t");
            }
            sb.Append('\t').Append(fontSize.ToString("0.#")).Append('\t').Append(wrap).Append('\t')
                .Append(preferredHeight.ToString("0.#")).Append('\t').Append(layout.Trim()).Append('\n');
        }

        private static string V(Vector2 v)
        {
            return v.x.ToString("0.#") + "," + v.y.ToString("0.#");
        }

        private static string FullPath(Transform tr)
        {
            var parts = new List<string>();
            for (; tr != null; tr = tr.parent)
            {
                parts.Insert(0, tr.name);
            }
            return string.Join("/", parts.ToArray());
        }
    }
}
