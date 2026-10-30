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
        // Pisos 4 e 5 do Acervo: a placa encostava numa estante do fundo.
        private static readonly HashSet<string> ShelfFloors = new HashSet<string> { "4", "5" };
        private const float ShelfRight = 5f;
        // Acervo: "Característica da Seita: ..." passava da borda direita; o grupo dela vai para o lado esquerdo da
        // barra de contribuição, no meio do espaço livre (o jogo reposiciona o texto dentro do grupo, então move-se o
        // grupo).
        private const float DiscountLeft = 1286f;
        // Salão do Conselho: botões Território/Táticas mais largos (140 → 170), um pouco para fora para não se
        // encostarem, e a largura máxima do texto dentro deles.
        private const float InterfactWidth = 170f;
        private const float TerritoryWidth = 180f;
        private const float PropOptionMaxWidth = 84f;
        private const float MartialTypeDown = 70f;
        private const float AuctionNameDown = 3f;
        private const float AuctionRowsDown = 12f;
        // Ficha: "Carisma:" tem 27 a mais de largura que "Sexo:", e os dois são alinhados à direita.
        private const float CharmRight = 27f;
        // Largura dos rótulos de referência das fichas (alinhados à direita).
        private const float ReputationWidth = 89.3f;
        private const float InterestsWidth = 88f;
        // Pavilhão dos Manuais: o balão tem uma ponta embaixo, e só a parte de cima serve para o texto.
        private const float BookBalloonBase = 86f;
        private const float BookBalloonHeight = 108f;
        private const int FactoryListPad = 12;
        // Mapa-múndi: o ícone maior do filtro vai até 11 depois do começo do texto.
        private const float MapFilterRight = 11f;
        // Quadro do guardião: rótulo + valor até o fim da linha separadora (225 de largura).
        private const float BeastRowWidth = 215f;
        private const float LoginButtonMaxWidth = 160f;
        private const float CacheButtonPad = 20f;
        private const float TabTextMaxWidth = 62f;
        private const float BookDialogueDown = 6f;
        private const float BuyTextMaxWidth = 56f;
        private const float NpcNameMaxWidth = 195f;
        // Território da Seita: espaço entre o ":" do rótulo e o valor que vem depois.
        private const float AfterLabelGap = 6f;
        private const float InterfactOut = 12f;
        private const float InterfactMaxWidth = 128f;
        // Abas verticais: a palavra mais longa (Minério, 7 letras) cabe em 90 de altura (a aba tem 116 com as pontas);
        // as outras usam o mesmo espaço entre letras.
        private const float VerticalMaxHeight = 90f;
        private const int VerticalMaxLetters = 7;
        // Herbologia: nomes das prescrições, que ficam a 135 uma da outra (o texto tinha 150 e invadia o vizinho).
        private const float HerbNameWidth = 128f;
        // Salão de Missões: "Benefício da Seita:" alinha com a descrição e as recompensas.
        private const float RewardTitleRight = 34f;
        // Picos dos Behemoths: os botões sobem e o rótulo PT fica embaixo de cada um.
        private const float BeastUp = 40f;
        private const string BeastLabel = "PtBrRotulo";
        private const int BeastLabelSize = 20;
        private const float BeastLabelWidth = 200f;
        // Caixa com rolagem criada em volta do texto.
        private const string ScrollName = "PtBrRolagem";
        private const float ScrollSensitivity = 20f;
        // Território da Seita: espaço entre o ":" de "Afinidade:" e os ícones.
        private const float IconGap = 0f;
        // Promoção: o jogo dá ao botão do cargo a largura do texto + 50; com 100, fica nos 150 dos outros. Só quebra
        // o que passa de 115 ("Líder da Seita" cabe numa linha).
        private const float PostTextWidth = 100f;
        private const float PostWrapAbove = 115f;
        // Promoção: o custo do desafio sobe, longe do botão Promoção.
        private const float TipCostUp = 16f;
        // Promoção: a coluna de requisitos e método vai para a esquerda (o ")" de Relâmpago tocava a borda).
        private const float NeedLeft = 10f;
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
            // > 0: largura e altura fixas do objeto.
            public float Width, Height;
            // > 0: como MaxWidth, mas descontando a largura do pai (o rótulo antes do valor).
            public float MaxRowWidth;
            // > 0: folga no topo do LayoutGroup do objeto.
            public int PadTop;
            // > 0: o pai (o botão) fica com a largura do texto mais esta folga.
            public float ParentPad;
            // > 0: quebra o texto em linhas e aumenta a altura do pai (o botão) até elas caberem com essa folga.
            public float GrowTall;
            // Com GrowTall: largura do texto quando não cabe numa linha e, > 0, a largura de linha a partir da qual
            // quebra (senão, a do texto).
            public float WrapWidth, WrapAbove;
            // Os ícones filhos do texto vão para depois da última letra, com IconGap de espaço.
            public bool IconsAfter;
            // Uma letra por linha (abas estreitas).
            public bool Vertical;
            // Esconde o objeto.
            public bool Hide;
            // Texto que o jogo mostra fora da tradução (hanzi fixo no prefab).
            public string Text;
            // Texto que não cabe na caixa: fonte normal e caixa com rolagem (arrastar ou roda do mouse).
            public bool Scroll;
            // Botão redondo: o hanzi original fica dentro e o PT vai para um rótulo preto embaixo.
            public string Hanzi;
            // Grupo de valores que começa logo depois do fim deste texto (irmão do pai do grupo).
            public string AfterLabel;
            // "99540Pedra Espiritual" vira "99540 Pedras Espirituais".
            public bool CountPlural;
            // Passa o texto pela troca da tela nesta varredura (texto fixo do prefab, que o jogo não escreve de novo).
            public bool Translate;
            // Só vale para o objeto com este texto; e o Offset move também este irmão.
            public string TextIs, Sibling;
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
            // Menu de opções do item: "Uso em Massa" numa linha só (o botão tem 88 de largura para o texto).
            new Rule { Window = "PropInfo", Name = "Text", Parent = "G:goOptionItem(Clone)", NoWrap = true,
                MaxWidth = PropOptionMaxWidth },
            // Detalhes da técnica: o ícone do tipo batia no nome; desce para a linha da recarga ou do custo em pontos.
            new Rule { Window = "MartialInfo", Name = "G:goMartialType",
                Offset = rt => new Vector2(0f, -MartialTypeDown) },
            // Casa de Leilões: nome longo em duas linhas (a segunda sumia), com preço e reino mais abaixo.
            new Rule { Window = "TownAuction", Name = "Name", Parent = "G:goPropItem(Clone)", NoClip = true,
                Offset = rt => new Vector2(0f, -AuctionNameDown) },
            new Rule { Window = "TownAuction", Name = "Price", Parent = "G:goPropItem(Clone)",
                Offset = rt => new Vector2(0f, -AuctionRowsDown) },
            new Rule { Window = "TownAuction", Name = "Grade", Parent = "G:goPropItem(Clone)",
                Offset = rt => new Vector2(0f, -AuctionRowsDown) },
            new Rule { Window = "TownAuction", Name = "G:textMyProp", NoWrap = true },
            new Rule { Window = "TownAuction", Name = "G:textTipGrade1_En", FitMin = 12 },
            // Tela inicial: botões de 175 de largura com o texto em uma linha, sem tocar a borda.
            new Rule { Window = "Login", Prefix = "G:text", Under = "btnGroup", NoWrap = true,
                MaxWidth = LoginButtonMaxWidth },
            // Problemas de save: o botão acompanha o texto, com a folga do botão do log (texto 230 em botão 250).
            new Rule { Window = "CacheHelper", Prefix = "G:textOpen", ParentPad = CacheButtonPad },
            new Rule { Window = "CacheHelper", Name = "G:textTitle", Parent = "Image", NoWrap = true },
            new Rule { Window = "CacheHelper", Name = "G:textCacheTip", FitMin = 12 },
            new Rule { Window = "CacheHelper", Name = "G:textCacheBackTip", FitMin = 12 },
            // Atalhos: nomes das teclas em duas linhas ("Botão Esquerdo do Mouse").
            new Rule { Window = "GameSetting", Name = "TextKey", FitMin = 10 },
            // Criação de personagem: abas do menu e o aviso dos Destinos.
            new Rule { Window = "CreatePlayer", Prefix = "G:text", Parent = "Checkmark", Under = "Grid_En", NoWrap = true,
                MaxWidth = TabTextMaxWidth },
            new Rule { Window = "CreatePlayer", Name = "G:textBornLuck2_Key_En", FitMin = 11 },
            // Pavilhões dos Manuais: fala da atendente, título do requisito e nome dos manuais.
            new Rule { Window = "TownMarketBook", Name = "G:textDialogue", NoClip = true,
                Offset = rt => new Vector2(0f, -BookDialogueDown) },
            new Rule { Window = "TownMarketBook", Name = "G:textTitle", Parent = "Image", NoWrap = true, MaxWidth = 196f },
            new Rule { Window = "TownMarketBook", Name = "G:textTokenTitle_En", NoWrap = true, MaxWidth = 155f },
            new Rule { Window = "TownMarketBook", Name = "TextName", FitMin = 10 },
            new Rule { Window = "SchoolLibrary", Name = "TextName", FitMin = 10 },
            // Forja e alquimia da vila: nome do diagrama e botão Comprar.
            new Rule { Window = "TownFactoryMadeShape", Name = "TextName", FitMin = 10 },
            new Rule { Window = "TownFactoryMadeShape", Name = "TextBuy", NoWrap = true, MaxWidth = BuyTextMaxWidth },
            new Rule { Window = "TownFactoryRefineElixir", Name = "TextBuy", NoWrap = true, MaxWidth = BuyTextMaxWidth },
            // Detalhes da técnica: títulos que perdiam a segunda linha.
            new Rule { Window = "MartialInfo", Name = "G:textPrefixTitle", NoWrap = true },
            new Rule { Window = "MartialInfo", Name = "G:textTrainSli", NoWrap = true },
            // Ficha do jogador e do NPC: abas, títulos e colunas de uma linha só.
            new Rule { Window = "NPCInfo", Name = "G:textTitle1", NoWrap = true, MaxWidth = 96f },
            new Rule { Window = "NPCInfo", Name = "G:textTitle2", NoWrap = true, MaxWidth = 96f },
            new Rule { Window = "NPCInfo", Name = "Name", Parent = "G:goItemMy", NoWrap = true, MaxWidth = 98f },
            new Rule { Window = "PlayerInfo", Name = "G:textLogTitle1_En", NoWrap = true, MaxWidth = 96f },
            new Rule { Window = "PlayerInfo", Name = "G:textLogTitle2_En", NoWrap = true, MaxWidth = 96f },
            new Rule { Window = "PlayerInfo", Name = "G:textEquipTitle", NoWrap = true, MaxWidth = 96f },
            new Rule { Window = "PlayerInfo", Name = "G:textArt", Parent = "Checkmark", NoWrap = true,
                MaxWidth = TabTextMaxWidth },
            new Rule { Window = "PlayerInfo", Name = "G:textPropTip", FitMin = 12 },
            new Rule { Window = "PlayerInfo", Name = "Text2", Parent = "Item", NoWrap = true, MaxWidth = 148f },
            // Seita: nome no topo, aura, hospital, Behemoths, membros, conselho, território e missões.
            new Rule { Window = "School", Name = "G:textName_En", Parent = "Image_En", NoWrap = true, MaxWidth = 470f },
            new Rule { Window = "SchoolAura", Name = "G:textTip", FitMin = 11 },
            new Rule { Window = "SchoolHospital", Name = "G:textOK_En", NoWrap = true, MaxWidth = 110f },
            new Rule { Window = "SchoolGodBeast", Name = "texItem", Parent = "G:textMessageItem(Clone)", NoWrap = true,
                MaxRowWidth = BeastRowWidth },
            new Rule { Window = "SchoolAdmin", Name = "G:textTitle", Under = "BG", NoWrap = true },
            new Rule { Window = "SchoolAdmin", Name = "Text", Parent = "Button", FitMin = 12 },
            new Rule { Window = "SchoolMainLobby", Name = "Text", Parent = "Toggle", FitMin = 16 },
            new Rule { Window = "SchoolMainLobby", Name = "Selected", Parent = "Toggle", FitMin = 16 },
            new Rule { Window = "SchoolMap", Name = "G:textAreaPoint1", NoWrap = true, MaxWidth = 220f },
            new Rule { Window = "SchoolMap", Name = "G:textAreaPoint2", NoWrap = true, MaxWidth = 220f },
            new Rule { Window = "SchoolMap", Name = "G:textInherit", NoWrap = true, MaxWidth = 148f },
            new Rule { Window = "SchoolMap", Name = "G:textSchoolCelue", NoWrap = true, MaxWidth = 236f },
            new Rule { Window = "SchoolMap", Name = "G:textSchoolName", Under = "G:goSchoolPanel", NoWrap = true,
                MaxWidth = 188f },
            new Rule { Window = "SchoolMap", Name = "textName", Parent = "imgName", NoWrap = true, MaxWidth = 70f },
            new Rule { Window = "SchoolTaskLobby", Name = "G:textHumanInfo", FitMin = 13 },
            new Rule { Window = "TaoistHeart", Name = "G:textAddSeed", NoWrap = true, MaxWidth = 250f },
            // Coração do Dao: o "?" ficava em cima do "(Terceiro Estágio)"; vai para depois do título.
            new Rule { Window = "TaoistHeart", Name = "G:goHeartTips", AfterLabel = "G:textHeartName" },
            // Textos fixos em chinês: trocados a cada varredura desta classe, sem esperar a varredura geral da tela.
            new Rule { Window = "CacheHelper", Name = "G:textTitle", Translate = true },
            new Rule { Window = "ArtifactInfo", Name = "G:textSell", Translate = true },
            new Rule { Window = "GameSetting", Name = "G:textCameraShake", Under = "GuiFixDataItem", Translate = true },
            new Rule { Window = "GameSetting", Name = "G:textSystemOK", Under = "GuiFixDataItem", Translate = true },
            new Rule { Window = "TextInfoLong", Name = "G:textOK", Translate = true },
            // Configurações: textos de duas linhas ("Exportar Log de Depuração").
            new Rule { Window = "GameSetting", Name = "G:textLog", FitMin = 11 },
            new Rule { Window = "GameSetting", Name = "G:textDynPortraitPlayAnim", FitMin = 11 },
            new Rule { Window = "GameSetting", Name = "G:textEffectTitle", FitMin = 11 },
            new Rule { Window = "GameSetting", Name = "G:textTextureTitle", FitMin = 11 },
            // Ficha do jogador e do NPC: "Carisma:" encostava no reino; a linha começa na coluna de "Sexo:".
            new Rule { Window = "PlayerInfo", Name = "Text1", Under = "G:goInfoRoot_En", TextIs = "Carisma:", Sibling = "Text2",
                Offset = rt => new Vector2(CharmRight, 0f) },
            // Ficha do jogador: a coluna da esquerda começa na mesma posição de "Reputação:" e "Interesses:".
            new Rule { Window = "PlayerInfo", Name = "Text1", Under = "G:goInfoRoot_En", TextIs = "Sobrenome:", Sibling = "Text2",
                Offset = LabelColumn(ReputationWidth) },
            new Rule { Window = "PlayerInfo", Name = "Text1", Under = "G:goInfoRoot_En", TextIs = "Nome:", Sibling = "Text2",
                Offset = LabelColumn(ReputationWidth) },
            new Rule { Window = "PlayerInfo", Name = "Text1", Under = "G:goInfoRoot_En", TextIs = "Posição:", Sibling = "Text2",
                Offset = LabelColumn(ReputationWidth) },
            new Rule { Window = "PlayerInfo", Name = "Text1", Under = "G:goInfoRoot_En", TextIs = "Reino:", Sibling = "Text2",
                Offset = LabelColumn(ReputationWidth) },
            // Ficha do NPC: Nome, Raça, Sexo e Carisma na coluna de "Interesses:"; Seita, Posição e Reino na de "Reputação:".
            new Rule { Window = "NPCInfo", Name = "Text1", Under = "G:goInfoRoot", TextIs = "Nome:", Sibling = "Text2",
                Offset = LabelColumn(InterestsWidth) },
            new Rule { Window = "NPCInfo", Name = "Text1", Under = "G:goInfoRoot", TextIs = "Raça:", Sibling = "Text2",
                Offset = LabelColumn(InterestsWidth) },
            new Rule { Window = "NPCInfo", Name = "Text1", Under = "G:goInfoRoot", TextIs = "Sexo:", Sibling = "Text2",
                Offset = LabelColumn(InterestsWidth) },
            new Rule { Window = "NPCInfo", Name = "Text1", Under = "G:goInfoRoot", TextIs = "Carisma:", Sibling = "Text2",
                Offset = LabelColumn(InterestsWidth) },
            new Rule { Window = "NPCInfo", Name = "Text1", Under = "G:goInfoRoot", TextIs = "Seita:", Sibling = "Text2",
                Offset = LabelColumn(ReputationWidth) },
            new Rule { Window = "NPCInfo", Name = "Text1", Under = "G:goInfoRoot", TextIs = "Posição:", Sibling = "Text2",
                Offset = LabelColumn(ReputationWidth) },
            new Rule { Window = "NPCInfo", Name = "Text1", Under = "G:goInfoRoot", TextIs = "Reino:", Sibling = "Text2",
                Offset = LabelColumn(ReputationWidth) },
            // Pavilhão dos Manuais: o balão cresce para baixo até caber a fala da atendente.
            new Rule { Window = "TownMarketBook", Name = "Image (1)", Parent = "G:goDialogue", Height = BookBalloonHeight,
                Offset = rt => new Vector2(0f, -(BookBalloonHeight - BookBalloonBase) / 2f) },
            // Forja: a primeira linha descia cortada pela borda da lista.
            new Rule { Window = "TownFactoryMadeShape", Name = "G:goItemRoot_En", PadTop = FactoryListPad },
            // Conquistas e dificuldade.
            new Rule { Window = "Achievement", Name = "G:textGetNum", NoWrap = true, MaxWidth = 195f },
            new Rule { Window = "Difficulty", Name = "G:textDieDesc_En", Size = 16 },
            // Outras telas com a segunda linha cortada.
            // Mapa-múndi: o texto do filtro começa depois do ícone e termina dentro do botão.
            new Rule { Window = "MinMap", Prefix = "G:textTgl", Under = "G:goFilterItemRoot", NoWrap = true, MaxWidth = 108f,
                Offset = rt => new Vector2(MapFilterRight, 0f) },
            new Rule { Window = "MinMap", Prefix = "G:textTgl", Under = "G:goUnitFilter_En", NoWrap = true, MaxWidth = 52f },
            new Rule { Window = "PlayerInfo", Name = "G:textMartialTrain", NoWrap = true, MaxWidth = 146f },
            new Rule { Window = "PlayerInfo", Name = "Text", Parent = "Open", Under = "Group:Art", FitMin = 11 },
            new Rule { Window = "ElderSkillInfo", Name = "G:textDescTitle", NoWrap = true },
            new Rule { Window = "ElderSkillInfo", Name = "G:textPrefixTitle", NoWrap = true },
            new Rule { Window = "ElderSkillInfo", Name = "G:textTrainSli", NoWrap = true },
            new Rule { Window = "ArtifactBookInfo", Name = "G:textPropCommon", NoWrap = true, MaxWidth = 196f },
            new Rule { Window = "ArtifactBookInfo", Name = "G:textPropRare", NoWrap = true, MaxWidth = 196f },
            new Rule { Window = "RefineRuneFormula", Name = "Text", Parent = "G:goOptionItem_En(Clone)", NoWrap = true,
                MaxWidth = 150f },
            new Rule { Window = "RefinePillFormula", Name = "G:textElementTitle", NoWrap = true },
            // Arena: "Locutor do Torneio Imortal" perdia a segunda linha.
            new Rule { Window = "TownFight", Name = "G:textNPC", NoWrap = true, MaxWidth = NpcNameMaxWidth },
            // Artesanato, Herbologia: nome da prescrição.
            new Rule { Window = "PlayerInfo", Name = "Text", Parent = "Open", Under = "G:goArtItemRoot3", FitMin = 11,
                Width = HerbNameWidth },
            // Alquimia, receita escolhida: nome longo em duas linhas e dificuldade com o atributo numa linha só.
            new Rule { Window = "RefinePillFormula", Name = "G:textPillName_En", FitMin = 14 },
            new Rule { Window = "RefinePillFormula", Name = "G:textLevel", NoWrap = true },
            // Talismãs: material de cada nível.
            new Rule { Window = "RefineRuneFormula", Name = "Text", Parent = "G:goMaterialItem(Clone)", FitMin = 11 },
            // Seita: regras, propósito e características.
            new Rule { Window = "School", Name = "G:textRule_En", Scroll = true },
            new Rule { Window = "School", Name = "G:textTenet_En", Scroll = true },
            new Rule { Window = "School", Name = "G:textIdiosyncrasy_En", Scroll = true },
            // Salão do Conselho: botões Território da Seita e Táticas da Seita numa linha.
            new Rule { Window = "SchoolMainLobby", Name = "Button", Parent = "Build", Under = "G:btnInterfact0",
                Width = InterfactWidth, Offset = rt => new Vector2(-InterfactOut, 0f) },
            new Rule { Window = "SchoolMainLobby", Name = "Button", Parent = "Build", Under = "G:btnInterfact1",
                Width = TerritoryWidth, Offset = rt => new Vector2(InterfactOut, 0f) },
            // O mesmo para o botão que o jogo mostra no lugar dele ao passar o mouse ou selecionar.
            new Rule { Window = "SchoolMainLobby", Name = "HightLightButton", Parent = "HightLight",
                Under = "G:btnInterfact0", Width = InterfactWidth, Offset = rt => new Vector2(-InterfactOut, 0f) },
            new Rule { Window = "SchoolMainLobby", Name = "HightLightButton", Parent = "HightLight",
                Under = "G:btnInterfact1", Width = TerritoryWidth, Offset = rt => new Vector2(InterfactOut, 0f) },
            new Rule { Window = "SchoolMainLobby", Name = "HightLightText", Parent = "HightLightButton",
                Under = "G:goInterfactRoot", NoWrap = true, MaxWidth = InterfactMaxWidth },
            new Rule { Window = "SchoolMainLobby", Name = "Text", Parent = "Button", Under = "G:goInterfactRoot",
                NoWrap = true, MaxWidth = InterfactMaxWidth },
            // Pavilhão do Tesouro: abas Todos, Minério, Erva (caixa de 35 de largura).
            new Rule { Window = "SchoolStore", Prefix = "Text", Under = "TagGroup", Vertical = true },
            // Acervo de Manuais: só a contribuição que sobra (a total não serve para nada), e a característica da
            // seita começa na borda esquerda.
            new Rule { Window = "SchoolLibrary", Name = "G:textTotalMoney", Hide = true },
            new Rule { Window = "SchoolLibrary", Name = "G:goDiscount", Parent = "dingcheng",
                Offset = rt => new Vector2(-DiscountLeft, 0f) },
            // Pavilhão do Espírito, transformar: "化灵" fixo no prefab.
            new Rule { Window = "SchoolGetSoul", Name = "Text", Parent = "G:btnOK", Text = "Transformar" },
            new Rule { Window = "SchoolGetSoul", Name = "G:textTitle", Parent = "Image", Text = "Transformação" },
            // Picos dos Behemoths: os três botões sobem; dentro, o hanzi; embaixo, o PT.
            new Rule { Window = "SchoolGodBeast", Name = "G:goBtnGroup", Offset = rt => new Vector2(0f, BeastUp) },
            new Rule { Window = "SchoolGodBeast", Name = "G:textGet", Hanzi = "化灵" },
            new Rule { Window = "SchoolGodBeast", Name = "G:textFeed", Hanzi = "喂养" },
            new Rule { Window = "SchoolGodBeast", Name = "G:textExchange", Hanzi = "交流" },
            // Promoção: Lista de Honra e o nome do cargo ("Discípulo Verdadeiro" cobria o ícone da posição atual).
            new Rule { Window = "SchoolPost", Name = "G:textHonor", FitMin = 12 },
            new Rule { Window = "SchoolPost", Name = "Text", Parent = "Toggle", Under = "G:goPostItem_En(Clone)",
                GrowTall = 26f, WrapWidth = PostTextWidth, WrapAbove = PostWrapAbove },
            // Promoção, à direita: "Qualquer Aptidão Marcial abaixo atinge 480" passava da borda, e o custo do
            // desafio, empurrado para baixo pela descrição longa, ficava embaixo do botão Promoção.
            new Rule { Window = "SchoolPost", Name = "G:goNeedGroup", Offset = rt => new Vector2(-NeedLeft, 0f) },
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
                Offset = rt => new Vector2(RealmRight + (ShelfFloors.Contains(rt.parent.name) ? ShelfRight : 0f),
                    -RealmDown) },
            new Rule { Window = "TownMarketBook", Name = "G:imgGrade",
                Offset = rt => new Vector2(RealmRight, -RealmDown) },
            // Território da Seita: ícones da afinidade em cima de "Afinidade:".
            new Rule { Window = "SchoolMap", Name = "G:textSchoolHaogan", IconsAfter = true },
            // Território da Seita, ponto de recurso: o que vem depois de "Produção Mensal:" e "Produção Especial:"
            // começava em cima do rótulo; vai para logo depois do ":", e o número ganha espaço e plural.
            new Rule { Window = "SchoolMap", Name = "G:goPointPointOutput_En", AfterLabel = "G:textPointOutput" },
            new Rule { Window = "SchoolMap", Name = "G:goPointReward_En", AfterLabel = "G:textPointReward" },
            new Rule { Window = "SchoolMap", Name = "G:goTextItem(Clone)", Under = "G:goPointPanel", CountPlural = true },
            // Coração do Dao de quem não tem semente: as pontas do texto ficavam cortadas.
            new Rule { Window = "NPCInfo", Name = "G:textWu", Size = 26 },
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
            new Rule { Window = "SchoolTaskLobby", Name = "Text", Parent = "G:goRewardTextRoot", NoWrap = true },
            new Rule { Window = "SchoolTaskLobby", Name = "G:textRewardTextTitle",
                Offset = rt => new Vector2(RewardTitleRight, 0f) },
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
            if (rule.Hide)
            {
                if (tr.gameObject.activeSelf)
                {
                    tr.gameObject.SetActive(false);
                }
                return;
            }
            var t = tr.GetComponent<UnityEngine.UI.Text>();
            if (rule.TextIs != null && (t == null || t.text == null || t.text.Trim() != rule.TextIs))
            {
                return;
            }
            if (t != null)
            {
                ApplyText(rule, t);
            }
            if (rule.AfterLabel != null)
            {
                AfterLabel(tr, rule.AfterLabel);
            }
            if (rule.Width > 0f || rule.Height > 0f)
            {
                var box = tr.TryCast<RectTransform>();
                Vector2 size = box == null ? Vector2.zero
                    : new Vector2(rule.Width > 0f ? rule.Width : box.sizeDelta.x, rule.Height > 0f ? rule.Height : box.sizeDelta.y);
                if (box != null && box.sizeDelta != size)
                {
                    box.sizeDelta = size;
                }
            }
            if (rule.PadTop > 0)
            {
                var layout = tr.GetComponent<UnityEngine.UI.LayoutGroup>();
                if (layout != null && layout.padding.top != rule.PadTop)
                {
                    layout.padding.top = rule.PadTop;
                    UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild(tr.TryCast<RectTransform>());
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
            Vector2 offset = rule.Offset(rt);
            MoveBy(rt, offset);
            var sibling = rule.Sibling == null ? null : tr.parent.Find(rule.Sibling);
            var siblingRt = sibling == null ? null : sibling.TryCast<RectTransform>();
            if (siblingRt != null)
            {
                MoveBy(siblingRt, offset);
            }
        }

        private static void MoveBy(RectTransform rt, Vector2 offset)
        {
            Vector2 cur = rt.anchoredPosition;
            Placed p;
            if (!placed.TryGetValue(rt.Pointer, out p) || cur != p.Applied)
            {
                p = new Placed { Base = cur };
                placed[rt.Pointer] = p;
            }
            Vector2 target = p.Base + offset;
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
            float maxWidth = rule.MaxWidth;
            if (rule.MaxRowWidth > 0f)
            {
                maxWidth = rule.MaxRowWidth - t.rectTransform.parent.TryCast<RectTransform>().rect.width;
            }
            if (maxWidth > 0f)
            {
                float width = t.preferredWidth;
                if (width > maxWidth)
                {
                    t.fontSize = Math.Max(1, (int)(t.fontSize * maxWidth / width));
                }
            }
            if (rule.Translate)
            {
                ScreenTextPtBr.Refresh(t);
            }
            if (rule.ParentPad > 0f)
            {
                var button = t.rectTransform.parent.TryCast<RectTransform>();
                float padded = t.preferredWidth + rule.ParentPad;
                if (button != null && Math.Abs(button.sizeDelta.x - padded) > 0.5f)
                {
                    button.sizeDelta = new Vector2(padded, button.sizeDelta.y);
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
            if (rule.Text != null && t.text != rule.Text)
            {
                t.text = rule.Text;
            }
            if (rule.Vertical)
            {
                Vertical(t);
            }
            if (rule.Scroll)
            {
                Scroll(t);
            }
            if (rule.Hanzi != null)
            {
                HanziInside(t, rule.Hanzi);
            }
            if (rule.CountPlural)
            {
                string plural = PluralPtBr.Rewrite(t.text);
                if (plural != null && plural != t.text)
                {
                    t.text = plural;
                }
            }
        }

        /// <summary>Uma letra por linha; "Todos" vira "T\no\nd\no\ns".</summary>
        private static void Vertical(UnityEngine.UI.Text t)
        {
            string s = t.text;
            if (s.Length < 2 || s.IndexOf('\n') >= 0)
            {
                return;
            }
            var letters = new string[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                letters[i] = s[i].ToString();
            }
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.lineSpacing = 1f;
            t.text = string.Join("\n", letters);
            // As linhas se aproximam como se a palavra tivesse VerticalMaxLetters letras em VerticalMaxHeight.
            float line = t.preferredHeight / s.Length;
            t.lineSpacing = VerticalMaxHeight / (VerticalMaxLetters * line);
        }

        /// <summary>
        /// Põe o texto numa caixa com rolagem do tamanho que ele tinha: o texto fica com a fonte normal e a altura
        /// inteira, e a parte que não cabe aparece ao arrastar ou com a roda do mouse.
        /// </summary>
        private static void Scroll(UnityEngine.UI.Text t)
        {
            var rt = t.rectTransform;
            var parent = rt.parent;
            if (parent == null)
            {
                return;
            }
            RectTransform view;
            if (parent.name == ScrollName)
            {
                view = parent.TryCast<RectTransform>();
            }
            else
            {
                if (t.resizeTextForBestFit)
                {
                    t.resizeTextForBestFit = false;
                }
                var go = new GameObject(ScrollName);
                view = go.AddComponent<RectTransform>();
                view.SetParent(parent, false);
                view.SetSiblingIndex(rt.GetSiblingIndex());
                view.anchorMin = rt.anchorMin;
                view.anchorMax = rt.anchorMax;
                view.pivot = rt.pivot;
                view.anchoredPosition = rt.anchoredPosition;
                view.sizeDelta = rt.sizeDelta;
                go.AddComponent<UnityEngine.UI.RectMask2D>();
                // Imagem transparente: recebe o arrasto e a roda do mouse fora das letras.
                go.AddComponent<UnityEngine.UI.Image>().color = new Color(0f, 0f, 0f, 0f);
                var scroll = go.AddComponent<UnityEngine.UI.ScrollRect>();
                scroll.horizontal = false;
                scroll.vertical = true;
                scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
                scroll.scrollSensitivity = ScrollSensitivity;
                scroll.viewport = view;
                rt.SetParent(view, false);
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(0f, view.rect.height);
                t.verticalOverflow = VerticalWrapMode.Overflow;
                scroll.content = rt;
            }
            if (view == null)
            {
                return;
            }
            float height = Math.Max(view.rect.height, t.preferredHeight);
            if (Math.Abs(rt.sizeDelta.y - height) > 0.5f)
            {
                rt.sizeDelta = new Vector2(0f, height);
            }
        }

        /// <summary>
        /// Botão redondo com texto grande demais: o hanzi original volta para dentro e o PT vai para um rótulo
        /// preto, copiado do texto, logo abaixo do botão.
        /// </summary>
        private static void HanziInside(UnityEngine.UI.Text t, string hanzi)
        {
            var button = t.transform.parent == null ? null : t.transform.parent.TryCast<RectTransform>();
            if (button == null || t.text == hanzi)
            {
                return;
            }
            var found = button.Find(BeastLabel);
            UnityEngine.UI.Text label;
            if (found == null)
            {
                var go = UnityEngine.Object.Instantiate(t.gameObject, button);
                go.name = BeastLabel;
                // Contorno e sombra do texto do botão não combinam com o preto.
                foreach (var shadow in go.GetComponents<UnityEngine.UI.Shadow>())
                {
                    UnityEngine.Object.Destroy(shadow);
                }
                label = go.GetComponent<UnityEngine.UI.Text>();
                var lrt = label.rectTransform;
                lrt.anchorMin = new Vector2(0.5f, 0.5f);
                lrt.anchorMax = new Vector2(0.5f, 0.5f);
                lrt.pivot = new Vector2(0.5f, 1f);
                lrt.sizeDelta = new Vector2(BeastLabelWidth, BeastLabelSize * 2f);
                lrt.anchoredPosition = new Vector2(0f, -button.rect.height / 2f);
                label.color = Color.black;
                label.resizeTextForBestFit = false;
                label.fontSize = BeastLabelSize;
                label.alignment = TextAnchor.UpperCenter;
                label.horizontalOverflow = HorizontalWrapMode.Wrap;
                label.verticalOverflow = VerticalWrapMode.Overflow;
                label.raycastTarget = false;
            }
            else
            {
                label = found.GetComponent<UnityEngine.UI.Text>();
            }
            label.text = t.text;
            t.text = hanzi;
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
            float widest = 0f;
            foreach (string word in t.text.Split(' ', '\n'))
            {
                widest = Math.Max(widest, LineWidth(t, word));
            }
            return widest;
        }

        /// <summary>Largura de uma linha com a fonte do texto, sem quebrar.</summary>
        private static float LineWidth(UnityEngine.UI.Text t, string line)
        {
            var settings = t.GetGenerationSettings(Vector2.zero);
            return t.cachedTextGeneratorForLayout.GetPreferredWidth(line, settings) / t.pixelsPerUnit;
        }

        /// <summary>Ícones filhos do texto: vão para a direita até ficarem depois da última letra.</summary>
        private static void IconsAfterText(UnityEngine.UI.Text t, float gap)
        {
            var rt = t.rectTransform;
            float left = float.MaxValue;
            for (int i = 0; i < rt.childCount; i++)
            {
                left = Math.Min(left, LeftEdge(rt, rt.GetChild(i)));
            }
            float shift = TextEnd(t) + gap - left;
            if (left == float.MaxValue || shift < 0.5f)
            {
                return;
            }
            for (int i = 0; i < rt.childCount; i++)
            {
                rt.GetChild(i).localPosition += new Vector3(shift, 0f, 0f);
            }
        }

        /// <summary>
        /// Move o objeto para o que está nele começar logo depois do fim do texto, que é irmão dele ou do pai dele.
        /// </summary>
        private static void AfterLabel(Transform group, string labelName)
        {
            var label = group.parent == null ? null : group.parent.Find(labelName);
            if (label == null && group.parent != null && group.parent.parent != null)
            {
                label = group.parent.parent.Find(labelName);
            }
            var t = label == null ? null : label.GetComponent<UnityEngine.UI.Text>();
            if (t == null)
            {
                return;
            }
            float left = LeftEdge(t.rectTransform, group);
            float shift = TextEnd(t) + AfterLabelGap - left;
            if (left != float.MaxValue && Math.Abs(shift) >= 0.5f)
            {
                group.localPosition += new Vector3(shift, 0f, 0f);
            }
        }

        /// <summary>
        /// Fim da última letra nas coordenadas do texto, sem o espaço que o jogo põe depois de "Afinidade:".
        /// </summary>
        private static float TextEnd(UnityEngine.UI.Text t)
        {
            Rect rect = t.rectTransform.rect;
            float width = t.preferredWidth;
            int align = (int)t.alignment % 3;
            float start = align == 0 ? rect.xMin : align == 1 ? rect.center.x - width / 2f : rect.xMax - width;
            return start + LineWidth(t, t.text.TrimEnd());
        }

        /// <summary>Borda esquerda do objeto ativo e dos filhos dele, nas coordenadas de root.</summary>
        private static float LeftEdge(RectTransform root, Transform tr)
        {
            float left = float.MaxValue;
            if (!tr.gameObject.activeSelf)
            {
                return left;
            }
            // Só o que aparece: o objeto de cada ícone é mais largo que a imagem dele.
            var rt = tr.TryCast<RectTransform>();
            if (rt != null && rt.rect.width > 0f && tr.GetComponent<UnityEngine.UI.Graphic>() != null)
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
        /// <summary>
        /// Rótulo alinhado à direita passa a começar onde começa um rótulo desta largura; o valor anda junto.
        /// </summary>
        private static Func<RectTransform, Vector2> LabelColumn(float width)
        {
            return rt => new Vector2(rt.rect.width - width, 0f);
        }

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
