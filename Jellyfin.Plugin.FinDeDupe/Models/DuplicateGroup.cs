namespace Jellyfin.Plugin.FinDeDupe.Models
{
    using System.Collections.Generic;

    /// <summary>A group of potential duplicate media items.</summary>
    public sealed class DuplicateGroup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateGroup"/> class.
        /// </summary>
        /// <param name="groupKey">Unique identifier for this duplicate group.</param>
        /// <param name="mediaType">The media type for all candidates in this group.</param>
        /// <param name="candidates">List of potential duplicate candidates.</param>
        /// <param name="suggestedKeeper">The system's suggested item to keep, if any.</param>
        public DuplicateGroup(
            string groupKey,
            MediaType mediaType,
            List<MediaFingerprint> candidates,
            MediaKey? suggestedKeeper = null)
        {
            GroupKey = groupKey;
            MediaType = mediaType;
            Candidates = candidates;
            SuggestedKeeper = suggestedKeeper;
        }

        /// <summary>Gets the unique group identifier.</summary>
        public string GroupKey { get; }

        /// <summary>Gets the media type for this group.</summary>
        public MediaType MediaType { get; }

        /// <summary>Gets the list of duplicate candidates.</summary>
        public List<MediaFingerprint> Candidates { get; }

        /// <summary>Gets the system's suggested item to keep.</summary>
        public MediaKey? SuggestedKeeper { get; }

        /// <summary>Gets the count of candidates in this group.</summary>
        public int CandidateCount => Candidates.Count;
    }
}
