# Tale of Immortal (鬼谷八荒) — análise para o mod de tradução PT-BR

Estado: **análise pausada** (2026-10-22). Este arquivo registra o que foi descoberto até agora e o que falta.

## 1. Estrutura do jogo

| Item | Valor |
|---|---|
| Engine | Unity 2020.3.9, backend **IL2CPP** (`GameAssembly.dll`, 116 MB) |
| Metadata IL2CPP | `guigubahuang_Data/il2cpp_data/Metadata/global-metadata.dat`, versão **27**, **não criptografado** (magic `AF 1B B1 FA`) |
| Loader de mods | MelonLoader 0.5.x embutido (Il2CppAssemblyUnhollower 0.4.18, Cpp2IL 2022.1.0-pre3) |
| Assemblies interop | `MelonLoader/Managed/Assembly-CSharp.dll` (gerado pelo Unhollower) |
| Dummy DLLs com RVA | `MelonLoader/Dependencies/Il2CppAssemblyGenerator/Cpp2IL/cpp2il_out/*.dll` (atributo `AddressAttribute` com RVA de cada método) |
| Kit oficial de mods | `Mod/modFQA/` (tutoriais .docx, `GGBH_API.chm`, tabelas de configuração em xlsx e JSON) |
| Assets | `resources.assets` (1,7 GB) + `resources.assets.resS` (24 GB) |

## 2. Onde está o texto do jogo

### 2.1 Tabelas de configuração (a maior parte)

As tabelas de config **são compiladas no código** (classes geradas `ConfXxx`/`ConfXxxBase` com métodos `Init` cheios de literais). Os textos ficam como string literals dentro do `global-metadata.dat` (435.173 literais). Não existe arquivo de dados separado com os textos.

O kit oficial traz uma cópia legível das tabelas em
`Mod/modFQA/配置修改教程/配置（只读）Json格式/` (761 JSONs). Conferência contra o binário:

| Tabela / coluna | Linhas | Presentes no binário |
|---|---|---|
| LocalText.key | 54.991 | 100,0% |
| LocalText.en (após desfazer escapes `\n`, `\'`) | 53.318 | 99,86% |
| RoleLogLocal.keyID | 26.651 | 100,0% |
| RoleLogLocal.en | 26.538 | 98,5% |

Conclusão: **os JSON do kit estão em sincronia com a versão instalada** e podem ser a fonte da tradução.

Tabelas com texto traduzível (colunas `ch`, `tc`, `en`, `kr`):

| Tabela | Linhas | Caracteres CH | Palavras EN | EN vazio | EN ainda com chinês |
|---|---|---|---|---|---|
| `LocalText` | 54.991 | 1.397.256 | 638.049 | 1.673 | 3.385 |
| `RoleLogLocal` (logs/falas de NPC) | 26.712 | 947.628 | 452.483 | 174 | 0 |
| **Total** | 81.703 | 2.344.884 | **~1,09 milhão** | 1.847 | 3.385 |

Outras tabelas com texto sem colunas de idioma: `NpcNameFirst/Last` (nomes), `BattleSkillPrefixName`, `HerdNPCNameFirst`.

Categorias principais de `LocalText` (ver `localtext-categorias.txt`): `drama_dialogue` (9.904 linhas, 213 mil palavras), `item_desc`, `startGameTipDesc` (patch notes), `drama_option`, descrições de skills, tarefas, talentos etc. As linhas sem inglês concentram-se em `skillPrefixName` (1.584), `spriteTalent_unlockDesc` (823), `spriteTalentDesc` (390), `effect_value_desc`, `wenjuan` (questionário), `ability_name`.

Em `RoleLogLocal`, as falas dinâmicas de NPC são `drama_dialogue*` (19.436), `dialogRoleChat` (2.426) e `dialogRoleRejectRelation` (278) — é aqui que mora a repetitividade dos diálogos (fase 2 do projeto).

### 2.2 Texto fora das tabelas

