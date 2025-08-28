namespace Jellyfin.Plugin.FinDeDupe.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>A plan for deleting duplicate media items.</summary>
    public sealed class DeletePlan
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeletePlan"/> class.
        /// </summary>
        /// <param name="planId">Unique identifier for this plan.</param>
        /// <param name="mediaType">The media type for this plan.</param>
        /// <param name="keeper">The media item to keep.</param>
        /// <param name="toDelete">The list of items to delete.</param>
        /// <param name="totalBytes">Total bytes that will be freed.</param>
        /// <param name="foldersToRemovePreview">Folders that may be removed if empty.</param>
        /// <param name="operationMode">The operation mode (dry-run or execute).</param>
        public DeletePlan(
            Guid planId,
            MediaType mediaType,
            MediaKey keeper,
            List<MediaKey> toDelete,
            long totalBytes,
            List<string> foldersToRemovePreview,
            OperationMode operationMode)
        {
            PlanId = planId;
            MediaType = mediaType;
            Keeper = keeper;
            ToDelete = toDelete;
            TotalBytes = totalBytes;
            FoldersToRemovePreview = foldersToRemovePreview;
            OperationMode = operationMode;
        }

        /// <summary>Gets the unique plan identifier.</summary>
        public Guid PlanId { get; }

        /// <summary>Gets the media type for this plan.</summary>
        public MediaType MediaType { get; }

        /// <summary>Gets the media item to keep.</summary>
        public MediaKey Keeper { get; }

        /// <summary>Gets the list of items to delete.</summary>
        public List<MediaKey> ToDelete { get; }

        /// <summary>Gets the total bytes that will be freed.</summary>
        public long TotalBytes { get; }

        /// <summary>Gets the folders that may be removed if empty.</summary>
        public List<string> FoldersToRemovePreview { get; }

        /// <summary>Gets the operation mode for this plan.</summary>
        public OperationMode OperationMode { get; }

        /// <summary>Gets the count of items to be deleted.</summary>
        public int ItemCount => ToDelete.Count;
    }
}
