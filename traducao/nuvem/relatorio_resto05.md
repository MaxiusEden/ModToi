# Relatório resto05 (src_resto05.json)

- Modelo: Claude Sonnet 5.5 (`claude-sonnet-5-5`), sessão na nuvem; tradução escrita por mim, sem modelo externo nem web.
- Total: 230 entradas em `out_resto05.json` (120 textos distintos). Verificador: 0 ERRO, 0 AVISO.
- Método: textos em inglês idênticos (e com os mesmos `nomes`) receberam a mesma tradução, escrita uma vez por mim.
- Marcadores a conferir no jogo (erros do `en`, copiados como estão):
  - `item_desc5031814` tem tags trocadas no `en` (`<g>{#battle_unit_name721}</r>`, `<g>{#building1001Name4}</r>`); mantive idêntico para não perder marcador. Convém corrigir a fonte para `<r>...</r>`.
  - `item_desc5021314` tem `{#battle_unit_namebattle_unit_name108}` (nome de variável duplicado no `en`); mantido.
- Dúvidas de termo:
  - `nomes` inconsistentes: "Areia da Compreensão G5..G2" mas "Areia de Compreensão G1"; "Vidro de Compreensão". Usei cada um como dado.
  - "Soul Stones/Crystals/Breaths" → "Pedras/Cristais/Sopros da Alma" ("Jade da Alma" pelos `nomes`); "Killer Vibe" → "Aura Assassina"; "Beast Lords" → "Senhores das Feras"; "Divine Soul Skills" → "Habilidades da Alma Divina".
  - "Outfit" → "Traje" (CLAUDE.md); "Pond of Lightning" → "Lago do Relâmpago".
