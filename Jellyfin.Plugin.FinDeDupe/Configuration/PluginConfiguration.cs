namespace Jellyfin.Plugin.FinDeDupe.Configuration
{
    using Jellyfin.Plugin.FinDeDupe.Models;
    using MediaBrowser.Model.Plugins;

    /// <summary>Configuration root for the FinDeDupe plugin.</summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>Gets or sets a value indicating whether the plugin is enabled.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Gets or sets the default operation mode (dry-run by default).</summary>
        public OperationMode DefaultOperationMode { get; set; } = OperationMode.DryRun;

        /// <summary>Gets or sets the log retention period in days.</summary>
        public int LogRetentionDays { get; set; } = 45;

        /// <summary>Gets or sets the fuzzy matching threshold for exact matches (0-100).</summary>
        public int ExactMatchThreshold { get; set; } = 90;

        /// <summary>Gets or sets the fuzzy matching threshold for conditional matches (0-100).</summary>
        public int ConditionalMatchThreshold { get; set; } = 85;

        /// <summary>Gets or sets the page size for scanning operations.</summary>
        public int ScanPageSize { get; set; } = 50;

        /// <summary>Gets or sets the exclusion settings.</summary>
        public ExclusionSettings Exclusions { get; set; } = new ExclusionSettings();
    }
}
