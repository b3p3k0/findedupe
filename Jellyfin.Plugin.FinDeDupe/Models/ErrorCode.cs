namespace Jellyfin.Plugin.FinDeDupe.Models
{
    /// <summary>Standard error codes for FinDeDupe operations.</summary>
    public static class ErrorCode
    {
        /// <summary>Item not found in Jellyfin library.</summary>
        public const string NotFound = "NotFound";

        /// <summary>Permission denied for file system operation.</summary>
        public const string PermissionDenied = "PermissionDenied";

        /// <summary>Item excluded by configuration rules.</summary>
        public const string Excluded = "Excluded";

        /// <summary>Conflicting operation in progress.</summary>
        public const string Conflict = "Conflict";

        /// <summary>Invalid input or configuration.</summary>
        public const string InvalidInput = "InvalidInput";

        /// <summary>Path validation failed - outside library roots.</summary>
        public const string PathValidationFailed = "PathValidationFailed";

        /// <summary>Operation cancelled by user or timeout.</summary>
        public const string Cancelled = "Cancelled";

        /// <summary>Internal server error occurred.</summary>
        public const string InternalError = "InternalError";
    }
}
