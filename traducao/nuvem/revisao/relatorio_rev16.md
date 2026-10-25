# Relatório rev16 (revisão do Hy-MT2)

- Lote de 1.000 entradas (town*, ui*, unlockDesc*, wenjuan*, ultimate_*); 284 corrigidas em `out_rev16.json`. Verificador `--parcial`: 0 ERRO, 18 AVISO.
- Erros mais comuns: "item" sem plural e "Materiales"/"itemes" (espanholismo); "Arte Imortal" com gênero errado ("seu Arte"); `en` em chinês traduzido de forma errada ("Quit"→"Concluído", "Sort"→"Sorte", "Clear"→"Claro"); "Imortal Galinha/Frango" no lugar de "Galinha Imortal"; "Array/阵法" como "padrão/matriz" em vez de "Formação"; pontuação com espaços duplos antes de `\n`.
- Padronizei "Desbloqueio:" nas descrições de desbloqueio (havia "Desbloquear:"/"Desbloqueie:") e "Lance" para bids.
- Os AVISO restantes são falsos alarmes: nomes compostos de Qi/Orbe (os `nomes` dão o nome completo, que foi usado).
- Dúvidas de termo para decisão:
  1. `nomes` dá "Respiração da Pena do Submundo" (uiWingmanScene_NeedTip), mas o glossário manda "Breath" → "Sopro". Usei o `nomes`.
  2. `nomes` tem erros de digitação: "Fusãodo Golem" (usei "Fusão do Golem") e "Flores Resplandescentes" (mantido como dado).
  3. `nomes` dá "Orbe de Qi do Relâmpago Condensada" etc. (feminino com "Orbe"); mantido como dado.
  4. "八荒" ficou "Oito Desertos" (o Hy-MT2 escreveu "Desiertos"); falta decidir o termo.
  5. Nomes dos Espíritos do Artefato (wenjuan7_*) em pinyin, como em `drama_option80143041`.
