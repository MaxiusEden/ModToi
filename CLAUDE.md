# ModToi — instruções para o Claude

## Modelos de tradução

A lista abaixo e `tools/ptbr/modelos.json` são a mesma lista, na mesma ordem. A fila lê a ordem do JSON. **Quando mudar uma, mude a outra no mesmo commit.**

Ordem de uso (teste com as mesmas 50 falas; nenhum quebrou marcador). O 1 é o tradutor principal; os de API, do mais preciso e rápido ao menos, são complemento e reserva:

1. `local/hy-mt2` (Hy-MT2-30B-A3B no KoboldCpp, nesta máquina): 50/50, marcadores intactos, melhor qualidade do teste. Lento (~20 tok/s), sem cota, `temperature` 0.7. Tiques para revisar: "Visual"/"Conjunto de roupas" (prefira "Traje"), "Nome Daoista" (o glossário agora pede "Título Daoísta"), maiúsculas a mais no meio da frase.
2. `gemini/gemini-3-flash-preview`: 50/50, 21 s. Conciso na interface. Deslize visto: "Nether" virou "Inferior" (o glossário corrige).
3. `cohere/command-a-vision-07-2025`: 50/50, 44 s. Fiel ao inglês e natural nos diálogos. Deslizes: "poison" virou "poção"; às vezes longo na interface. `max_tokens` ≤ 4096.
4. `gemini/gemini-2.5-flash`: 50/50, 40 s. O mais natural nos diálogos. Deslize: às vezes "Um(a) ... encontrado(a)".
5. `groq/openai/gpt-oss-120b`: 50/50, 14 s. **Só textos curtos de interface.** Deslizes: às vezes livre demais; já usou hífen Unicode (a ponte marca `ok=false`).
6. `cohere/command-r-plus-08-2024`: 49/50, 76 s. Boa qualidade, mais lento. `max_tokens` ≤ 4096.
7. `cohere/command-a-reasoning-08-2025`: 50/50, 43 s. Reserva. Deslizes: às vezes copia pontuação chinesa do `ch` (ex.: "1、") e trocou "poison" por "poção". `max_tokens` ≤ 4096.
8. `groq/qwen/qwen3.8-27b`: 50/50, 60 s. Último recurso. Deslize: um anglicismo ("encontraram cada vez mais difícil").

### Hy-MT2 local

- **Está no ar?** `curl http://127.0.0.1:5001/api/v1/model` deve devolver `koboldcpp/Hy-MT2-30B-A3B-Q4_K_M`. Se não responder, peça ao usuário para rodar o `Iniciar-Traducao.bat` (ver abaixo) ou só o `C:\Users\filip\Downloads\bot\Start-HyMT2.bat`; não inicie sozinho. O modelo leva ~1 min para carregar.
- **Comando:** `python tools/ptbr/ptbr.py local --continuo` (o modo normal) ou `local --src <src.json> --out <out_hymt2.json> [--limit N]` (um arquivo avulso, para testes). Entrada chave → {ch, en, pt}, só as de `pt` vazio; saída chave → {pt, ok, model: "local/hy-mt2"}, como a do `omni_translate`. Uma linha por pedido, sem system prompt; retomável e salva a cada 20 linhas. Julga a saída como a ponte (`normalize_punct`, `placeholders_ok`, idêntico ao inglês com mais de 4 caracteres e alguma minúscula, exceto os textos e as chaves de nome de `glossario/iguais_ao_ingles.json`; uma barra que o en não tem também dá `ok=false`). Se o KoboldCpp for fechado no meio, salva o que fez e para.
- **Prompt:** em chaves de nome (pessoas, seitas, lugares), pede para manter o pinyin e traduzir só as palavras inglesas pelo sentido, sem alternativas. Quando há `$…$`, `&…&` ou `{…}`, explica que são variáveis que o jogo troca por valores de atributos e habilidades e pede para copiá-las exatamente.
- **`--continuo`:** termina o lote local que estiver aberto (`traducao/auto/local/src_loteNNN.json`) e reserva sozinho o próximo (500 entradas, na prioridade da fila), até acabarem as pendentes livres ou o KoboldCpp fechar. Nunca escreve em `traducao/LocalText`: merge, `validate` e `build` são só da fila.
- **Reservas:** cada entrada pendente tem no máximo um dono. O local não pega o que está em outro lote local, numa saída de API ainda não juntada (`traducao/auto/fila/*/*.json`) ou na reserva da fila em curso (`traducao/auto/fila/reserva_apis.json`, com o pid dela). As APIs só traduzem o que reservaram, em blocos de 1000, e a reserva some no fim da rodada. As duas reservas passam pela trava `traducao/auto/reservas.lock`. Teste: `python tools/ptbr/teste_local_continuo.py` (KoboldCpp e OmniRoute falsos, repositório temporário).
- **Na fila** (`tools/ptbr/fila.py`): (a) junta as `ok=true` de todos os lotes do Hy-MT2 e, se nenhum `local` estiver vivo, reserva o próximo lote e inicia o `local --continuo`; (b) as `ok=false` do Hy-MT2 vão para os modelos de API na ordem acima; (c) as APIs adiantam o que reservaram; (d) merge, `validate` e `build`.

