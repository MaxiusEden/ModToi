# Relatório rev06

- Lote revisado: 1.000 entradas (interface: habilidades de vida, Sopros de monstros, prefixos de técnica, propriedades, alquimia, atributos); 177 corrigidas em `out_rev06.json`.
- Verificador `--parcial`: 177 sem erro, 0 com erro, 0 avisos.
- Erros mais comuns:
  - nome dado em `nomes` ignorado (Sopros: "Dispositivo Desviado", "Senhor dos Mortos", "Caranguejo Gigante" etc.; "Aura da Vida");
  - erros de digitação e plural ("materiales", "material necessários", "itemes", "Pêssega", "Nebla", "Demoníacoas", "solado");
  - sentido trocado ou inventado ("Sort" → Ordenar, "Clear" → Limpar, "Owned" → Possui, "Shower" → Aguaceiro, "Focused Fingertip", "Canopying Soul", "Escort", "My Treat", powerInfo);
  - variantes do mesmo termo (Percepção da Lança, Herbalista, Mente Frutuosa, Vento Hábil vs. Vento Ágil) e Iniciais Maiúsculas em nomes de traço/técnica.
- Dúvidas para decisão do usuário:
  - "Upgrade" (prefixos de técnica) ficou como "Atualização"; "Aprimoramento" talvez soe melhor.
  - "Spiritlock Circle" ficou "Círculo do Spiritlock" (77 ocorrências no inglês); decidir se traduz.
  - `prefixNameS303006` (Canopying Soul, ch 真灵) ficou "Alma em Dossel", palpite.
  - `prefixNameS30128` (Crit Rate Boost, ch 窥虚) ficou "Chance Crítica" pelo glossário, mas o `ch` parece desalinhado.
