namespace Jellyfin.Plugin.FinDeDupe
{
    using System;
    using System.Collections.Generic;
    using MediaBrowser.Common.Configuration;
    using MediaBrowser.Common.Plugins;
    using MediaBrowser.Model.Plugins;
    using MediaBrowser.Model.Serialization;

    /// <summary>Jellyfin plugin entry point for FinDeDupe.</summary>
    public class FinDeDupePlugin : BasePlugin<Configuration.PluginConfiguration>, IHasWebPages
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinDeDupePlugin"/> class.
        /// </summary>
        /// <param name="applicationPaths">Application paths provider.</param>
        /// <param name="xmlSerializer">XML serializer instance.</param>
        public FinDeDupePlugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
        }

        /// <summary>Gets the plugin display name.</summary>
        public override string Name => "FinDeDupe";

        /// <summary>Gets the plugin unique identifier.</summary>
        public override Guid Id => Guid.Parse("AA3B5C55-4B15-4D3F-8078-D664C80F7D89");

        /// <summary>Gets the plugin description.</summary>
        public override string Description => "Detects and manages duplicate media items.";

        /// <summary>Gets the admin configuration pages exposed by the plugin.</summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="PluginPageInfo"/> describing admin pages.</returns>
        public IEnumerable<PluginPageInfo> GetPages()
        {
            yield return new PluginPageInfo
            {
                Name = "Configuration",
                EmbeddedResourcePath = "Jellyfin.Plugin.FinDeDupe.Configuration.configPage.html",
            };
        }
    }
}
