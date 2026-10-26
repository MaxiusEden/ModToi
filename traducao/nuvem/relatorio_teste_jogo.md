# Relatório do teste no jogo (2026-10-26)

Sem acesso ao jogo nem ao `ptbr_tela_desconhecidas.txt`: achei os textos pelo `pt`/`en` do LocalText. Nada foi conferido na tela.

## Corrigido
- **D**: `battleRule_daojie` = "Reino Dao {0}"; grau "G1…G6" no fim em 444 nomes (`item_name`, `effect_name/desc`, `achievementTitle`); `fateName*` (110) com `pt` = `ch`. Decisões em `decisoes.md`.
- **C**: `school_fenduoming` = "Filial {0}"; `schoolBranch`/`nameFix_*`/`schoolAllot_tiaopeidizi`/`modCondition*` (Ramo → Filial, só no sentido de seita).
- **A** (limites em `limites.json`): `game_baocunbizhi`, `mobilePaper_baocunbizhi`, `game_daochucundang`, `game_daorucundang`, `game_cundangwenti`, `shezhi_anjianfengge`, `game_tiaoshirizhi`, `shezhi_shoubingtishi`; `achievement_kejiesuo/yijiesuo` e "Unlock" → "Liberar"; `achievementDesc180014-180039` e `1801xx` (descrições curtas); `diffUnit*`, `diffPrice*` (valor "Extremo"), `diffDescribe1-3`; `common_waimao` = "Visual"; `common_nan/nv` = "Masc."/"Fem."; `artifactShape_*` (custo/s, recargas); `skill_ui_lengque*`/`skill_lengque`/`wuxue_jinenglengque` = "Recarga"; "Skill Info"/"Mission Info" = "Detalhes"; `D_role_grade_name1-30` abreviados ("N. Dourado"); `town_chuwukongjian` = "Cofre"; `npcInfo_zengsong` = "Dar"; `pub_text3` = "Pagar rodada"; `schoolGodBeast_talk` = "Usar"; `schoolAura_tip` encurtado; `refineElixir_text2` = "Receita"; 72 prescrições "Presc. de X"; `town_huafei` e "Time needed" (duração em "d").
- **B**: `liquorName1001/1002` (bebidas da taverna). `Tela.json` ganhou 19 trocas por texto exato (dificuldades, Name/Description/Load into the game, Mod Store, Repair/Forge/Enhance, abas Martial/Spiritual/Special/Ultimate/Motion). São palpites sobre o texto exato: confirmar no registro de desconhecidas.

## Não encontrado
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
