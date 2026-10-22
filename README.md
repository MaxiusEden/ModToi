# ModToi — Tradução PT-BR para Tale of Immortal (鬼谷八荒)

Mod de tradução completa para português brasileiro (textos em chinês e inglês). Fase seguinte: enriquecer os diálogos de NPC.

- `docs/ANALISE.md` — análise técnica do jogo (estrutura, onde fica o texto, API de localização, criptografia, fontes, estratégia do mod).
- `docs/localtext-categorias.txt` — estatísticas das chaves de `LocalText` e `RoleLogLocal`.
- `docs/TOI-mod-usage-management-guide.pdf` — guia de uso/gestão de mods (Cyfire, 2024).
- `ModProject_6b2SYs/` — projeto de mod criado pelo editor do jogo (namespace `MOD_6b2SYs`). A raiz de projetos do editor deve apontar para este repositório (ver `docs/ANALISE.md`, seção 9).
- `tools/re/` — scripts Python de engenharia reversa usados na análise.
- `tools/ptbr/ptbr.py` — pipeline da tradução: `extract` (dump de runtime → `traducao/LocalText/`), `validate`, `build` (→ `ModAssets/ptbr/LocalText.json`). Ver `docs/ANALISE.md`, seção 10.

O `.csproj` procura as DLLs do jogo em `$(GameDir)`, cujo padrão é `C:\Games\Tale of Immortal`. Se o jogo estiver em outro caminho, defina `GameDir` (ver `docs/ANALISE.md`, seção 9.1).
