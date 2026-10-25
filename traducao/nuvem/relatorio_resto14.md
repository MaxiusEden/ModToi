# Relatório resto14 (src_resto14.json)

- Modelo: Claude Sonnet 5.5 (`claude-sonnet-5-5`), sessão na nuvem; tradução escrita por mim, sem modelo externo nem web.
- Total: 500 entradas em `out_resto14.json` (nomes e descrições de traços adquiridos `role_feature_postnatal_name*` e `tips*` até o 5031).
- Verificador: 0 ERRO, 2 AVISO, falsos alarmes ou dado problemático:
  - `name9200120`: "Unbelievable Charm" → "Charme Inacreditável" (charme, não o item Talismã).
  - `name80130535`: o `nomes` dá "Murderous → Intent Assassinato" (corrompido); usei "Surto Assassino".
- Dúvidas de termo (decisão do usuário):
  - `tips116/117` vêm em chinês: "No fim de cada mês, a unidade fixa retorna ao local onde a Fortuna foi adicionada" (气运) e "Parar a execução da IA" (nomes internos).
  - "Max HP" (`tips2800002-2800005`) → "PV Máx.", pois o glossário não cobre "HP".
  - Nomes de criaturas em "Sopro de X" mantêm o pinyin (Suanyu, Leonid, Kui, Carpia, Coatl, Jimeng, Taotie, Lydra, Taowu) e traduzem os dados pelos `nomes`; "Harpy" → "Harpia", "Sphinx" → "Esfinge" (sem `nomes`).
  - "Ophryon" (`name9173`) → "Ofrião Sombrio" (palpite; sem `ch` consultado).
  - Almas `92xx` ("Blameshifter Soul" etc.) → "Alma Transferidora de Culpa", "Alma Casamenteira", "Alma Fofoqueira"...
  - "Hot Key" → "Tecla de Atalho"; "Qi Refining Middle" etc. pelo glossário (Médio/Final no masculino, como lá).
