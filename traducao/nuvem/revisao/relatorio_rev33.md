# Relatório rev33

- Lote: 562 entradas (diálogos de eventos, dicas de reinos, aventuras). Revisão feita pelo Claude (sessão na nuvem).
- Corrigidas: 127 entradas em `out_rev33.json`; o resto estava aceitável.
- Erros mais comuns do Hy-MT2: imperativo no lugar do indicativo ("Prometa ao velho..."), "materiales"/"item"/"ancestrales" (typos), tempo verbal trocado (passado no lugar do presente), impessoal "ouve-se/percebe-se" no lugar de "você", "te" no lugar de "você".
- Sentido: "Freeze" virou "Congelamento" (é "Parado"); "Transcendent Enlightenment" virou "Iluminação com Transcendência"; invenções ("com exceção de mim", "com uma grande carga nas costas", "bem bem cultivado").
- Nomes: padronizei "Frutos do/da X" conforme `nomes`, "Fada da Lua", "Cultivador de Azul", "Homem Bem-Vestido", "Homem de Branco", "Seita Azurita", "Terras Ardentes", "Mercador Fantasma", "Membro de Shennong".
- Verificador (--parcial): 0 ERRO. Os 9 AVISO restantes são falsos alarmes (plural "Frutos", "Aura do Céu e Terra", "Freeze" = "Parado").
- Dúvidas para decisão: placa "Poem Dream Studio" ficou em inglês (nome de lugar; sem tradução oficial nos dados); "Stormvale" também ficou como no inglês; "Chicken King's Here!" segue `nomes` ("Rei dos Frangos Chegou!").
