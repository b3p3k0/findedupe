namespace FinDeDupe.Tests.Services
{
    using System.Collections.Generic;
    using Jellyfin.Plugin.FinDeDupe.Configuration;
    using Jellyfin.Plugin.FinDeDupe.Models;
    using Jellyfin.Plugin.FinDeDupe.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>Unit tests for ExclusionEngine service.</summary>
    public class ExclusionEngineTests
    {
        private readonly Mock<ILogger<ExclusionEngine>> _mockLogger;
        private readonly ExclusionEngine _exclusionEngine;

        public ExclusionEngineTests()
        {
            _mockLogger = new Mock<ILogger<ExclusionEngine>>();
            _exclusionEngine = new ExclusionEngine(_mockLogger.Object);
        }

        [Fact]
        public void IsExcluded_NullFingerprint_ReturnsTrue()
        {
            var result = _exclusionEngine.IsExcluded(
                null!,
                "library1",
                new ExclusionSettings(),
                new List<string> { "/media" });

            Assert.True(result);
        }

        [Fact]
        public void IsExcluded_PathOutsideLibraryRoots_ReturnsTrue()
        {
            var fingerprint = CreateTestFingerprint("/outside/movie.mkv");
            var libraryRoots = new List<string> { "/media/movies", "/media/tv" };

            var result = _exclusionEngine.IsExcluded(
                fingerprint,
                "library1",
                new ExclusionSettings(),
                libraryRoots);

            Assert.True(result);
        }

        [Fact]
        public void IsExcluded_LibraryIdExcluded_ReturnsTrue()
        {
            var fingerprint = CreateTestFingerprint("/media/movies/test.mkv");
            var exclusionSettings = new ExclusionSettings
            {
                LibraryIds = new List<string> { "excluded-library" }
            };

            var result = _exclusionEngine.IsExcluded(
                fingerprint,
                "excluded-library",
                exclusionSettings,
                new List<string> { "/media" });

            Assert.True(result);
        }

        [Fact]
        public void IsExcluded_PathPrefixExcluded_ReturnsTrue()
        {
            var fingerprint = CreateTestFingerprint("/media/movies/archived/test.mkv");
            var exclusionSettings = new ExclusionSettings
            {
                PathPrefixes = new List<string> { "/media/movies/archived" }
            };

            var result = _exclusionEngine.IsExcluded(
                fingerprint,
                "library1",
                exclusionSettings,
                new List<string> { "/media" });

            Assert.True(result);
        }

        [Fact]
        public void IsExcluded_NoExclusions_ReturnsFalse()
        {
            var fingerprint = CreateTestFingerprint("/media/movies/test.mkv");
            var exclusionSettings = new ExclusionSettings();

            var result = _exclusionEngine.IsExcluded(
                fingerprint,
                "library1",
                exclusionSettings,
                new List<string> { "/media" });

            Assert.False(result);
        }

        [Theory]
        [InlineData("**/Archive/**", "/media/movies/Archive/old-movie.mkv", true)]
        [InlineData("**/Archive/**", "/media/movies/Current/new-movie.mkv", false)]
        [InlineData("*.sample.*", "/media/movies/test.sample.mkv", true)]
        [InlineData("*.sample.*", "/media/movies/test.mkv", false)]
        public void IsExcluded_GlobPatterns_WorksCorrectly(string pattern, string path, bool expectedExcluded)
        {
            var fingerprint = CreateTestFingerprint(path);
            var exclusionSettings = new ExclusionSettings
            {
                GlobPatterns = new List<string> { pattern }
            };

            _exclusionEngine.UpdateExclusionRules(exclusionSettings);

            var result = _exclusionEngine.IsExcluded(
                fingerprint,
                "library1",
                exclusionSettings,
                new List<string> { "/media" });

            Assert.Equal(expectedExcluded, result);
        }

        [Fact]
        public void ValidatePathPrefixes_ValidPaths_ReturnsValidResults()
        {
            var pathPrefixes = new List<string> { "/media/movies", "/media/tv" };
            var libraryRoots = new List<string> { "/media" };

            var results = ExclusionEngine.ValidatePathPrefixes(pathPrefixes, libraryRoots);

            Assert.All(results, r => Assert.True(r.IsValid));
        }

        [Fact]
        public void ValidatePathPrefixes_PathsOutsideRoots_ReturnsInvalidResults()
        {
            var pathPrefixes = new List<string> { "/outside/movies" };
            var libraryRoots = new List<string> { "/media" };

            var results = ExclusionEngine.ValidatePathPrefixes(pathPrefixes, libraryRoots);

            Assert.All(results, r => Assert.False(r.IsValid));
        }

        [Theory]
        [InlineData("**/Archive/**", true)]
        [InlineData("*.mkv", true)]
        [InlineData("**/*.{mkv,mp4}", true)]
        [InlineData("[invalid", false)]
        public void ValidateGlobPatterns_VariousPatterns_ReturnsExpectedValidity(string pattern, bool expectedValid)
        {
            var patterns = new List<string> { pattern };

            var results = ExclusionEngine.ValidateGlobPatterns(patterns);

            Assert.Single(results);
            Assert.Equal(expectedValid, results[0].IsValid);
        }

        [Fact]
        public void ValidateGlobPatterns_EmptyOrNullPatterns_HandlesGracefully()
        {
            var emptyResults = ExclusionEngine.ValidateGlobPatterns(new List<string>());
            var nullResults = ExclusionEngine.ValidateGlobPatterns(null);

            Assert.Empty(emptyResults);
            Assert.Empty(nullResults);
        }

        private static MediaFingerprint CreateTestFingerprint(string path)
        {
            var key = new MediaKey(Guid.NewGuid(), MediaType.Movie);
            return new MediaFingerprint(
                key,
                "Test Movie",
                "test movie",
                2023,
                new Dictionary<string, string>(),
                path,
                "/media/movies",
                1024 * 1024 * 1024); // 1GB
        }
    }
}