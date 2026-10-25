# Relatório rev18 (revisão do Hy-MT2, feita por Claude na nuvem)

- 212 entradas revisadas (títulos daoístas, descrições de habilidades do Olho de Deus e tarefas da seita); 42 corrigidas em `out_rev18.json`.
- Verificador `--parcial`: 0 erros; 1 aviso falso (7374: "Fires" lido como o elemento Fogo).
- Erros mais comuns: nome de Flor deixado em inglês ou traduzido como "Florescimento" (Ink/Frost/Misty/Wooden/Imperial Bloom); tipo de cultivador trocado por "Artes Marciais" (1003, 1106, 1205); item com termo errado ("lápides de gelo", "picos", "faixas") ou nome ignorado (Picada de Bambu Demônio, Correntes de Alma); "Lutas da Morte" unificado em "Lutas Mortais"; "Pontos de Justo" virou "Pontos de Retidão"; frase com ordem quebrada (51521, 7322); caractere chinês solto (7684).
- 110063: o `en` traz "power/s" sem tradução; troquei por "de dano por segundo" (o marcador `&` estava desbalanceado).
- Dúvidas de termo para o relatório de consistência: nomes dos títulos de "No.1 ... Cultivator" variam entre "Cultivador da Lâmina No 1 de X", "Primeiro Cultivador de X" e "1o Cultivador de X"; "Misty Bloom" ficou "Flor Nebulosa"; "7203 Lightning Root X%" no `en` perdeu o verbo (ficou "fica em").
