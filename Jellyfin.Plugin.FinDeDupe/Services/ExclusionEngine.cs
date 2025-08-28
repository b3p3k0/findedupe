namespace Jellyfin.Plugin.FinDeDupe.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Jellyfin.Plugin.FinDeDupe.Configuration;
    using Jellyfin.Plugin.FinDeDupe.Models;
    using Microsoft.Extensions.Logging;

    /// <summary>Service for determining if media items should be excluded from scanning and deletion.</summary>
    public class ExclusionEngine
    {
        private readonly ILogger<ExclusionEngine> _logger;
        private readonly List<Regex> _compiledGlobPatterns;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExclusionEngine"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public ExclusionEngine(ILogger<ExclusionEngine> logger)
        {
            _logger = logger;
            _compiledGlobPatterns = new List<Regex>();
        }

        /// <summary>
        /// Updates the exclusion rules from configuration.
        /// </summary>
        /// <param name="exclusionSettings">The exclusion settings to apply.</param>
        public void UpdateExclusionRules(ExclusionSettings exclusionSettings)
        {
            _compiledGlobPatterns.Clear();

            if (exclusionSettings?.GlobPatterns != null)
            {
                foreach (var pattern in exclusionSettings.GlobPatterns)
                {
                    try
                    {
                        var regex = ConvertGlobToRegex(pattern);
                        _compiledGlobPatterns.Add(regex);
                        _logger.LogDebug("Compiled glob pattern: {Pattern} -> {Regex}", pattern, regex.ToString());
                    }
                    catch (ArgumentException ex)
                    {
                        _logger.LogWarning(ex, "Invalid glob pattern ignored: {Pattern}", pattern);
                    }
                }
            }
        }

        /// <summary>
        /// Determines if a media item should be excluded from processing.
        /// </summary>
        /// <param name="fingerprint">The media fingerprint to check.</param>
        /// <param name="libraryId">The library ID of the item.</param>
        /// <param name="exclusionSettings">The current exclusion settings.</param>
        /// <param name="libraryRoots">Valid library root paths for security validation.</param>
        /// <returns>True if the item should be excluded.</returns>
        public bool IsExcluded(
            MediaFingerprint fingerprint,
            string libraryId,
            ExclusionSettings exclusionSettings,
            IReadOnlyCollection<string> libraryRoots)
        {
            if (fingerprint == null)
            {
                return true;
            }

            // Path security validation - ensure path is under a library root
            if (!IsPathUnderLibraryRoots(fingerprint.Path, libraryRoots))
            {
                _logger.LogWarning("Path outside library roots excluded for security: {Path}", fingerprint.Path);
                return true;
            }

            if (exclusionSettings == null)
            {
                return false;
            }

            // Check library ID exclusion
            if (IsLibraryExcluded(libraryId, exclusionSettings.LibraryIds))
            {
                _logger.LogDebug("Item excluded by library ID: {LibraryId}", libraryId);
                return true;
            }

            // Check path prefix exclusion
            if (IsPathExcluded(fingerprint.Path, exclusionSettings.PathPrefixes))
            {
                _logger.LogDebug("Item excluded by path prefix: {Path}", fingerprint.Path);
                return true;
            }

            // Check glob pattern exclusion
            if (IsGlobExcluded(fingerprint.Path))
            {
                _logger.LogDebug("Item excluded by glob pattern: {Path}", fingerprint.Path);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Validates path prefixes for security and existence.
        /// </summary>
        /// <param name="pathPrefixes">Path prefixes to validate.</param>
        /// <param name="libraryRoots">Valid library root paths.</param>
        /// <returns>List of validation results.</returns>
        public static List<ValidationResult> ValidatePathPrefixes(
            IEnumerable<string> pathPrefixes,
            IReadOnlyCollection<string> libraryRoots)
        {
            var results = new List<ValidationResult>();

            if (pathPrefixes == null)
            {
                return results;
            }

            foreach (var prefix in pathPrefixes)
            {
                if (string.IsNullOrWhiteSpace(prefix))
                {
                    results.Add(new ValidationResult(prefix, false, "Path prefix cannot be empty"));
                    continue;
                }

                try
                {
                    var normalizedPrefix = Path.GetFullPath(prefix);

                    // Check if path is under library roots
                    if (!IsPathUnderLibraryRoots(normalizedPrefix, libraryRoots))
                    {
                        results.Add(new ValidationResult(prefix, false, "Path prefix must be under a configured library root"));
                        continue;
                    }

                    // Check if path exists
                    var exists = Directory.Exists(normalizedPrefix) || File.Exists(normalizedPrefix);
                    var message = exists ? "Valid" : "Path does not exist but will be accepted";

                    results.Add(new ValidationResult(prefix, true, message));
                }
                catch (Exception ex)
                {
                    results.Add(new ValidationResult(prefix, false, $"Invalid path: {ex.Message}"));
                }
            }

            return results;
        }

        /// <summary>
        /// Validates glob patterns for syntax correctness.
        /// </summary>
        /// <param name="globPatterns">Glob patterns to validate.</param>
        /// <returns>List of validation results.</returns>
        public static List<ValidationResult> ValidateGlobPatterns(IEnumerable<string> globPatterns)
        {
            var results = new List<ValidationResult>();

            if (globPatterns == null)
            {
                return results;
            }

            foreach (var pattern in globPatterns)
            {
                if (string.IsNullOrWhiteSpace(pattern))
                {
                    results.Add(new ValidationResult(pattern, false, "Glob pattern cannot be empty"));
                    continue;
                }

                try
                {
                    ConvertGlobToRegex(pattern);
                    results.Add(new ValidationResult(pattern, true, "Valid glob pattern"));
                }
                catch (ArgumentException ex)
                {
                    results.Add(new ValidationResult(pattern, false, $"Invalid glob pattern: {ex.Message}"));
                }
            }

            return results;
        }

        /// <summary>
        /// Checks if a library ID is in the exclusion list.
        /// </summary>
        /// <param name="libraryId">Library ID to check.</param>
        /// <param name="excludedLibraryIds">List of excluded library IDs.</param>
        /// <returns>True if excluded.</returns>
        private static bool IsLibraryExcluded(string libraryId, ICollection<string> excludedLibraryIds)
        {
            if (string.IsNullOrWhiteSpace(libraryId) || excludedLibraryIds == null)
            {
                return false;
            }

            return excludedLibraryIds.Contains(libraryId, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if a path starts with any excluded prefix.
        /// </summary>
        /// <param name="path">Path to check.</param>
        /// <param name="excludedPrefixes">List of excluded path prefixes.</param>
        /// <returns>True if excluded.</returns>
        private static bool IsPathExcluded(string path, ICollection<string> excludedPrefixes)
        {
            if (string.IsNullOrWhiteSpace(path) || excludedPrefixes == null)
            {
                return false;
            }

            try
            {
                var normalizedPath = Path.GetFullPath(path);

                return excludedPrefixes.Any(prefix =>
                {
                    if (string.IsNullOrWhiteSpace(prefix))
                    {
                        return false;
                    }

                    try
                    {
                        var normalizedPrefix = Path.GetFullPath(prefix);
                        return normalizedPath.StartsWith(normalizedPrefix, StringComparison.OrdinalIgnoreCase);
                    }
                    catch
                    {
                        return false;
                    }
                });
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if a path matches any compiled glob pattern.
        /// </summary>
        /// <param name="path">Path to check.</param>
        /// <returns>True if excluded by glob pattern.</returns>
        private bool IsGlobExcluded(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || _compiledGlobPatterns.Count == 0)
            {
                return false;
            }

            try
            {
                var normalizedPath = Path.GetFullPath(path).Replace('\\', '/');

                return _compiledGlobPatterns.Any(regex => regex.IsMatch(normalizedPath));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates that a path is under one of the allowed library roots.
        /// </summary>
        /// <param name="path">Path to validate.</param>
        /// <param name="libraryRoots">Allowed library root paths.</param>
        /// <returns>True if path is under a library root.</returns>
        private static bool IsPathUnderLibraryRoots(string path, IReadOnlyCollection<string> libraryRoots)
        {
            if (string.IsNullOrWhiteSpace(path) || libraryRoots == null || libraryRoots.Count == 0)
            {
                return false;
            }

            try
            {
                var normalizedPath = Path.GetFullPath(path);

                return libraryRoots.Any(root =>
                {
                    if (string.IsNullOrWhiteSpace(root))
                    {
                        return false;
                    }

                    try
                    {
                        var normalizedRoot = Path.GetFullPath(root);
                        return normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
                    }
                    catch
                    {
                        return false;
                    }
                });
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Converts a glob pattern to a regular expression.
        /// </summary>
        /// <param name="globPattern">Glob pattern with * and ** wildcards.</param>
        /// <returns>Compiled regular expression.</returns>
        private static Regex ConvertGlobToRegex(string globPattern)
        {
            if (string.IsNullOrWhiteSpace(globPattern))
            {
                throw new ArgumentException("Glob pattern cannot be empty", nameof(globPattern));
            }

            // Escape regex special characters except * and ?
            var escaped = Regex.Escape(globPattern)
                              .Replace(@"\*", "STAR_PLACEHOLDER")
                              .Replace(@"\?", "QUESTION_PLACEHOLDER");

            // Replace ** with match-any-path pattern
            escaped = escaped.Replace("STAR_PLACEHOLDER/STAR_PLACEHOLDER", ".*", StringComparison.Ordinal)
                            .Replace("STAR_PLACEHOLDER\\STAR_PLACEHOLDER", ".*", StringComparison.Ordinal);

            // Replace single * with match-any-filename pattern
            escaped = escaped.Replace("STAR_PLACEHOLDER", "[^/\\\\]*", StringComparison.Ordinal);

            // Replace ? with single character match
            escaped = escaped.Replace("QUESTION_PLACEHOLDER", ".", StringComparison.Ordinal);

            // Normalize path separators to forward slashes for matching
            escaped = escaped.Replace(@"\\", "/", StringComparison.Ordinal);

            // Make case-insensitive and compile
            return new Regex($"^{escaped}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
    }

    /// <summary>Result of validating an exclusion rule.</summary>
    public sealed class ValidationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResult"/> class.
        /// </summary>
        /// <param name="value">The value that was validated.</param>
        /// <param name="isValid">Whether the value is valid.</param>
        /// <param name="message">Validation message.</param>
        public ValidationResult(string value, bool isValid, string message)
        {
            Value = value;
            IsValid = isValid;
            Message = message;
        }

        /// <summary>Gets the validated value.</summary>
        public string Value { get; }

        /// <summary>Gets a value indicating whether the value is valid.</summary>
        public bool IsValid { get; }

        /// <summary>Gets the validation message.</summary>
        public string Message { get; }
    }
}