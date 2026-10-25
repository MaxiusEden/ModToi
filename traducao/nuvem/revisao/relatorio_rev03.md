# Relatório rev03

- Modelo: revisão manual (Claude), sem scripts de tradução.
- Lote: 1000 entradas (interface do editor de mods: tipos de efeito, enums e títulos de função). Corrigidas: 173.
- Erros mais comuns do Hy-MT2: enum lido como verbo/nome errado ("Minas para 10 jogadores", "O Jogador 9 pratica o cultivo", "Fim do ciclo trimestral"), número do prefixo deslocado ("O Reino atual do Jogador 100"), "Cedo/Tarde/Atrasado" no lugar de Inicial/Médio/Final, "Parente"/"Criança" para Parent/Child, "item" sem plural ("itens"), "tile" e "azulejo" em vez de "quadrado", "Incomum" x "Pouco comum", "Upgrade" deixado em inglês.
- Verificador (--parcial): sem ERRO. Aviso restante: modEnum10079EnumDesc5, falso alarme (o `nomes` de "Artifact Map" aponta para outro item; mantive "Mapa de Artefato", como nas chaves vizinhas).
- Dúvidas de termo para decisão: "Parent/Child" (adotei "Pai ou mãe" e "Filho"); "Sworn brother" (adotei "Irmão juramentado"); "Gift" como "Presente(ar)"; "Wayward Desert" ("Deserto Rebelde", não alterado); "Spar" (usei "combate de treino").
