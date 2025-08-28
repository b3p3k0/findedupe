namespace Jellyfin.Plugin.FinDeDupe.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>Result of a delete operation execution.</summary>
    public sealed class DeleteResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteResult"/> class.
        /// </summary>
        /// <param name="runId">Unique identifier for this run.</param>
        /// <param name="started">When the operation started.</param>
        /// <param name="ended">When the operation ended, if completed.</param>
        /// <param name="requested">Number of items requested for deletion.</param>
        /// <param name="deleted">Number of items successfully deleted.</param>
        /// <param name="skipped">Number of items skipped.</param>
        /// <param name="failed">Number of items that failed to delete.</param>
        /// <param name="items">Detailed results for each item.</param>
        /// <param name="csvPath">Path to the CSV log file.</param>
        /// <param name="operationMode">The operation mode used.</param>
        public DeleteResult(
            Guid runId,
            DateTimeOffset started,
            DateTimeOffset? ended,
            int requested,
            int deleted,
            int skipped,
            int failed,
            List<ItemResult> items,
            string csvPath,
            OperationMode operationMode)
        {
            RunId = runId;
            Started = started;
            Ended = ended;
            Requested = requested;
            Deleted = deleted;
            Skipped = skipped;
            Failed = failed;
            Items = items;
            CsvPath = csvPath;
            OperationMode = operationMode;
        }

        /// <summary>Gets the unique run identifier.</summary>
        public Guid RunId { get; }

        /// <summary>Gets when the operation started.</summary>
        public DateTimeOffset Started { get; }

        /// <summary>Gets when the operation ended.</summary>
        public DateTimeOffset? Ended { get; }

        /// <summary>Gets the number of items requested for deletion.</summary>
        public int Requested { get; }

        /// <summary>Gets the number of items successfully deleted.</summary>
        public int Deleted { get; }

        /// <summary>Gets the number of items skipped.</summary>
        public int Skipped { get; }

        /// <summary>Gets the number of items that failed to delete.</summary>
        public int Failed { get; }

        /// <summary>Gets the detailed results for each item.</summary>
        public List<ItemResult> Items { get; }

        /// <summary>Gets the path to the CSV log file.</summary>
        public string CsvPath { get; }

        /// <summary>Gets the operation mode used.</summary>
        public OperationMode OperationMode { get; }

        /// <summary>Gets whether the operation is completed.</summary>
        public bool IsCompleted => Ended.HasValue;

        /// <summary>Gets the total bytes freed from all successful operations.</summary>
        public long TotalBytesFreed
        {
            get
            {
                long total = 0;
                foreach (var item in Items)
                {
                    if (item.BytesFreed.HasValue)
                    {
                        total += item.BytesFreed.Value;
                    }
                }

                return total;
            }
        }
    }
}