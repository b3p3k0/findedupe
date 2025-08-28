namespace Jellyfin.Plugin.FinDeDupe.Configuration
{
    using System.Collections.Generic;

    /// <summary>Settings for excluding content from scanning and deletion.</summary>
    public class ExclusionSettings
    {
        /// <summary>Gets or sets the library IDs to exclude from scanning.</summary>
        public ICollection<string> LibraryIds { get; set; } = new List<string>();

        /// <summary>Gets or sets the path prefixes to exclude (absolute paths).</summary>
        public ICollection<string> PathPrefixes { get; set; } = new List<string>();

        /// <summary>Gets or sets the glob patterns to exclude.</summary>
        public ICollection<string> GlobPatterns { get; set; } = new List<string>();
    }
}