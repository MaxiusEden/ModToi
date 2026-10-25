# Decisões de tradução PT-BR

Estado: **aprovado em 2026-10-22**, com ajustes: fases Inicial/Médio/Final e glossário em dois níveis. A largura das abreviações de atributo ainda será conferida no piloto.

## Arquivos

| Arquivo | Conteúdo |
|---|---|
| `obrigatorios.json` | Termos próprios do jogo (reinos, técnicas, seitas, atributos, destinos, cultivo). O `ptbr.py validate` marca `revisar` quando o termo aparece na fonte e a tradução aprovada não aparece no `pt`. |
| `sugestoes.json` | Palavras genéricas (Master, Friend, Girl, Power, Path…). Vão para o modelo, mas não disparam `revisar`. |
| `glossario.json` | **Gerado** por `ptbr.py glossario` (junta os dois níveis); é o `glossary_path` do `omni_translate`. Não edite à mão. |
| `excecoes.json` | Nomes próprios (lista de strings) que o validador não confere por dentro. Ex.: "Tao Tie" (饕餮) contém "Tao", mas fica em pinyin. Termos fora do nome continuam sendo conferidos. |
| `candidatos.tsv` | Candidatos gerados por `tools/ptbr/glossario_candidatos.py`. |

Chaves em chinês (ex.: `暴击倍率`) são conferidas contra o `ch`; as demais, contra o `en`, com maiúsculas e minúsculas exatas e palavra inteira. Vale a correspondência mais longa, contando também as sugestões: em "Taoist Mind" só se exige "Coração do Dao". No `en`, o termo também casa no plural (-s/-es). No `pt`, a busca ignora acento e maiúscula e aceita a tradução no singular ou no plural: as palavras antes do primeiro conectivo variam em número (al↔ais, el↔eis, ão↔ões, m↔ns, -s), então "Pedra Espiritual" aceita "Pedras Espirituais" e "Artes Marciais" aceita "Arte Marcial". Por isso, termo obrigatório que muda de gênero (como "Crit": Crítico/Crítica) fica nas sugestões.

## Regras gerais

1. **Fonte:** o `en` do kit oficial. O `ch` é referência quando o `en` está vazio ou ambíguo.
2. **Marcadores intactos:** `{…}`, `&…&`, `$…$`, `%s`, as tags `<…>` e as quebras de linha ficam iguais e na mesma quantidade (`ptbr.py validate`). A ordem pode mudar para a frase soar natural.
3. **Nomes próprios em pinyin, como no `en`:** pessoas (Zhang San, Feng Pingmo), regiões (Hua Feng, Mu Xian, Yun Mo, Yong Ning, Chi You), cidades e sílabas de seita (Tian Yuan, Yao Chenzun). Só a parte descritiva é traduzida: `Tian Yuan Mountain` → `Montanha Tian Yuan`, `X Sect` → `Seita X`, `X Pavilion` → `Pavilhão X`.
   - As sílabas que o `en` traduziu por sentido dentro de nomes de seita (Heavenly, Dragon, Golden…) seguem o grupo "Seitas, lugares e títulos" e os adjetivos de "Interface e jogo".
   - Exceções conhecidas de "Tao": em nome próprio (sílaba de seita 涛, a criatura "Tao Tie" 饕餮) fica em pinyin. Esses nomes vão para `excecoes.json`, para o validador não marcar `revisar`. Nos outros casos, `Tao` → `Dao`.
4. **Tratamento:** "você" por padrão. "O senhor / a senhora" quando o `en` marca respeito ou hierarquia (Senior, Elder, Master, My Lord, Venerable).
   - Discípulos de uma mesma seita: `Irmão/Irmã Marcial Sênior/Júnior`.
   - Ao falar de si mesmo com humildade ("this old man", "this junior"): "este velho", "este júnior".
   - Gênero: usar o do `en`. Se for indefinido e se referir ao jogador, preferir construção neutra ("quem cultiva", "você").
5. **Termos de cultivo:** `Qi` e `Dao` sem tradução, e `Taoist` → `Daoísta` (com acento desde 2026-10-25). `Elixir` (丹药) é sempre Elixir; `Pill` só quando o `en` diz Pill.
6. **Reinos:** traduzidos, com a fase depois do nome (`Golden Core Late` → `Núcleo Dourado Final`). Fases: Early = Inicial, Middle = Médio, Late = Final. As combinações reino + fase estão no glossário uma a uma, para que "late" solto numa frase comum não vire "Final". Os numerais romanos ficam: `Golden Core (IV)` → `Núcleo Dourado (IV)`.
7. **Técnicas, artefatos, destinos e traços:** sempre traduzidos, com Iniciais Maiúsculas como no `en`. Termos culturais ficam: Bagua, Guqin, Feng Shui, Dantian.
8. **Elementos:** Earth, Lightning, Water, Fire, Wood e Wind viram Terra, Relâmpago, Água, Fogo, Madeira e Vento, inclusive em compostos (`Lightning Root` → `Raiz de Relâmpago`).
   - "Raio" fica reservado para alcance (`Skill Radius` → `Raio da Habilidade`).
9. **Atributos na interface:** usar abreviações curtas onde o `en` abrevia (ATK → ATQ; DEF e RES ficam; Max X → X Máx.), porque o espaço na UI é limitado. A largura será conferida no piloto.
10. **Crítico:** o `en` usa "Crit Rate" para duas coisas diferentes, então a tradução sai do `ch`:
    - 暴击倍率 e 会心倍率 (multiplicador; tokens `_bjbl`, `_critV`) → **Multiplicador Crítico**;
    - 暴击率 e 暴击概率 (chance em %; tokens `_bjgl`, `_critUp`) → **Chance Crítica**;
    - `Crit Chance` → Chance Crítica.
