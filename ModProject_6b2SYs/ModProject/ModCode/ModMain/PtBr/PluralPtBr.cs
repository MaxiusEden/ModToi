using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MOD_6b2SYs
{
    /// <summary>
    /// Plural de nomes de item em português: "99540Pedra Espiritual" vira "99540 Pedras Espirituais". Só as palavras
    /// antes da primeira preposição mudam ("Minério de Ferro" vira "Minérios de Ferro"). Aplicar de novo não muda
    /// nada, porque a tela é reescrita a cada varredura.
    /// </summary>
    public static class PluralPtBr
    {
        private static readonly Regex CountName = new Regex(@"^(\d+)\s*(\p{L}.*)$", RegexOptions.Singleline);
        private static readonly Regex Roman = new Regex(@"^[IVXLCDM]+$");

        private static readonly HashSet<string> Stop = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "de", "do", "da", "dos", "das", "em", "no", "na", "nos", "nas", "com", "sem", "para", "por", "ao", "aos",
            "à", "às", "e", "ou", "contra", "sobre", "entre",
        };

        // Palavras estrangeiras no meio do nome, que a regra geral transformaria.
        private static readonly HashSet<string> Keep = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "qi", "super", "tai", "yam", "cornflower",
        };

        // Plurais que a regra geral erraria.
        private static readonly Dictionary<string, string> Irregular =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "grão", "grãos" }, { "mão", "mãos" }, { "irmão", "irmãos" }, { "órgão", "órgãos" }, { "chão", "chãos" },
            { "cristão", "cristãos" }, { "pão", "pães" }, { "cão", "cães" }, { "capitão", "capitães" },
            { "alemão", "alemães" }, { "raiz", "raízes" }, { "mel", "méis" }, { "mal", "males" },
        };

        /// <summary>"N Nome" com o espaço e o plural certos, ou null se o texto não for número seguido de nome.</summary>
        public static string Rewrite(string text)
        {
            var m = CountName.Match(text ?? "");
            if (!m.Success)
            {
                return null;
            }
            string count = m.Groups[1].Value;
            // 0 também leva plural em português; só 1 fica no singular.
            bool one = count.TrimStart('0') == "1";
            return count + " " + (one ? m.Groups[2].Value : Name(m.Groups[2].Value));
        }

        public static string Name(string name)
        {
            var words = name.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (Stop.Contains(words[i]) || !char.IsLetter(words[i], 0) || Roman.IsMatch(words[i]))
                {
                    break;
                }
                if (!Keep.Contains(words[i]))
                {
                    words[i] = Word(words[i]);
                }
            }
            return string.Join(" ", words);
        }

        private static string Word(string w)
        {
            string plural;
            if (Irregular.TryGetValue(w, out plural))
            {
                return char.IsUpper(w[0]) ? char.ToUpperInvariant(plural[0]) + plural.Substring(1) : plural;
            }
            string stem = w.Length > 1 ? w.Substring(0, w.Length - 1) : "";
            string lower = w.ToLowerInvariant();
            char last = lower[lower.Length - 1];
            if (lower.EndsWith("ão", StringComparison.Ordinal))
            {
                return w.Substring(0, w.Length - 2) + "ões";
            }
            // Com acento antes do fim, o "l" final não é tônico: Nível/Níveis, Fóssil/Fósseis.
            bool stressedEarlier = stem.ToLowerInvariant().Any(c => "áéíóúâêô".IndexOf(c) >= 0);
            switch (last)
            {
                case 'a': case 'e': case 'i': case 'o': case 'u':
                case 'á': case 'é': case 'í': case 'ó': case 'ú': case 'â': case 'ê': case 'ô': case 'ã':
                    return w + "s";
                case 'r': case 'z':
                    return w + "es";
                case 'm':
                    return stem + "ns";
                case 'l':
                    return Lateral(w, lower, stressedEarlier);
                default:
                    // Termina em "s" ou "x" (já serve no plural) ou é pinyin ("Fuling"): fica como está.
                    return w;
            }
        }

        private static string Lateral(string w, string lower, bool stressedEarlier)
        {
            string root = w.Substring(0, w.Length - 2);
            switch (lower[lower.Length - 2])
            {
                case 'a':
                    return root + "ais";
                case 'e':
                    return root + (stressedEarlier ? "eis" : "éis");
                case 'i':
                    return root + (stressedEarlier ? "eis" : "is");
                case 'o':
                    return root + (stressedEarlier ? "ois" : "óis");
                case 'u':
                    return root + "uis";
                default:
                    return w;
            }
        }
    }
}
