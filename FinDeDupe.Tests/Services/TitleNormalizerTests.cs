namespace FinDeDupe.Tests.Services
{
    using Jellyfin.Plugin.FinDeDupe.Services;
    using Xunit;

    /// <summary>Unit tests for TitleNormalizer service.</summary>
    public class TitleNormalizerTests
    {
        [Theory]
        [InlineData("The Matrix", "the matrix")]
        [InlineData("  The  Matrix  ", "the matrix")]
        [InlineData("THE MATRIX", "the matrix")]
        public void Normalize_BasicTitles_ReturnsNormalizedTitle(string input, string expected)
        {
            var result = TitleNormalizer.Normalize(input);
            Assert.Equal(expected, result.NormalizedTitle);
        }

        [Theory]
        [InlineData("The Matrix [1999] [1080p]", "the matrix")]
        [InlineData("Avatar (2009) (Extended Edition)", "avatar")]
        [InlineData("Inception {BluRay} {x264}", "inception")]
        public void Normalize_BracketedContent_RemovesBrackets(string input, string expectedTitle)
        {
            var result = TitleNormalizer.Normalize(input);
            Assert.Equal(expectedTitle, result.NormalizedTitle);
            Assert.True(result.HadBracketedContent);
        }

        [Theory]
        [InlineData("The Matrix Remastered", "the matrix")]
        [InlineData("Blade Runner Director's Cut", "blade runner")]
        [InlineData("Avatar Extended Edition", "avatar")]
        [InlineData("Movie 1080p BluRay x264", "movie")]
        public void Normalize_EditionTags_RemovesEditionTags(string input, string expectedTitle)
        {
            var result = TitleNormalizer.Normalize(input);
            Assert.Equal(expectedTitle, result.NormalizedTitle);
            Assert.True(result.HadEditionTag);
        }

        [Theory]
        [InlineData("The Godfather Part II", "the godfather part 2")]
        [InlineData("Rocky II", "rocky 2")]
        [InlineData("Home Alone Two", "home alone 2")]
        [InlineData("Star Wars Episode IV", "star wars episode 4")]
        public void Normalize_SequelNumbers_NormalizesToDigits(string input, string expected)
        {
            var result = TitleNormalizer.Normalize(input);
            Assert.Equal(expected, result.NormalizedTitle);
        }

        [Theory]
        [InlineData("Spider-Man: No Way Home", "spider man no way home")]
        [InlineData("Mission: Impossible", "mission impossible")]
        [InlineData("X-Men: Days of Future Past", "x men days of future past")]
        public void Normalize_Punctuation_RemovesPunctuation(string input, string expected)
        {
            var result = TitleNormalizer.Normalize(input);
            Assert.Equal(expected, result.NormalizedTitle);
        }

        [Theory]
        [InlineData("Die Hard", "Die Hard 2", true)]
        [InlineData("The Matrix", "The Matrix Reloaded", true)]
        [InlineData("Avatar", "Avengers", false)]
        [InlineData("Star Wars", "Star Trek", false)]
        public void AreFromSameSeries_VariousInputs_ReturnsExpectedResult(string title1, string title2, bool expected)
        {
            var result = TitleNormalizer.AreFromSameSeries(title1, title2);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("The Lord of the Rings: The Fellowship of the Ring", "lord of the rings fellowship of the ring")]
        [InlineData("Harry Potter and the Philosopher's Stone", "harry potter and the philosopher s stone")]
        [InlineData("Fast & Furious", "fast furious")]
        public void Normalize_ComplexTitles_HandlesCorrectly(string input, string expected)
        {
            var result = TitleNormalizer.Normalize(input);
            Assert.Equal(expected, result.NormalizedTitle);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData(null, "")]
        [InlineData("   ", "")]
        public void Normalize_EmptyOrWhitespace_ReturnsEmpty(string? input, string expected)
        {
            var result = TitleNormalizer.Normalize(input!);
            Assert.Equal(expected, result.NormalizedTitle);
        }

        [Theory]
        [InlineData("Amélie", "amelie")] // French accent
        [InlineData("Tokyo Drift", "tokyo drift")] // No special characters
        [InlineData("Crouching Tiger, Hidden Dragon", "crouching tiger hidden dragon")]
        public void Normalize_InternationalTitles_HandlesCorrectly(string input, string expected)
        {
            var result = TitleNormalizer.Normalize(input);
            Assert.Equal(expected, result.NormalizedTitle);
        }
    }
}