11. **Títulos de ranking:** `<Lugar> No. 1 <Arma/Elemento> Cultivator` → `Nº 1 em <Arma/Elemento> de <Lugar>`. Exemplo: `Hua Feng No. 1 Sword Cultivator` → `Nº 1 em Espada de Hua Feng`. Os títulos `<X> Deity` usam `Divindade <de X>`.
12. **Possessivo inglês:** `Sect's X` → `X da Seita`.

## Decisões pontuais (desvios do `en` ou ambiguidades)

| Termo | Decisão | Motivo |
|---|---|---|
| Taoist Mind (道心) | Coração do Dao | É a forma consagrada de 道心; "mente taoista" soa estranho. |
| Crit Rate | Pelo `ch` (regra 10) | Conferido em 2026-10-22: das 60 entradas com "Crit Rate", 39 são multiplicador (暴击倍率 35, 会心倍率 4) e 20 são chance em % (暴击率 12, 暴击概率 8). Ex.: `BattleEffectDesc515103` "Every stack gives Crit rate +&515101_bjgl&%" = 暴击率. |
| Breath (气息, tipo de item) | Sopro | Item consumível; "respiração" não serve. |
| Map (图纸, tipo de item) | Mapa | Segue o `en` do jogo, embora o `ch` seja "planta/projeto". |
| Nether (冥) | Submundo | É usado como prefixo em dezenas de nomes (Nether Mountains → Montanhas do Submundo). |
| Fairy (honorífico) | Fada | Tratamento para cultivadoras; `Flower Faerie` (monstro) → Fada das Flores. |
| Holy (天机, destino) | Sagrado | Segue o `en` do jogo. |
| Lifespan (寿命) | Longevidade | É mais curto que "expectativa de vida" para a UI. |
| Array / Formation (阵) | Formação | 阵 = formação, como nas novels traduzidas. "Grande Formação" deixava nomes como "Stone Array" longos demais para o mapa (alterado depois do piloto). |
| Tao (道) | Dao | No `en`, "Tao" é 道 em 265 de 270 ocorrências; "Dao" nunca aparece. |
| Imp (壶妖) | Diabrete | O piloto misturou Imp, Diabinho e Diabrete; padronizado em 2026-10-23. |
| Skill Stat (战斗资质) | Aptidão Marcial | É a aptidão de combate (Blade, Spear…), não um atributo da habilidade. O modelo escrevia "Atributo de Habilidade", que é longo e errado. |
| `school_name_first*` / `school_name_second*` | não traduzir | O `en` oficial está em chinês (sílabas de nome de seita); com o jogo em inglês valem `schoolNameFirst/Second`. Traduzir o sentido ("Luz", "Céu") quebraria a regra do pinyin. |
| Branch (分舵) | Filial | `Branch Leader` → Líder de Filial. |
| Taoist Title (道号) | Título Daoísta | Decidido em 2026-10-24. No jogo é um título que se equipa (`AppellationTitle`), e "Nome Daoista" soava como nome próprio. O "í" existe em todas as fontes do jogo menos na `cutiHanwen`, que não tem nenhum acento. Em 2026-10-25 os outros termos (`Taoist`, `Daoist`, `Fellow Daoist`) também passaram a "Daoísta", com acento, e as traduções já feitas foram corrigidas. |
| Unificação de 2026-10-25 (relatório de consistência da nuvem) | obrigatórios novos | Iron Shard → Fragmento de Ferro; Mindsnap (Temple) → (Templo) Quebra-Mente; Everfrost → Geada Eterna; Ground of No Return → Solo do Não Retorno; Wayward Desert → Deserto Errante; Rogue Cultivator → Cultivador Errante ("renegado" soa como traidor); Interspatial Ring → Anel Interespacial; Legendary Beast → Fera Lendária; Divine Beast → Fera Divina; Boss/BOSS → Chefe (CHEFE só onde o `en` é rótulo em caixa alta). |
| `No. 1` em títulos (regra 11) | Nº 1 | Decidido em 2026-10-25: "Nº 1 em <Arma> de <Lugar>". O "º" e o "ª" foram liberados no validador e na ponte: `hanwen`, `heiti`, `LiberationSans` e `cutiHanwen` têm os dois; `cuti` e `HYShangWeiShouShuW` não, mas também não têm "ç" e "ã". |
| casket | pelo contexto | "caixão" quando guarda um morto; "caixa" quando é recipiente de item. |
| Upgrade | pelo contexto | "aprimorar/aprimoramento" para item, habilidade e construção; "atualização" só para atualização do jogo (notas de patch). |

## Interface apertada (teste do piloto em 2026-10-23)

- **Abas laterais do personagem** (cabem ~7 letras; o `en` era Stats/Skills/Item/Experience/Relationship): Status, Habil., Itens, Diário, Laços.
- **`common_shouming` (Lifespan, "40/770")** → **Idade**. "Longevidade" empurrava o ícone para baixo da barra lateral. A chave ganhou exceção de glossário em `excecoes.json` (`por_chave`).
- **`abreviacoes.json`:** formas curtas que o validador aceita no lugar da forma completa (Habilidade → Habil., Técnica → Téc.). O modelo continua recebendo a forma completa pelo glossário; a abreviação é escolhida na revisão de largura.
- **Limites de largura:** `traducao/limites.json` fixa o máximo de caracteres por chave (abas = 7). Sem limite fixo, um texto de UI de até 20 caracteres, sem quebra de linha e com `en` latino, não pode passar de `en + 3` nem de 130% do `en`. O `validate` marca `revisar: largura: …`.
- **Botões curtos:** OK fica OK, Male/Female → Homem/Mulher, Middle Way (中庸) → Neutro (par de Justo/Demoníaco). Para jogo salvo usar "Save" em rótulos curtos ("Último Save").

