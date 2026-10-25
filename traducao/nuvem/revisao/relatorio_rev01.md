# Relatório rev01 (revisão do Hy-MT2, interface)

- 470 entradas revisadas; 104 corrigidas em `out_rev01.json`. Verificador `--parcial`: 0 erro, 2 avisos (falsos alarmes: nomes de "Qi de Água/Relâmpago" isolados que não aparecem no texto).
- Erros mais comuns:
  - nomes de lugares/incursões inconsistentes entre si (Raid = Incursão, Takeover = Tomada, Huge = Enorme, ordem do adjetivo de tamanho; "Evergreen" = Sempre-Verde conforme `nomes`);
  - maiúsculas em nomes de técnica, destino e evento (Erupção Escaldante, Postura Tranquila etc.);
  - termo errado ou deixado em inglês (Azure, Palma Talent, Sunny mantido como nome);
  - texto do botão/aviso (Aprender Receitas, Excluir jogo salvo, Mostrar dicas, Desafiando {0});
  - `nomes` não usados (Lago de Energia, Leopardo Vermelho de Cinco Caudas, Pequena Fada das Flores);
  - frases repetidas de "como obter" com "Grandes Vila", "grandes chances" e "Matar" no imperativo.
- `handlekey_uibattleimmortal_wanfashuoming2`: 仙法 estava como "Fada"; passou a "Arte Imortal"/"poder imortal" (ch). A tag `<r>` desbalanceada do `en` no item 5 foi mantida para passar no verificador.
- Dúvidas de termo: "Daoístas Miseráveis" (Wretch Taoists) e "Cultivador de Raiz Nula" (Nil Root) merecem confirmação; "Arctic Field" → "Campo Ártico".
