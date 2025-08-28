namespace FinDeDupe.Tests.TestData
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Jellyfin.Plugin.FinDeDupe.Models;
    using Jellyfin.Plugin.FinDeDupe.Services;

    /// <summary>Generates realistic test data for media fingerprints.</summary>
    public static class MediaTestDataGenerator
    {
        /// <summary>Sample movie titles with various edge cases.</summary>
        private static readonly string[] MovieTitles = new[]
        {
            // Basic titles
            "The Matrix",
            "Avatar",
            "Inception",
            "The Dark Knight",
            "Pulp Fiction",
            
            // Sequels with different numbering styles
            "The Godfather",
            "The Godfather Part II",
            "The Godfather: Part III",
            "Rocky",
            "Rocky II",
            "Rocky III",
            "Home Alone",
            "Home Alone 2: Lost in New York",
            
            // Franchise titles
            "Star Wars: Episode IV - A New Hope",
            "Star Wars: Episode V - The Empire Strikes Back",
            "The Lord of the Rings: The Fellowship of the Ring",
            "The Lord of the Rings: The Two Towers",
            
            // Special characters and punctuation
            "Spider-Man: No Way Home",
            "Mission: Impossible",
            "X-Men: Days of Future Past",
            "Fast & Furious",
            "Borat: Cultural Learnings of America for Make Benefit Glorious Nation of Kazakhstan",
            
            // International titles
            "Amélie",
            "Crouching Tiger, Hidden Dragon",
            "Pan's Labyrinth",
            "The Girl with the Dragon Tattoo",
            
            // Edition variations
            "Blade Runner",
            "Blade Runner: Director's Cut",
            "Blade Runner: The Final Cut"
        };

        /// <summary>Sample TV series titles.</summary>
        private static readonly string[] SeriesTitles = new[]
        {
            "Breaking Bad",
            "The Office",
            "Game of Thrones",
            "Stranger Things",
            "The Mandalorian",
            "Better Call Saul",
            "The Crown",
            "Westworld",
            "Lost",
            "Friends",
            "The Sopranos",
            "Mad Men",
            "House of Cards",
            "Ozark",
            "The Wire",
            "Sherlock",
            "Doctor Who",
            "The Big Bang Theory",
            "How I Met Your Mother",
            "Dexter"
        };

        /// <summary>Quality and edition tags to add variation.</summary>
        private static readonly string[] QualityTags = new[]
        {
            "[1080p]",
            "[720p]",
            "[4K]",
            "[2160p]",
            "(BluRay)",
            "(DVD)",
            "[x264]",
            "[x265]",
            "[HEVC]",
            "(Extended Edition)",
            "(Director's Cut)",
            "(Unrated)",
            "(Remastered)"
        };

        /// <summary>File extensions for media files.</summary>
        private static readonly string[] Extensions = new[] { ".mkv", ".mp4", ".avi", ".m4v", ".mov" };

        /// <summary>Provider ID sources.</summary>
        private static readonly string[] ProviderSources = new[] { "imdb", "tmdb", "tvdb", "omdb" };

        /// <summary>
        /// Generates a collection of realistic movie fingerprints.
        /// </summary>
        /// <param name="count">Number of movies to generate.</param>
        /// <param name="includeVariations">Whether to include quality/edition variations.</param>
        /// <param name="basePath">Base path for generated file paths.</param>
        /// <returns>List of movie fingerprints.</returns>
        public static List<MediaFingerprint> GenerateMovies(int count, bool includeVariations = true, string basePath = "/media/movies")
        {
            var random = new Random(42); // Fixed seed for reproducible tests
            var fingerprints = new List<MediaFingerprint>();
            var usedTitles = new HashSet<string>();

            for (var i = 0; i < count; i++)
            {
                var baseTitle = MovieTitles[i % MovieTitles.Length];
                var variation = includeVariations && random.Next(3) == 0 ? QualityTags[random.Next(QualityTags.Length)] : string.Empty;
                var titleWithVariation = string.IsNullOrEmpty(variation) ? baseTitle : $"{baseTitle} {variation}";
                
                // Ensure unique titles by adding suffix if needed
                var finalTitle = titleWithVariation;
                var counter = 1;
                while (usedTitles.Contains(finalTitle))
                {
                    finalTitle = $"{titleWithVariation} ({counter})";
                    counter++;
                }
                usedTitles.Add(finalTitle);

                var fingerprint = CreateMovieFingerprint(finalTitle, baseTitle, basePath, random);
                fingerprints.Add(fingerprint);
            }

            return fingerprints;
        }

        /// <summary>
        /// Generates a collection of realistic TV series fingerprints.
        /// </summary>
        /// <param name="count">Number of series to generate.</param>
        /// <param name="basePath">Base path for generated file paths.</param>
        /// <returns>List of series fingerprints.</returns>
        public static List<MediaFingerprint> GenerateSeries(int count, string basePath = "/media/tv")
        {
            var random = new Random(42); // Fixed seed for reproducible tests
            var fingerprints = new List<MediaFingerprint>();

            for (var i = 0; i < count; i++)
            {
                var title = SeriesTitles[i % SeriesTitles.Length];
                var fingerprint = CreateSeriesFingerprint(title, basePath, random);
                fingerprints.Add(fingerprint);
            }

            return fingerprints;
        }

        /// <summary>
        /// Generates known duplicate groups for testing duplicate detection.
        /// </summary>
        /// <param name="basePath">Base path for generated file paths.</param>
        /// <returns>List of media fingerprints with known duplicates.</returns>
        public static List<MediaFingerprint> GenerateKnownDuplicates(string basePath = "/media/test")
        {
            var random = new Random(42);
            var fingerprints = new List<MediaFingerprint>();

            // Duplicate set 1: The Matrix with different quality tags
            fingerprints.Add(CreateMovieFingerprint("The Matrix [1080p] [BluRay]", "The Matrix", basePath, random));
            fingerprints.Add(CreateMovieFingerprint("The Matrix (1999) [x264]", "The Matrix", basePath, random));
            fingerprints.Add(CreateMovieFingerprint("The Matrix - Director's Cut", "The Matrix", basePath, random));

            // Duplicate set 2: Inception with different paths
            fingerprints.Add(CreateMovieFingerprint("Inception", "Inception", Path.Combine(basePath, "movies1"), random));
            fingerprints.Add(CreateMovieFingerprint("Inception [2160p]", "Inception", Path.Combine(basePath, "movies2"), random));

            // Duplicate set 3: Breaking Bad series
            fingerprints.Add(CreateSeriesFingerprint("Breaking Bad", basePath, random));
            fingerprints.Add(CreateSeriesFingerprint("Breaking Bad (Complete Series)", basePath, random));

            // Non-duplicates for comparison
            fingerprints.Add(CreateMovieFingerprint("Avatar", "Avatar", basePath, random));
            fingerprints.Add(CreateSeriesFingerprint("The Office", basePath, random));

            return fingerprints;
        }

        /// <summary>
        /// Creates a realistic movie fingerprint.
        /// </summary>
        /// <param name="displayTitle">The display title (with quality tags).</param>
        /// <param name="baseTitle">The base title for normalization.</param>
        /// <param name="basePath">Base directory path.</param>
        /// <param name="random">Random generator for variation.</param>
        /// <returns>Movie fingerprint.</returns>
        private static MediaFingerprint CreateMovieFingerprint(string displayTitle, string baseTitle, string basePath, Random random)
        {
            var key = new MediaKey(Guid.NewGuid(), MediaType.Movie);
            var normalizedTitle = TitleNormalizer.Normalize(baseTitle).NormalizedTitle;
            var year = 1990 + random.Next(35); // Years 1990-2024
            var extension = Extensions[random.Next(Extensions.Length)];
            var fileName = SanitizeFileName(displayTitle) + extension;
            var fullPath = Path.Combine(basePath, SanitizeFileName(baseTitle), fileName);
            
            var providerIds = new Dictionary<string, string>();
            if (random.Next(3) > 0) // 66% chance of having provider IDs
            {
                providerIds[ProviderSources[random.Next(ProviderSources.Length)]] = GenerateProviderId(random);
            }

            var bytes = (long)(1024 * 1024 * (500 + random.Next(7500))); // 500MB to 8GB

            return new MediaFingerprint(
                key,
                displayTitle,
                normalizedTitle,
                year,
                providerIds,
                fullPath,
                basePath,
                bytes);
        }

        /// <summary>
        /// Creates a realistic series fingerprint.
        /// </summary>
        /// <param name="title">The series title.</param>
        /// <param name="basePath">Base directory path.</param>
        /// <param name="random">Random generator for variation.</param>
        /// <returns>Series fingerprint.</returns>
        private static MediaFingerprint CreateSeriesFingerprint(string title, string basePath, Random random)
        {
            var key = new MediaKey(Guid.NewGuid(), MediaType.Series);
            var normalizedTitle = TitleNormalizer.Normalize(title).NormalizedTitle;
            var year = 2000 + random.Next(25); // Years 2000-2024
            var seriesPath = Path.Combine(basePath, SanitizeFileName(title));
            
            var providerIds = new Dictionary<string, string>();
            if (random.Next(3) > 0) // 66% chance of having provider IDs
            {
                providerIds["tvdb"] = random.Next(100000, 999999).ToString();
                if (random.Next(2) == 0)
                {
                    providerIds["tmdb"] = random.Next(1000, 99999).ToString();
                }
            }

            var bytes = (long)(1024 * 1024 * (2000 + random.Next(48000))); // 2GB to 50GB (multi-season)

            return new MediaFingerprint(
                key,
                title,
                normalizedTitle,
                year,
                providerIds,
                seriesPath,
                basePath,
                bytes);
        }

        /// <summary>
        /// Sanitizes a title for use as a file/folder name.
        /// </summary>
        /// <param name="title">Title to sanitize.</param>
        /// <returns>Sanitized file name.</returns>
        private static string SanitizeFileName(string title)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return string.Join("_", title.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>
        /// Generates a realistic provider ID.
        /// </summary>
        /// <param name="random">Random generator.</param>
        /// <returns>Provider ID string.</returns>
        private static string GenerateProviderId(Random random)
        {
            return "tt" + random.Next(1000000, 9999999).ToString();
        }

        /// <summary>
        /// Creates test directory structure with sidecar files.
        /// </summary>
        /// <param name="baseTestPath">Base path for test data.</param>
        /// <param name="fingerprints">Media fingerprints to create directories for.</param>
        public static void CreateTestDirectoryStructure(string baseTestPath, List<MediaFingerprint> fingerprints)
        {
            foreach (var fingerprint in fingerprints)
            {
                var directory = Path.GetDirectoryName(fingerprint.Path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Create main media file (empty for testing)
                File.WriteAllText(fingerprint.Path, "test media file content");

                // Create sidecar files
                var basePath = Path.GetFileNameWithoutExtension(fingerprint.Path);
                var sidecarDir = Path.GetDirectoryName(fingerprint.Path)!;

                File.WriteAllText(Path.Combine(sidecarDir, basePath + ".nfo"), "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><movie></movie>");
                File.WriteAllText(Path.Combine(sidecarDir, basePath + ".srt"), "1\n00:00:01,000 --> 00:00:02,000\nTest subtitle");
                File.WriteAllText(Path.Combine(sidecarDir, "poster.jpg"), "fake image data");
            }
        }
    }
}