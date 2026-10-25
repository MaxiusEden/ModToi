# Relatório do piloto (traducao/nuvem/src_piloto.json)

- Modelo: Claude Sonnet 5.5 (`claude-sonnet-5-5`), sessão na nuvem; tradução direta, entrada por entrada, sem scripts de tradução.
- Total: 100 entradas em `out_piloto.json`; marcadores conferidos pelo verificador (nenhum erro de marcador).
- Verificador: 5 ERRO, 2 AVISO. Todos são falsos alarmes:
  - ERRO "idêntico ao inglês": `drama_dialogue2000351` (Bitterfly?), `spriteName2006` (Jiuzhu), `wingmanBaseName1003` (Miemeng), `zhiZhuNv` (Tianxiang) são nomes em pinyin; `modEnum10079EnumName2` ("2-Elixir") segue a regra "Elixir é sempre Elixir". Essas chaves não estão em `iguais_ao_ingles.json`; sugiro incluí-las.
  - AVISO `Earth Qi`/`Wood Qi`: o texto traz só "Condensed Earth/Wood Qi", e usei o nome aprovado mais longo (Qi Condensado da Terra/da Madeira).
- Dúvidas de termo:
  - "Crit DR" (抗暴倍率) usei "RES Crítico"; talvez valha "Resistência ao Multiplicador Crítico".
  - "Land of Beyond" (无极之境) → "Terra do Além"; "Reed" (草精) → "Junco"; "Cloud Cave" → "Caverna das Nuvens".
  - Prefixos de habilidade com `en` em chinês: 瑕 → "Falho", 荒 → "Ermo" (chaves `skillPrefixName5411656`, `skill_name_first37`).
  - "concoct" → "preparar" (Elixir/Fornalha); "Berserk" → "Fúria"; "Clawing" → "Garrada"; "Thrusting" → "Perfurante".
  - "Taoist Devine Tree" (erro de digitação no `en`) → "Árvore Divina Daoísta".
