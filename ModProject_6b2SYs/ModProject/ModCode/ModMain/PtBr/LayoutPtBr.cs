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
        // Filtro do Pavilhão da Seita: ícone e nome vão para a esquerda; a caixa de marcar, presa ao fim do nome,
        // vai para a borda direita do item.
        private const float FilterLeft = 20f;
        private const float FilterMaxWidth = 100f;
        private const float FilterBoxRight = 32f;
        // Pavilhões dos Manuais: a placa do reino desce da altura da placa do tipo para a do "Requer ...".
        private const float RealmDown = 49f;
        private const float RealmRight = 5f;
        // Território da Seita: espaço entre "Afinidade:" e os ícones.
        private const float IconGap = 0f;
        // Promoção: o jogo dá ao botão do cargo a largura do texto + 50; com 100, fica nos 150 dos outros. Só quebra
        // o que passa de 115 ("Líder da Seita" cabe numa linha).
        private const float PostTextWidth = 100f;
        private const float PostWrapAbove = 115f;
        // Promoção: o custo do desafio sobe, longe do botão Promoção.
        private const float TipCostUp = 16f;
        // Botão que cresce na altura: fonte mínima para a palavra mais longa caber na largura.
        private const int TallMinSize = 11;
        // Aprimorar Habilidade: "Cultivo", o ícone e os dias vão para a direita, longe do botão Máx.
        private const float TrainDaysRight = 15f;
        private const int MaxPlaced = 5000;

        private class Rule
        {
            // Nome do objeto (null: qualquer um), começo do nome, nome do pai, começo do nome do pai, de um
            // ancestral e um nome a pular.
            public string Window, Name, Prefix, Parent, ParentPrefix, Under, Except;
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
            // > 0: quebra o texto em linhas e aumenta a altura do pai (o botão) até elas caberem com essa folga.
            public float GrowTall;
            // Com GrowTall: largura do texto quando não cabe numa linha e, > 0, a largura de linha a partir da qual
            // quebra (senão, a do texto).
            public float WrapWidth, WrapAbove;
            // Os ícones filhos do texto vão para depois da última letra, com IconGap de espaço.
            public bool IconsAfter;
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
                GrowTall = 26f, WrapWidth = PostTextWidth, WrapAbove = PostWrapAbove },
            // Promoção, à direita: "Qualquer Aptidão Marcial abaixo atinge 480" passava da borda, e o custo do
            // desafio, empurrado para baixo pela descrição longa, ficava embaixo do botão Promoção.
            new Rule { Window = "SchoolPost", Name = "G:textCertificationTitle", MaxWidth = 300f },
            new Rule { Window = "SchoolPost", Name = "G:textTipDesc", Size = 14 },
            new Rule { Window = "SchoolPost", Name = "G:textTipGroup", Size = 14,
                Offset = rt => new Vector2(0f, TipCostUp) },
            // Pavilhão da Seita: filtro de manuais ("Relâmpago" passava para baixo da caixa de marcar).
            new Rule { Window = "SchoolLibrary", Prefix = "G:textFilter", Except = "G:textFilterTitle",
                Under = "Group:MartialFilter", NoWrap = true, MaxWidth = FilterMaxWidth,
                Offset = rt => new Vector2(-FilterLeft, 0f) },
            new Rule { Window = "SchoolLibrary", Name = "Background", ParentPrefix = "G:textFilter",
                Under = "Group:MartialFilter", Offset = rt => new Vector2(FilterBoxRight, 0f) },
            // Pavilhões dos Manuais da Seita e da vila: placa do reino ("SoulFormation") que a placa do tipo cobria.
            new Rule { Window = "SchoolLibrary", Name = "G:imgGrade",
                Offset = rt => new Vector2(RealmRight, -RealmDown) },
            new Rule { Window = "TownMarketBook", Name = "G:imgGrade",
                Offset = rt => new Vector2(RealmRight, -RealmDown) },
            // Território da Seita: ícones da afinidade em cima de "Afinidade:".
            new Rule { Window = "SchoolMap", Name = "G:textSchoolHaogan", IconsAfter = true },
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
            new Rule { Window = "SchoolAdmin", Prefix = "G:text", Except = "G:textAdmin", Under = "ViewState",
                FitMin = 11 },
            new Rule { Window = "SchoolAdmin", Name = "G:textAdmin", Under = "ViewState", GrowTall = 18f },
        };

        private static readonly Dictionary<IntPtr, Placed> placed = new Dictionary<IntPtr, Placed>();
        private static readonly Dictionary<IntPtr, float> buttonBase = new Dictionary<IntPtr, float>();
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
                    buttonBase.Clear();
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
                && (rule.ParentPrefix == null || parent.StartsWith(rule.ParentPrefix, StringComparison.Ordinal))
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
            if (rule.GrowTall > 0f)
            {
                GrowTall(rule, t);
            }
            if (rule.IconsAfter)
            {
                IconsAfterText(t, IconGap);
            }
        }

        /// <summary>Botão com texto longo: o texto quebra em linhas e o botão cresce na altura, não na largura.</summary>
        private static void GrowTall(Rule rule, UnityEngine.UI.Text t)
        {
            var button = t.transform.parent == null ? null : t.transform.parent.TryCast<RectTransform>();
            var rt = t.rectTransform;
            float width = rule.WrapWidth > 0f ? rule.WrapWidth : rt.rect.width;
            if (button == null || t.preferredWidth <= (rule.WrapAbove > 0f ? rule.WrapAbove : width))
            {
                return;
            }
            float baseHeight;
            if (!buttonBase.TryGetValue(button.Pointer, out baseHeight))
            {
                baseHeight = button.sizeDelta.y;
                buttonBase[button.Pointer] = baseHeight;
            }
            if (rule.WrapWidth > 0f)
            {
                // O ContentSizeFitter deixaria o texto com a largura da linha inteira.
                var fitter = t.GetComponent<UnityEngine.UI.ContentSizeFitter>();
                if (fitter != null && fitter.enabled)
                {
                    fitter.enabled = false;
                }
                if (rt.sizeDelta.x != rule.WrapWidth)
                {
                    rt.sizeDelta = new Vector2(rule.WrapWidth, rt.sizeDelta.y);
                }
            }
            if (t.horizontalOverflow != HorizontalWrapMode.Wrap)
            {
                t.horizontalOverflow = HorizontalWrapMode.Wrap;
            }
            if (t.verticalOverflow != VerticalWrapMode.Overflow)
            {
                t.verticalOverflow = VerticalWrapMode.Overflow;
            }
            while (t.fontSize > TallMinSize && WidestWord(t) > width - 4f)
            {
                t.fontSize--;
            }
            Vector2 old = button.sizeDelta;
            var target = new Vector2(old.x, Math.Max(baseHeight, t.preferredHeight + rule.GrowTall));
            bool taller = Math.Abs(target.y - old.y) >= 0.5f;
            // Destaque e seleção (com a largura do botão, que o jogo acerta) ficam sempre com a altura dele; o que
            // fica acima do meio ("Pode ser promovido") sobe com a borda de cima.
            for (int i = 0; i < button.childCount; i++)
            {
                var child = button.GetChild(i).TryCast<RectTransform>();
                if (child == null || child.Pointer == rt.Pointer)
                {
                    continue;
                }
                if (Math.Abs(child.sizeDelta.x - old.x) < 0.5f)
                {
                    if (child.sizeDelta != target)
                    {
                        child.sizeDelta = target;
                    }
                }
                else if (taller && child.anchoredPosition.y > 0f)
                {
                    child.anchoredPosition += new Vector2(0f, (target.y - old.y) / 2f);
                }
            }
            if (taller)
            {
                button.sizeDelta = target;
            }
        }

        private static float WidestWord(UnityEngine.UI.Text t)
        {
            var settings = t.GetGenerationSettings(Vector2.zero);
            var generator = t.cachedTextGeneratorForLayout;
            float widest = 0f;
            foreach (string word in t.text.Split(' ', '\n'))
            {
                widest = Math.Max(widest, generator.GetPreferredWidth(word, settings) / t.pixelsPerUnit);
            }
            return widest;
        }

        /// <summary>Ícones filhos do texto: vão para a direita até ficarem depois da última letra.</summary>
        private static void IconsAfterText(UnityEngine.UI.Text t, float gap)
        {
            var rt = t.rectTransform;
            Rect rect = rt.rect;
            float width = t.preferredWidth;
            int align = (int)t.alignment % 3;
            float start = align == 0 ? rect.xMin : align == 1 ? rect.center.x - width / 2f : rect.xMax - width;
            float left = float.MaxValue;
            for (int i = 0; i < rt.childCount; i++)
            {
                left = Math.Min(left, LeftEdge(rt, rt.GetChild(i)));
            }
            float shift = start + width + gap - left;
            if (left == float.MaxValue || shift < 0.5f)
            {
                return;
            }
            for (int i = 0; i < rt.childCount; i++)
            {
                rt.GetChild(i).localPosition += new Vector3(shift, 0f, 0f);
            }
        }

        /// <summary>Borda esquerda do objeto ativo e dos filhos dele, nas coordenadas de root.</summary>
        private static float LeftEdge(RectTransform root, Transform tr)
        {
            float left = float.MaxValue;
            if (!tr.gameObject.activeSelf)
            {
                return left;
            }
            var rt = tr.TryCast<RectTransform>();
            if (rt != null && rt.rect.width > 0f)
            {
                left = root.InverseTransformPoint(rt.TransformPoint(new Vector3(rt.rect.xMin, 0f, 0f))).x;
            }
            for (int i = 0; i < tr.childCount; i++)
            {
                left = Math.Min(left, LeftEdge(root, tr.GetChild(i)));
            }
            return left;
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
