# Arquivos do jogo para a sessão na nuvem (copiados em 2026-10-26)

- `ptbr_tela_desconhecidas.txt`: o registro do patch `ScreenTextPtBr` com os textos que apareceram na tela e que ele não sabe trocar (texto, tipo do componente e caminho na hierarquia da tela). É a fonte para o `Tela.json`: use o texto exato daqui.
- `ptbr_missing_LocalText.txt`: chaves do LocalText que o jogo pediu e que não tinham tradução (chave, tabulação, en).
- `RoleLogLocal.json`: a tabela de onde vêm as notícias do mês, as criaturas avistadas, a descrição do Diário de eventos e as falas padrão e aleatórias de NPC (`id` → `keyID`, `ch`, `en`). O mod ainda não troca essa tabela: traduza para `traducao/nuvem/jogo/out_RoleLogLocal*.json` (`{"id": "tradução"}`), com os mesmos marcadores, e o patch que aplica a tradução no jogo será feito localmente, porque precisa compilar o mod.
