# Tradução na sessão na nuvem

Instruções para a sessão do Claude Code na nuvem que traduz os lotes desta pasta. A exceção que permite isso está no `CLAUDE.md` (seção "Modelos de tradução", regra "O Claude não traduz em volume"), autorizada pelo usuário em 2026-10-25 e válida até 2026-11-05.

## O que fazer

1. Leia as regras em `tools/ptbr/prompt_anthropic.md` e o glossário:
   - `glossario/obrigatorios.json`: termo inglês → tradução que deve ser usada;
   - `glossario/sugestoes.json`: preferências, para usar quando couber;
   - `glossario/iguais_ao_ingles.json`: textos que podem ficar iguais ao inglês e prefixos de chave de nome (pinyin);
   - `glossario/decisoes.md`, se precisar entender o porquê de uma regra.
2. Para cada `traducao/nuvem/src_<nome>.json` que ainda não tem um `out_<nome>.json` completo, traduza cada entrada do `en` para português do Brasil, você mesmo, entrada por entrada.
   - Cada entrada tem `en`, `ch` e, às vezes, `nomes` (traduções aprovadas de nomes que aparecem no texto; use-as exatamente).
   - Você pode consultar `traducao/LocalText/*.json` para manter nomes e termos já traduzidos, mas não edite esses arquivos.
3. Grave `traducao/nuvem/out_<nome>.json`: um objeto JSON `{"chave": "tradução"}` com as mesmas chaves do lote, na mesma ordem, em UTF-8 (`ensure_ascii=False`, `indent=1`).
4. Rode o verificador e corrija todo `ERRO`:

   ```bash
   python tools/ptbr/nuvem_checar.py traducao/nuvem/src_<nome>.json traducao/nuvem/out_<nome>.json
   ```

   Leia também os `AVISO`. Corrija os que forem erro de verdade (termo do glossário trocado, nome dado não usado) e deixe os falsos alarmes como estão.
5. Escreva `traducao/nuvem/relatorio_<nome>.md`, com no máximo 15 linhas: o modelo usado, o total de entradas, os erros que sobraram e por quê, e as dúvidas de termo que valem uma decisão do usuário.
6. Faça commit só de `traducao/nuvem/out_<nome>.json` e `traducao/nuvem/relatorio_<nome>.md`, com a mensagem `Traduz <nome> na nuvem`, e faça push na branch `traducao-nuvem`.

## Limites

- Traduza direto, sem scripts que gerem tradução e sem ferramentas externas, web ou outros modelos. Scripts só para ler, gravar e conferir JSON.
- Não altere nenhum outro arquivo: nem o glossário, nem o código, nem `traducao/LocalText`, nem o `CLAUDE.md`.
- Não faça merge em `main` e não abra pull request.
- Marcadores (`{…}`, `&…&`, `$…$`, `%s`, tags `<…>`, quebras de linha) são variáveis do jogo: copie cada um exatamente, quantas vezes aparecer. Um marcador quebrado faz o jogo mostrar o texto errado.
- Se uma entrada for impossível de traduzir com segurança, deixe a melhor tentativa, explique no relatório e siga.

## Lotes

- `src_piloto.json`: 100 entradas de teste (descrições de itens, talentos, traços, opções de diálogo, habilidades e casos difíceis de marcador). Feito em 2026-10-25 (`out_piloto.json`).
- `src_falhas1.json`, `src_falhas2.json`, `src_falhas3.json` (494 entradas no total, liberados em 2026-10-25): entradas em que o tradutor local falhou na validação, quase sempre por marcador perdido ou estragado, alternativa com barra ou texto deixado em inglês. Faça um lote por vez, na ordem, com commit e push ao fim de cada um, para o trabalho poder ser retomado. Nomes em pinyin que ficam iguais ao inglês são esperados; se o verificador acusar "idêntico ao inglês" num nome assim, deixe como está e cite no relatório.
