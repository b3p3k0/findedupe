namespace Jellyfin.Plugin.FinDeDupe.Models
{
    /// <summary>Enumeration of operation modes for duplicate management.</summary>
    public enum OperationMode
    {
        /// <summary>Dry run mode - preview operations without executing (default).</summary>
        DryRun,

        /// <summary>Execute mode - perform actual deletion operations.</summary>
        Execute,
    }
}