namespace Jellyfin.Plugin.FinDeDupe.Models
{
    using System.Collections.Generic;

    /// <summary>Fingerprint data for a media item used in duplicate detection.</summary>
    public sealed class MediaFingerprint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaFingerprint"/> class.
        /// </summary>
        /// <param name="key">The media key.</param>
        /// <param name="titleRaw">The original title.</param>
        /// <param name="titleNorm">The normalized title for comparison.</param>
        /// <param name="year">The release year, if available.</param>
        /// <param name="providerIds">External provider IDs (TVDB, TMDB, IMDB, etc.).</param>
        /// <param name="path">The file system path to the media.</param>
        /// <param name="rootFolder">The library root folder.</param>
        /// <param name="bytes">The file size in bytes, if available.</param>
        public MediaFingerprint(
            MediaKey key,
            string titleRaw,
            string titleNorm,
            int? year,
            Dictionary<string, string> providerIds,
            string path,
            string rootFolder,
            long? bytes)
        {
            Key = key;
            TitleRaw = titleRaw;
            TitleNorm = titleNorm;
            Year = year;
            ProviderIds = providerIds;
            Path = path;
            RootFolder = rootFolder;
            Bytes = bytes;
        }

        /// <summary>Gets the media key.</summary>
        public MediaKey Key { get; }

        /// <summary>Gets the original title.</summary>
        public string TitleRaw { get; }

        /// <summary>Gets the normalized title for comparison.</summary>
        public string TitleNorm { get; }

        /// <summary>Gets the release year, if available.</summary>
        public int? Year { get; }

        /// <summary>Gets the external provider IDs.</summary>
        public Dictionary<string, string> ProviderIds { get; }

        /// <summary>Gets the file system path.</summary>
        public string Path { get; }

        /// <summary>Gets the library root folder.</summary>
        public string RootFolder { get; }

        /// <summary>Gets the file size in bytes, if available.</summary>
        public long? Bytes { get; }
    }
}
