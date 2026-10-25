# Relatório falhas1 (src_falhas1.json)

- Modelo: Claude Sonnet 5.5 (`claude-sonnet-5-5`), sessão na nuvem; tradução direta, sem scripts de tradução.
- Total: 165 entradas em `out_falhas1.json`. Verificador: 33 ERRO, 13 AVISO; nenhum de marcador.
- Os 33 ERRO são todos "idêntico ao inglês" e esperados:
  - rótulos de enum que ficam iguais (`modEnum10008EnumName6` "6-Item", `...10020/10095EnumName1` "1-Elixir", `...10131EnumName107`, `...10038/1017EnumName1-2` "Qi"/"Feng Shui", `...10083`/`1013` "Normal", `modEnum10011EnumName2` "Sutra");
  - pinyin e nomes: `modEnum1021EnumName*` (Bai Yuan, Chi You, Yong Ning, Lei Ze, Hua Feng, Yun Mo, Mu Xian), `muOu` (Yin'er), `passiveMethod_name7005` (Khayanirodha);
  - palavras iguais em português: `mod_biaoqian5`/`mod_biaoqianid2` (Drama), `mod_guaiwu25/26` ((Elite)/(Normal)), `mod_mozuid` (Mod {0}), `s_3001_fangyu` (DEF);
  - `s_5100x1_dmg00` (7 chaves): o `en` é o próprio nome da chave, sem texto; copiado como está.
- AVISO ("nome dado não usado" de Qi): falsos alarmes; o texto usa o nome composto mais longo (Orbe de Qi do Fogo, Qi Condensado da Madeira etc.).
- Dúvidas para decisão:
  - `nomes` de "Murderous" diz "Intent Assassinato" (inglês misturado, parece dado quebrado); usei "Assassino" em `prefixNameS104007` e "Ascensão Assassina" em `logContent4`.
  - Em `mod_huodongneirong` evitei "º" (o verificador rejeita): "posição 1" no lugar de "1º lugar"; recuos de espaço ideográfico viraram espaços comuns.
  - Nomes de Reescrever o Destino e de aventuras em `logContent4` traduzidos por mim; vale revisar.
  - `fortuitous_event_name10833` "Fortune Teller" → "Adivinho da Fortuna" (para o glossário Fortune).
