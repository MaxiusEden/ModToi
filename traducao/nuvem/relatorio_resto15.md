# Relatório resto15 (src_resto15.json)

- Modelo: Claude Sonnet 5.5 (`claude-sonnet-5-5`), sessão na nuvem; tradução escrita por mim, sem modelo externo nem web.
- Total: 500 entradas em `out_resto15.json` (dicas `role_feature_postnatal_tips*`: efeitos de Destinos, traços de batalha e bônus). Verificador: 0 ERRO, 0 AVISO.
- Marcadores (`&var&`, `$s_xxx$`, `{luckUseCount}`, `{#areaNameN}`, `<g>`, `<r>`, `\n`) copiados idênticos; conferi pelo verificador.
- Dúvidas de termo (decisão do usuário):
  - Muitas frases deixam o verbo dentro da variável ("cooldown &var&%", "damage taken &var&%", "Agility &var&%"); mantive a estrutura do `en` ("a Recarga ... &var&%"). Convém ver no jogo se a frase fica natural com o valor substituído.
  - `tips700158`: "Crit rate" → "Multiplicador Crítico" (`ch` 暴击倍率). `tips8000200`: "Crit DR" → "RD Crítica" (sigla incerta).
  - "HP" (várias dicas) → "PV"; "Field Skills" → "Habilidades de Domínio"; "Thundercall" → "Invocação do Trovão" (dado); "Following Dragon" → "Dragão Seguidor"; "Greenscale Spring" → "Fonte Escama Verde"; "AoE damage" → "dano em área".
  - "Dragon Roar"/"Tiger Howl" → "Rugido do Dragão"/"Uivo do Tigre"; "Mystery Man Jo" → "Homem Misterioso Jo"; "Chicken King" → "Rei dos Frangos".
  - `tips8012601/8012602`: "Main Manual Pavilion" → "Pavilhão dos Manuais Principal" (para manter o termo do glossário).
  - `tips8104207`: "Debate gains" → "ganhos de Debate".
