# Relatório rev38 (revisão do Hy-MT2)

- Modelo: Claude (revisão manual entrada por entrada). Lote com 462 entradas (diálogos de eventos: Sábio Feio e diabretes, talismã de chuva, aldeia misteriosa, tarefas de seita, Poço das Iscas, Tushan).
- Corrigidas: 206 entradas. Verificador `--parcial`: 206 sem erro, 0 com erro, 2 avisos.
- Avisos que ficam: `drama_dialogue860301` e `860321` ("Respiração do Submundo" vem de `nomes` e vale mais que "Sopro" do glossário).
- Erros mais comuns: variações da mesma frase repetida (talismã de chuva, Poço das Iscas, assembleia), uniformizadas; "Veja:" no lugar de "Você vê"; pessoa e número trocados ("te", "vocês", "o meu diabrete" onde o `en` diz "your"); tempo verbal passado onde o `en` está no presente; nomes de `nomes` ignorados (Soul Lure, Nether Breath, Imp EXP Elixir, Imp Realm Elixir, Gourd Imp's True Qi, Monster Lure Talisman, Sky Prayer Talisman, Teleportation Talisman); maiúsculas a mais ("Daoísta", "Justos"); trechos com sentido errado (energia que suga em 8200014, "atacá-lo" em 8110021).
- Dúvidas de termo: "Soul Lure" ficou como "Atração de Alma" (plural "Atrações de Alma") pelo `nomes`, mas soa estranho para um objeto jogado no lago; vale confirmar. "Backup Disciple" ficou "Discípulo Reserva". "Herbs and Ores" nas notas de seita ficou "Ervas e Minérios" (nomes de recurso).
