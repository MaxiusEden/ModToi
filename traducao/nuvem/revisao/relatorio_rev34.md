# Relatório rev34 (revisão na nuvem)

- Modelo: Claude (revisão manual, entrada por entrada). Lote com 561 entradas (eventos e diálogos de aventura, `drama_dialogue`).
- Corrigidas: 153 entradas, gravadas em `out_rev34.json`. Verificador `--parcial`: 0 ERRO, 1 AVISO.
- Erros mais comuns do Hy-MT2:
  - espanholismos e typos: "materiales", "especiales", "itemes", "Escoli", "entusiado", "galhias", "Attacando";
  - pessoa trocada: "Ele deixa o lugar", "o senhor acendeu" em texto na 2ª pessoa; plural "vocês/Protejam-me" onde o `en` fala com uma pessoa; "teus/Traz-me";
  - voz impessoal ("percebe-se", "ouve-se") onde o `en` usa "você";
  - nome/termo: "Vale dos Caídos" → Vale Caído, "Templo Mindsnap" → Templo Quebra-Mente, "Mirage" → Miragem (Orbe de Qi Miragem), "feras de espírito" → feras espirituais, "Nível 4" → Grau 4 e "3 Fruto(s)" duplicado;
  - sentido: "primavera" por fonte (spring), "estátua" por figura, "coleta de vizinhos", "Em breve" por "Logo".
- O verificador avisa "Fox Spirit → Espírito da Raposa" em `drama_dialogue630039`: falso alarme, o nome dado para "Fox Spirit's Orb" é "Orbe do Espírito de Raposa" e foi usado.
- Dúvidas de termo para o usuário: "Sea Sparkle" aparece como "Mar Cintilante" (adotado aqui); "trial" foi unificado como "provação" (Provação da Transcendência), mas o glossário não define; "Mirage" como lugar ficou "Miragem" para concordar com o nome do Orbe.
