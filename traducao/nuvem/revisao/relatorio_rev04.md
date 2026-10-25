# Relatório rev04

- Modelo: Claude (revisão na nuvem) sobre a tradução do Hy-MT2. Lote de 1000 entradas (interface do editor de mods).
- Corrigidas: 169 entradas, gravadas em `out_rev04.json`. Verificador `--parcial`: 169 sem erro, 0 aviso.
- Erros mais comuns:
  - "plot" traduzido como "Lote", "Quadrinhos", "Gráfico", "plotagem" ou deixado em inglês; padronizado em "enredo".
  - "character" traduzido como "caractere" (padronizado em "personagem"); "Calabouço" e "quartos" no lugar de "masmorra" e "salas".
  - Sentido trocado: "Close game" virou "Jogo acirrado", "Quit" virou "Concluído", "Sort" virou "Sorte", "Mod" virou "Modo", "Hot" virou "Quente".
  - Português de máquina: "itemes", "na mapa", "Mod de Exportação", "Extrato NPC-A", "Caractere de plotagem", "Tale of Imortal".
  - "Artifact Map" ajustado para o nome dado em `nomes` (Mapa do Artefato da Lâmpada da Alma) em duas entradas.
  - "Creative Workshop" e "Oficina Criativa" unificados em "Oficina Criativa".
- Dúvidas de termo:
  - `nomes` dá "Mapa do Artefato da Lâmpada da Alma" para "Artifact Map" em textos genéricos do editor; parece nome de item específico aplicado por engano.
  - "nurture" aparece como "caminho traçado" e "criação" (clearFeature, delNPCLuck); deixei como está por falta de termo no glossário.
  - BOSS e CHEFE alternam no lote; deixei, só unifiquei dentro das entradas corrigidas.
