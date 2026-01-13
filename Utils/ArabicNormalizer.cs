using System.Text;
using System.Text.RegularExpressions;

namespace Tarnim.Utils
{
    /// <summary>
    /// Utility class for normalizing Arabic text to enable consistent searching.
    /// Removes diacritics (Tashkeel) and normalizes character variants.
    /// </summary>
    public static class ArabicNormalizer
    {
        // Arabic diacritics (Tashkeel) - Unicode range U+064B to U+0652
        private static readonly Regex TashkeelPattern = new Regex(
            @"[\u064B-\u0652\u0670\u0640]",
            RegexOptions.Compiled);

        // Alef variants: أ إ آ ٱ → ا
        private static readonly Dictionary<char, char> AlefVariants = new()
        {
            { 'أ', 'ا' },  // Alef with Hamza Above
            { 'إ', 'ا' },  // Alef with Hamza Below
            { 'آ', 'ا' },  // Alef with Madda
            { 'ٱ', 'ا' },  // Alef Wasla
        };

        // Other normalizations
        private static readonly Dictionary<char, char> OtherNormalizations = new()
        {
            { 'ة', 'ه' },  // Taa Marbuta → Haa
            { 'ى', 'ي' },  // Alef Maksura → Yaa
            { 'ؤ', 'و' },  // Waw with Hamza → Waw
            { 'ئ', 'ي' },  // Yaa with Hamza → Yaa
        };

        /// <summary>
        /// Normalizes Arabic text for search matching.
        /// - Removes all Tashkeel (diacritical marks)
        /// - Normalizes Alef variants
        /// - Normalizes Taa Marbuta and other characters
        /// </summary>
        /// <param name="text">The Arabic text to normalize.</param>
        /// <returns>Normalized text suitable for search comparison.</returns>
        public static string Normalize(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Step 1: Remove Tashkeel (diacritics) and Tatweel (kashida)
            string result = TashkeelPattern.Replace(text, string.Empty);

            // Step 2: Apply character normalizations
            var sb = new StringBuilder(result.Length);
            foreach (char c in result)
            {
                if (AlefVariants.TryGetValue(c, out char normalizedAlef))
                {
                    sb.Append(normalizedAlef);
                }
                else if (OtherNormalizations.TryGetValue(c, out char normalizedOther))
                {
                    sb.Append(normalizedOther);
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Checks if a string contains Arabic characters.
        /// </summary>
        public static bool ContainsArabic(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            // Arabic Unicode range: U+0600 to U+06FF
            return Regex.IsMatch(text, @"[\u0600-\u06FF]");
        }

        /// <summary>
        /// Checks if a string is a valid song number (digits only).
        /// </summary>
        public static bool IsNumericOnly(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            return Regex.IsMatch(text.Trim(), @"^\d+$");
        }
    }
}
