# Relatório resto13 (src_resto13.json)

- Modelo: Claude Sonnet 5.5 (`claude-sonnet-5-5`), sessão na nuvem; tradução escrita por mim, sem modelo externo nem web.
- Total: 500 entradas em `out_resto13.json` (nomes de traços adquiridos `role_feature_postnatal_name*` e as últimas dicas de seita `tips9176-9182`).
- Verificador: 0 ERRO, 4 AVISO, todos falsos alarmes ou dados problemáticos:
  - `name5008` ("Orbe de Qi da Água", usado inteiro pelo `nomes`), `name5012` ("Encanto da Raposa": Charm = encanto, não o item Talismã), `name5036` ("Hipervitalidade").
  - `name700158`: o `nomes` dá "Murderous → Intent Assassinato", que parece corrompido; usei "Ascensão Assassina". Vale corrigir o dado.
- Dúvidas de termo (decisão do usuário):
  - `name700050`: o `nomes` traz "Fusao de Espíritos I" (sem acento); usei "Fusão de Espíritos I" e as variantes II/III.
  - `name116/117` vêm em chinês (固定单位, 停止AI): "Unidade Fixa", "Parar IA" (nomes internos).
  - `name8011422/8011435` ("Superficial", `ch` 浮躁): "Fútil", pois "Superficial" é idêntico ao inglês.
  - "Mysterious Shield II" (`ch` 中宝的盾) → "Escudo do Zhong Bao", par de Xiao Bao e Da Bao dados pelos `nomes`.
  - "Burrow" → "Escavação"; "Star-crossed" → "Fadado ao Infortúnio"; "Motherlode" → "Filão Principal"; "Build ..." (`name800000x`) mantido como "Build" (jargão de jogo).
