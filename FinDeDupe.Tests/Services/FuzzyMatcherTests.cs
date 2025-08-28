namespace FinDeDupe.Tests.Services
{
    using System.Collections.Generic;
    using Jellyfin.Plugin.FinDeDupe.Services;
    using Xunit;

    /// <summary>Unit tests for FuzzyMatcher service.</summary>
    public class FuzzyMatcherTests
    {
        [Theory]
        [InlineData("the matrix", "the matrix", 100)]
        [InlineData("the matrix", "matrix the", 100)]
        [InlineData("the matrix", "the matrix reloaded", 67)]
        [InlineData("avatar", "avengers", 29)]
        public void CalculateSimilarity_VariousInputs_ReturnsExpectedScore(string title1, string title2, int expectedScore)
        {
            var score = FuzzyMatcher.CalculateSimilarity(title1, title2);
            Assert.True(Math.Abs(score - expectedScore) <= 5, $"Expected ~{expectedScore}, got {score}");
        }

        [Fact]
        public void IsSameTitle_ExactMatch_ReturnsTrue()
        {
            var result = FuzzyMatcher.IsSameTitle(
                "the matrix",
                "the matrix",
                null,
                null,
                new Dictionary<string, string>(),
                new Dictionary<string, string>());

            Assert.True(result);
        }

        [Fact]
        public void IsSameTitle_HighSimilarity_ReturnsTrue()
        {
            var result = FuzzyMatcher.IsSameTitle(
                "the matrix",
                "matrix the",
                null,
                null,
                new Dictionary<string, string>(),
                new Dictionary<string, string>(),
                exactThreshold: 90);

            Assert.True(result);
        }

        [Fact]
        public void IsSameTitle_ConditionalMatchWithSameYear_ReturnsTrue()
        {
            var result = FuzzyMatcher.IsSameTitle(
                "the matrix",
                "the matrix reloaded",
                1999,
                1999,
                new Dictionary<string, string>(),
                new Dictionary<string, string>(),
                exactThreshold: 90,
                conditionalThreshold: 85);

            Assert.False(result); // Should be false because similarity is too low even with year match
        }

        [Fact]
        public void IsSameTitle_ConditionalMatchWithProviderIds_ReturnsTrue()
        {
            var providerIds1 = new Dictionary<string, string> { { "imdb", "tt0133093" } };
            var providerIds2 = new Dictionary<string, string> { { "imdb", "tt0234215" }, { "tmdb", "603" } };

            var result = FuzzyMatcher.IsSameTitle(
                "matrix",
                "the matrix",
                null,
                null,
                providerIds1,
                providerIds2,
                exactThreshold: 90,
                conditionalThreshold: 85);

            Assert.True(result); // Should match due to high similarity
        }

        [Fact]
        public void IsSameTitle_ExactProviderIdMatch_ReturnsTrue()
        {
            var providerIds1 = new Dictionary<string, string> { { "imdb", "tt0133093" } };
            var providerIds2 = new Dictionary<string, string> { { "imdb", "tt0133093" } };

            var result = FuzzyMatcher.IsSameTitle(
                "completely different title",
                "another different title",
                null,
                null,
                providerIds1,
                providerIds2);

            Assert.True(result); // Should match due to identical provider ID
        }

        [Fact]
        public void IsSameTitle_LowSimilarityNoHelpers_ReturnsFalse()
        {
            var result = FuzzyMatcher.IsSameTitle(
                "the matrix",
                "avatar",
                null,
                null,
                new Dictionary<string, string>(),
                new Dictionary<string, string>());

            Assert.False(result);
        }

        [Theory]
        [InlineData("", "", 100)]
        [InlineData("test", "", 0)]
        [InlineData("", "test", 0)]
        public void CalculateSimilarity_EdgeCases_HandlesCorrectly(string title1, string title2, int expectedScore)
        {
            var score = FuzzyMatcher.CalculateSimilarity(title1, title2);
            Assert.Equal(expectedScore, score);
        }

        [Fact]
        public void IsSameTitle_YearToleranceTest_ReturnsCorrectResult()
        {
            // Test 1-year tolerance
            var result1 = FuzzyMatcher.IsSameTitle(
                "test movie",
                "test movie",
                2020,
                2021,
                new Dictionary<string, string>(),
                new Dictionary<string, string>(),
                exactThreshold: 90,
                conditionalThreshold: 85);

            Assert.True(result1); // Within tolerance

            // Test beyond tolerance
            var result2 = FuzzyMatcher.IsSameTitle(
                "test movie",
                "somewhat similar movie",
                2020,
                2023,
                new Dictionary<string, string>(),
                new Dictionary<string, string>(),
                exactThreshold: 90,
                conditionalThreshold: 85);

            Assert.False(result2); // Beyond tolerance and low similarity
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData(null, "")]
        [InlineData("", "")]
        public void IsSameTitle_NullOrEmptyTitles_ReturnsFalse(string? title1, string? title2)
        {
            var result = FuzzyMatcher.IsSameTitle(
                title1!,
                title2!,
                null,
                null,
                new Dictionary<string, string>(),
                new Dictionary<string, string>());

            Assert.False(result);
        }
    }
}