## Glossário por grupo (408 termos: 252 obrigatórios, 156 sugestões)

### Reinos (42)

| en / ch | PT-BR | nível | freq | ch |
|---|---|---|---|---|
| Qi Refining | Refinamento de Qi | obrigatório | 90 | 炼气 |
| Foundation | Fundação | obrigatório | 148 | 筑基 |
| Qi Condensation | Condensação de Qi | obrigatório | 174 | 结晶 |
| Golden Core | Núcleo Dourado | obrigatório | 185 | 金丹 |
| Origin Spirit | Espírito de Origem | obrigatório | 145 | 具灵 |
| Nascent Soul | Alma Nascente | obrigatório | 141 | 元婴 |
| Soul Formation | Formação da Alma | obrigatório | 126 | 化神 |
| Enlightenment | Iluminação | obrigatório | 131 | 悟道 |
| Reborn | Renascimento | obrigatório | 141 | 羽化 |
| Transcendent | Transcendência | obrigatório | 185 | 登仙 |
| Qi Refining Early | Refinamento de Qi Inicial | obrigatório | 2 | 炼气前期 |
| Qi Refining Middle | Refinamento de Qi Médio | obrigatório | 6 | 炼气中期 |
| Qi Refining Late | Refinamento de Qi Final | obrigatório | 8 | 炼气后期 |
| Foundation Early | Fundação Inicial | obrigatório | 3 | 筑基前期 |
| Foundation Middle | Fundação Médio | obrigatório | 6 | 筑基中期 |
| Foundation Late | Fundação Final | obrigatório | 6 | 筑基后期 |
| Qi Condensation Early | Condensação de Qi Inicial | obrigatório | 2 | 结晶前期 |
| Qi Condensation Middle | Condensação de Qi Médio | obrigatório | 6 | 结晶中期 |
| Qi Condensation Late | Condensação de Qi Final | obrigatório | 6 | 结晶后期 |
| Golden Core Early | Núcleo Dourado Inicial | obrigatório | 2 | 金丹前期 |
| Golden Core Middle | Núcleo Dourado Médio | obrigatório | 6 | 金丹中期 |
| Golden Core Late | Núcleo Dourado Final | obrigatório | 6 | 金丹后期 |
| Origin Spirit Early | Espírito de Origem Inicial | obrigatório | 2 | 具灵前期 |
| Origin Spirit Middle | Espírito de Origem Médio | obrigatório | 6 | 具灵中期 |
| Origin Spirit Late | Espírito de Origem Final | obrigatório | 6 | 具灵后期 |
| Nascent Soul Early | Alma Nascente Inicial | obrigatório | 2 | 元婴前期 |
| Nascent Soul Middle | Alma Nascente Médio | obrigatório | 6 | 元婴中期 |
| Nascent Soul Late | Alma Nascente Final | obrigatório | 6 | 元婴后期 |
| Soul Formation Early | Formação da Alma Inicial | obrigatório | 2 | 化神前期 |
| Soul Formation Middle | Formação da Alma Médio | obrigatório | 6 | 化神中期 |
| Soul Formation Late | Formação da Alma Final | obrigatório | 6 | 化神后期 |
| Enlightenment Early | Iluminação Inicial | obrigatório | 2 | 悟道前期 |
| Enlightenment Middle | Iluminação Médio | obrigatório | 6 | 悟道中期 |
| Enlightenment Late | Iluminação Final | obrigatório | 5 | 悟道后期 |
| Reborn Early | Renascimento Inicial | obrigatório | 7 | 羽化前期 |
| Reborn Middle | Renascimento Médio | obrigatório | 10 | 羽化中期 |
| Reborn Late | Renascimento Final | obrigatório | 17 | 羽化后期 |
| Transcendent Early | Transcendência Inicial | obrigatório | 6 | 登仙前期 |
| Transcendent Middle | Transcendência Médio | obrigatório | 8 | 登仙中期 |
| Transcendent Late | Transcendência Final | obrigatório | 6 | 登仙后期 |
| Realm | Reino | obrigatório | 557 | 境界 |
| Realms | Reinos | sugestão | 104 |  |

### Tratamento (48)

