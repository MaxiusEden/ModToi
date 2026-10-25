# Relatório rev24

- Lote: revisao/src_rev24.json (402 entradas, narrativa do Templo do Caldeirão Divino e do Jo). Revisão feita por Claude, sem retraduzir.
- Corrigidas: 108 entradas. Verificador `--parcial`: 0 erros, 2 avisos.
- Avisos (falsos alarmes): `drama_dialogue2102051` e `2102117` usam "Anciã Shi" (feminino de Ancião).
- Erros mais comuns:
  - Tratamento: "vocês/vos/vossa" para um único "you"; frases impessoais ("Percebe-se", "Veja", "vemos") onde o `en` está em segunda pessoa.
  - Sentido: "plot" como "enredo" (eram truques), "overlord" como "excedente", "Amigo da onça" para "Best friend", "Lembre-se da aura" no lugar de "Você se lembra".
  - Gênero errado do falante (Shi Qing "Obrigado", Zhang San "impressionada") e do objeto ("lê-lo" para a receita).
  - Inconsistência de termos dentro do lote (ver dúvidas).
  - Espaços duplos antes de quebra de linha e erro de digitação ("irrrompesse").
- Dúvidas de termo para decisão do usuário (padronizei no lote):
  - Master's Recluse Room / Recluse's Room → "Sala de Reclusão do Mestre" / "Sala de Reclusão"; vale conferir os outros lotes.
  - casket → "caixa" (havia cofre, caixão, sarcófago); Hunting Fairy → "Fada da Caça"; Master/Senior/Junior Soul → "Alma Mestre/Sênior/Júnior".
  - "ruined branch" do templo → "filial em ruínas"; Divine Cauldron Temple → "Templo do Caldeirão Divino" (havia "Divino Templo do Caldeirão").
