# Revisão rev12

- Lote de 1000 entradas; 321 corrigidas (`out_rev12.json`). Verificador `--parcial`: 321 sem erro, 0 aviso.
- O lote é quase todo de prefixos de nome de habilidade (`skillPrefixName*`, um caractere chinês no `en`) e de rótulos `skill_content_*`. As 54 chaves `skill_attack_extra*` são "N/A" e ficam como estão.
- Erros mais comuns: sentido errado do caractere (穿 "Vestir", 绝 "Nunca", 御 "Por favor/Vossa", 镇 "Cidade", 月 "Mês", 日 "Dia", 煞 "Argh/Puxa", 掣 "Interruptor"); pinyin deixado (仞 ren, 凛 Rin, 沧 Cang, 璃 Lí, 凌 Líng); variantes do mesmo caractere (锋, 溟, 苍, 碧, 霜, 岳, 伏, 灭); erros de grafia ("Geloo", "Nebla", "Travessar"); maiúsculas ("pena", "interruptor").
- Termos do glossário aplicados: 冥 Submundo, 雷 Relâmpago, 紫 Púrpura, 邪 Maligno, 神 Divino, 碎 Despedaçado, 荒 Ermo (como em out_falhas), 劫 Tribulação.
- `skill_content_*`: aptidões padronizadas como "Aptidão de Espada/Lâmina/Punho/Palma/Lança/Dedo/Forja/Alquimia", raízes como "Raiz de Terra/Fogo/Água/Relâmpago aumentada/reduzida" (glossário), no lugar de "facas", "digitação", "tiro", "Raylingen" etc.
- Dúvidas para decisão do usuário: os prefixos vêm sem contexto, então escolhi uma única tradução por caractere (ex.: 仞 Altura, 掣 Arranco, 煞 Funesto, 璃 Vítreo, 沧 Vasto, 伏 Emboscada, 无 Nulo). Vale conferir no jogo se combinam com os nomes das habilidades.
- O restante dos caracteres (Alma, Sombra, Lótus, Jade, etc.) estava aceitável e não foi tocado.
- `skill_content_basThunder31` está fora deste lote (cai no rev13).