- ~3.556 literais em inglês com cara de frase **não** aparecem em nenhuma tabela do kit. Parte é mensagem de erro de bibliotecas (.NET, Json, XML), parte é conteúdo de jogo (DLC, eventos). Precisam de captura em tempo de execução.
- ~26.650 literais em chinês fora de LocalText/RoleLogLocal: lista de palavras proibidas, pools de nomes, notas internas de efeitos de batalha (separadas por TAB) e algumas strings de UI hardcoded (ex.: `对你好感度：`, `没有宗主需要处理`).
- Prefabs de UI: muitas telas têm grupos `LanguageGroup` com variantes `_En` (layout alternativo para texto latino). O componente `UILanguageBinding` liga um texto de prefab a uma chave `LocalTextKey`; `UILanguageActived` alterna objetos `goCh/goHant/goEn`.

## 3. API de localização (descoberta via metadata + `GGBH_API.chm`)

```
enum LanguageType { CH = 0, ZHHant = 1, English = 2, Korean = 3 }

class ConfLocalTextItem { string key, ch, tc, en, kr; }
class ConfLocalText : ConfBase {
    Dictionary allText;   // key -> item
    List allConfList;
    string GetText(string key);
}
static class ConfLocalTextEx { static string text(this ConfLocalTextItem item); }

class ConfRoleLogLocalItem { keyID, part, goal, ..., ch, tc, en, kr }
class ConfRoleLogLocal { allItemInKey, LS(...), RandomItem(...) }

static class GameTool {
    string LS(string key);                // "获取文本" — pega texto pela chave
    string LS(string key, int bgType);
    string LSText<T>(T t, getCH, getZHHant, getEnglish);
    object LSObj(object ch, object zhHant, object english);
    void UpdateTextLanguage(GameObject go); // troca de idioma em UI
}
class UIGameSetting { LanguageType curLanguageType; }
```

RVAs nativos (GameAssembly.dll) dos pontos de hook mais prováveis:

| Método | RVA |
|---|---|
| `ConfLocalText::GetText` | `0x5C95790` |
| `ConfLocalText::Init` / `OnInit` | `0x5C95800` / `0x5C95B00` |
| `ConfLocalTextEx::text` | `0x5C952F0` |
| `ConfRoleLogLocal::LS` | `0x4A21EE0`, `0x4A223B0` |
| `GameTool::LS` | `0x2FD1E20`, `0x2FD1BB0` |
| `GameTool::LSText` / `LSObj` | `0x32E8AC0` / `0x2FD14B0` |
| `GameTool::UpdateTextLanguage` | `0x2FD5070` |
| `UILanguageBinding::Start` | `0x3026E50` |

Não é preciso usar RVA no mod: com MelonLoader + Harmony os métodos são acessados pelos tipos do `Assembly-CSharp.dll` interop.

## 4. Arquivos criptografados

### 4.1 `EncryptTool` (classe do jogo)

| Método | O que faz |
|---|---|
| `EncryptDES/DecryptDES(string, key)` | DES-CBC; `MD5(UTF8(key))` → bytes 0–7 = Key, 8–15 = IV; saída Base64 com `/` trocado por `@` |
| `EncryptMult/DecryptMult(byte[], key)` | Cifra aditiva: header + `out[i] = in[i] ± keyBytes[i % len]` |

Formato `EncryptMult`:
- Header de 21 bytes = string literal `2a;ad.,&fSf^SX.,:12@D` com cada char XOR 3 → `1b8bg-/%ePe]P[-/921CG`.
- Corpo: `cifrado[i] = (plano[i] + key[i % len(key)]) & 0xFF`, onde `key` = UTF-8 de uma string.
- Para decifrar: remover o header e subtrair a chave.

Quem usa:

| Chamador | Função |
|---|---|
| `CacheMgr::Read/ReadByte/ReadCacheFileData/Save` | saves e `StreamingAssets/cacheSimple/*.cache` (chave = campo estático `+0x38` de uma classe de config) |
| `CacheMgr::GetPath` | nomes de arquivo de cache = `EncryptDES(nome)` em Base64 |
| `ModExportData::Export`, `ModExcelTool::Export*` | `ModExportData.cache` e tabelas exportadas por mods |
| `ModImportTool::ImportJson`, `ModTool::LoadTextInFile/LoadSpriteInFile` | leitura dos mods (chave = campo estático `+0x48` da mesma classe) |
| `ResMgr::LoadAB` | AssetBundles de mods |
| `CacheHandlerFix` (`DecryptDES`) | correção de cache por versão |

