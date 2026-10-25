# Relatório resto21 (src_resto21.json)

- Modelo: Claude Sonnet 5.5 (`claude-sonnet-5-5`), sessão na nuvem; tradução escrita por mim, sem modelo externo nem web.
- Total: 500 entradas em `out_resto21.json` (talentos de Espírito do Artefato `spriteTalentDesc*` e condições de desbloqueio `spriteTalent_unlockDesc*`; muitas são "0", "X" ou "No Cost", traduzidas como "0", "X" e "Sem custo"). Verificador: 0 ERRO, 12 AVISO.
- Avisos que deixei como estão (falsos alarmes): "Martial/Spiritual Skill" saiu "Técnica Marcial/Espiritual" (o glossário pede "Técnica Espiritual" para o termo isolado); "Qi Deviated" dentro do nome já aprovado "Cultivador Desviado pelo Qi"; "Breath of Chaos" dentro do nome aprovado "Respiração do Caos Abençoada".
- Nomes com "1º Grau" nos `nomes` foram escritos "de Grau 1" (o verificador não aceita "º"), então o aviso "nome dado não usado" aparece em `100431` e `100545`.
- Dúvidas de termo (decisão do usuário):
  - `nomes` traz "Respiração do Caos Abençoada" para "Blessed Breath of Chaos", mas o glossário manda "Breath" → "Sopro" ("Sopro do Caos"); segui o `nomes`.
  - Elixires com grau: os `nomes` misturam "G3 Elixir X", "Elixir X G3", "Grau 4" e "de 1º Grau"; usei cada nome como veio, só trocando "º".
  - "Normal Monster"/"Elite Monster" → "Monstro Comum"/"Monstro de Elite"; "Monsters Behind" → "Monstros Atrás da Muralha"; "Spar" → "Treine combate".
