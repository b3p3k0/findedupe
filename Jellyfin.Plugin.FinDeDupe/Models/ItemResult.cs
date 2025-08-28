namespace Jellyfin.Plugin.FinDeDupe.Models
{
    /// <summary>Result of processing a single media item.</summary>
    public sealed class ItemResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemResult"/> class.
        /// </summary>
        /// <param name="key">The media key.</param>
        /// <param name="title">The media title.</param>
        /// <param name="path">The file path.</param>
        /// <param name="action">The action performed.</param>
        /// <param name="status">The result status.</param>
        /// <param name="message">Additional message or error details.</param>
        /// <param name="bytesFreed">Bytes freed by the operation, if applicable.</param>
        public ItemResult(
            MediaKey key,
            string title,
            string path,
            string action,
            string status,
            string message,
            long? bytesFreed = null)
        {
            Key = key;
            Title = title;
            Path = path;
            Action = action;
            Status = status;
            Message = message;
            BytesFreed = bytesFreed;
        }

        /// <summary>Gets the media key.</summary>
        public MediaKey Key { get; }

        /// <summary>Gets the media title.</summary>
        public string Title { get; }

        /// <summary>Gets the file path.</summary>
        public string Path { get; }

        /// <summary>Gets the action performed.</summary>
        public string Action { get; }

        /// <summary>Gets the result status.</summary>
        public string Status { get; }

        /// <summary>Gets the message or error details.</summary>
        public string Message { get; }

        /// <summary>Gets the bytes freed, if applicable.</summary>
        public long? BytesFreed { get; }
    }
}
