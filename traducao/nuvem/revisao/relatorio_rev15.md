# Relatório rev15

- Modelo: Claude (revisão manual). Lote com 1000 entradas (nomes de itens/habilidades, frases de traje, tutoriais de Coração do Dao, nomes de missões, textos de torneio).
- Corrigidas: 285 entradas (`out_rev15.json`). Verificador `--parcial`: 0 ERRO, 7 AVISO.
- Erros mais comuns:
  - nomes dados em `nomes` ignorados (Sempre-Verde, Leviatan, Lâmina Vil, Fênix no lugar de "Pássaro de Fogo", Senhor das Harpias, Pista de Estilete Afiado);
  - termo deixado em inglês ou mal traduzido (Rock, Token, Banner, Scout, Mulberry Branch, "Setembro" para Set Out, "Materiales", "itemes especiales");
  - "te/lhe" no lugar de "você" nos textos longos do Coração do Dao e "cultivação" no lugar de "Cultivo";
  - inconsistência entre lotes de missões (Colete/Coletar, Recupere/Recuperar, Mate/Matar, Domando/Subjugando, "o Jo", Torneio Imortal/do Imortal);
  - maiúsculas de nome de item/missão e uso de "Everfrost" em vez de Geada Eterna.
- AVISOS restantes são falsos alarmes: "Qi Deviated" está contido no nome maior "Cultivador Desviado pelo Qi", e "Fox Spirit" no nome "Orbe do Espírito Raposo" dado em `nomes`.
- Dúvidas para decisão: "Skygate" foi para "Portão Celestial" (como em `Skygate Keeper`); "Agar" ficou sem acento nas missões e "Ágar" em suit_historyName26 (não alterado); "Fleeceflower" foi para "Fo-Ti".
