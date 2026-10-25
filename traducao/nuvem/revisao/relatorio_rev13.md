# Relatório rev13

- Lote de 1000 entradas (interface de habilidades: atributos, descrições, campos e nomes de técnicas). Revisão feita pelo próprio Claude na nuvem, sem outros modelos.
- 308 entradas corrigidas em `out_rev13.json`; verificador `--parcial`: 0 erros, 3 avisos que são nomes dados em `nomes` (Investida Pesadelo, Respiração Congelada).
- Erros mais comuns: atributos chineses mal traduzidos (体力 como "energia/força física" em vez de Vitalidade, 灵力 como "energia espiritual" em vez de Energia, 念力 como "telecinesia" em vez de Foco); "Raiz Espiritual" e "Chance Crítica" fora do glossário; "I" virou "Eu"; nomes de `nomes` ignorados (Esmagador de Relâmpagos, Estandarte Solar, Manto Devorador de Sombras etc.); "charm" traduzido como talismã; "Range" como "Faixa"; termos soltos em inglês (Kenshin, Sempron, Rock, Palisade, Skyfall, Verdant, Stinger); animais de dragão trocados por dinossauros (Tiranossauro, Velociraptor); concordância ("Lótus Vermelho").
- Dúvidas para decisão do usuário:
  - `nomes` parece errado em alguns casos: "Corte Demoníaca" (concordância), "Pedra do Calamidade" e "Golpe Sundering" (mantive a tradução do `src` nos dois primeiros, e usei o nome dado em "Golpe Sundering").
  - "Sopro" (Fiery Breath) x "Respiração" (Frozen Breath, dado em `nomes`) para Breath.
  - "Investida Pesadelo" (dado em `nomes`) contraria o glossário Charge = Carga.
  - Nomes de feitiços não resolvidos por falta de contexto: Momentum, Hamstring, Rainmaker, Divine Absorption.
  - 81041 (Máscara de Sangue Tecido) e 92321: o inglês omite o verbo (aumenta/reduz); mantive literal.
