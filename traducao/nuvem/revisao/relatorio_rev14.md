# Revisão rev14 (1000 entradas: habilidades de espírito, falas dos espíritos, passos de movimento)

- Modelo: Claude (revisão manual, sem scripts de tradução). 149 entradas corrigidas em `out_rev14.json`; verificador `--parcial` sem ERRO.
- Erros mais comuns:
  - nomes de habilidade meio traduzidos ou errados ("Vôlei" para Volley, "Isquiotibiais" para Hamstring, "Curse Encantadora", "Skybreak" mantido, "Transferência Divino"), unificados entre `skill_name_middle*` e `sprite_*_skill_name_*`;
  - concordância e gênero (Mengyi e Lingge falam no feminino; "sua Carisma", "uma centopeia", "à Árvore-Cofre");
  - palavras em espanhol ou inglês ("Supongo", "merge", "Blink", "Dash") e "celestial reino";
  - sentido errado ("o gol" para punch, "não consigo suportar" para can't take this, "Hmm" para Wuss);
  - maiúsculas em nomes de arte de espírito e pronome de pessoa errado (vocês, tu/te-ás).
- Aviso do verificador (falso alarme): `stepDesc4` "Charge → Carga", aqui é verbo ("Avance").
- Dúvidas para decisão: (1) fragmentos de nome de habilidade de um caractere (`skill_name_second*`, `stepPrefixName*`) são traduzidos sem contexto; troquei só os claramente errados (月 Mês→Lua, 日 Dia→Sol, 钟 Relógio→Sino, 鼎 Panela→Caldeirão). (2) 溟 ficou "Ming" (pinyin) nos dois usos. (3) Gênero de Muqing, Ningshuang e dos espíritos 2001-2024 é incerto; mantive o que o Hy-MT2 usou, salvo contradição no mesmo texto.