**Pendente:** descobrir as duas strings de chave (campos estáticos `+0x38` e `+0x48`). Caminhos: (a) achar o `.cctor` da classe dona dos campos; (b) ataque de texto conhecido sobre um `ModExportData.cache` exportado por nós mesmos (sabemos o JSON original); (c) ler o valor em runtime com um mod MelonLoader. A opção (c) é a mais simples.

Os `.cache` de `StreamingAssets/cacheSimple` são dados de save/cache, **não** contêm a localização. Decifrá-los não é necessário para a tradução.

## 5. Fontes

Fontes TTF embutidas em `resources.assets` e cobertura dos caracteres do português (`á à â ã é ê í ó ô õ ú ü ç` + maiúsculas):

| Fonte | Glyphs | Acentos PT |
|---|---|---|
| `hanwen` | 23.176 | completo |
| `heiti` | 30.789 | completo |
| `LiberationSans` | 2.294 | completo |
| `cutiHanwen` | 12.258 | falta todos os 26 |
| `cuti` | 7.590 | faltam 19 |
| `HYShangWeiShouShuW` | 10.073 | faltam 19 |

Risco: títulos/textos estilizados que usam `cuti*`/`HYShangWeiShouShuW` podem mostrar quadrados nos acentos. Testar no jogo; se acontecer, adicionar fallback TMP em runtime (ex.: `LiberationSans`/`heiti`).

## 6. Estratégia recomendada para o mod

1. **Seleção de idioma:** não existe slot para PT. Usar o slot `English` (layouts `_En` já acomodam texto latino) e substituir o conteúdo de `en` pelo PT-BR. Opcionalmente sobrescrever `ch` também, para cobrir textos que só existem em chinês.
2. **Carga dos textos (DLL MelonLoader/Harmony, em `ModMain.Init`):** ler `ptbr/LocalText.json` e `ptbr/RoleLogLocal.json` da pasta do mod (`g.mod.GetModPathRoot(...)`) e, depois que `ConfMgr` inicializar, percorrer `g.conf.localText.allText` e `g.conf.roleLogLocal.allConfList`, trocando `en` (e `ch` quando vazio em inglês). Vantagem: não depende de patch por chamada e cobre tudo que passa por LocalText.
3. **Rede de segurança:** patch Harmony em `GameTool.LS` / `ConfLocalText.GetText` para fallback e para registrar chaves que ainda não têm tradução.
4. **Texto hardcoded e prefabs:** patch em setters de `TMP_Text.text` / `UnityEngine.UI.Text.text` com dicionário `inglês/chinês → PT-BR`, e log de strings desconhecidas em arquivo, para alimentar a tradução incremental.
5. **Alternativa sem código:** o sistema oficial de config de mods permite sobrescrever linhas de `LocalText` pelo mesmo `id` (ModExcel/JSON). Funciona, mas só cobre tabelas; a DLL cobre mais.
6. **Pipeline de tradução:** extrair `key` + `ch` + `en` dos JSON do kit → traduzir (preservando placeholders `{0}`, `{name|A}`, `&15112_range&`, `$s_quan$`, tags `<color>`, `<size>`, `\n`) → validar placeholders automaticamente → gerar os JSON PT-BR.

## 7. Pendências

- [ ] Chaves de `EncryptMult` (cache e mods).
- [ ] Teste de fontes/acentos no jogo. Fonte dos botões da tela inicial: OK com ç/õ ("Configurações"). Falta testar títulos com `cuti*`/`HYShangWeiShouShuW`.
- [ ] Mapear tabelas DLC (`DLC*` e `dlcCacheSimple`) e confirmar se o texto de DLC passa por `LocalText`.
- [x] Protótipo da DLL: troca de `en` em `ConfLocalText` + log de chaves sem tradução. Testado em 2026-10-22 (seção 9.3).
- [x] Ferramenta de extração/validação de placeholders (seção 10). Falta fazer o mesmo para `RoleLogLocal`.

## 8. Ferramentas (`tools/re/`)

