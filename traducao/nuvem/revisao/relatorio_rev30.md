# Revisão rev30 (Claude, sessão na nuvem)

- Lote de 456 entradas (diálogos de eventos narrativos: Terra Sem Retorno, Yuanzhi, Mo Xue, o barqueiro). Corrigidas 146 em `out_rev30.json`; verificador `--parcial` sem ERRO.
- Erros mais comuns do Hy-MT2: verbos no imperativo ou infinitivo onde o `en` narra em "você" ("Pegue o tesouro", "Conte ao rapaz"); concordância de gênero (Yuanzhi e Mo Xue são mulheres: "preso", "disposto", "obrigado"); termos deixados em inglês ("Ground", "Ape Shade", "Examine Oneself", "Loner's Poetry"); "madeira" por "mata"; "Montaria" por "Monte"; nomes de `nomes` ignorados (Sempre-Verde, Marionete de Sangue, Anel da Pedra Endurecida/Submersa, Cultivador Desviado pelo Qi); "te/vocês" onde era "você".
- Unifiquei dentro do lote: Terra Sem Retorno, Nenhum Retorno (a placa), Senhora Yan, dojô (loach, que o modelo traduziu de cinco jeitos), figueira-de-bengala, Examine-se (título do livro), sentido espiritual, baú.
- Avisos que sobraram: "Qi Deviated" (o `nomes` dá a forma composta, usada) e "Transcendent → Transcendência" (mantive "ser/mulher de Transcendência" pelo glossário; "Transcendente" soaria melhor).
- Dúvidas para decisão: "Ground of No Return" aparece como "Terra Sem Retorno" e "Terra Sem Volta" em outras saídas (verificar no relatório de consistência); "loach" → "dojô"; "Lady Yan" → "Senhora Yan".
