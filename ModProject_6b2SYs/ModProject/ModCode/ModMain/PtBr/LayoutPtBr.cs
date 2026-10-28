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
        // Filtro do Pavilhão da Seita: ícone e nome vão para a esquerda, longe da caixa de marcar à direita.
        private const float FilterLeft = 20f;
        private const float FilterMaxWidth = 100f;
        // Aprimorar Habilidade: "Cultivo", o ícone e os dias vão para a direita, longe do botão Máx.
        private const float TrainDaysRight = 10f;
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
            // Não corta na altura: a segunda linha que passa da caixa continua visível.
            public bool NoClip;
            // > 0: fonte fixa.
            public int Size;
            // > 0: diminui a fonte até a linha caber nessa largura.
            public float MaxWidth;
            // > 0: texto que se ajusta à largura (ContentSizeFitter) e passa dela quebra em linhas com a fonte WrapSize.
            public float WrapWidth;
            public int WrapSize;
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
            new Rule { Window = "School", Name = "G:textIdiosyncrasy_En", FitMin = 9 },
            // Promoção: Lista de Honra e o nome do cargo ("Discípulo Verdadeiro" cobria o ícone da posição atual).
            new Rule { Window = "SchoolPost", Name = "G:textHonor", FitMin = 12 },
            new Rule { Window = "SchoolPost", Name = "Text", Parent = "Toggle", Under = "G:goPostItem_En(Clone)",
                WrapWidth = 100f, WrapSize = 16 },
            // Pavilhão da Seita: filtro de manuais ("Relâmpago" passava para baixo da caixa de marcar).
            new Rule { Window = "SchoolLibrary", Prefix = "G:textFilter", Except = "G:textFilterTitle",
                Under = "Group:MartialFilter", NoWrap = true, MaxWidth = FilterMaxWidth,
                Offset = rt => new Vector2(-FilterLeft, 0f) },
            // Aprimorar Habilidade: dias de cultivo.
            new Rule { Window = "MartialTrain", Name = "G:textDayTitle", Parent = "G:goTrainManExp",
                Offset = rt => new Vector2(TrainDaysRight, 0f) },
            new Rule { Window = "MartialTrain", Name = "Text", Parent = "G:goTrainManExp",
                Offset = rt => new Vector2(TrainDaysRight, 0f) },
            new Rule { Window = "MartialTrain", Name = "G:textTrainCostDay", Parent = "G:goTrainManExp",
                Offset = rt => new Vector2(TrainDaysRight, 0f) },
            // Estalagem: custo e, na linha de baixo, a duração.
            new Rule { Window = "TownHotel", Name = "G:textCost", NoClip = true },
            // Salão de Missões: aba Relatórios e botão de pedir informações.
            new Rule { Window = "SchoolTaskLobby", Name = "G:textTglReportTask_En", Size = 12 },
            new Rule { Window = "SchoolTaskLobby", Name = "G:textMess", FitMin = 11 },
            // Membros da Seita: Expulsar, Redistribuir, Marcar.
            new Rule { Window = "SchoolAdmin", Prefix = "G:text", Under = "ViewState", FitMin = 11 },
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
            var t = tr.GetComponent<UnityEngine.UI.Text>();
            if (t != null)
            {
                ApplyText(rule, t);
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

        private static void ApplyText(Rule rule, UnityEngine.UI.Text t)
        {
            if (rule.FitMin > 0 && !t.resizeTextForBestFit)
            {
                t.resizeTextMaxSize = t.fontSize;
                t.resizeTextMinSize = Math.Min(rule.FitMin, t.fontSize);
                t.resizeTextForBestFit = true;
            }
            if (rule.NoWrap && t.horizontalOverflow != HorizontalWrapMode.Overflow)
            {
                t.horizontalOverflow = HorizontalWrapMode.Overflow;
            }
            if (rule.NoClip && t.verticalOverflow != VerticalWrapMode.Overflow)
            {
                t.verticalOverflow = VerticalWrapMode.Overflow;
            }
            if (rule.Size > 0 && t.fontSize != rule.Size)
            {
                t.fontSize = rule.Size;
            }
            if (rule.MaxWidth > 0f)
            {
                float width = t.preferredWidth;
                if (width > rule.MaxWidth)
                {
                    t.fontSize = Math.Max(1, (int)(t.fontSize * rule.MaxWidth / width));
                }
            }
            if (rule.WrapWidth > 0f && t.preferredWidth > rule.WrapWidth)
            {
                var fitter = t.GetComponent<UnityEngine.UI.ContentSizeFitter>();
                if (fitter != null && fitter.enabled)
                {
                    fitter.enabled = false;
                }
                t.horizontalOverflow = HorizontalWrapMode.Wrap;
                if (t.fontSize != rule.WrapSize)
                {
                    t.fontSize = rule.WrapSize;
                }
                var rt = t.rectTransform;
                if (rt.sizeDelta.x != rule.WrapWidth)
                {
                    rt.sizeDelta = new Vector2(rule.WrapWidth, rt.sizeDelta.y);
                }
            }
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