| en / ch | PT-BR | nível | freq | ch |
|---|---|---|---|---|
| Master | Mestre | sugestão | 526 | 拜为师父 |
| Grandmaster | Grão-Mestre | sugestão | 1 |  |
| Young Master | Jovem Mestre | sugestão | 9 |  |
| Sect Master | Mestre da Seita | obrigatório | 1 |  |
| Sect Leader | Líder da Seita | obrigatório | 116 | 宗主 |
| Leader | Líder | sugestão | 165 | 首领 |
| Elder | Ancião | obrigatório | 250 | 长老 |
| Grand Elder | Grão-Ancião | obrigatório | 39 | 大长老 |
| Ancestor | Ancestral | sugestão | 40 |  |
| Patriarch | Patriarca | sugestão | — |  |
| Venerable | Venerável | sugestão | 10 |  |
| Immortal | Imortal | sugestão | 403 | 不朽 |
| Disciple | Discípulo | sugestão | 371 | 内门弟子 |
| Disciples | Discípulos | sugestão | 516 |  |
| Apprentice | Aprendiz | sugestão | 26 | 徒弟 |
| Teacher | Professor | sugestão | 2 |  |
| Senior | Sênior | sugestão | 110 |  |
| Junior | Júnior | sugestão | 24 |  |
| Senior Brother | Irmão Marcial Sênior | obrigatório | 1 |  |
| Senior Sister | Irmã Marcial Sênior | obrigatório | — |  |
| Junior Brother | Irmão Marcial Júnior | obrigatório | 1 |  |
| Junior Sister | Irmã Marcial Júnior | obrigatório | — |  |
| Brother | Irmão | sugestão | 39 |  |
| Sister | Irmã | sugestão | 27 |  |
| Fellow Cultivator | Colega Cultivador | obrigatório | 11 |  |
| Fellow Daoist | Colega Daoísta | obrigatório | — |  |
| Daoist | Daoísta | obrigatório | 1 |  |
| Taoist | Daoísta | obrigatório | 674 |  |
| Friend | Amigo | sugestão | 203 | 好友 |
| My Friend | Meu amigo | sugestão | 110 |  |
| Little Friend | Pequeno amigo | sugestão | 9 |  |
| Young Friend | Jovem amigo | sugestão | 3 |  |
| Lord | Senhor | sugestão | 150 |  |
| My Lord | Meu senhor | sugestão | 36 |  |
| Madam | Senhora | sugestão | 10 |  |
| Miss | Senhorita | sugestão | 17 |  |
| Young Lady | Jovem senhorita | sugestão | 2 |  |
| Fairy | Fada | sugestão | 14 |  |
| Benefactor | Benfeitor | sugestão | 18 |  |
| Old Man | Velho | sugestão | 135 | 老者 |
| Young Man | Rapaz | sugestão | 18 |  |
| Girl | Garota | sugestão | 150 |  |
| Kid | Garoto | sugestão | 39 |  |
| Brat | Moleque | sugestão | 36 |  |
| Grandpa | Vovô | sugestão | 30 |  |
| Husband | Marido | sugestão | 8 | 丈夫 |
| Wife | Esposa | sugestão | 24 | 妻子 |
| Dao Companion | Companheiro(a) do Dao | sugestão | — |  |

### Cultivo (43)

| en / ch | PT-BR | nível | freq | ch |
|---|---|---|---|---|
| Qi | Qi | obrigatório | 1144 | 灵气 |
| Tao | Dao | obrigatório | 125 | 涛 |
| Cultivator | Cultivador | obrigatório | 1078 | 修仙者 |
| Cultivators | Cultivadores | obrigatório | 650 |  |
| Cultivation | Cultivo | obrigatório | 439 | 修炼 |
| Breakthrough | Avanço | obrigatório | 567 | 突破 |
| Bottleneck | Gargalo | obrigatório | 17 | 陷入瓶颈 |
| Tribulation | Tribulação | obrigatório | 5 |  |
| Meridians | Meridianos | obrigatório | 26 |  |
| Dantian | Dantian | obrigatório | 1 |  |
| Spiritual Root | Raiz Espiritual | obrigatório | 54 | 灵根资质 |
| All Spiritual Roots | Todas as Raízes Espirituais | obrigatório | 19 | 所有灵根 |
| Lifespan | Longevidade | obrigatório | 105 | 寿命 |
| Mortal | Mortal | sugestão | 157 |  |
| Demon | Demônio | sugestão | 182 |  |
| Demons | Demônios | sugestão | 117 |  |
| Demonic | Demoníaco | sugestão | 204 | 魔道 |
| Righteous | Justo | sugestão | 160 | 正 |
| Evil | Maligno | sugestão | 149 | 邪恶 |
| Spirit Stone | Pedra Espiritual | obrigatório | 46 | 灵石 |
| Spirit Stones | Pedras Espirituais | obrigatório | 432 |  |
| Taoist Mind | Coração do Dao | obrigatório | 195 | 道心 |
| Taoist Title | Título Daoísta | obrigatório (era "Nome Daoista" até 2026-10-24) | 400 | 道号 |
| Taoist Fruit | Fruto do Dao | obrigatório (acrescentado no piloto) | — | — |
| A Taoist Title | Um título daoísta | obrigatório (era "Um nome daoista" até 2026-10-24) | 97 |  |
| Tao Power | Poder do Dao | obrigatório | 25 | 道力 |
| Destiny | Destino | obrigatório | 239 | 气运 |
| Rewrite Destiny | Reescrever o Destino | obrigatório | 128 | 逆天改命 |
| Condensed Qi | Qi Condensado | obrigatório | 81 | 灵斗气 |
| Artifact Spirit | Espírito do Artefato | obrigatório | 649 | 器灵 |
| Heaven and Earth | Céu e Terra | sugestão | 126 |  |
| Heavenly Power | Poder Celestial | sugestão | 99 |  |
| Ethereal Power | Poder Etéreo | sugestão | 77 |  |
| Alchemy | Alquimia | obrigatório | 153 | 炼丹 |
| Forge | Forja | obrigatório | 73 | 炼器 |
| Geomancy | Geomancia | obrigatório | 133 | 堪舆 |
| Feng Shui | Feng Shui | obrigatório | 143 | 风水 |
| Formation | Formação | obrigatório | 144 |  |
| Array | Formação | obrigatório | 76 | 大阵 |
| Field | Domínio | sugestão | 76 | 领域 |
| Chaos | Caos | sugestão | 79 | 洪荒 |
| Adventure | Aventura | sugestão | 97 | 奇遇 |
| Affinity | Afinidade | obrigatório | 91 | 好感度 |
| Contribution | Contribuição | obrigatório | 74 | 贡献 |

