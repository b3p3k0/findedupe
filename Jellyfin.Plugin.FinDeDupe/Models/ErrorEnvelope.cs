namespace Jellyfin.Plugin.FinDeDupe.Models
{
    /// <summary>Standard error response envelope for API operations.</summary>
    public sealed class ErrorEnvelope
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorEnvelope"/> class.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="message">Human-readable error message.</param>
        /// <param name="details">Additional error details, if any.</param>
        public ErrorEnvelope(string code, string message, object? details = null)
        {
            Code = code;
            Message = message;
            Details = details;
        }

        /// <summary>Gets the error code.</summary>
        public string Code { get; }

        /// <summary>Gets the human-readable error message.</summary>
        public string Message { get; }

        /// <summary>Gets additional error details.</summary>
        public object? Details { get; }
    }
}
