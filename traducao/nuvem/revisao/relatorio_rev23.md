# Relatório rev23 (revisão do Hy-MT2, narrativas)

- Modelo: Claude (revisão manual, entrada por entrada). Lote com 348 entradas; 107 corrigidas, 241 mantidas.
- Verificador `--parcial`: 107 entradas, 0 ERRO, 2 AVISO (falsos alarmes: 2010506 e 2010525 usam os nomes completos "Respiração do Espírito do Caos/da Paz", dados em `nomes`, e o checador procura "Sopro" dentro deles).
- Erros mais comuns: "vocês/sigam/peguem" para um "você" singular; "te"/"o senhor" fora do lugar; erros de tradução (board → "conselho"/"plataforma" em vez de "tabuleiro", branch → "ramos/galhos" em vez de "filiais", melting pot → "caldeirão de culturas", mayflies → "efeméridas", charms → "talismãs"); nomes dados em `nomes` ignorados (Respiração do Espírito do Caos/da Paz, Chama Solar); espaços antes de quebra de linha (10 entradas); typos ("cravtei", "cravoi", "progesso", "mortalis", "Materiales", "ancestrales", "vocÊ") e concordância ("das monstros", "o Shi Qing").
- Dúvidas de termo para o usuário:
  - Wayward Desert: o lote misturava "Deserto Desregrado" e "Deserto Errante"; unifiquei em "Deserto Errante". Outras saídas usam "Deserto Rebelde" e "Deserto Selvagem".
  - Desolate Sandbar: usei "Banco de Areia Desolado" (já usado em outras saídas); o Hy-MT2 tinha "Areia Desolada".
  - Hundred Thousand Hills: padronizei "Cem Mil Colinas".
  - "Main Manual Pavilion": usei "Pavilhão dos Manuais Principal da Aliança".
  - "pot" (pote) vs "cauldron" (caldeirão) na cadeia do Templo do Caldeirão Divino: unifiquei "pote" para o pote de jade e "caldeirão" para os demais.
