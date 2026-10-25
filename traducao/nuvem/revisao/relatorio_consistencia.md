# Relatório de consistência de termos

Escopo: `out_*.json` e `revisao/out_rev*.json` da branch `traducao-nuvem` (18.831 entradas; a revisão sobrescreve o `out_` da mesma chave). **resto26–30 estão na branch `claude/project-thread-lwgr77`, ainda não mesclada, e não entraram na contagem**; vale refazer a contagem depois do merge. Contagens são ocorrências no `pt` das entradas cujo `en` contém o termo (exceto onde indicado). Nenhuma tradução foi alterada.

## Divergências que pedem unificação

| Termo (en) | Variantes no pt (ocorrências) | Sugestão |
|---|---|---|
| **Everfrost** | Geada Eterna (32), Everfrost (12) | **Geada Eterna** (decisão já adotada nos lotes). Os 12 "Everfrost" ficaram sem traduzir; checar se são nome próprio de lugar fixo em algum lote. |
| **Ground of No Return** | Solo do Não Retorno (11), Terra Sem Volta (5), Terra Sem Retorno (3) | **Solo do Não Retorno**, a mais frequente e a mais literal. |
| **casket** | caixa (16), caixão (11), caixas (5) | Depende do contexto: **caixão** quando é para o morto, **caixa** para recipiente de item. Revisar as 30 entradas caso a caso; "cofre" não ocorre (a memória citava "Árvore-Cofre" só como nome de árvore). |
| **Breath** | Sopro (207 entradas), Respiração (33). Em nomes: Sopro do Caos (8) vs Respiração do Caos (10, ex. "Respiração do Caos Abençoada"); Sopro Condensado (~71) | Glossário fixa `Breath → Sopro`. Unificar **Sopro** (Sopro do Caos). Respiração fica só para o ato de respirar (Breathing). |
| **Wayward Desert** | Deserto Errante (5), Deserto Rebelde (4); "Selvagem" é de outra frase | **Deserto Errante** (alinha com Cultivador Errante). Só 9 entradas; correção barata. |
| **Mindsnap Temple** | Templo Mindsnap (27), Templo Quebra-Mente (22) | **Templo Quebra-Mente**: é tradução, e a memória do projeto já adotou "Templo Quebra-Mente". Conferir se `Mindsnap` é nome próprio em pinyin (então manter e registrar em `excecoes.json`). Decisão do usuário. |
| **Rogue Cultivator** | Cultivador(es) Errante(s) (13), renegado(s) (3) | **Cultivador Errante**. "Renegado" soa como traidor da seita. |
| **Upgrade** | aprimorado/aprimorar/aprimoramento (~75), melhorar/melhora/melhorado (~30), atualização/atualizado (~11), melhoria (1), "Upgrade"/"UpGrade" (3) | **Aprimorar** para melhoria de item/habilidade; **Atualização** só para update do jogo (notas de patch). Corrigir os "Upgrade" soltos. |
| **BOSS** | chefe/Chefe (37), CHEFE (22), BOSS (8) | **Chefe** (CHEFE só onde o inglês está em caixa alta como rótulo de interface). Padronizar os 8 "BOSS". |
| **Iron Shard** | Lasca de Ferro (10), Estilhaço de Ferro (6), Fragmento(s) de Ferro (7) | **Fragmento de Ferro** (item genérico "Shard" no glossário/jogo costuma ser Fragmento). Decisão do usuário entre Lasca e Fragmento. |
| **Interspatial/Interdimensional Ring** | Anel Interespacial (38), Anel Interdimensional (2) | **Anel Interespacial**. |
| **No. 1 (títulos)** | No. 1 (64), Número 1 (48), No 1 (5); `decisoes.md` item 11 manda **Nº 1** e a forma `Nº 1 em <Arma> de <Lugar>`. Ex.: "Cultivador de Lâmina No. 1 de Yong Ning" | Seguir `decisoes.md`: **Nº 1 em Lâmina de Yong Ning**. Hoje nenhuma ocorrência usa "Nº". É a divergência mais ampla (~117 linhas). |
| **Fera sagrada / divina / lendária** | fera sagrada (17), fera lendária (17), fera divina (4), besta divina (2) | Maioria em notas de atualização (en "Legendary Beast" ou "Divine Beast" no patch). Unificar **fera lendária** onde o inglês é Legendary Beast e **fera divina** onde é Divine Beast; "besta" → "fera" (glossário: Beast = Fera). "Fera sagrada" não tem correspondente claro no inglês; confirmar o original chinês. |

## Sem divergência encontrada

- **Leviathan**: só "Leviatan" (43 ocorrências), nunca "Leviatã". Padrão do jogo, não é divergência; se preferir a forma portuguesa, Leviatã seria a única mudança.
- **Mythical Beast**: Fera Mítica, conforme glossário.

## Observações

- Para aplicar as sugestões é preciso nova rodada de revisão; este relatório não alterou nenhuma tradução.
- Termos ainda não verificados: nomes de lugares em pinyin que conviveram com tradução (como Mindsnap/Everfrost). Uma varredura de "palavra inglesa sem traduzir" nos `pt` pode achar outros.
