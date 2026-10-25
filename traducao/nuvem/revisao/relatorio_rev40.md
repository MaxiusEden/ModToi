# Relatório rev40

- Modelo: Claude (revisão manual do `pt` do Hy-MT2). Lote com 1000 entradas (diálogos do pássaro de fogo/pega e opções de drama).
- 212 entradas corrigidas; verificador `--parcial`: 212 sem erro, 0 com erro, 0 com aviso.
- Tipos de erro mais comuns:
  - opções de diálogo em substantivo ou plural em vez de imperativo singular ("Luta", "Ajuda", "Declínio", "Lutem", "Ataquem");
  - sentido trocado ("Kick down" → "Redução de marcha", "Release" → "Lançamento", "Endure" → "Endurecer", "Prepare and return" → "Preparar e devolver");
  - variações soltas do mesmo texto repetido (13006xxx "Take action now": Age/Aja/Ague/Aguaçem...), unificadas;
  - erros de espécie e gênero: "papagaio", "papa-moscas", "pega-me-deu", narrador "feia/adotada" para o pega macho;
  - segunda pessoa trocada ("te matou", "vê-se"), "itemes", anglicismo "nonsense", termo em inglês ("Desolate Sandbar", "Nod").
- Dúvidas de termo para o usuário:
  - "Firebird" com `nomes` = Fênix só na entrada 970078; as demais ficam "pássaro de fogo".
  - Pega macho: usei "o pega"/"Irmão Pega" (o jogo trata como homem), mas o substantivo é feminino.
  - "Sharp Awl" → "Furador Afiado" (estava "Agulha Afiada"/"furador afiado"); "Jiuying Cub" → "Filhote de Jiuying".
  - Hexagramas "Li", "Qian", "Song" mantidos em pinyin.
