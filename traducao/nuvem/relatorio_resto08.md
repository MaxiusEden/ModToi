# Relatório resto08 (src_resto08.json)

- Modelo: Claude Sonnet 5.5 (`claude-sonnet-5-5`), sessão na nuvem; tradução escrita por mim, sem modelo externo nem web.
- Total: 346 entradas em `out_resto08.json` (228 textos distintos). Verificador: 0 ERRO, 6 AVISO.
- Os 6 AVISO são falsos alarmes: subnomes dos `nomes` que não aparecem sozinhos (`Shield Talisman → Talismã de Escudo`, dentro do nome composto "Talismã do Escudo Espiritual"; `Qi Deviated → Desviado do Qi`, enquanto o composto dado é "Cultivador Desviado pelo Qi", usado exatamente).
- Método: textos idênticos (e com os mesmos `nomes`) receberam a mesma tradução, escrita uma vez por mim. Um `en` em chinês (`item_desc6081013`) traduzido do `ch`.
- Dúvidas de termo (decisão do usuário):
  - Habilidades das Fornalhas, em colchetes, traduzidas por mim: [Perpétuo], [Sina] (Kismet), [Fado] (Fate), [Baluarte], [Revigoramento de X], etc. "Holy Jade" e "Sacred Jade" (lote 07) ficaram ambos "Jade Sagrado" pelo glossário (Holy/Sacred → Sagrado); podem colidir no jogo.
  - "Udderly Lucky" (Destino, `item_desc6341016`) → "Sorte Leiteira" (trocadilho com vaca); Destiny (Nurture) → "Destino (Criação)".
  - Os `nomes` de `item_desc6321016` misturam "Desviado do Qi" e "Desviado pelo Qi"; usei o composto.
  - "Spiritual Pet" → "Pet Espiritual"; "minion" → "lacaio"; "Sect Tryouts" → "Testes da Seita"; "Interspatial Ring" → "Anel Interespacial".
  - `item_desc6171103`: "Pedra do Núcleo da Terra" e outros nomes de pedra/essência seguem os `nomes` ("Essência do corredor da morte" em minúsculas, como dado).