Scripts Python usados na análise. Dependências em `tools/re/requirements.txt` (versões fixas, todas com wheel para Python 3.14 no Windows). Instalação num venv na raiz (já ignorado pelo git):

```
py -3.14 -m venv venv
venv\Scripts\python -m pip install -r tools\re\requirements.txt
```

O `capstone.__version__` informa 5.0.7 mesmo com o pacote 5.0.9 instalado (`importlib.metadata`); é só a string interna. `callers.py`, `disasm.py`, `eng.py` e `prefix.py` ainda têm a pasta do jogo fixa em `D:/Games/...`.

| Script | Uso |
|---|---|
| `il2cppmeta.py` | parser do `global-metadata.dat` v27: tipos, campos, métodos, valores de enum |
| `literals.py` | extrai todas as string literals do metadata |
| `compare.py` | compara JSON do kit com literais do binário |
| `cpp2il_rva.py` | lê `AddressAttribute` das dummy DLLs do Cpp2IL → mapa método→RVA |
| `disasm.py` | desmonta funções do GameAssembly anotando nomes e literais |
| `callers.py` | encontra chamadas diretas a um RVA |
| `scan.py`, `prefix.py`, `eng.py` | estatísticas de texto |
| `fonts.py`, `ua_list.py` | extração de fontes e inventário de assets |
| `cachean.py`, `deskey.py`, `keyrecover.py` | análise dos `.cache` |

A pasta `ModProject_6b2SYs/` é o projeto de mod criado pelo editor do jogo (namespace `MOD_6b2SYs`, .NET Framework 4.7.2). Ver seção 9.

## 9. Projeto do mod e editor do jogo

| Item | Onde / como |
|---|---|
| Raiz dos projetos | PlayerPrefs `ModTool_modProjectRootPath` (`HKCU\Software\guigugame\guigubahuang`). Muda pelo botão "Change path" do editor (`UIModMainProject.UpdateRootPath`). Neste repositório a raiz é a própria pasta do repositório. |
| Último projeto aberto | PlayerPrefs `ModProject_lastSelectProjectSoleID` |
| Descoberta | O editor lista subpastas `ModProject_<soleID>` da raiz que tenham `ModProject/ModProject.cache`. O nome da pasta tem de bater com o `soleID`. |
| `ModProject.cache` | JSON puro (`soleID`, `name`, `author`, `ver`, `excelEncrypt`…). |
| `ModData.cache` | JSON puro com os dados das tabelas editadas no editor. |
| `ModCode/` | Criado só quando a aba Code é aberta. Copia `Mod/modFQA/代码编写教程/ModMain`, troca `ModMain` por `MOD_<soleID>` em `RootNamespace`/`AssemblyName`, reescreve os `<HintPath>` com a pasta do jogo e copia `dll/Assembly-CSharp.dll`. O `obj/` que vem do modelo é lixo da máquina do desenvolvedor. Se `ModCode/` já existe, o editor não mexe nele: depois da troca para `$(GameDir)`, abrir a aba Code deixou o `.csproj` e a `dll/` intactos (hash igual). |
| Exportação | `<jogo>/ModExportData/Mod_<soleID>/`: `ModAssets/` (cópia literal), `ModExcel/` (cifrado se `excelEncrypt`), `ModCode/dll/MOD_<soleID>.dll` (vem de `ModCode/ModMain/bin/Release`) e `ModExportData.cache`. |
| Carga | `ConfMgr.Init` → `ModMgr.LoadAllMod` → `LoadMod(<pasta>/ModExportData.cache)`. O namespace vem do `.cache`, não do nome da pasta (ex.: `Mod_InstantDialog` contém `MOD_KIolfL.dll`). |
| Arquivos do mod em runtime | `g.mod.GetModPathRoot("<soleID>")` devolve a raiz exportada; os arquivos de `ModAssets/` ficam em `<raiz>/ModAssets/`. |

Em outro PC: clonar, apontar a raiz do editor para o clone e, se o jogo estiver em outro lugar, definir `GameDir` no build.

### 9.1 Pasta do jogo no build (`GameDir`)

Os `HintPath` do `ModMain.csproj` usam `$(GameDir)`, com padrão `C:\Games\Tale of Immortal`. Para sobrescrever, use qualquer uma destas opções (todas conferidas com MSBuild `/v:diag`):

