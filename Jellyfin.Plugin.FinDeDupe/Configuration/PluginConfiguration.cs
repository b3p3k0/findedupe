namespace Jellyfin.Plugin.FinDeDupe.Configuration
{
    using MediaBrowser.Model.Plugins;

    /// <summary>Configuration root for the FinDeDupe plugin.</summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>Gets or sets a value indicating whether the plugin is enabled.</summary>
        public bool Enabled { get; set; } = true;
    }
}