### Técnicas (37)

| en / ch | PT-BR | nível | freq | ch |
|---|---|---|---|---|
| Skill | Habilidade | obrigatório | 904 |  |
| Skills | Habilidades | obrigatório | 249 | 技能 |
| Skill Damage | Dano da Habilidade | sugestão | 188 |  |
| Martial Arts | Artes Marciais | obrigatório | 258 | 功法 |
| Martial Skill | Técnica Marcial | obrigatório | 132 | 武技 |
| Mind Skill | Técnica Mental | obrigatório | 135 | 心法 |
| Spiritual Skill | Técnica Espiritual | obrigatório | 149 | 灵技 |
| Motion Skill | Técnica de Movimento | obrigatório | 84 | 身法 |
| Ultimate Skill | Técnica Suprema | obrigatório | 167 | 神通 |
| Special Skill | Técnica Especial | obrigatório | 242 | 绝技 |
| Heavenly Art | Arte Imortal | obrigatório | 155 | 仙法 |
| Secret Technique | Técnica Secreta | obrigatório | 9 | 秘法 |
| Manual | Manual | sugestão | 218 | 秘籍 |
| Manuals | Manuais | sugestão | 159 |  |
| Sky Double Pupils | Pupilas Duplas do Céu | obrigatório | 21 | 青空重瞳 |
| Transformation | Transformação | obrigatório | 21 | 形态转换 |
| Boomerang Finger | Dedo Bumerangue | obrigatório | 17 | 轮回指 |
| Invigoration | Revigoramento | obrigatório | 15 | 鼓舞 |
| Water Dragon Cauldron | Caldeirão do Dragão de Água | obrigatório | 15 | 溟霖宝鼎 |
| Dragon Pond | Lago do Dragão | obrigatório | 12 | 龙渊 |
| Lightning Cloud | Nuvem de Relâmpagos | obrigatório | 12 | 雷云术 |
| Wind Blade | Lâmina de Vento | obrigatório | 12 | 地煞风刃 |
| Qi Triplication | Triplicação de Qi | obrigatório | 11 | 三清秘术 |
| Spiritual Torrent | Torrente Espiritual | obrigatório | 11 | 灵流术 |
| Purple Lightning | Relâmpago Púrpura | obrigatório | 10 | 紫电闪 |
| Spiritual Lightning | Relâmpago Espiritual | obrigatório | 10 | 灵雷术 |
| Blue Bird Blast | Explosão do Pássaro Azul | obrigatório | 9 | 青鸾冲击 |
| Sinful Flame | Chama do Pecado | obrigatório | 9 | 业火术 |
| Icy Aura | Aura Gélida | obrigatório | 8 | 冰气术 |
| Lightning Retribution | Retribuição do Relâmpago | obrigatório | 8 | 天雷劫 |
| Poisonous Marsh | Pântano Venenoso | obrigatório | 8 | 毒沼术 |
| Quicksand | Areia Movediça | obrigatório | 8 | 流沙术 |
| Shadowless Sword | Espada Sem Sombra | obrigatório | 8 | 绝影剑 |
| Ironbody Fist | Punho de Corpo de Ferro | obrigatório | 7 | 金刚拳 |
| Sword Flurry | Rajada de Espadas | obrigatório | 7 | 万剑 |
| Titan Fist | Punho de Titã | obrigatório | 7 | 巨王拳 |
| Wild Wind | Vento Selvagem | obrigatório | 7 | 狂风术 |

### Essências e elementos (44)

| en / ch | PT-BR | nível | freq | ch |
|---|---|---|---|---|
| Earth | Terra | obrigatório | 347 | 土 |
| Lightning | Relâmpago | obrigatório | 277 | 雷 |
| Water | Água | obrigatório | 209 | 水 |
| Fire | Fogo | obrigatório | 200 | 火 |
| Wood | Madeira | obrigatório | 156 | 木 |
| Wind | Vento | obrigatório | 247 | 风 |
| Ice | Gelo | sugestão | 83 |  |
| Water Root | Raiz de Água | obrigatório | 74 | 水灵根 |
| Fire Root | Raiz de Fogo | obrigatório | 49 | 火灵根 |
| Earth Root | Raiz de Terra | obrigatório | 46 | 土灵根 |
| Lightning Root | Raiz de Relâmpago | obrigatório | 43 | 雷灵根 |
| Wind Root | Raiz de Vento | obrigatório | 42 | 风灵根 |
| Wood Root | Raiz de Madeira | obrigatório | 42 | 木灵根 |
| Sword | Espada | obrigatório | 275 | 剑法资质 |
| Blade | Lâmina | obrigatório | 110 | 刀法资质 |
| Spear | Lança | obrigatório | 107 | 枪法资质 |
| Fist | Punho | obrigatório | 96 | 拳法资质 |
| Palm | Palma | obrigatório | 115 | 掌法资质 |
| Finger | Dedo | obrigatório | 107 | 指法资质 |
| Shield | Escudo | sugestão | 120 | 护盾 |
| Charge | Carga | obrigatório | 65 | 蓄力 |
| Perfect Charge | Carga Perfeita | obrigatório | 11 | 完美蓄力 |
| Dragon Vein | Veia do Dragão | obrigatório | 61 | 龙脉 |
| Dragon Blood | Sangue de Dragão | sugestão | 19 | 龙血 |
| Dragon Soul | Alma do Dragão | sugestão | 9 | 龙魂 |
| Shattered | Despedaçado | sugestão | 39 | 击破 |
| Poison Damage | Dano de Veneno | obrigatório | 34 | 毒伤害 |
| Internal Injury | Ferimento Interno | obrigatório | 21 | 内伤 |
| Freeze | Congelamento | obrigatório | 17 | 冻结 |
| Bleed | Sangramento | obrigatório | 7 | 流血 |
| Fiery Flame | Chama Ardente | sugestão | 19 | 烈焰 |
| Phantom | Fantasma | sugestão | 16 | 幻影 |
| Palm Wind | Vento da Palma | sugestão | 15 | 掌风 |
| Arctic | Ártico | sugestão | 13 | 极寒 |
| Poisonous Thorn | Espinho Venenoso | sugestão | 13 | 毒刺 |
| Whirlwind | Redemoinho | sugestão | 7 | 旋风 |
| Earth Golem | Golem de Terra | obrigatório | 24 | 土傀儡 |
| Flower Faerie | Fada das Flores | obrigatório | 36 | 花妖 |
| Essence | Essência | sugestão | 119 | 精华 |
| Aura | Aura | sugestão | 82 |  |
| Soul | Alma | sugestão | 731 | 魂 |
| Souls | Almas | sugestão | 68 | 元魂 |
| Spirit | Espírito | sugestão | 1491 | 魄 |
| Blood | Sangue | sugestão | 205 |  |

