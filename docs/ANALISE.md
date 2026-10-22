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
- [ ] Teste de fontes/acentos no jogo.
- [ ] Mapear tabelas DLC (`DLC*` e `dlcCacheSimple`) e confirmar se o texto de DLC passa por `LocalText`.
- [ ] Protótipo da DLL: troca de `en` em `ConfLocalText` + log de chaves sem tradução.
- [ ] Ferramenta de extração/validação de placeholders.

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

### 9.2 Compilador

O MSBuild 4.0 que vem com o Windows (`C:\Windows\Microsoft.NET\Framework64\v4.0.30319`) **não serve**: o `csc` dele (C# 5, pré-Roslyn) falha com `CS0008 … 'Registro não localizado na pesquisa.'` ao ler qualquer DLL gerada pelo Unhollower (`UnityEngine.CoreModule.dll`, `UnityEngine.AccessibilityModule.dll`…). É preciso um compilador Roslyn, por exemplo o do VS Build Tools. Sem o targeting pack 4.7.2, o MSBuild só avisa (MSB3644) e usa os assemblies do .NET 4.8 instalado.

Os 6 `HintPath` relativos (`dll\…`: DOTweenPro, hlapi, Grid/Terrain/VR/XR modules) apontam para DLLs que o editor não copia; hoje nenhuma é usada pelo código.
