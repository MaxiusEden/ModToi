# Relatório falhas2 (src_falhas2.json)

- Modelo: Claude Sonnet 5.5 (`claude-sonnet-5-5`), sessão na nuvem; tradução direta, sem scripts de tradução.
- Total: 165 entradas em `out_falhas2.json`. Verificador: 14 ERRO, 0 AVISO; nenhum de marcador.
- Os 14 ERRO são todos "idêntico ao inglês" e esperados:
  - nomes em pinyin: `shuXian` (Lan You) e `suit_historyName*` (-- Jagu, Ruochuan, Bingyi, Feng Yunchuo, Sunny, Jingwei, Zhu Yaoyue, Xi, Hou Yi, Ji Guhun);
  - palavras iguais em português: `storePropNameS20001` (Sutra), `village_building_jitai` (Altar), `uiherdInfo_any` (", etc.").
- Marcador a conferir: `skill_desc92012` tem "&92011_tssh&%s" no `en` (parece erro de digitação; o `ch` não tem o "s"). Mantive o "%s" para o verificador não acusar marcador perdido; talvez valha remover o "s" no jogo.
- Dúvidas para decisão:
  - ~35 entradas têm o `en` em chinês (prefixos/sufixos de habilidade de um caractere). Padronizei: 痕 Marca, 荒 Ermo, 劫 Tribulação, 冥 Sombrio, 寂 Silêncio, 魄 Espectro, 瑕 Falho, 炽 Ardente, 阴 Yin, 诛 Execução, 翔 Voo, 荡 Varredura, 妖 Demônio, 阵 Formação, 冰霜 Geada, 雷电 Relâmpago, 八卦 Bagua, 御气 Domínio do Qi.
  - `schoolNameFirst110/163` ("Dreamy ", "Watchful ") ficaram "Sonhador "/"Vigilante " (masculino, com o espaço final); o gênero depende da palavra que vem depois.
  - Nomes de habilidade inventados por mim (sem glossário): Amarras do Vento, Disparo de Fogo/Elétrico, Frio nos Ossos, Quebra-Vendaval, Trovão Colossal (Tonitrus), Toma Lá, Dá Cá (Quid Pro Quo).
  - `wingmanBaseDesc1004/1010` mantêm os dois espaços ideográficos do começo, como no `en`.
  - `town_liupai` "Bought-In" → "Sem Lance" (item não vendido em leilão).
