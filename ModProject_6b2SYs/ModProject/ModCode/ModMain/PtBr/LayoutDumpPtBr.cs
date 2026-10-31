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
        private static readonly System.Text.RegularExpressions.Regex Hanzi =
            new System.Text.RegularExpressions.Regex(@"[一-鿿]");
        private static readonly HashSet<string> LongHanzi = new HashSet<string>();

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
            sb.Append("tipo\tcaminho\ttexto\tanchoredPos\tsizeDelta\tpivot\tanchorMin\tanchorMax\tfonte\tquebra\taltura pref.\tlayout do pai\tna tela (x0..x1,y0..y1)\n");
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
                    // Com ajuste automático, a fonte desenhada é a do gerador, não a de fontSize.
                    string fit = t.resizeTextForBestFit ? "/ajuste " + t.cachedTextGenerator.fontSizeUsedForBestFit : "";
                    Line(sb, "Text", t, t.text, t.fontSize, t.horizontalOverflow + "/" + t.verticalOverflow + fit, t.preferredHeight);
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
            LocalTextPtBr.WriteMissing();
            RoleLogLocalPtBr.WriteMissing();
            MelonLogger.Msg(string.Format("[PtBr] Layout: F9 #{0}, {1} textos gravados em {2}", count, n, path));
        }

        private static void Line(StringBuilder sb, string kind, Component c, string text, float fontSize, string wrap,
            float preferredHeight)
        {
            var rt = c.transform.TryCast<RectTransform>();
            string clean = text.Replace("\r", "").Replace("\n", "\\n").Replace("\t", " ");
            if (clean.Length > 80)
            {
                // Texto longo ainda em chinês: guarda inteiro, para poder ser traduzido.
                if (Hanzi.IsMatch(clean) && LongHanzi.Add(clean))
                {
                    File.AppendAllText(Path.Combine(g.mod.GetModPathRoot(ModSoleID), "ptbr_hanzi_longos.txt"),
                        clean + "\n", new UTF8Encoding(false));
                }
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
                .Append(preferredHeight.ToString("0.#")).Append('\t').Append(layout.Trim()).Append('\t')
                .Append(OnScreen(c, rt)).Append('\n');
        }

        /// <summary>Retângulo nas coordenadas do Canvas raiz, para comparar objetos de ramos diferentes.</summary>
        private static string OnScreen(Component c, RectTransform rt)
        {
            var canvas = c.GetComponentInParent<Canvas>();
            if (rt == null || canvas == null)
            {
                return "";
            }
            var root = canvas.rootCanvas.transform;
            Rect r = rt.rect;
            Vector3 a = root.InverseTransformPoint(rt.TransformPoint(new Vector3(r.xMin, r.yMin, 0f)));
            Vector3 b = root.InverseTransformPoint(rt.TransformPoint(new Vector3(r.xMax, r.yMax, 0f)));
            return a.x.ToString("0") + ".." + b.x.ToString("0") + "," + a.y.ToString("0") + ".." + b.y.ToString("0");
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
