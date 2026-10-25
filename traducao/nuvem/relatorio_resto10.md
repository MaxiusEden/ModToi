# Relatório resto10 (src_resto10.json)

- Modelo: Claude Sonnet 5.5 (`claude-sonnet-5-5`), sessão na nuvem; tradução escrita por mim, sem modelo externo nem web.
- Total: 500 entradas em `out_resto10.json` (500 textos distintos: habilidades de pet `potmonSkillLifeDesc*` e descrições de traços `prefixDescS*`). Verificador: 0 ERRO, 0 AVISO. Quebras de linha conferidas (mesma quantidade do `en`).
- Marcadores a conferir no jogo (copiados como estão):
  - `prefixDescS1101` (`prefixDescS1102007`): o `en` tem `1102_sevenDmg&` sem o `&` inicial (variável quebrada na fonte); mantive idêntico.
- Dúvidas de termo (decisão do usuário):
  - Os `en` de traços usam "Every X will &var&%..." sem verbo (a variável do jogo já traz o sinal/verbo; o `ch` é "可使...&var&%"). Traduzi com "altera ... em &var&%" quando o verbo não aparece no `en` e com "aumenta" quando o `en` diz "increase". Convém ver no jogo se a frase fica natural com o valor substituído.
  - "Crit Rate" → "Multiplicador Crítico" (pelo `ch` 暴击倍率); "Leech Rate" → "Taxa de Roubo de Vida"; "Skill Radius" → "Raio da Habilidade"; "Range"/"Skill Range" → "Alcance"/"Alcance da Habilidade"; "stat" → "aptidão" (Aptidão Marcial); "Cooldown" → "Recarga"; "Frost" → "Geada"; "Knock-back" → "Repulsão"; "Blast" → "Explosão"; "Thunderstorm" → "Tempestade"; "Sever" → "Decepar"; "Dragon Rise" → "Ascensão do Dragão".
  - `nomes` "Recarregando" (de "Recharging") ficou como "Ponto Recarregando" em `prefixDescS10205/10206`; soa estranho, mas segue o dado.
  - "condensed X's Breath" → "Sopro condensado de X" (X pelos `nomes` ou mantido: Lydra, Sagittar, Zhujian; "Sphinx" → "Esfinge", "Harpy" → "Harpia", sem `nomes`).
  - Foraging books → "livros de Coleta".