### Atributos (46)

| en / ch | PT-BR | nível | freq | ch |
|---|---|---|---|---|
| Energy | Energia | obrigatório | 791 | 灵力 |
| Max Energy | Energia Máx. | obrigatório | 97 | 最大灵力 |
| Energy Recovery | Recuperação de Energia | obrigatório | 14 | 灵力恢复 |
| Energy Cost | Custo de Energia | obrigatório | 111 |  |
| Vitality | Vitalidade | obrigatório | 757 | 体力 |
| Max Vitality | Vitalidade Máx. | obrigatório | 293 | 最大体力 |
| Vitality Restoration | Recuperação de Vitalidade | obrigatório | 23 | 体力回复 |
| Focus | Foco | obrigatório | 266 | 念力 |
| Max Focus | Foco Máx. | obrigatório | 74 | 念力上限 |
| Stamina | Vigor | obrigatório | 76 | 精力 |
| Max Stamina | Vigor Máx. | obrigatório | 26 | 精力上限 |
| Health | Saúde | obrigatório | 41 | 健康 |
| Max Health | Saúde Máx. | obrigatório | 13 | 健康上限 |
| Mood | Humor | obrigatório | 53 | 心情 |
| Max Mood | Humor Máx. | obrigatório | 14 | 心情上限 |
| Agility | Agilidade | obrigatório | 209 | 移动速度 |
| ATK | ATQ | obrigatório | 176 | 攻击 |
| DEF | DEF | obrigatório | 172 | 防御 |
| RES | RES | obrigatório | 105 |  |
| Insight | Percepção | obrigatório | 157 | 悟性 |
| Crit | Crítico | sugestão (o gênero varia: Chance Crítica) | 132 | 暴击 |
| Crit Rate / Crit rate | Multiplicador Crítico (se ch=暴击倍率) ou Chance Crítica (se ch=暴击率/暴击概率) | sugestão (dica ao modelo; quem decide é o termo chinês) | 60 | — |
| Crit RES | RES Crítico | obrigatório | 83 | 护心 |
| Crit Chance | Chance Crítica | obrigatório | 35 | 暴击概率 |
| 暴击倍率 | Multiplicador Crítico | obrigatório | — |  |
| 会心倍率 | Multiplicador Crítico | obrigatório | — |  |
| 暴击率 | Chance Crítica | obrigatório | — |  |
| 暴击概率 | Chance Crítica | obrigatório | — |  |
| EXP | EXP | obrigatório | 76 | 经验 |
| Charisma | Carisma | obrigatório | 74 | 魅力 |
| Luck | Sorte | obrigatório | 44 | 幸运 |
| Reputation | Reputação | obrigatório | 44 | 声望 |
| Attack Speed | Velocidade de Ataque | obrigatório | 48 | 攻击速度 |
| Travel Speed | Velocidade de Viagem | obrigatório | 45 | 脚力 |
| Dodge | Esquiva | obrigatório | 19 | 闪避 |
| Resistance | Resistência | sugestão | 18 | 抗性 |
| Damage | Dano | sugestão | 309 | 伤害统计 |
| Damage Bonus | Bônus de Dano | obrigatório | 15 | 伤害加成 |
| Skill Duration | Duração da Habilidade | obrigatório | 16 | 技能持续时间: |
| Skill Radius | Raio da Habilidade | obrigatório | 14 | 技能范围: |
| Projectile Range | Alcance do Projétil | obrigatório | 9 | 弹道射程: |
| Leech Chance | Chance de Roubo de Vida | obrigatório | 8 | 吸血概率 |
| Power | Poder | sugestão | 303 | 实力 |
| Cost | Custo | sugestão | 317 | 技能消耗 |
| Stats | Atributos | sugestão | 70 | 属性 |
| Points | Pontos | sugestão | 65 | 积分 |
| Talismans | Talismãs | sugestão | 21 | 画符 |

### Itens (54)

