# Relatório rev31 (revisão do Hy-MT2, narrativas Yuan Zhi e eventos de viagem)

- Modelo revisor: Claude (leitura entrada por entrada, sem scripts de tradução).
- 384 entradas lidas; 100 corrigidas em `out_rev31.json`.
- Verificador (`--parcial`): 0 ERRO, 1 AVISO. O aviso (`drama_dialogue6003011`, "Goodness → Bondade") é falso alarme: é interjeição, "Meu Deus" fica.
- Erros mais comuns:
  - sentido trocado: "raio de luz" por relâmpago, "talismãs" por encantos, "mediar" por meditar, "Macaco Terrível" no lugar do nome dado (Macaco Diabólico), "Domínio do Dao", "grande fortuna" no vidente;
  - tratamento errado: "temas/temais/vos/pagais", "temam/derem/de vocês" (plural onde o `en` é singular), "te";
  - português europeu ou espanhol: "supongo", "algures", "consigo", "num machado";
  - termos inconsistentes: Espírito de Cervo/Veado, monge anfitrião/recebedor, "piscina" para poça/lagoa, pagoda/pagode, "gorjeta" para "tip";
  - tempo verbal passado onde o `en` narra no presente.
- Dúvidas de termo: "Everfrost" ficou sem tradução, como no resto do corpus (outras threads usam "Geada Eterna"; vale unificar). "Destiny (Nurture)" ficou "Destino (Criação)", como nos textos já traduzidos.
