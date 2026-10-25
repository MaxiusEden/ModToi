# Revisão rev26 (narrativa: Tian Yuan, Feng Lixi, Zhang San e Dragão Negro)

- Modelo: Claude (revisão na nuvem, entrada por entrada). Lote de 428 entradas; 86 corrigidas em `out_rev26.json`.
- Verificador `--parcial`: 2 ERRO, ambos falsos alarmes: `drama_dialogue2221051` e `drama_dialogue2222086` são só o nome "Lixi" (o Hy-MT2 tinha escrito "Lixo"), que fica igual ao inglês por ser pinyin.
- Erros mais comuns corrigidos:
  - palavras erradas ou em espanhol: "cachimbo/canudo/flautim" para bamboo pipe (agora "tubo de bambu"), "ancestrales", "espirituales", "Tonterias", "Padre", "espinhas" por quills, "faíscas eletrônicas";
  - termos variando dentro do lote, unificados: "Décimo Terceiro Verso" (havia Melodia e versículo), "som antigo" (havia ancestral), "Fechadura Crucial" (havia Cadeado, Tranca e Fecha-Chave), "Refúgio do Dragão" (havia Morada), "Fada da Caça", "Caixas de Pó dos Sonhos", "Poder do Desejo", "Caverna You Huang", "Grilhão Mental";
  - gênero e número trocados (Nuwa e a Fada da Caça são mulheres; Pingmo é "ela"; "vocês" para uma só pessoa; "te/tu" em vez de "você");
  - trechos inventados ou omitidos ("carregando uma enorme carga de energia", "Querido" por Little one, "cúpula" por topo).
- Dúvidas de termo para o usuário: "Mind Fetter" não está no glossário (usei "Grilhão Mental"); "Crucial Lock" (usei "Fechadura Crucial"); "Dream Dust Boxes" (usei "Caixas de Pó dos Sonhos"). Vale registrar os três.