| en / ch | PT-BR | nível | freq | ch |
|---|---|---|---|---|
| Elixir | Elixir | obrigatório | 1847 | 丹药 |
| Elixirs | Elixires | obrigatório | 314 |  |
| Elixir Recipe | Receita de Elixir | obrigatório | 317 | 丹方 |
| Recipe | Receita | sugestão | 327 |  |
| Pill | Pílula | obrigatório | 106 |  |
| Prescription | Prescrição | obrigatório | 249 | 药谱 |
| Furnace | Fornalha | obrigatório | 225 | 丹炉 |
| Cauldron | Caldeirão | sugestão | 78 |  |
| Artifact | Artefato | obrigatório | 770 | 法宝 |
| Treasure | Tesouro | sugestão | 342 | 宝箱 |
| Heaven and Earth Treasure | Tesouro do Céu e da Terra | obrigatório | 110 | 天材地宝 |
| Herb | Erva | sugestão | 209 | 药材 |
| Herbs | Ervas | sugestão | 178 |  |
| Spiritual Fruit | Fruto Espiritual | obrigatório | 199 | 灵果 |
| Skill Fruit | Fruto de Habilidade | obrigatório | 10 | 道心果 |
| Fruit | Fruto | sugestão | 386 |  |
| Seed | Semente | sugestão | 164 | 种子 |
| Nether Seed | Semente do Submundo | obrigatório | 68 | 冥种 |
| Charm | Talismã | obrigatório | 127 | 符箓 |
| Talisman | Talismã | obrigatório | 116 |  |
| Teleportation Charm | Talismã de Teletransporte | obrigatório | 19 | 传送符 |
| Breath | Sopro | obrigatório | 307 | 气息 |
| Flame | Chama | sugestão | 109 | 火焰 |
| Ore | Minério | sugestão | 95 | 矿材 |
| Material | Material | sugestão | 34 | 材料 |
| Ring | Anel | sugestão | 92 | 戒指 |
| Map | Mapa | sugestão | 86 | 图纸 |
| Chart | Diagrama | sugestão | 30 | 图谱 |
| Mythical Beast Map | Mapa das Feras Míticas | obrigatório | 22 | 五岳真形图 |
| Poison | Veneno | sugestão | 62 | 毒丹 |
| poison | veneno | obrigatório (2026-10-23: modelos cohere trocaram por "poção") | — | 毒 |
| Lamp | Lâmpada | sugestão | 48 | 灯盏 |
| Letter | Carta | sugestão | 45 | 书信 |
| Recommendation Letter | Carta de Recomendação | obrigatório | 8 | 推荐信 |
| Book | Livro | sugestão | 31 | 书 |
| Books | Livros | sugestão | 10 | 书籍 |
| Textbook | Livro de Estudo | sugestão | 25 | 学习类书籍 |
| Mount | Montaria | sugestão | 16 | 坐骑 |
| Orb | Orbe | sugestão | 230 |  |
| Qi Orb | Orbe de Qi | obrigatório | 148 |  |
| Soul Orb | Orbe de Alma | obrigatório | 16 | 魂珠 |
| Jade | Jade | sugestão | 162 |  |
| Enigmatic Stone | Pedra Enigmática | obrigatório | 90 |  |
| Enigmatic Stones | Pedras Enigmáticas | obrigatório | 85 |  |
| Shards | Fragmentos | sugestão | 132 |  |
| Feather | Pena | sugestão | 108 |  |
| Gourd | Cabaça | sugestão | 98 |  |
| Flute | Flauta | sugestão | 12 | 笛 |
| Guqin | Guqin | obrigatório | 10 | 琴 |
| Relocation Flag | Bandeira de Realocação | obrigatório | 8 | 定岳旗 |
| Fortune Stick | Varinha da Sorte | obrigatório | 7 | 灵签 |
| Item | Item | sugestão | 39 | 道具 |
| Other | Outros | sugestão | 29 | 其它 |
| Special | Especial | sugestão | 282 | 特殊 |
| Incense | Incenso | sugestão | 87 |  |

### Destinos e traços (24)

| en / ch | PT-BR | nível | freq | ch |
|---|---|---|---|---|
| Bagua Jade | Jade Bagua | obrigatório | 52 | 双鱼玉佩 |
| Soul Lamp | Lâmpada da Alma | obrigatório | 47 | 玲珑魂灯 |
| Body Reconstruction | Reconstrução do Corpo | obrigatório | 46 | 重塑肉身 |
| Holy | Sagrado | obrigatório | 44 | 天机 |
| Vein-reversing Elixir | Elixir de Inversão de Veias | obrigatório | 44 | 逆脉丹 |
| Holy Spring | Fonte Sagrada | obrigatório | 42 | 偶遇灵泉 |
| Last Breath | Último Suspiro | obrigatório | 35 | 一丝元魂 |
| Following Bug | Grudento | obrigatório | 31 | 跟屁虫 |
| Three Prodigals | Três Gerações Pródigas | obrigatório | 31 | 百年穷三代 |
| Double Pupils | Pupilas Duplas | obrigatório | 22 | 重瞳之力 |
| Heavenly Blessing | Bênção Celestial | obrigatório | 21 | 仙运加身 |
| Water Qi Orb | Orbe de Qi da Água | obrigatório | 21 | 水灵珠 |
| Fortune | Fortuna | obrigatório | 20 | 福生 |
| Heavily Wounded | Gravemente Ferido | obrigatório | 19 | 重伤 |
| Treasure Hunter | Caçador de Tesouros | obrigatório | 16 | 寻珍觅宝 |
| Blessed | Abençoado | obrigatório | 16 | 天生道子 |
| Alchemist | Alquimista | obrigatório | 14 | 炼丹狂魔 |
| Haunted | Assombrado | obrigatório | 11 | 冤魂缠身 |
| Mirage Pot | Pote da Miragem | obrigatório | 16 | 幻灵锅 |
| Elder Dragon Aura | Aura do Dragão Ancião | obrigatório | 11 | 龙祖灵息 |
| Ghost Summoning Banner | Estandarte de Invocação de Fantasmas | obrigatório | 11 | 御鬼幡 |
| Incredible Elixirs | Elixires Incríveis | obrigatório | 12 | 灵丹妙药 |
| Dragon Spear | Lança do Dragão | obrigatório | 17 | 降龙武者 |
| Feather of Golden Crow | Pena do Corvo Dourado | obrigatório | 15 | 金乌玉羽 |

