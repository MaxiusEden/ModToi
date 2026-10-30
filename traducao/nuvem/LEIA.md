# Tradução na sessão na nuvem

Instruções para a sessão do Claude Code na nuvem que traduz os lotes desta pasta. A exceção que permite isso está no `CLAUDE.md` (seção "Modelos de tradução", regra "O Claude não traduz em volume"), autorizada pelo usuário em 2026-10-25 e válida até 2026-11-05.

## Tarefa atual: revisão por prioridade (crédito limitado)

Esta é a única tarefa em aberto; as seções seguintes são o histórico das anteriores. Sobra pouco crédito, então gaste o mínimo em cada passo: o que não for feito aqui fica para depois, sem problema.

`prioridade/src_pNNN.json` (169 lotes, do mais ao menos importante; `prioridade/INDICE.md` lista o conteúdo). Grupo A (p001–p042): interface, nomes e descrições que nunca foram revisados. Grupo B (p043–p169): segunda leitura de missões e diálogos, sem o campo `ch`.

Como trabalhar:

- Um lote por vez, na ordem numérica, começando pelo primeiro sem `out_pNNN.json`. Sem threads paralelas, sem subagentes e sem explicações entre os passos.
- Leia uma vez `tools/ptbr/prompt_anthropic.md` e `glossario/obrigatorios.json`. Não leia `glossario/decisoes.md`, os relatórios, `traducao/LocalText` nem os lotes antigos.

Para cada lote:

1. Compare `pt` com `en` (e `ch`, se houver) e corrija só o que estiver errado: sentido trocado, omitido ou inventado; termo do glossário ou nome de `nomes` errado; tratamento errado; maiúscula a mais no meio da frase; trecho em inglês; concordância errada; português que soa a tradução de máquina. Se estiver aceitável, deixe como está: não reescreva por estilo.
2. Interface (grupo A): não deixe o texto mais longo do que o `pt` atual, salvo para corrigir um erro; o espaço na tela é curto.
3. Nomes de item, efeito, lugar e técnica: mude só com erro claro, porque outros textos usam esses nomes. Na dúvida, mantenha.
4. Grave `prioridade/out_pNNN.json` só com as corrigidas: `{"chave": "tradução corrigida"}`. Se nada precisar de correção, grave `{}`.
5. Rode o verificador e corrija todo `ERRO`; `AVISO` só se for de fato um erro seu:

   ```bash
   python tools/ptbr/nuvem_checar.py --parcial traducao/nuvem/prioridade/src_pNNN.json traducao/nuvem/prioridade/out_pNNN.json
   ```

6. Acrescente uma linha a `prioridade/PROGRESSO.md`: `pNNN: N corrigidas` e, só se houver, uma dúvida de termo em poucas palavras.
7. Commit só desses dois arquivos (`Revisa pNNN na nuvem`) e push, antes de começar o próximo lote. Não edite os `src_*`.

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
- `src_falhas1.json`, `src_falhas2.json`, `src_falhas3.json` (494 entradas no total, liberados em 2026-10-25): entradas em que o tradutor local falhou na validação, quase sempre por marcador perdido ou estragado, alternativa com barra ou texto deixado em inglês. Feitos em 2026-10-25. Nomes em pinyin que ficam iguais ao inglês são esperados; se o verificador acusar "idêntico ao inglês" num nome assim, deixe como está e cite no relatório.
- `src_resto01.json` … `src_resto30.json` (10.700 entradas, liberados em 2026-10-25): todo o texto que ainda falta, reservado para esta sessão até 2026-11-05. Cada lote tem até ~60 mil caracteres de inglês. Os lotes 25 (parte) a 30 são notas de atualização do jogo (`startGameTipDesc`), longas e de menor prioridade.
  - Faça um lote por vez, na ordem, com commit e push ao fim de cada um, como nos anteriores.
  - **Pare depois de cada 5 lotes** (05, 10, 15, 20, 25) e espere o usuário mandar continuar: ele confere o crédito entre um bloco e outro.
  - Em lotes grandes, grave a saída em partes se precisar, mas o `out_restoNN.json` final tem todas as chaves do lote, na ordem.
  - Feitos em 2026-10-25.
- `src_extra.json` (11 entradas): as últimas que o tradutor local errou. Traduza como os outros lotes, com `out_extra.json` e `relatorio_extra.md`.

## Revisão das traduções do tradutor local (`traducao/nuvem/revisao/`)

`revisao/src_revNN.json` (47 lotes, 31.961 entradas; 01–17 interface, 18–47 narrativas) traz, além de `en`, `ch` e `nomes`, o campo `pt`: a tradução que o tradutor local (Hy-MT2) fez e que já passou na validação automática de marcadores. A tarefa é revisar, não retraduzir.

1. Para cada entrada, compare `pt` com `en` (e `ch` quando ajudar) e corrija só o que estiver errado:
   - sentido trocado, omitido ou inventado;
   - termo do glossário obrigatório trocado, ou nome dado em `nomes` diferente;
   - tratamento errado ("tu", "te", "ti", "vos" no lugar de "você"; pessoa diferente do `en`);
   - maiúscula a mais no meio da frase, alternativas com barra, trecho deixado em inglês;
   - erro de concordância ou português que soa a tradução de máquina.
   Se a tradução estiver aceitável, deixe como está: não reescreva por estilo.
