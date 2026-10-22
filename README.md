# ModToi — Tradução PT-BR para Tale of Immortal (鬼谷八荒)

Mod de tradução completa para português brasileiro (textos em chinês e inglês). Fase seguinte: enriquecer os diálogos de NPC.

- `docs/ANALISE.md` — análise técnica do jogo (estrutura, onde fica o texto, API de localização, criptografia, fontes, estratégia do mod).
- `docs/localtext-categorias.txt` — estatísticas das chaves de `LocalText` e `RoleLogLocal`.
- `docs/TOI-mod-usage-management-guide.pdf` — guia de uso/gestão de mods (Cyfire, 2024).
- `ModProject_XaLtr3/` — projeto de mod criado pelo editor do jogo (namespace `MOD_XaLtr3`).
- `tools/re/` — scripts Python de engenharia reversa usados na análise.

O `.csproj` referencia as DLLs em `D:\Games\Tale of Immortal\MelonLoader`. Se o jogo estiver em outro caminho, troque esse caminho no `ModMain.csproj`.
