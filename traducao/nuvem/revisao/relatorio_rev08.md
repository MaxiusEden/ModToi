# Relatório rev08 (revisão do Hy-MT2)

- Modelo: revisão feita pelo próprio Claude, entrada por entrada (692 entradas); 185 corrigidas, 507 mantidas.
- Verificador `--parcial`: 0 ERRO; 1 AVISO falso (s_8200101_hftl: "Martial/Spiritual Skills" é genérico).
- Erros mais comuns: unidade "s" perdida depois de marcador (`&x&s` virou `&x&`); verbos de direção inventados ("reduzido em", "aumenta em") onde o `en` só traz o valor com sinal, trocados por frase neutra; nomes de habilidade deixados em inglês (Groundfire, Sunfire, Frostbrand, Soulreaper, Earthfall, Meteorfall, Shockwave, Stormward, Justice Doer, Entangle); o mesmo nome com várias traduções (Relâmpago Veloz/Ágil/Elegante/Impetuoso, Tristeza/Luto/Fogo da Tristeza de Fogo, Bola de Solo/Terra, Vento Devastador).
- Alinhei os nomes com `out_resto17.json`: Tristeza de Fogo, Fogo Solar, Ceifa-Almas, Relâmpago Veloz, Relâmpago Colisor, Vento Saqueador, Ventos Varredores, Bola de Terra, Espada Espiritual.
- Dúvidas de termo (decisão do usuário): Groundfire → "Fogo Rasteiro", Frostbrand → "Lâmina de Geada", Meteorfall → "Queda de Meteoro", Earthfall → "Queda da Terra", Stormward → "Guarda-Tempestade", Justice Doer → "Justiceiro", Shockwave → "Onda de Choque", Lightning Blast → "Explosão de Relâmpago" (escolhas minhas; esses nomes estão sem tradução em `LocalText`).
- Sentido: "pilha(s)" e "acúmulo(s)" alternam no lote; não unifiquei (ambos aceitáveis). `Agility` aqui é a velocidade de movimento (ch 移动速度).