### Rodar a fila (manual)

O PC é desligado toda noite, então **não há tarefa agendada** (a `ModToi - fila de traducao` está desativada). Não crie tarefas de inicialização nem nada que suba sozinho com o Windows.

- **Quem inicia:** o usuário roda `C:\Users\filip\Downloads\bot\Iniciar-Traducao.bat`. Ele sobe o OmniRoute e o Hy-MT2 se não estiverem no ar, espera os dois responderem (até 5 min) e roda `uv run --script tools/ptbr/fila.py`.
- **No início**, a fila checa `http://127.0.0.1:20128/v1/models` (qualquer resposta HTTP, até 401, conta como no ar) e `http://127.0.0.1:5001/api/v1/model`. Quem estiver fora do ar fica registrado como "OmniRoute fora do ar" ou "Hy-MT2 fora do ar" no `traducao/auto/fila/fila.log` e no relatório do dia, e só a etapa dele é pulada.
- **O `local` roda destacado:** sem console e sem herdar handles. Ele continua se a fila ou a janela do bat fecharem, passa de um lote para o outro sozinho e só para quando acabam as pendentes livres ou quando o KoboldCpp é fechado (o usuário fecha para liberar a placa). A fila não inicia um segundo se já houver um vivo.
- **Progresso do `local`:** `traducao/auto/local/local.log` (o lote em que entrou, uma linha por entrada e, a cada 20, linhas/min e ok/ok=false) e `out_loteNNN_hymt2.json` (as traduções de cada lote). O `local.pid` existe enquanto ele roda.
- **Retomar no dia seguinte:** rodar o `Iniciar-Traducao.bat` de novo. A fila junta o que o `local` fez, manda as `ok=false` para as APIs e reinicia o `local --continuo`, que volta à primeira linha que falta no lote aberto. Para retomar só o `local`, sem a fila: `python tools/ptbr/ptbr.py local --continuo` (o que ele traduzir entra no `traducao/` na próxima rodada da fila).
- **Fim de cada rodada:** `traducao/auto/fila/relatorio-AAAA-MM-DD.md`. A fila não faz commit; o resultado (`traducao/LocalText`, `ModAssets/ptbr/LocalText.json`) é revisado e commitado à parte.

Regras:

- Use sempre o ID exato acima no parâmetro `model`; não use `auto/*` nem combos.
- Ao receber 429 ou erro repetido, passe para o próximo da lista.
  - A cota diária do Gemini volta às 07:00 UTC (04:00 de Brasília).
  - Os três modelos `cohere/` usam a mesma chave de trial, então dividem a mesma cota.
- **Modelos proibidos** (falharam no teste ou violam termos de uso):
  - todo `cloudflare-ai/`, `cf/` e `cfp/`;
  - `nvidia/*`;
  - `cohere/command-a-03-2025`;
  - `openrouter/*` (exceto se o usuário liberar);
  - `aug/`, `dva/`, `cxa/`, `zc/`, `ddgw/`, `unc/`, `veo*`.
- **O Claude não traduz em volume.** Se toda a lista estiver sem cota, pare e avise o usuário em vez de traduzir.
  - **Exceção (autorizada pelo usuário em 2026-10-25, vale até 2026-11-05):** uma sessão do Claude Code **na nuvem**, iniciada com o aval do usuário, pode traduzir os lotes de `traducao/nuvem/` (`src_*.json` → `out_*.json`), seguindo `traducao/nuvem/LEIA.md`, `tools/ptbr/prompt_anthropic.md` e o glossário. Só esses arquivos: a sessão não edita `traducao/LocalText`, o glossário nem o código, e roda `tools/ptbr/nuvem_checar.py` em cada lote antes do commit. As saídas entram no `traducao/` só pela fila local, depois de baixadas e validadas. Fora dessa sessão, a regra acima continua valendo.
- Toda saída passa pelo `validate` antes do merge. As `ok=false` vão para `revisar`: marcador quebrado, caractere fora do ASCII e dos acentos latinos, ou texto idêntico ao inglês.
