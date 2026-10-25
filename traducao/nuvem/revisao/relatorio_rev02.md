# Relatório rev02

- Modelo: revisão feita pelo Claude na nuvem, entrada por entrada (923 entradas; sem scripts de tradução).
- Corrigidas: 214 entradas (gravadas em `out_rev02.json`). Verificador `--parcial`: sem ERRO.
- Erros mais comuns do Hy-MT2:
  - termo inventado ou errado ("Ônibus Penetrante", "Penas Verdesjurásicas", "Martejador Solitário", "Tchau, rodada", "engrenagens" por equipamentos, "Estátua de Feng Shui");
  - nomes de `nomes` ignorados (Corneta da Desolação, Sino Cerúleo, Sempre-Verde, Senhor dos Mortos, Caranguejo Gigante etc.);
  - "Durability" como "Resistência"; "character" como "caractere"; "plot" como "lote"/"plotagem";
  - "Eye of Providence" e "Mod Editor" deixados em inglês ou com maiúscula irregular, "Win-win" sem tradução;
  - inconsistência de título (Novas Vestimentas/Fantasias/Roupa → Novos Trajes; Reescrevendo Destinos → Destinos Reescritos; Cabaço → Cabaça);
  - espaços duplos no fim de linha nas notas de atualização e erros ortográficos ("materiales", "especiales").
- Falsos alarmes deixados: "Fortune Teller" (Cartomante) e `nomes` com erro de dado ("Primeval Thunder → Thunder Primordial", usei "Trovão Primordial").
- Dúvidas para o usuário: "Imp" ficou "Diabrete" (como em `nomes`); "Destinos Reescritos" vs. "Reescrever o Destino" do glossário nos títulos de notas de atualização; "Charm" em moDefensefectType (ch vazio, mantido "Talismã").
