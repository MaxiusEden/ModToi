using System;
using UnityEngine;

namespace MOD_6b2SYs
{
    /// <summary>
    /// Desloca na tela os textos que o PT-BR deixou sobrepostos (caminhos achados com o F9 do LayoutDumpPtBr).
    /// O deslocamento é somado à posição que o jogo deu e reaplicado se o jogo a refizer.
    /// </summary>
    public static class LayoutPtBr
    {
        private const string UiRoot = "Game/UIRoot/Canvas/Root/UI/";
        private const int Interval = 20;
        // Uma linha da ficha do NPC: os traços descem para sair da frente do nome da seita.
        private const float TraitsDown = 28f;

        private class Rule
        {
            public string Window, Name, Parent;
            public Func<RectTransform, Vector2> Offset;
            public RectTransform Target;
            public Vector2 Base, Applied;
        }

        private static readonly Rule[] Rules = {
            // Criação de personagem: o reino quebra em duas linhas e a primeira encosta no carisma.
            new Rule { Window = "CreatePlayer", Name = "G:textLevelValue_En", Offset = FirstLineOnLabel },
            // Ficha do NPC: Traços Internos e Externos.
            new Rule { Window = "NPCInfo", Name = "Item2", Parent = "Group:Property",
                Offset = rt => new Vector2(0f, -TraitsDown) },
            // Minhas Missões: o ícone fica entre as aspas de 'Select "   "'; "Selecione" é mais longo.
            new Rule { Window = "PlayerTask", Name = "G:textPathTip1", Offset = rt => KeepRightEdge(rt, "Select \"   \"") },
        };

        private static int frame;
        private static bool off;

        public static void Tick()
        {
            if (off || ++frame % Interval != 0)
            {
                return;
            }
            try
            {
                foreach (var rule in Rules)
                {
                    Apply(rule);
                }
            }
            catch (Exception e)
            {
                off = true;
                MelonLoader.MelonLogger.Error("[PtBr] Layout: " + e);
            }
        }

        private static void Apply(Rule rule)
        {
            var window = GameObject.Find(UiRoot + rule.Window);
            var tr = window == null ? null : Find(window.transform, rule.Name, rule.Parent);
            var rt = tr == null ? null : tr.TryCast<RectTransform>();
            if (rt == null)
            {
                rule.Target = null;
                return;
            }
            Vector2 cur = rt.anchoredPosition;
            if (rule.Target == null || rule.Target.Pointer != rt.Pointer || cur != rule.Applied)
            {
                rule.Target = rt;
                rule.Base = cur;
            }
            Vector2 target = rule.Base + rule.Offset(rt);
            if (target != cur)
            {
                rt.anchoredPosition = target;
            }
            rule.Applied = target;
        }

        private static Transform Find(Transform tr, string name, string parent)
        {
            for (int i = 0; i < tr.childCount; i++)
            {
                var child = tr.GetChild(i);
                if (child.name == name && (parent == null || tr.name == parent))
                {
                    return child;
                }
                var found = Find(child, name, parent);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        /// <summary>Texto de várias linhas centrado no rótulo: desce até a primeira linha ficar na altura dele.</summary>
        private static Vector2 FirstLineOnLabel(RectTransform rt)
        {
            var t = rt.GetComponent<UnityEngine.UI.Text>();
            int lines = t == null ? 1 : t.cachedTextGenerator.lineCount;
            if (lines < 2)
            {
                return Vector2.zero;
            }
            return new Vector2(0f, -t.preferredHeight * (lines - 1) / lines / 2f);
        }

        /// <summary>Texto centrado: anda para a esquerda o que o PT tem a mais que o inglês, para o fim ficar no mesmo
        /// lugar.</summary>
        private static Vector2 KeepRightEdge(RectTransform rt, string en)
        {
            var t = rt.GetComponent<UnityEngine.UI.Text>();
            if (t == null)
            {
                return Vector2.zero;
            }
            float enWidth = t.cachedTextGeneratorForLayout.GetPreferredWidth(en, t.GetGenerationSettings(Vector2.zero))
                / t.pixelsPerUnit;
            return new Vector2(-Math.Max(0f, t.preferredWidth - enWidth) / 2f, 0f);
        }
    }
}
