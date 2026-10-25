# Revisão rev09

- Lote: 795 entradas (descrições de efeitos e estados de batalha `s_*`, textos de gestão da seita `school*`). Corrigidas: 195.
- Verificador `--parcial`: 195 sem ERRO; 1 aviso (`s_julingkuilei`: o nome sugerido "Gigante do Espírito" é o sub-nome; o nome completo "Golem Gigante de Espírito" segue `nomes`, falso alarme).
- Erros mais comuns:
  - "Os" sobrando depois de `{0}`/`{1}` nos logs de Missão ("{0} Os Discípulos iniciantes");
  - verbos de aplicar efeito errados ("infligirá N inimigos com uma pilha", "causará dano a");
  - nomes de efeito fora de `nomes` (Olhar Gélido, Lago de Energia, Veneno Erosivo, Pequena Fada das Flores etc.) e títulos sem Iniciais Maiúsculas;
  - glossário: "Máx" sem ponto, "Habilidade Suprema/Marcial" por "Técnica", "Leech" e "Instakill" deixados em inglês, "DMG/Dano Crítico" por Multiplicador Crítico (ch 暴击倍率);
  - "redeploy" traduzido de cinco jeitos; unifiquei em "redistribuir"; "pilha/stack/acúmulo" misturados nas frases de efeito;
  - traduções com sentido errado ou sem sentido ("Manual Principal da Seita dos Interruptores", "Mecânica de Seleção de Elenco", "Rock Espiritual", "Materiales", "{0}'s {1}" em inglês).
- Gênero de ATQ (o) e DEF (a) padronizado.
- Dúvidas de termo: `nomes` traz "Floral Spirit → Espírito Vital" e "Demonic Cleave → Corte Demoníaca" (provável erro de digitação; usei "Corte Demoníaco"); "Crit DR" ficou como "DR de Crítico", sem termo no glossário.
- Frases cujo verbo falta no `en` ("your damage X%") mantiveram a direção (aumenta/reduz) já escolhida pelo Hy-MT2.
