# Relatório resto06 (src_resto06.json)

- Modelo: Claude Sonnet 5.5 (`claude-sonnet-5-5`), sessão na nuvem; tradução escrita por mim, sem modelo externo nem web.
- Total: 136 entradas em `out_resto06.json` (109 textos distintos). Verificador: 0 ERRO, 53 AVISO.
- Os 53 AVISO são falsos alarmes: "nome dado não usado" de subnomes (`Fire Qi → Qi de Fogo`, `Condensed Water Qi → Qi Condensado da Água` etc.) que não aparecem sozinhos no texto, só dentro do nome composto dado (`Qi condensado do Fogo`, `Orbe de Qi da Água Condensada`...), usado exatamente.
- Método: textos idênticos (e com os mesmos `nomes`) receberam a mesma tradução, escrita uma vez por mim.
- Dúvidas de termo:
  - `nomes` inconsistentes em gênero/caixa: "Qi condensado do Fogo" (c minúsculo) vs. "Qi Condensado da Água"; "Orbe de Qi da Água Condensada" (concorda com "Água", não com "Orbe"). Usei cada um como dado.
  - Lugares sem glossário: Land of the Spirits/Arcane/Beyond/No Return → "Terra dos Espíritos/dos Arcanos/do Além/Sem Retorno"; Spiritlock Circle → "Círculo de Trava Espiritual"; Demonseal Circle → "Círculo de Selo Demoníaco"; Rule Fields → "Domínios de Regra"; Bloodeye Array → "Formação do Olho de Sangue"; Arctic Field → "Domínio Ártico".
  - Nomes de feras mantidos em inglês (Bloodspell Carpia, Hellish Dangkang, Cerulean Coati, Lonely Cloud Leonid; "Dreambreaker Espectro da Montanha" e "Hatuibwari das Nuvens Nove" pelos `nomes`); "Lord of Beasts" → "Senhor das Feras".
