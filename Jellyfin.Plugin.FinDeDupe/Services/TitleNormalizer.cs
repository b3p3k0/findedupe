namespace Jellyfin.Plugin.FinDeDupe.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>Service for normalizing media titles for comparison.</summary>
    public static class TitleNormalizer
    {
        /// <summary>Common edition and quality tags to remove from titles.</summary>
        private static readonly string[] EditionTags = new[]
        {
            "remastered", "extended", "director's cut", "uncut", "theatrical", "special edition",
            "ultimate edition", "collector's edition", "anniversary edition", "restored",
            "1080p", "720p", "4k", "2160p", "uhd", "hdr", "x264", "x265", "hevc", "h264", "h265",
            "bluray", "blu-ray", "dvd", "webrip", "web-dl", "hdtv", "pdtv", "cam", "ts", "tc",
            "dvdscr", "brrip", "hdrip", "ac3", "dts", "aac", "mp3", "5.1", "7.1", "atmos"
        };

        /// <summary>Roman numeral to digit mapping for sequel detection.</summary>
        private static readonly Dictionary<string, string> RomanNumerals = new(StringComparer.OrdinalIgnoreCase)
        {
            { "i", "1" },
            { "ii", "2" },
            { "iii", "3" },
            { "iv", "4" },
            { "v", "5" },
            { "vi", "6" },
            { "vii", "7" },
            { "viii", "8" },
            { "ix", "9" },
            { "x", "10" },
            { "xi", "11" },
            { "xii", "12" }
        };

        /// <summary>English number words to digits mapping.</summary>
        private static readonly Dictionary<string, string> NumberWords = new(StringComparer.OrdinalIgnoreCase)
        {
            { "one", "1" },
            { "two", "2" },
            { "three", "3" },
            { "four", "4" },
            { "five", "5" },
            { "six", "6" },
            { "seven", "7" },
            { "eight", "8" },
            { "nine", "9" },
            { "ten", "10" },
            { "eleven", "11" },
            { "twelve", "12" }
        };

        /// <summary>Regex for removing bracketed content.</summary>
        private static readonly Regex BracketedContentRegex = new(@"[\[\(\{][^\]\)\}]*[\]\)\}]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>Regex for detecting sequel patterns at the end of titles.</summary>
        private static readonly Regex SequelPatternRegex = new(@"\s+(part\s+)?(\d+|[ivx]+|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve)(\s|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>Regex for normalizing whitespace and punctuation.</summary>
        private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);
        private static readonly Regex PunctuationRegex = new(@"[^\w\s]", RegexOptions.Compiled);

        /// <summary>
        /// Normalizes a title for comparison purposes.
        /// </summary>
        /// <param name="title">The original title to normalize.</param>
        /// <returns>A normalized title and flags indicating what was changed.</returns>
        public static NormalizationResult Normalize(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return new NormalizationResult(string.Empty, false, false);
            }

            var normalized = title.Trim();
            var hadEditionTag = false;
            var hadBracketedContent = false;

            // Remove bracketed content (quality tags, codec info, etc.)
            var beforeBrackets = normalized;
            normalized = BracketedContentRegex.Replace(normalized, " ").Trim();
            if (normalized != beforeBrackets)
            {
                hadBracketedContent = true;
            }

            // Remove common edition and quality tags
            var lowerNormalized = normalized.ToLowerInvariant();
            foreach (var tag in EditionTags)
            {
                var beforeTag = lowerNormalized;
                lowerNormalized = Regex.Replace(lowerNormalized, @"\b" + Regex.Escape(tag) + @"\b", " ", RegexOptions.IgnoreCase);
                if (lowerNormalized != beforeTag)
                {
                    hadEditionTag = true;
                }
            }

            normalized = lowerNormalized;

            // Normalize punctuation and whitespace
            normalized = PunctuationRegex.Replace(normalized, " ");
            normalized = WhitespaceRegex.Replace(normalized, " ").Trim();

            // Handle sequel numbering at the end of the title
            normalized = NormalizeSequelNumbers(normalized);

            // Final cleanup
            normalized = normalized.ToLowerInvariant();

            return new NormalizationResult(normalized, hadEditionTag, hadBracketedContent);
        }

        /// <summary>
        /// Normalizes sequel numbers at the end of titles (roman numerals, words to digits).
        /// </summary>
        /// <param name="title">The title to process.</param>
        /// <returns>The title with normalized sequel numbers.</returns>
        private static string NormalizeSequelNumbers(string title)
        {
            var match = SequelPatternRegex.Match(title);
            if (!match.Success)
            {
                return title;
            }

            var sequelPart = match.Groups[2].Value.ToLowerInvariant();

            // Check if it's a Roman numeral
            if (RomanNumerals.TryGetValue(sequelPart, out var romanDigit))
            {
                var replacement = match.Groups[1].Success ? $" part {romanDigit}" : $" {romanDigit}";
                return title.Substring(0, match.Index) + replacement;
            }

            // Check if it's an English number word
            if (NumberWords.TryGetValue(sequelPart, out var wordDigit))
            {
                var replacement = match.Groups[1].Success ? $" part {wordDigit}" : $" {wordDigit}";
                return title.Substring(0, match.Index) + replacement;
            }

            // Already a digit, normalize the format
            if (int.TryParse(sequelPart, out var digit))
            {
                var replacement = match.Groups[1].Success ? $" part {digit}" : $" {digit}";
                return title.Substring(0, match.Index) + replacement;
            }

            return title;
        }

        /// <summary>
        /// Checks if two normalized titles should be considered the same for sequel detection.
        /// </summary>
        /// <param name="title1">First normalized title.</param>
        /// <param name="title2">Second normalized title.</param>
        /// <returns>True if they appear to be from the same series/franchise.</returns>
        public static bool AreFromSameSeries(string title1, string title2)
        {
            if (string.IsNullOrWhiteSpace(title1) || string.IsNullOrWhiteSpace(title2))
            {
                return false;
            }

            // Remove sequel numbers and compare base titles
            var baseTitle1 = RemoveSequelNumbers(title1);
            var baseTitle2 = RemoveSequelNumbers(title2);

            return string.Equals(baseTitle1, baseTitle2, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Removes sequel numbers from a title to get the base series name.
        /// </summary>
        /// <param name="title">The title to process.</param>
        /// <returns>The title without sequel numbers.</returns>
        private static string RemoveSequelNumbers(string title)
        {
            var match = SequelPatternRegex.Match(title);
            if (match.Success)
            {
                return title.Substring(0, match.Index).Trim();
            }

            return title;
        }
    }

    /// <summary>Result of title normalization.</summary>
    public sealed class NormalizationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizationResult"/> class.
        /// </summary>
        /// <param name="normalizedTitle">The normalized title.</param>
        /// <param name="hadEditionTag">Whether edition tags were removed.</param>
        /// <param name="hadBracketedContent">Whether bracketed content was removed.</param>
        public NormalizationResult(string normalizedTitle, bool hadEditionTag, bool hadBracketedContent)
        {
            NormalizedTitle = normalizedTitle;
            HadEditionTag = hadEditionTag;
            HadBracketedContent = hadBracketedContent;
        }

        /// <summary>Gets the normalized title.</summary>
        public string NormalizedTitle { get; }

        /// <summary>Gets a value indicating whether edition tags were removed.</summary>
        public bool HadEditionTag { get; }

        /// <summary>Gets a value indicating whether bracketed content was removed.</summary>
        public bool HadBracketedContent { get; }
    }
}