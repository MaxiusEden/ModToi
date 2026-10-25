# Relatório rev46

- Modelo: Claude (revisão direta, sem scripts de tradução). Lote: 242 entradas; 56 corrigidas em `out_rev46.json`.
- Verificador `--parcial`: 56 sem erro, 0 com erro, 0 com aviso.
- Erros mais comuns: pessoa trocada em histórias de Avanço (impessoal "se alcança", "nós" no lugar de "você"); maiúsculas a mais no meio da frase (Foco, Poder, Artefato, Seitas); sentido errado ("wild boars" virou "feras selvagens"; "lucky love" virou "amigo sortudo"; "você deve direcioná-las"); ortografia ("reunem", "item raros", "os material necessários"); espaços antes de quebra de linha que o inglês não tem; tempo verbal trocado (212).
- Unifiquei nomes que variavam entre irmãs: "Elixir da Força do Dragão", "Flores de Sangue Flamejante", "Geada Eterna" (Everfrost), "Montanha do Oeste", "Não Busque a Verdade" (nome dado em `nomes`).
- Dúvidas: "o ATQ / a ATQ" e "o DEF / a DEF" variam entre entradas irmãs (deixei, é estilo); "Flores de Sangue Flamejante" e "Elixir da Força do Dragão" ainda não têm tradução fixa no glossário; "Mt. West" (poema) ficou "Montanha do Oeste".
