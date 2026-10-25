# Relatório resto25 (src_resto25.json)

- Modelo: Claude Sonnet 5.5 (`claude-sonnet-5-5`), sessão na nuvem; tradução escrita por mim, sem modelo externo nem web.
- Total: 145 entradas em `out_resto25.json`: 98 descrições de missões `taskDesc*` e 47 notas de atualização `startGameTipDesc*`. Verificador: 0 ERRO, 3 AVISO. Quebras de linha conferidas (mesma quantidade do `en`).
- Avisos deixados: `startGameTipDesc111` mantém "语言—Change 简体中文 to English (Alpha)" porque é o nome do menu no jogo; "Taoist" em `130` e `136` ("Heart of a Taoist") saiu "Coração do Dao".
- Vários `startGameTipDesc` (100–109, 117–129) têm o `en` em chinês; traduzi pelo sentido, com datas no formato "Atualizado às 14h de 8 de julho de 2021", títulos "[Notas de Atualização X]" e nomes de itens/habilidades pelo inglês oficial do `LocalText` (ex.: Lago de Pérolas, Punho Quebra-Montanha). Nomes de jogadores ficaram em pinyin.
- Dúvidas de termo (decisão do usuário):
  - "神兽" (chinês) → "fera sagrada", mas o `en` dos lotes 130–139 diz "legendary/divine beast" e saiu "fera lendária"/"fera divina"; vale unificar.
  - "True Disciple" → "Discípulo Direto"; "Heaven-chosen" → "Escolhido do Céu"; "Sect Competition" → "Torneio da Seita" (nos chineses, "Grande/Pequeno Torneio da Seita"); "Flying boat/canoe" → "barco voador"/"canoa voadora".
  - `startGameTipDesc110`: "Hou Yi Shoots the Suns" → "Hou Yi Abate os Sóis"; "Love knot" → "Laço de Amor".
  - Caminho de pasta "C:\\Users\\用户名" saiu "NomeDoUsuário".
  - `taskDesc2112*`/`3000*`/`8000*`: "Tavern Task" → "Tarefa da Taverna"; "Outfit" → "Traje".