### Criaturas (12)

| en / ch | PT-BR | nível | freq | ch |
|---|---|---|---|---|
| Beast | Fera | sugestão | 218 | 妖兽 |
| Beasts | Feras | sugestão | 211 |  |
| Mythical Beast | Fera Mítica | obrigatório | 150 | 大妖兽 |
| Mythical Beasts | Feras Míticas | obrigatório | 201 |  |
| Monster | Monstro | sugestão | 236 | 怪物 |
| Monsters | Monstros | sugestão | 161 |  |
| Wave of Monsters | Onda de Monstros | obrigatório | 89 | 兽潮 |
| Nether Demons | Demônios do Submundo | obrigatório | 80 |  |
| Dragon | Dragão | sugestão | 342 | 御 |
| Phoenix | Fênix | sugestão | 39 | 凤 |
| Crow | Corvo | sugestão | 71 |  |
| Three-legged | de Três Pernas | sugestão | 75 |  |

### Seitas, lugares e títulos (39)

| en / ch | PT-BR | nível | freq | ch |
|---|---|---|---|---|
| Sect | Seita | obrigatório | 1017 | 宗门 |
| Sects | Seitas | obrigatório | 101 |  |
| Pavilion | Pavilhão | obrigatório | 440 | 阁 |
| Palace | Palácio | obrigatório | 62 | 宫 |
| Hall | Salão | obrigatório | 47 | 殿 |
| Temple | Templo | obrigatório | 123 | 观 |
| Shrine | Santuário | sugestão | 100 |  |
| Valley | Vale | sugestão | 113 | 山谷 |
| Peak | Pico | sugestão | 11 |  |
| Mountain | Montanha | sugestão | 279 | 大山 |
| Mountains | Montanhas | sugestão | 205 | 大山脉 |
| Hills | Colinas | sugestão | 59 | 岳 |
| Tian Yuan Mountain | Montanha Tian Yuan | obrigatório | 175 | 天元山 |
| Nether Mountains | Montanhas do Submundo | obrigatório | 151 | 冥山 |
| Nether | Submundo | obrigatório | 605 |  |
| Nether Air | Ar do Submundo | obrigatório | 228 |  |
| Treasure Pavilion | Pavilhão do Tesouro | obrigatório | 165 | 聚宝仙楼 |
| Manual Pavilion | Pavilhão dos Manuais | obrigatório | 80 | 琅琊阁 |
| Taoist Mind Pavilion | Pavilhão do Coração do Dao | obrigatório | 71 | 道心阁 |
| Night Rain | Chuva Noturna | obrigatório | 87 | 夜雨楼 |
| Alliance | Aliança | obrigatório | 129 | 盟 |
| Town | Vila | sugestão | 16 | 镇 |
| City | Cidade | sugestão | 2 | 城 |
| Sea | Mar | sugestão | 100 |  |
| Forest | Floresta | sugestão | 82 |  |
| Ruins | Ruínas | sugestão | 84 | 遗迹 |
| Lair | Covil | sugestão | 68 |  |
| Altar | Altar | sugestão | 64 | 祭坛 |
| Herb Garden | Jardim de Ervas | sugestão | 69 | 药园 |
| Workshops | Oficinas | sugestão | 260 |  |
| Markets | Mercados | sugestão | 132 |  |
| Heaven | Céu | sugestão | 224 |  |
| Heavenly | Celestial | sugestão | 429 | 天 |
| Champion | Campeão | sugestão | 24 |  |
| World Champion | Campeão do Mundo | obrigatório | 3 | 八荒魁首 |
| Deity | Divindade | sugestão | 38 |  |
| Title | Título | sugestão | 401 |  |
| Legendary | Lendário | sugestão | 86 | 名垂青史 |
| Tree Vault | Árvore-Cofre | obrigatório (acrescentado em 2026-10-24) | 36 | 建木 |

### Interface e jogo (20)

| en / ch | PT-BR | nível | freq | ch |
|---|---|---|---|---|
| Mission | Missão | sugestão | 152 | 任务 |
| Missions | Missões | sugestão | 86 |  |
| Battle | Batalha | sugestão | 84 | 战斗 |
| Tournament | Torneio | sugestão | 78 |  |
| Gathering | Coleta | sugestão | 91 | 采集 |
| Patch Notes | Notas de Atualização | sugestão | 190 |  |
| Update Time | Hora da Atualização | sugestão | 78 |  |
| Spiritual | Espiritual | sugestão | 523 |  |
| Mythical | Mítico | sugestão | 328 |  |
| Enigmatic | Enigmático | sugestão | 162 |  |
| Ethereal | Etéreo | sugestão | 68 | 空明 |
| Divine | Divino | sugestão | 200 | 仙姿 |
| Golden | Dourado | sugestão | 225 | 金 |
| Ultimate | Supremo | sugestão | 217 | 太 |
| Sacred | Sagrado | sugestão | 32 | 圣 |
| Arcane | Arcano | sugestão | 29 | 玄 |
| Sage | Sábio | sugestão | 106 |  |
| Origin | Origem | sugestão | 143 |  |
| Path | Caminho | sugestão | 126 |  |
| Speed | Velocidade | sugestão | 126 |  |
