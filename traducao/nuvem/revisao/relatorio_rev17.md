# Relatório rev17

Revisão do lote `src_rev17.json` (658 entradas, interface: wenjuan, asas/Poder Etéreo, wuxue, yinyangEyeSoul, clãs, títulos Appellation). Corrigi 219 entradas; as demais ficaram como estavam.
Verificador (`--parcial`): 219 conferidas, 0 ERRO, 0 AVISO.

Erros mais comuns corrigidos:
- Nomes de série inconsistentes: "Cultivador de X No. 1 de Seita" (48 títulos + 12 do mundo), "Alma X" (yinyangEyeSoul, ordem trocada ou termo em inglês), Roc Dourado / Pássaro Vermelho / Luz de Velas / Pavão unificados.
- Descrições dos títulos: "esse Um título daoísta" (artigo duplicado), "Técnica Marcial e Técnica Especial" no singular, "Cultivador renegado/errante" unificado em "Cultivador Errante".
- Sentido: direção inventada em "cooldown ±" (agora neutro), frases do Poder Etéreo com aptidão/dano truncados, "Insights" como "[Percepção]" singular em texto plural (tutorial de compreensão), "Frango/Galinha".
- Entradas em chinês (wenjuan9_x): "Truques avançados" para 秘籍 virou "Manuais avançados"; Pílula/Elixir e Tesouro do Céu e da Terra conforme o glossário.
- Typos e colagens: "Vermiliono", "Roca", "os material", "novos item", "Imortal Galos".

Dúvidas de termo para o usuário:
- O verificador não aceita "º": usei "No. 1" nos títulos de ranking (o `en` usa "No. 1").
- "Pássaro Vermelho" (朱雀/Vermilion Bird) e "Cultivador Errante" (Rogue Cultivator) foram escolhas minhas; vale confirmar.
- "Burned Down" (101008) ficou como "Queimadura"; "Cooldown ±" nas asas ficou neutro por não ter o sinal do valor.
