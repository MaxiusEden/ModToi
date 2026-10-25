# Relatório de unificação de termos (2026-10-25)
649 trocas em 50 arquivos `out_*.json` (grupos A–F; nenhum `src_*`, glossário, código ou `LocalText` tocado). Por termo (grupo: contagem; arquivos):
- **Upgrade → aprimorar, 236:** D 215 (rev06 183, rev03 22, rev10/11/01/04), A 18 (piloto, resto05), E 2, C 1.
- **Nº 1, 158:** E 109 (rev17 nomes 60, rev18 descrições 49), A 33 (falhas3), B 12 (resto16), D 2, C 1, F 1.
- **Boss → Chefe, 51:** C 37 (resto25–30), D 13 (rev03–05), A 1 (falhas1).
- **Mindsnap → Quebra-Mente, 36:** E 29 (rev21/22), F 3 (rev43), A 2, C 2 (resto26).
- **Ground of No Return → Solo do Não Retorno, 30:** C 14, E 8 (rev29/30), A 7 (resto06/07), D 1 (rev02).
- **Rogue Cultivator → Cultivador Errante, 27:** E 20, F 6 (rev47), D 1.
- **Everfrost → Geada Eterna, 23:** E 18 (rev27/31), C 2, B 1 (resto16), D 1, F 1.
- **Fera Lendária/Divina, 22:** C (resto25 19 "fera sagrada", resto26 3 "besta divina").
- **Iron Shard → Fragmento de Ferro, 19:** E 13 (rev27/28), C 6 (resto24).
- **Breath → Sopro, 19:** C 10, E 3, F 3 (uma foi o inverso: "sopro" → "respiração", o `en` é o ato de respirar), D 2, A 1.
- **casket → caixa, 17:** F 12 (rev41), E 5 (rev25); nenhum era morto, são caixas de visões.
- **Wayward Desert → Deserto Errante, 6:** C 4, D 2. **Interspatial Ring, 5:** D 2, E 2, F 1.

Ficou de fora: "Último Suspiro" e respirar literal (Breathing); upgrade de Reino, de save, de mod ou de loja; "rogue" adjetivo, vilão ou sem Cultivator (rev45, resto24); "CHEFE" em rótulo de caixa alta (`mod_guaiwu24`); "Bestas do Submundo" (rev02) e "Leviatan", fora da tabela. "Land of No Return" (不归之境) foi tratado como Ground of No Return (A 7, C 14); "River of No Return" (rev42, rev46) não foi tocado. "Fera sagrada" → "lendária" em resto25 seguiu o `ch` 神兽.
Método: trocas decididas entrada por entrada; exceções por correspondência exata de textos idênticos (183 rótulos `prefixNameS*` em rev06, 22 "Stats/Value to upgrade" em rev03) e os 60 `AppellationTitleName*` do rev17, de um modelo `Nº 1 em <Arma> de <Lugar>`, sem conferência individual.

Verificador: sem ERRO novo em nenhum arquivo editado. Restam só ERRO "idêntico ao inglês" que já existiam (conferido em `df7d266`): falhas1 33, falhas2 14, falhas3 32, piloto 2, resto12 3, rev26 2; são nomes em pinyin, a deixar segundo o LEIA.md. AVISOs esperados: "nome dado não usado: Breath → Respiração" (`resto09` item_desc7000174, `rev38` 860301/860321, o `nomes` do `src` ainda traz "Respiração") e "Cultivator → Cultivador" nos 12 títulos Nº 1 de resto16.
Achado fora de escopo: `out_rev40` `drama_dialogue970094` não corresponde ao `en` ("Taking a deep breath, you aim at the bird…").
