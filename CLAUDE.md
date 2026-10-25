# ModToi — instruções para o Claude

## Modelos de tradução

A lista abaixo e `tools/ptbr/modelos.json` são a mesma lista, na mesma ordem. A fila diária lê a ordem do JSON. **Quando mudar uma, mude a outra no mesmo commit.**

Ordem de uso (teste com as mesmas 50 falas; nenhum quebrou marcador). O 1 é o tradutor principal; os de API, do mais preciso e rápido ao menos, são complemento e reserva:

1. `local/hy-mt2` (Hy-MT2-30B-A3B no KoboldCpp, nesta máquina): 50/50, marcadores intactos, melhor qualidade do teste. Lento (~20 tok/s), sem cota, `temperature` 0.7. Tiques para revisar: "Visual"/"Conjunto de roupas" (prefira "Traje"), "Nome Daoista", maiúsculas a mais no meio da frase.
2. `gemini/gemini-3-flash-preview`: 50/50, 21 s. Conciso na interface. Deslize visto: "Nether" virou "Inferior" (o glossário corrige).
3. `cohere/command-a-vision-07-2025`: 50/50, 44 s. Fiel ao inglês e natural nos diálogos. Deslizes: "poison" virou "poção"; às vezes longo na interface. `max_tokens` ≤ 4096.
4. `gemini/gemini-2.5-flash`: 50/50, 40 s. O mais natural nos diálogos. Deslize: às vezes "Um(a) ... encontrado(a)".
5. `groq/openai/gpt-oss-120b`: 50/50, 14 s. **Só textos curtos de interface.** Deslizes: às vezes livre demais; já usou hífen Unicode (a ponte marca `ok=false`).
6. `cohere/command-r-plus-08-2024`: 49/50, 76 s. Boa qualidade, mais lento. `max_tokens` ≤ 4096.
7. `cohere/command-a-reasoning-08-2025`: 50/50, 43 s. Reserva. Deslizes: às vezes copia pontuação chinesa do `ch` (ex.: "1、") e trocou "poison" por "poção". `max_tokens` ≤ 4096.
8. `groq/qwen/qwen3.8-27b`: 50/50, 60 s. Último recurso. Deslize: um anglicismo ("encontraram cada vez mais difícil").

### Hy-MT2 local

- **Está no ar?** `curl http://127.0.0.1:5001/api/v1/model` deve devolver `koboldcpp/Hy-MT2-30B-A3B-Q4_K_M`. Se não responder, peça ao usuário para rodar `C:\Users\filip\Downloads\bot\Start-HyMT2.bat` (não inicie sozinho); o modelo leva ~1 min para carregar.
- **Comando:** `python tools/ptbr/ptbr.py local --src <src.json> --out <out_hymt2.json> [--limit N]`. Entrada chave → {ch, en, pt}, só as de `pt` vazio; saída chave → {pt, ok, model: "local/hy-mt2"}, como a do `omni_translate`. Uma linha por pedido, sem system prompt; retomável e salva a cada 20 linhas. Julga a saída como a ponte (`normalize_punct`, `placeholders_ok`, idêntico ao inglês).
- **Na fila diária** (`tools/ptbr/fila.py`): (a) o Hy-MT2 traduz o lote local (`traducao/auto/local/src_loteNNN.json`, 500 entradas); a fila junta as `ok=true`, cria o próximo lote quando o atual acaba e deixa o `local` rodando em segundo plano; (b) as `ok=false` do Hy-MT2 vão para os modelos de API na ordem acima; (c) merge, `validate` e `build`. Enquanto o `local` roda, as APIs adiantam outras entradas e não tocam no lote local.

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
- Toda saída passa pelo `validate` antes do merge. As `ok=false` vão para `revisar`: marcador quebrado, caractere fora do ASCII e dos acentos latinos, ou texto idêntico ao inglês.
