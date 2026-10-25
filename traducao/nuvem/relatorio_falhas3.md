# Relatório falhas3 (src_falhas3.json)

- Modelo: Claude Sonnet 5.5 (`claude-sonnet-5-5`), sessão na nuvem; tradução direta, sem scripts de tradução.
- Total: 164 entradas em `out_falhas3.json`. Verificador: 32 ERRO, 12 AVISO; nenhum de marcador.
- Os 32 ERRO são todos "idêntico ao inglês" e esperados:
  - nomes em pinyin: `xueNv`, `zhugeChen`, `drama_option80001011/12`, `80061491/93`, `80143041-48`, `80060081`, `21150681`, `22101342/22102542`, `drama_dialogue2000155/2000331/2231410/2301837/71177/71193`;
  - nomes de hexagramas em pinyin (`drama_option20153042/43/52/53/54`: Tongren, Dayou, Guimei, Wuwang, Dayou);
  - falas só com nome e reticências ("Jagu: ...", `drama_dialogue610356/610358`) e "Ah...!" (`drama_option70050161`), iguais em português.
- AVISOS são falsos alarmes: "Fire → Fogo" bate com o verbo "Fires/Fire" em frases de raio/vento/pedra; "Sword/Finger/Spear → Espada/Dedo/Lança" aparecem nos títulos de Cultivador de Punho, Relâmpago e Palma, onde o `en` trocou a arma (erro do `en`). Segui o `ch`: Punho, Relâmpago, Palma.
- Dúvidas para decisão:
  - "No.1" virou "Número 1", porque o verificador rejeita "º".
  - `wuxue_canwuguize2`: o `en` junta os itens 4 e 5 e traduz 悟/魔 por "Wu"/"Mo"; mantive "Wu"/"Mo" entre aspas e reparei a numeração (4.) sem mudar as quebras de linha.
  - `effect_desct9901131`: o nome dado "Recharging" → "Recarregando" ficou literal ("pontos de Recarregando"); vale revisar o nome.
  - Interjeições trocadas por equivalentes (Hunf!, Ehem!, Argh..., Hihi., Bem...); "Enrolar" para 敷衍; "Depósito" para 饵阁; "Dama Moradora da Geada" para 栖寒.
