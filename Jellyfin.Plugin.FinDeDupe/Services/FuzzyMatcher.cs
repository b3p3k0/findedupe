namespace Jellyfin.Plugin.FinDeDupe.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>Service for fuzzy matching of media titles.</summary>
    public static class FuzzyMatcher
    {
        /// <summary>
        /// Determines if two titles represent the same media item.
        /// </summary>
        /// <param name="title1">First normalized title.</param>
        /// <param name="title2">Second normalized title.</param>
        /// <param name="year1">Release year of first item, if available.</param>
        /// <param name="year2">Release year of second item, if available.</param>
        /// <param name="providerIds1">Provider IDs of first item.</param>
        /// <param name="providerIds2">Provider IDs of second item.</param>
        /// <param name="exactThreshold">Threshold for exact matches (0-100).</param>
        /// <param name="conditionalThreshold">Threshold for conditional matches (0-100).</param>
        /// <returns>True if the titles should be considered the same item.</returns>
        public static bool IsSameTitle(
            string title1,
            string title2,
            int? year1,
            int? year2,
            Dictionary<string, string> providerIds1,
            Dictionary<string, string> providerIds2,
            int exactThreshold = 90,
            int conditionalThreshold = 85)
        {
            if (string.IsNullOrWhiteSpace(title1) || string.IsNullOrWhiteSpace(title2))
            {
                return false;
            }

            // Check for exact provider ID matches first
            if (HasMatchingProviderIds(providerIds1, providerIds2))
            {
                return true;
            }

            // Calculate similarity score
            var score = CalculateSimilarity(title1, title2);

            // Exact match threshold
            if (score >= exactThreshold)
            {
                return true;
            }

            // Conditional match - requires year match or provider ID intersection
            if (score >= conditionalThreshold)
            {
                var yearMatch = YearsMatch(year1, year2);
                var providerIntersection = HasProviderIdIntersection(providerIds1, providerIds2);

                return yearMatch || providerIntersection;
            }

            return false;
        }

        /// <summary>
        /// Calculates similarity score between two titles using token-based comparison.
        /// </summary>
        /// <param name="title1">First title.</param>
        /// <param name="title2">Second title.</param>
        /// <returns>Similarity score from 0 to 100.</returns>
        public static int CalculateSimilarity(string title1, string title2)
        {
            if (string.IsNullOrWhiteSpace(title1) || string.IsNullOrWhiteSpace(title2))
            {
                return 0;
            }

            // Exact match
            if (string.Equals(title1.Trim(), title2.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return 100;
            }

            // Token set ratio - most effective for media titles
            var tokenSetScore = CalculateTokenSetRatio(title1, title2);

            // Token sort ratio - good for reordered words
            var tokenSortScore = CalculateTokenSortRatio(title1, title2);

            // Levenshtein ratio for character-level similarity
            var levenshteinScore = CalculateLevenshteinRatio(title1, title2);

            // Return the highest score with token-based methods weighted more heavily
            return Math.Max(Math.Max(tokenSetScore, tokenSortScore), levenshteinScore);
        }

        /// <summary>
        /// Calculates token set ratio between two strings.
        /// </summary>
        /// <param name="str1">First string.</param>
        /// <param name="str2">Second string.</param>
        /// <returns>Token set similarity score 0-100.</returns>
        private static int CalculateTokenSetRatio(string str1, string str2)
        {
            var tokens1 = GetTokens(str1);
            var tokens2 = GetTokens(str2);

            if (tokens1.Count == 0 && tokens2.Count == 0)
            {
                return 100;
            }

            if (tokens1.Count == 0 || tokens2.Count == 0)
            {
                return 0;
            }

            var intersection = tokens1.Intersect(tokens2, StringComparer.OrdinalIgnoreCase).ToList();
            var union = tokens1.Union(tokens2, StringComparer.OrdinalIgnoreCase).ToList();

            if (union.Count == 0)
            {
                return 0;
            }

            return (int)Math.Round((double)intersection.Count / union.Count * 100);
        }

        /// <summary>
        /// Calculates token sort ratio between two strings.
        /// </summary>
        /// <param name="str1">First string.</param>
        /// <param name="str2">Second string.</param>
        /// <returns>Token sort similarity score 0-100.</returns>
        private static int CalculateTokenSortRatio(string str1, string str2)
        {
            var tokens1 = GetTokens(str1);
            var tokens2 = GetTokens(str2);

            var sorted1 = string.Join(" ", tokens1.OrderBy(t => t, StringComparer.OrdinalIgnoreCase));
            var sorted2 = string.Join(" ", tokens2.OrderBy(t => t, StringComparer.OrdinalIgnoreCase));

            return CalculateLevenshteinRatio(sorted1, sorted2);
        }

        /// <summary>
        /// Calculates Levenshtein distance ratio between two strings.
        /// </summary>
        /// <param name="str1">First string.</param>
        /// <param name="str2">Second string.</param>
        /// <returns>Levenshtein similarity score 0-100.</returns>
        private static int CalculateLevenshteinRatio(string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1) && string.IsNullOrEmpty(str2))
            {
                return 100;
            }

            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
            {
                return 0;
            }

            var distance = CalculateLevenshteinDistance(str1.ToLowerInvariant(), str2.ToLowerInvariant());
            var maxLength = Math.Max(str1.Length, str2.Length);

            if (maxLength == 0)
            {
                return 100;
            }

            return (int)Math.Round((1.0 - (double)distance / maxLength) * 100);
        }

        /// <summary>
        /// Calculates Levenshtein distance between two strings.
        /// </summary>
        /// <param name="str1">First string.</param>
        /// <param name="str2">Second string.</param>
        /// <returns>Edit distance between the strings.</returns>
        private static int CalculateLevenshteinDistance(string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1))
            {
                return string.IsNullOrEmpty(str2) ? 0 : str2.Length;
            }

            if (string.IsNullOrEmpty(str2))
            {
                return str1.Length;
            }

            var matrix = new int[str1.Length + 1][];
            for (var i = 0; i <= str1.Length; i++)
            {
                matrix[i] = new int[str2.Length + 1];
            }

            // Initialize first column and row
            for (var i = 0; i <= str1.Length; i++)
            {
                matrix[i][0] = i;
            }

            for (var j = 0; j <= str2.Length; j++)
            {
                matrix[0][j] = j;
            }

            // Fill the matrix
            for (var i = 1; i <= str1.Length; i++)
            {
                for (var j = 1; j <= str2.Length; j++)
                {
                    var cost = str1[i - 1] == str2[j - 1] ? 0 : 1;

                    matrix[i][j] = Math.Min(
                        Math.Min(matrix[i - 1][j] + 1, matrix[i][j - 1] + 1),
                        matrix[i - 1][j - 1] + cost);
                }
            }

            return matrix[str1.Length][str2.Length];
        }

        /// <summary>
        /// Extracts tokens (words) from a string.
        /// </summary>
        /// <param name="input">Input string.</param>
        /// <returns>List of tokens.</returns>
        private static List<string> GetTokens(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return new List<string>();
            }

            return input.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                       .Where(token => !string.IsNullOrWhiteSpace(token))
                       .ToList();
        }

        /// <summary>
        /// Checks if years match within a reasonable tolerance.
        /// </summary>
        /// <param name="year1">First year.</param>
        /// <param name="year2">Second year.</param>
        /// <returns>True if years are considered matching.</returns>
        private static bool YearsMatch(int? year1, int? year2)
        {
            // If either year is missing, don't use year for matching
            if (!year1.HasValue || !year2.HasValue)
            {
                return false;
            }

            // Allow 1 year difference to account for release date variations
            return Math.Abs(year1.Value - year2.Value) <= 1;
        }

        /// <summary>
        /// Checks for exact matches in provider IDs.
        /// </summary>
        /// <param name="providerIds1">First set of provider IDs.</param>
        /// <param name="providerIds2">Second set of provider IDs.</param>
        /// <returns>True if any provider ID matches exactly.</returns>
        private static bool HasMatchingProviderIds(Dictionary<string, string> providerIds1, Dictionary<string, string> providerIds2)
        {
            if (providerIds1 == null || providerIds2 == null)
            {
                return false;
            }

            foreach (var kvp in providerIds1)
            {
                if (providerIds2.TryGetValue(kvp.Key, out var value) && 
                    string.Equals(kvp.Value, value, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks for any intersection in provider IDs.
        /// </summary>
        /// <param name="providerIds1">First set of provider IDs.</param>
        /// <param name="providerIds2">Second set of provider IDs.</param>
        /// <returns>True if there's any overlap in provider IDs.</returns>
        private static bool HasProviderIdIntersection(Dictionary<string, string> providerIds1, Dictionary<string, string> providerIds2)
        {
            if (providerIds1 == null || providerIds2 == null)
            {
                return false;
            }

            return providerIds1.Keys.Intersect(providerIds2.Keys, StringComparer.OrdinalIgnoreCase).Any();
        }
    }
}