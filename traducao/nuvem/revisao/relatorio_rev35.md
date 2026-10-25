# Relatório rev35 (diálogos de missões: Corvos de Três Pernas, Hou Yi, Cem Mil Colinas, Mar do Leste, Bi Fang, legados Selados)

- Modelo: revisão feita pelo Claude (Sonnet 5.5). Lote com 728 entradas; 203 corrigidas (`out_rev35.json`).
- Verificador `--parcial`: 0 ERRO. 2 AVISO falsos (71292, 71382): "Qi Deviated" está coberto pelo nome maior "Cultivador Desviado pelo Qi", usado como dado.
- Erros mais comuns:
  - concordância de gênero nos termos (Água/Árvore/Pedra/Amoreira "Divino" → "Divina");
  - nomes dados em `nomes` ignorados: Ramo de Amoreira, Senhor das Harpias, Leviatan, Homem Cogumelo, Pedra Mágica do Vento, Selado - X;
  - "Nível 4"/"Grau 4" misturados e número duplicado ("<r>3</r> <g>3 Frutos...") nas missões do fruto espiritual;
  - tratamento "tu/te/teus" e pessoa trocada (vocês/você, Preparem-se/Prepare-se);
  - sentido errado ("Volte quando tiver tempo" por "quando o tiver"; "filial de Mulberry"; "Pena do Senhor Harpia <g>Dourada</g>");
  - espanhol ("Supongo"), typos ("aljogo", "monstrosos", "ever") e "Remarkável" → "Notável";
  - "Everfrost" deixado em inglês e "Geloinfinito" → "Geada Eterna"; "Vila/Aldeia da Dualidade" e "Mar Oriental/Mar do Leste" unificados.
- Dúvidas para decisão: "Leviatan" (vem de `nomes`, talvez devesse ser "Leviatã"); "Montanha Moonfall" ficou como está (nome sem tradução definida).
