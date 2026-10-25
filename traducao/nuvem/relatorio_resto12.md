# Relatório resto12 (src_resto12.json)

- Modelo: Claude Sonnet 5.5 (`claude-sonnet-5-5`), sessão na nuvem; tradução escrita por mim, sem modelo externo nem web.
- Total: 500 entradas em `out_resto12.json` (introduções, nomes e dicas de bônus dos traços de nascimento `role_feature_congenital_*`).
- Verificador: 3 ERRO e 5 AVISO restantes, todos falsos alarmes:
  - ERRO "idêntico ao inglês": `name1413` ("Youxia"), `name2111` ("Acalanatha") são nomes próprios e ficam; `tips1607` ("Feng Shui +25") é termo cultural que fica.
  - AVISO "Reborn → Renascimento": nomes de traço "Alquimista Renascido", "Santo da Guerra Renascido" e "Qi Renascido I/II/III"; "Renascido" é o natural aqui. Também "Wood Chopper" → "Rachador de Lenha" (lenha, não o elemento) e "Fire/Earth/Lightning Roots" → "Raízes de Fogo/Terra/Relâmpago" (Raiz já está no glossário).
- Dúvidas de termo (decisão do usuário):
  - Nomes de traço: "Alopecian Wisdom" → "Sabedoria Calva"; "Fleetfoot" → "Pés Ligeiros"; "Windchaser" → "Perseguidor do Vento"; "Nether Aired" → "Afetado pelo Ar do Submundo"; "Jolly/Merry Accident" → "Acidente Jovial/Alegre" (para diferenciar de "Happy Accident" = "Feliz Acidente"); "Disciple of Three Pure Ones" → "Discípulo das Três Purezas".
  - "Crit +N" → "Crítico +N"; "Righteous/Demonic +N" → "Justo/Demoníaco +N"; "Lifespan" → "Longevidade"; "Attack Range" → "Alcance de Ataque".
  - `tips1914/1916` terminam com vírgula no `en`; mantive.
