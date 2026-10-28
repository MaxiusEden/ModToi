using System;
using System.Collections.Generic;
using UnityEngine;

namespace MOD_6b2SYs
{
    /// <summary>
    /// Ajusta na tela os textos que o PT-BR deixou sobrepostos ou cortados (caminhos achados com o F9 do
    /// LayoutDumpPtBr). O deslocamento é somado à posição que o jogo deu e reaplicado se o jogo a refizer; o ajuste
    /// de tamanho liga o "best fit" do UnityEngine.UI.Text, que diminui a fonte até o texto caber na caixa.
    /// </summary>
    public static class LayoutPtBr
    {
        private const string UiRoot = "Game/UIRoot/Canvas/Root/UI/";
        private const int Interval = 20;
        // Uma linha da ficha do NPC: os traços descem para sair da frente do nome da seita.
        private const float TraitsDown = 28f;
        // Pavilhão dos Manuais: nome, preços e botão descem para sair da moldura do ícone.
        private const float ManualDown = 6f;
        // Filtro do Pavilhão da Seita: ícone e nome vão para a direita, longe da caixa de marcar.
        private const float FilterRight = 8f;
        private const int MaxPlaced = 5000;

        private class Rule
        {
            // Nome do objeto (null: qualquer um), começo do nome, nome do pai, de um ancestral e um nome a pular.
            public string Window, Name, Prefix, Parent, Under, Except;
            public Func<RectTransform, Vector2> Offset;
            // > 0: liga o best fit do texto com essa fonte mínima.
            public int FitMin;
            // Não quebra a linha: a palavra que passa um pouco da caixa não perde a última letra para a linha de baixo.
            public bool NoWrap;
        }

        private class Placed
        {
            public Vector2 Base, Applied;
        }

        private static readonly Rule[] Rules = {
            // Criação de personagem: o reino quebra em duas linhas e a primeira encosta no carisma.
            new Rule { Window = "CreatePlayer", Name = "G:textLevelValue_En", Offset = FirstLineOnLabel },
            // Ficha do NPC: Traços Internos e Externos.
            new Rule { Window = "NPCInfo", Name = "Item2", Parent = "Group:Property",
                Offset = rt => new Vector2(0f, -TraitsDown) },
            // Pavilhão dos Manuais: tudo do item menos o ícone.
            new Rule { Window = "TownMarketBook", Parent = "Root", Under = "G:goItemRoot_En", Except = "Icon",
                Offset = rt => new Vector2(0f, -ManualDown) },
            // Aprimorar Habilidade e a aba Habil.: reino e nome das técnicas na lista e no quadro.
            new Rule { Window = "MartialTrain", Name = "TextGrade", FitMin = 10 },
            new Rule { Window = "MartialTrain", Name = "TextName", FitMin = 10 },
            new Rule { Window = "MartialTrain", Name = "G:textInfoGrade_En", FitMin = 10 },
            new Rule { Window = "PlayerInfo", Name = "TextGrade", Under = "Group:Skill", FitMin = 10 },
            new Rule { Window = "PlayerInfo", Name = "TextName", Under = "Group:Skill", FitMin = 10 },
            // Artesanato, Herbologia: nome da prescrição.
            new Rule { Window = "PlayerInfo", Name = "Text", Parent = "Open", Under = "G:goArtItemRoot3", FitMin = 11 },
            // Talismãs: material de cada nível.
            new Rule { Window = "RefineRuneFormula", Name = "Text", Parent = "G:goMaterialItem(Clone)", FitMin = 11 },
            // Seita: regras, propósito e características.
            new Rule { Window = "School", Name = "G:textRule_En", FitMin = 11 },
            new Rule { Window = "School", Name = "G:textTenet_En", FitMin = 11 },
            new Rule { Window = "School", Name = "G:textIdiosyncrasy_En", FitMin = 11 },
            // Promoção: Lista de Honra.
            new Rule { Window = "SchoolPost", Name = "G:textHonor", FitMin = 12 },
            // Pavilhão da Seita: filtro de manuais ("Relâmpago" quebrava o "o" para baixo da caixa de marcar).
            new Rule { Window = "SchoolLibrary", Prefix = "G:textFilter", Under = "Group:MartialFilter", NoWrap = true,
                Offset = rt => new Vector2(FilterRight, 0f) },
        };

        private static readonly Dictionary<IntPtr, Placed> placed = new Dictionary<IntPtr, Placed>();
        private static readonly List<string> ancestors = new List<string>();
        private static readonly HashSet<string> walked = new HashSet<string>();
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
                walked.Clear();
                foreach (var rule in Rules)
                {
                    // Uma varredura por janela aplica todas as regras dela.
                    if (!walked.Add(rule.Window))
                    {
                        continue;
                    }
                    var window = GameObject.Find(UiRoot + rule.Window);
                    if (window != null)
                    {
                        ancestors.Clear();
                        Walk(window.transform, rule.Window);
                    }
                }
                if (placed.Count > MaxPlaced)
                {
                    placed.Clear();
                }
            }
            catch (Exception e)
            {
                off = true;
                MelonLoader.MelonLogger.Error("[PtBr] Layout: " + e);
            }
        }

        private static void Walk(Transform tr, string window)
        {
            ancestors.Add(tr.name);
            for (int i = 0; i < tr.childCount; i++)
            {
                var child = tr.GetChild(i);
                string name = child.name;
                foreach (var rule in Rules)
                {
                    if (rule.Window == window && Matches(rule, name, tr.name))
                    {
                        Apply(rule, child);
                    }
                }
                Walk(child, window);
            }
            ancestors.RemoveAt(ancestors.Count - 1);
        }

        private static bool Matches(Rule rule, string name, string parent)
        {
            return (rule.Name == null || rule.Name == name) && name != rule.Except
                && (rule.Prefix == null || name.StartsWith(rule.Prefix, StringComparison.Ordinal))
                && (rule.Parent == null || rule.Parent == parent)
                && (rule.Under == null || ancestors.Contains(rule.Under));
        }

        private static void Apply(Rule rule, Transform tr)
        {
            if (rule.FitMin > 0 || rule.NoWrap)
            {
                var t = tr.GetComponent<UnityEngine.UI.Text>();
                if (t != null && rule.FitMin > 0 && !t.resizeTextForBestFit)
                {
                    t.resizeTextMaxSize = t.fontSize;
                    t.resizeTextMinSize = Math.Min(rule.FitMin, t.fontSize);
                    t.resizeTextForBestFit = true;
                }
                if (t != null && rule.NoWrap && t.horizontalOverflow != HorizontalWrapMode.Overflow)
                {
                    t.horizontalOverflow = HorizontalWrapMode.Overflow;
                }
            }
            if (rule.Offset == null)
            {
                return;
            }
            var rt = tr.TryCast<RectTransform>();
            if (rt == null)
            {
                return;
            }
            Vector2 cur = rt.anchoredPosition;
            Placed p;
            if (!placed.TryGetValue(rt.Pointer, out p) || cur != p.Applied)
            {
                p = new Placed { Base = cur };
                placed[rt.Pointer] = p;
            }
            Vector2 target = p.Base + rule.Offset(rt);
            if (target != cur)
            {
                rt.anchoredPosition = target;
            }
            p.Applied = target;
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
    }
}