- linha de comando: `msbuild ModMain.csproj /p:GameDir="D:\Games\Tale of Immortal"`;
- variável de ambiente `GameDir`;
- `<PropertyGroup><GameDir>…</GameDir></PropertyGroup>` em `ModMain.csproj.user` (fora do git). Funciona porque o MSBuild avalia todas as propriedades antes dos itens, então o valor do `.user`, importado depois, vale nos `HintPath`.

Os 6 `HintPath` relativos (`dll\…`: DOTweenPro, hlapi, Grid/Terrain/VR/XR modules) apontam para DLLs que o editor não copia; hoje nenhuma é usada pelo código.

### 9.2 Compilador

O MSBuild 4.0 que vem com o Windows (`C:\Windows\Microsoft.NET\Framework64\v4.0.30319`) **não serve**: o `csc` dele (C# 5, pré-Roslyn) falha com `CS0008 … 'Registro não localizado na pesquisa.'` ao ler qualquer DLL gerada pelo Unhollower (`UnityEngine.CoreModule.dll`, `UnityEngine.AccessibilityModule.dll`…). É preciso um compilador Roslyn, por exemplo o do VS Build Tools. Sem o targeting pack 4.7.2, o MSBuild só avisa (MSB3644) e usa os assemblies do .NET 4.8 instalado.

Instalação usada (VS Build Tools 2022 17.14, workload MSBuild e targeting pack 4.7.2). Chame `winget.exe` com a extensão: neste PC há um arquivo vazio `C:\WINDOWS\system32\winget` que faz o `winget` sem extensão falhar.

```
winget.exe install --id Microsoft.VisualStudio.2022.BuildTools --source winget --override "--quiet --wait --norestart --add Microsoft.VisualStudio.Workload.MSBuildTools --add Microsoft.Net.Component.4.7.2.TargetingPack"
```

Build (gera `ModCode/ModMain/bin/Release/MOD_6b2SYs.dll`, que o editor copia na exportação):

```
"C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" ModMain.csproj -t:Rebuild -p:Configuration=Release
```

### 9.3 Protótipo de tradução de LocalText

- Código: `ModCode/ModMain/PtBr/LocalTextPtBr.cs`, chamado em `ModMain.Init`. Se `g.conf.localText.allConfList` ainda estiver vazio, tenta de novo a cada frame em `OnUpdate`, uma vez por `Init`.
- Entrada: `<raiz do mod>/ModAssets/ptbr/LocalText.json`, no formato `{ "key": "texto" }`, em UTF-8. A raiz vem de `g.mod.GetModPathRoot("6b2SYs")`. A leitura usa o `Newtonsoft.Json` do MelonLoader, que já é carregado em runtime (o mod InstantDialog, `MOD_KIolfL`, faz igual: `GetModPathRoot` + `ModAssets` + `JsonConvert`).
- Efeito: troca `item.en`, então só aparece com o jogo em inglês.
- Saída: `<raiz do mod>/ptbr_missing_LocalText.txt` (`key<TAB>en`, com `\n` e `\t` escapados) e uma linha `[PtBr] LocalText: …` no `MelonLoader/Latest.log`.
- Chaves de teste (tela inicial): `login_jinruhuijuan` (Enter World), `login_shezhi` (Settings) e `login_tuichuhuijuan` (Quit). "Configurações" também testa ç/õ na fonte do menu.
- Teste: compilar em Release, exportar pelo editor e ativar o mod, com o jogo em inglês.

Resultado do teste (2026-10-22):

- Os 3 botões apareceram em PT-BR, e "Configurações" renderizou ç/õ na fonte da tela inicial.
- Não é preciso reiniciar o jogo. Ao ativar o mod e voltar ao menu, o jogo recarrega os mods (`ModMgr.LoadAllMod` de novo) e chama o `Init`. A config já estava carregada e a tela inicial foi remontada com o texto novo.
- O log registrou `3 traduzidas, 55312 sem traducao, 0 chaves do JSON inexistentes`. O arquivo de faltantes tem 55.312 linhas (5,3 MB), sem chaves repetidas, e 399 delas estão com `en` vazio.
- O `LocalText` em runtime tem 55.315 chaves: as 54.991 do kit, menos 11 que não existem no jogo, mais 335 que não estão no kit. Parte dessas 335 tem IDs aleatórios de 8 caracteres (ex.: `0D3jDfdR`) e parece vir de outros mods ativos que injetam linhas pelo `ModExcel`. O restante tem prefixos normais (`role*`, `item*`, `startGameTipDesc*`, `task*`). A tradução precisa partir do dump de runtime, não só do kit.
- Risco de ordem de carga: mods de tradução como "Enhanced english translation" também mexem em `LocalText`. Se um deles carregar depois do nosso, pode sobrescrever o `en`. Isso ainda não foi verificado.
- O `Latest.log` do MelonLoader 0.5.4 não é gravado em UTF-8 (acentos e chinês viram `?`/`�`). As mensagens do mod no log devem ser só ASCII.

## 10. Pipeline de tradução (`tools/ptbr/ptbr.py`)

Só usa a biblioteca padrão do Python. A pasta do jogo vem da variável `GAME_DIR` (padrão `C:\Games\Tale of Immortal`).

| Comando | O que faz |
|---|---|
| `extract [--dump arquivo]` | Mescla o dump de runtime (`ModExportData/Mod_6b2SYs/ptbr_missing_LocalText.txt`) em `traducao/LocalText/*.json`. O `ch` vem do kit. Chaves já traduzidas não aparecem no dump; vêm do `LocalText.json` atual do mod, com o `en` do kit. Nunca apaga entradas nem `pt`. Se o `en` de uma entrada já traduzida mudou, marca `"revisar": true`. É idempotente. |
| `validate [--report arquivo]` | Confere cada `pt` contra o `en` (ou contra o `ch`, quando o `en` está vazio). Também recalcula os motivos `glossário: termo → tradução` em `revisar`: entra quando um termo de `glossario/obrigatorios.json` aparece na fonte e a tradução aprovada falta no `pt`, e sai quando o `pt` é corrigido (regras em `glossario/decisoes.md`). Grava `traducao/validacao.txt` e sai com código 1 se houver erro. |
| `glossario` | Junta `glossario/obrigatorios.json` e `glossario/sugestoes.json` em `glossario/glossario.json`, a entrada do `omni_translate`. |
| `merge arquivo…` | Traz as saídas do `omni_translate` (`chave -> {pt, ok, model}`) para `traducao/`, preenchendo **só entradas com `pt` vazio**. Nunca sobrescreve uma tradução existente, então pode ser repetido sem risco. Grava `auto = <modelo>`. Com `ok=false`, grava o `pt` e acrescenta `omni ok=false` em `revisar`. Também recalcula os motivos de glossário. |

O `build` exclui as entradas com `omni ok=false` em `revisar` até alguém revisar e apagar o motivo.

O campo `revisar` de uma entrada é uma **lista de motivos**: `en mudou depois da tradução` (posto pelo `extract`), `glossário: …` e `largura: …` (recalculados pelo `validate`) e `omni ok=false` (pelo `merge`). Quem revisa apaga o campo. Avisos não bloqueiam o `build`. Outros campos: `auto` guarda o modelo que traduziu, e `revisado: true` indica que uma pessoa conferiu ou corrigiu uma tradução automática.

### 10.1 Tradução automática (`omni_translate`)

As saídas brutas ficam em `traducao/auto/<arquivo>.<modelo>.json` (fora do git) e entram em `traducao/` pelo `merge`.

Piloto de 2026-10-23 com `common` (100 entradas) e `ui` (510):

- **Resultado:** 520 traduzidas, todas `ok=true`, sem marcadores divergentes. Por modelo: `gemini-3-flash-preview` fez 235 e `gemini-2.5-flash` (a reserva) fez 285. Ficaram 90 de `ui` pendentes porque os dois modelos entraram em espera (429).
- **Resposta cortada:** o Gemini 3 raciocina antes de responder e gasta o `max_tokens`. Com lotes de 30 e 6.000 tokens, as respostas vinham cortadas ("Unterminated string"); **lotes de 15 com `max_tokens=12000`** resolveram.
- **Limite gratuito:** os dois Gemini entram em espera (429 "cooling down", ~20–25 min) depois de algumas centenas de entradas. Para continuar com a reserva, use **o mesmo arquivo de saída**: a ponte pula o que já está nele, e cada entrada registra o próprio modelo.
- **HTTP 401** em todos os Gemini = credencial OAuth do provedor expirada no OmniRoute; é preciso reautenticar.
- **Revisão:** todas as `ok=false` (nenhuma), os 9 avisos e 30 `ok=true` por amostra.
  - Os 7 avisos de glossário eram falsos positivos de singular/plural; o validador foi corrigido.
  - 3 traduções precisaram de ajuste manual: "LMB" virou "botão esquerdo do mouse", saiu um "Por favor" que o inglês não tem, e "comissioná-lo" foi reescrito.
  - Os nomes de idioma (简体中文/繁体中文) ficam em chinês de propósito.

Segunda leva (2026-10-23), com as categorias de interface vistas nos prints (`setting_`, `player_`, `playerInfo_`, `townBuildingName`, `schoolBuildingName`, `school_`, `defaultKeys`, `role_character_name`, `login_`, `diffName`). O arquivo de origem foi montado por prefixo de chave em `traducao/auto/src_interface.json`; o `merge` trabalha por chave, então qualquer recorte serve.

- **Resultado:** 570 de 641 entradas (300 com `gemini-3-flash-preview`, 270 com `gemini-2.5-flash`), sendo 2 `ok=false` e 15 faltando na resposta. A reserva entrou em espera de **~10 h** (`reset_seconds` 35.905), ou seja, é cota diária. Ficaram 71 entradas de interface e 90 de `ui` pendentes.
- **Custo:** o cabeçalho `x-omniroute-response-cost` do `gemini-2.5-flash` veio diferente de zero (0,005–0,011 por lote), enquanto o do `gemini-3-flash-preview` veio 0. Falta confirmar no OmniRoute se isso é cobrança real.
- **Revisão:** 48 correções (largura, os 2 `ok=false`, "Skill Stat" → Aptidão Marcial e dois nomes que tinham ficado em chinês). As 32 sílabas `school_name_*` voltaram ao original (ver `glossario/decisoes.md`).

### 10.2 Teste do piloto no jogo (2026-10-23, prints em `docs/prints/`)

- As traduções de `common`/`ui` apareceram: tela inicial, Configurações, OK/Cancelar, Idioma, rótulos do personagem (Sobrenome, Nome, Posição, Reino, Reputação…).
- **O que ficou em inglês é, quase todo, de categorias ainda não traduzidas**, não falha do mod: `setting_*` (menu de pausa, configurações), `player_*`/`playerInfo_*` (atributos e blocos General/Combat), `townBuildingName`/`schoolBuildingName` (prédios), `school_*` (painel da seita), `defaultKeys*` (atalhos), `role_character_name` (traços), `login_*`, `diffName`, `drama_dialogue` (diálogos).
- **Falas curtas de NPC** nas lojas ("Hello, looking food or lodgings?", "Great deals! Come have a look!") vêm de **`RoleLogLocal`**, que o mod ainda não troca.
- Alguns textos vêm do `en` reescrito pelo mod "Enhanced english translation" (Travel = `player_jiaoli`, Refining = `player_lianqizizi`, Gentle/Loyal = `role_character_name3/9`, Reload = `setting_chongxinjiazaicundang`). As chaves existem e serão traduzidas normalmente.
- **Não estão em tabela nenhuma:** "Mine", "Geo", "Battle Graphics" (ícones do mapa e configurações). Ou são texto fixo em prefab, ou são imagem; dependem do patch em `TMP_Text`/`Text` (seção 6, item 4).
- **Largura:** as abas laterais do personagem cortam acima de ~7 letras ("Atribut", "Habilid", "Relacio"); até o inglês "Experience" aparece cortado. "Longevidade" empurrou o ícone para baixo da barra lateral. 73 dos 329 textos curtos do piloto eram bem mais longos que o `en`. Daí a regra de largura (`glossario/decisoes.md`, "Interface apertada") e o encurtamento de 55 textos.
| `build` | Gera `ModAssets/ptbr/LocalText.json` só com traduções sem erro (avisos não bloqueiam). Não grava nada se não houver nenhuma tradução válida. |

Formato de `traducao/LocalText/<categoria>.json`: `{ "<key>": { "ch": …, "en": …, "pt": "" } }`, com quebras de linha reais (`\n` no JSON). A categoria é o prefixo da chave sem os dígitos finais (regra do `prefix.py`). Categorias com menos de 100 chaves caem para o prefixo de letras e depois para `_diversos.json`; as chaves aleatórias de outros mods vão para `_outros_mods.json`. São 85 arquivos (10,7 MB); os maiores são `drama_dialogue` (2,8 MB) e `item_desc` (1,3 MB).

Regras de validação, levantadas no dump de runtime:

| Tipo | Padrão | Exemplos (contagem no dump) | Falha |
|---|---|---|---|
| Referência | `{…}` | `{0}` (2.417), `{#areaName1}`, `{itemLink6121036}`, `{name|A}` | erro |
| Valor de skill | `&…&` (ASCII, pode ter `|`, `x`, `/`) | `&26113_dmg&`, `&22113_xzsh|x22113_dmg|/100|f0&` (1.893 distintos) | erro |
| Ícone/termo | `$…$` | `$s_jian$`, `$s_quan$` (111 distintos) | erro |
| Formato C | `%s`, `%d` | 1 ocorrência | erro |
| Tags | `<…>` | `<g>`, `<r>`, `<o>`, `<y>`, `<b>`, `<rootN>`, `<size=N>`, `<color=#…>` | erro (o conjunto de tags tem de ser igual) |
| Quebra de linha | `\n` | 7.571 em 1.944 entradas | aviso |
| Chinês no `pt` | CJK | — | aviso |

Os tokens são comparados como multiconjunto: a ordem pode mudar, mas a quantidade não. Em 45 entradas o `en` do jogo já tem tokens diferentes do `ch` (ex.: `item_desc6331002` perdeu `{#fortuitous_event_name…}`). O validador usa o `en` como referência.

**Candidatos ao glossário** (`tools/ptbr/glossario_candidatos.py` → `glossario/candidatos.tsv`, colunas `termo, frequencia, ch, tipo, categoria, fonte, exemplo1, exemplo2`). A frequência é o número de entradas cujo `en` contém o termo. O script junta três fontes:

- **N-gramas capitalizados do `en`**, de 1 a 5 palavras, com `of`/`the`/`and` permitidos no meio. Fica de fora a primeira palavra de cada frase e uma lista de palavras comuns, e só entram termos com frequência ≥ 5.
- **Nomes das tabelas do kit**, que apontam para chaves de `LocalText`. Elas definem o tipo do termo: `RoleGrade`/`DLCUnitGrade` (reino), `SchoolName` (seita), `ItemType` (tipo de item), `BattleEffect`/`SkillEffectDesc` (efeito), `AppellationTitle` (título), `RoleCreateFeature` (destino/traço), `BattleSkillAttack`/`PassiveMethod`/`GodEyeSkills` (técnica) e `TownName` (lugar).
- **Uma lista-semente** de tratamentos e conceitos de cultivo, contada sem diferenciar maiúscula, porque no `en` eles aparecem em minúscula ("my friend", "old man", "spirit stones").

O `ch` mostra até 3 variantes, vindas dos nomes exatos e das tabelas. Nomes de seita e de cidade são compostos de sílabas em pinyin (`You`, `An`, `Feng`, `Qi`…), por isso termos de uma palavra só contam quando aparecem com maiúscula fora do início de frase. Resultado: 6.251 candidatos, 3.113 com `ch`.

**Fonte do `en`** (decisão de 2026-10-22): o `en` do kit oficial é a fonte padrão. O `en` do dump só vale para as 335 chaves que não existem no kit e para as 146 em que o kit tem `en` vazio. O dump reflete o inglês **com os mods ativos**: em 2.990 chaves ele difere do kit porque "Enhanced english translation" e `Mod_Sky8ub` sobrescrevem o `LocalText` via `ModExcel`. Usar o kit deixa a fonte independente da lista de mods de cada PC. Ao aplicar essa regra, 2.844 entradas trocaram de `en` (nenhuma tinha `pt`).