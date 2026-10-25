# Relatório rev07 (revisão do tradutor local)

- Modelo: Claude (revisão manual). Lote com 874 entradas (descrições de habilidades `s_*`, regras `ruleDesc*`, passatempos, nomes de lendas). Corrigidas: 281. Verificador `--parcial`: 281 sem erro, 0 com erro, 0 com aviso.
- Erros mais comuns do Hy-MT2: "um/uma X% de chance" (concordância); "stacks"/"camadas"/"acúmulos"/"pilhas" misturados (padronizei os desvios; "pilha" é o mais usado); sufixo de unidade perdido ("durante &x&," sem segundos); "reduzido em"/"aumenta em" inventados onde o `en` só traz o valor com sinal (deixei neutro para não dobrar o sinal); nomes de técnica em inglês ou inconsistentes dentro do mesmo grupo (Gale Sword, Roar, Vortex, Firebolt, Fist Shadow, Skycleaver, Skyshatter, Tidebreak); "Dash" virou "Traço/dash/teletransporta" (padronizei "Investida"); "$s_x$ pontos" virou "pontos de $s_x$"; mercados "vila" no singular; `nomes` (Leviathan → Leviatan).
- Dúvidas de termo para o usuário:
  1. "Battle Soul" (Santuário da Alma de Batalha): a tradução do local trazia "Santuário da Batalha da Alma"; usei "Alma de Batalha". Vale conferir nos outros lotes.
  2. "Leech" (ruleDesc101): usei "Roubo de Vida", como no glossário de "Leech Chance".
  3. Nomes de técnica sem tradução aprovada: Splitting Dragon Spear (Lança do Dragão Fendente), Hellspear (Lança Infernal), Tidebreak (Quebra-Maré), Skycleaver/Skyshatter (Corta-Céu/Quebra-Céu), Overbearing Blade (Lâmina Dominador, conforme `nomes`). Convém unificar com os demais lotes.
  4. "Dash" → "Investida" (campo `ruleDesc201`, `s_23112_tjjl*`, `s_510021_*`).
