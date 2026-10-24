# ModToi — instruções para o Claude

## Modelos de tradução

A lista abaixo e `tools/ptbr/modelos.json` são a mesma lista, na mesma ordem. A fila diária lê a ordem do JSON. **Quando mudar uma, mude a outra no mesmo commit.**

Ordem do mais preciso e rápido ao menos (teste com as mesmas 50 falas; nenhum quebrou marcador):

1. `gemini/gemini-3-flash-preview`: 50/50, 21 s. Conciso na interface. Deslize visto: "Nether" virou "Inferior" (o glossário corrige).
2. `cohere/command-a-vision-07-2025`: 50/50, 44 s. Fiel ao inglês e natural nos diálogos. Deslizes: "poison" virou "poção"; às vezes longo na interface. `max_tokens` ≤ 4096.
3. `gemini/gemini-2.5-flash`: 50/50, 40 s. O mais natural nos diálogos. Deslize: às vezes "Um(a) ... encontrado(a)".
4. `groq/openai/gpt-oss-120b`: 50/50, 14 s. **Só textos curtos de interface.** Deslizes: às vezes livre demais; já usou hífen Unicode (a ponte marca `ok=false`).
5. `cohere/command-r-plus-08-2024`: 49/50, 76 s. Boa qualidade, mais lento. `max_tokens` ≤ 4096.
6. `cohere/command-a-reasoning-08-2025`: 50/50, 43 s. Reserva. Deslizes: às vezes copia pontuação chinesa do `ch` (ex.: "1、") e trocou "poison" por "poção". `max_tokens` ≤ 4096.
7. `groq/qwen/qwen3.8-27b`: 50/50, 60 s. Último recurso. Deslize: um anglicismo ("encontraram cada vez mais difícil").

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
