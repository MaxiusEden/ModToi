# Relatório do teste no jogo (2026-10-26)

Nada foi conferido na tela (sem acesso ao jogo). Rodada 2 (abaixo, "Atualização") usou `traducao/nuvem/jogo/`.

## Corrigido
- **D**: `battleRule_daojie` = "Reino Dao {0}"; grau "G1…G6" no fim em 444 nomes (`item_name`, `effect_name/desc`, `achievementTitle`); `fateName*` (110) com `pt` = `ch`. Decisões em `decisoes.md`.
- **C**: `school_fenduoming` = "Filial {0}"; `schoolBranch`/`nameFix_*`/`schoolAllot_tiaopeidizi`/`modCondition*` (Ramo → Filial, só no sentido de seita).
- **A** (limites em `limites.json`): `game_baocunbizhi`, `mobilePaper_baocunbizhi`, `game_daochucundang`, `game_daorucundang`, `game_cundangwenti`, `shezhi_anjianfengge`, `game_tiaoshirizhi`, `shezhi_shoubingtishi`; `achievement_kejiesuo/yijiesuo` e "Unlock" → "Liberar"; `achievementDesc180014-180039` e `1801xx` (descrições curtas); `diffUnit*`, `diffPrice*` (valor "Extremo"), `diffDescribe1-3`; `common_waimao` = "Visual"; `common_nan/nv` = "Masc."/"Fem."; `artifactShape_*` (custo/s, recargas); `skill_ui_lengque*`/`skill_lengque`/`wuxue_jinenglengque` = "Recarga"; "Skill Info"/"Mission Info" = "Detalhes"; `D_role_grade_name1-30` abreviados ("N. Dourado"); `town_chuwukongjian` = "Cofre"; `npcInfo_zengsong` = "Dar"; `pub_text3` = "Pagar rodada"; `schoolGodBeast_talk` = "Usar"; `schoolAura_tip` encurtado; `refineElixir_text2` = "Receita"; 72 prescrições "Presc. de X"; `town_huafei` e "Time needed" (duração em "d").
- **B**: `liquorName1001/1002` (bebidas da taverna). (os 19 palpites de `Tela.json` foram conferidos na rodada 2.)

## Não encontrado (rodada 1; ver Atualização)
- Notícias do mês, criaturas avistadas, descrição do Diário de eventos, falas padrão de NPC e diálogos de NPC aleatório, falas dos Picos do Behemoth: vêm da `RoleLogLocal` (26.712 linhas), que não está no repositório e que o mod não troca.
- Nomes das habilidades na aba Habilidades: procurei `skill_name_*`, `skillNameS`, `SkillGroupName`, `ability_name`; todos traduzidos. A tela deve ler outro campo.
- Letreiros dos prédios (cidade e seita), Pavilhão do Espírito, controle "drag" do prazo de cultivo, cidades sem "town" no Diário: não achei chaves; podem ser imagens ou texto de prefab.
- Colunas/botões da aba de mods locais e Mod Store: as chaves `mod_*` já estão em PT; o que ficou em inglês é texto de prefab (só pelo `Tela.json`).
- "Informações da…", "Quantidade de Monstros:" sem valor, "Interagir" no botão pequeno: ajustei por chave provável; se persistir, a chave é outra.
- Receitas de pílula sem "Receita de Elixir" na oficina: não achei a regra que corta o nome (item_name `Receita de Elixir X` está intacto).

## Depende de código ou layout
- **Ordem dos nomes** ("Supremo Horizonte Seita"): o jogo concatena `schoolNameFirst` + `schoolNameSecond` + `school_name_last` (e `townName` + "City/Town") em código; sem formato no LocalText. Precisa de um patch no `.cs` (não editado): ou Harmony no montador de nomes, ou regra regex no `ScreenTextPtBr` que reescreva `^(<adjetivo> )(<substantivo>) Seita(…)?$` para "Seita d{o|a} <substantivo> <adjetivo>…", com tabela de gênero para o do/da (listas `schoolNameFirst/Second` 101+ e `townName` 201-212). O do/da não dá para ajustar só por dados.
- "Carisma e Reino muito próximos", fonte/tamanho do texto de pílulas (quebra de linha), larguras reais de botões: layout.
- `fateName` em chinês: se a fonte do jogo em inglês não tiver os caracteres, aparecem quadrados (então voltar o `pt`).
- `school_fenduoming`: assumi que `{0}` é só a região; se for o nome inteiro da seita, a ordem sai errada.

## Atualização (rodada 2, `traducao/nuvem/jogo/`)
- **Tela.json** (637 trocas): conferido contra `ptbr_tela_desconhecidas.txt`. Saíram os palpites não confirmados (Hard, Nightmare, Forge…, Martial…); ficaram Name, Description, Local Mods, Mod Store. Entraram letreiros, "drag", abas de habilidade, dificuldades dos saves, nomes de habilidade, cidades do Diário, 94 descrições de habilidade (DLC) e nomes de seita/cidade já compostos (ordem PT). Texto com número/variável embutido não casa (troca é por texto exato).
- **LocalText**: 183 chaves de `ptbr_missing_LocalText.txt` traduzidas (notas de versão, pesquisa e chaves curtas). Chaves só em chinês ou "0" ficaram de fora.
- **RoleLogLocal**: 26.712 ids em `out_RoleLogLocal01..34.json` (+ `out_RoleLogLocal_copia.json` com 518 ids sem texto traduzível, copiados do en). Verificador: 0 ERRO real (sobram nomes em pinyin e interjeições iguais ao inglês, e AVISOs falsos "Anciã"/"Focus" verbo/"Líderes"). Ordem: drama_monthLog, eventLog*, dialogRoleChat, depois o resto; lotes por ids repetidos, depois expandidos.
- Convenções: sem possessivo/artigo antes de `{relation|…}` e `{callIgnore|…}`; "Seita", "Líder da Seita", "Anciã Luo"; onde `en` e `ch` divergem, vale o `en`.
- `validate` 0 erro, `build` ok (55.059 traduções no LocalText.json).

## Ainda falta
- Aplicar RoleLogLocal no jogo: o mod não troca essa tabela; precisa do patch local que leia `out_RoleLogLocal*.json`.
- Textos dinâmicos da tela (com número/variável) e ordem dos nomes de seita/cidade: patch no `.cs`.
- Conferir no jogo: `school_fenduoming`, glifos de `fateName`, "Fada" para 仙子 e termos novos dos lotes (Serva Guardiã, Lastro do Rio, etc.).
