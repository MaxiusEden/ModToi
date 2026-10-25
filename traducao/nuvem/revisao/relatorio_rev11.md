# Relatório rev11 (revisão de `src_rev11.json`)

- Modelo: Claude (revisão direta, entrada por entrada). 1000 entradas revisadas, 281 corrigidas em `out_rev11.json`.
- Verificador `--parcial`: 0 ERRO; 2 AVISO falsos (`skillDescS1604` "emaranha" no lugar de "Emaranhar", `skillDescS202` "Recarregando" vs "Ponto de Recarga").
- Erros mais comuns:
  - Prefixos de nome de habilidade (`skillPrefixName*`, um caractere chinês no `en`): traduções soltas ou erradas ("月" Mês, "镇" Cidade, "穿" Vestir, "气" Ar, "仞" ren, "苍" Azul-claro/cinza, "御" Respeitosamente). Unificadas por caractere.
  - `skillPrefixDesc*`: "resfriamento" vs "recarga", "energia espiritual" no lugar de Energia, "técnica corporal" no lugar de Técnica de Movimento, "energia/qi/aura da espada" misturados, nomes de efeito sem maiúscula.
  - Nomes do `nomes` ignorados ou mal aplicados (Dominador, Alma da Espada, Rajada Espiritual, Lâmina Vil, Tempestade de Areia, Divindade do Vento/Fogo).
  - Anglicismos e termos soltos ("Rock", "Greenwave", "Excluir Salvar", "Backup"), e `filial` trocada por "Seita".
  - Espaços antes de quebra de linha ("  \n") copiados do tradutor local em várias entradas, removidos.
- Dúvidas de termo para decidir: "Spiritual Rock" (usei Rocha Espiritual para não confundir com Pedra Espiritual); prefixo 神 ficou "Deus" (talvez "Divino"); prefixos 空 e 虚 ambos "Vazio".
