# Relatório rev41

- Modelo: Claude (sessão na nuvem). Lote de 1.000 opções de diálogo (`drama_option*`), todas lidas.
- Corrigidas: 103 entradas. Verificador `--parcial`: 103 sem erro, 0 aviso.
- Erros mais comuns:
  - "te" no lugar de "você" (perseguir, abandonar, salvar, encontrar);
  - gênero errado em falas dirigidas a mulheres (Shuang Ling, Yuan'er, "minha senhora");
  - verbo de opção em forma diferente do resto ("Luta", "Declínio", "Ataque", "Ajuda" → Lutar, Recusar, Atacar, Ajudar; "Discordo" no lugar de "Não concordo");
  - sentido trocado: "trial" como "período de teste", "submit" como "envie", "haven't got it yet" como "não recebi", "Stick" deixado em inglês ou como "Manípulo";
  - "caixotes/caixas" no feminino errado, "Exquisito" (espanhol), "virrei", "desperdçe".
- Dúvidas de termo (decisões minhas, vale conferir na consistência):
  - "Crucial Lock" → Fechadura Crucial; "Move Stick" → Haste (Haste A/B/C, do Céu/da Terra/do Mortal);
  - "Sandbank of Mist" aparecia como Banco de neblina, Areal de Neblina e Areal de Névoa; unifiquei em "Banco de areia da névoa";
  - "Insightful Meet" → Encontro Perspicaz; "Foolish Old Man" → Velho Tolo; "Arachnid Spirit" → Espírito Aranha (pelo `nomes`).