2. Grave `revisao/out_revNN.json` só com as entradas corrigidas: `{"chave": "tradução corrigida"}`, na ordem do lote. Se nada precisar de correção, grave `{}`.
3. Rode o verificador no modo parcial e corrija todo `ERRO`:

   ```bash
   python tools/ptbr/nuvem_checar.py --parcial traducao/nuvem/revisao/src_revNN.json traducao/nuvem/revisao/out_revNN.json
   ```

4. Escreva `revisao/relatorio_revNN.md` (no máximo 15 linhas): quantas entradas corrigiu, os tipos de erro mais comuns e as dúvidas de termo.
5. Faça commit só de `out_revNN.json` e `relatorio_revNN.md` (`Revisa revNN na nuvem`) e push.

Os lotes são independentes e podem ser feitos em paralelo, cada um por uma thread. A ordem de prioridade é a numérica: interface antes de narrativa.

## Consistência de termos

`revisao/relatorio_consistencia.md`: uma lista das variações de tradução de um mesmo termo entre as saídas da nuvem (`out_*.json` e `revisao/out_rev*.json`), por exemplo "Sopro" e "Respiração", ou "fera sagrada" e "fera lendária". Para cada termo: as variantes, quantas vezes cada uma aparece e a sugestão de unificação segundo o glossário. Só o relatório; não altere as traduções por isso.

## Unificação de termos (decidida pelo usuário em 2026-10-25)

Aplique as decisões abaixo, que vêm do `revisao/relatorio_consistencia.md` e já estão em `glossario/obrigatorios.json` e na tabela "Decisões pontuais" de `glossario/decisoes.md`, em todas as traduções desta pasta:

- traduções da nuvem: edite no próprio arquivo os `out_piloto.json`, `out_falhas1-3.json`, `out_extra.json` e `out_resto01-30.json`;
- traduções do tradutor local (o campo `pt` de `revisao/src_revNN.json`): a correção vai para `revisao/out_revNN.json`. Atualize a entrada se ela já estiver lá; senão, acrescente. Nunca edite os `src_*`.

| Termo (en) | Use | Observação |
|---|---|---|
| Iron Shard | Fragmento de Ferro | no lugar de Lasca e Estilhaço |
| Mindsnap / Mindsnap Temple | Quebra-Mente / Templo Quebra-Mente | no lugar de "Templo Mindsnap" |
| Everfrost | Geada Eterna | também nos nomes compostos |
| Ground of No Return | Solo do Não Retorno | no lugar de Terra Sem Volta/Retorno |
| Wayward Desert | Deserto Errante | no lugar de Deserto Rebelde |
| Rogue Cultivator(s) | Cultivador(es) Errante(s) | no lugar de "renegado" |
| Interspatial Ring | Anel Interespacial | no lugar de Interdimensional |
| Breath | Sopro | inclusive em nomes ("Sopro do Caos"); "Respiração" só para o ato de respirar (Breathing) |
| Boss / BOSS | Chefe | CHEFE só onde o `en` é um rótulo em caixa alta; nada de "BOSS" no português |
| Legendary Beast / Divine Beast | Fera Lendária / Fera Divina | "besta" vira "fera"; "fera sagrada" só se o `ch` justificar |
| No. 1 em títulos | Nº 1 | regra 11: `Hua Feng No. 1 Sword Cultivator` → `Nº 1 em Espada de Hua Feng`. O "º" agora é aceito. Nesses títulos, o aviso "glossário: Cultivator → Cultivador" é esperado. |
| casket | caixão / caixa | caixão quando guarda um morto; caixa quando é recipiente de item |
| Upgrade | aprimorar / atualização | aprimorar e aprimoramento para item, habilidade e construção; atualização só para atualização do jogo |

Como fazer:

1. Use scripts só para encontrar as ocorrências (pela palavra em inglês no `en` e pelas variantes no `pt`). Faça cada troca à mão, ajustando a concordância (gênero, número, artigo, preposição) e mantendo os marcadores.
2. Depois de editar um arquivo, rode o verificador nele: sem `--parcial` para os `out_*.json` da nuvem, com `--parcial` para os `revisao/out_revNN.json`. Corrija todo `ERRO`.
3. Faça commit por grupo de arquivos (`Unifica termos em <grupo>`) e push (`git pull --rebase` antes, porque outras threads também enviam).
4. Ao fim, escreva `revisao/relatorio_unificacao.md` (no máximo 25 linhas): quantas trocas por termo, em que arquivos, e o que ficou de fora e por quê.

Grupos para dividir entre threads (cada thread só edita os arquivos do seu grupo):

- A: `out_piloto.json`, `out_falhas1-3.json`, `out_extra.json`, `out_resto01-10.json`
- B: `out_resto11-20.json`
- C: `out_resto21-30.json`
- D: `revisao/out_rev01-16.json`
- E: `revisao/out_rev17-32.json`
- F: `revisao/out_rev33-47.json`
