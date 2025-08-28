namespace Jellyfin.Plugin.FinDeDupe.Models
{
    using System;

    /// <summary>Unique identifier for a media item.</summary>
    public sealed class MediaKey : IEquatable<MediaKey>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaKey"/> class.
        /// </summary>
        /// <param name="itemId">The Jellyfin item identifier.</param>
        /// <param name="mediaType">The media type.</param>
        public MediaKey(Guid itemId, MediaType mediaType)
        {
            ItemId = itemId;
            MediaType = mediaType;
        }

        /// <summary>Gets the Jellyfin item identifier.</summary>
        public Guid ItemId { get; }

        /// <summary>Gets the media type.</summary>
        public MediaType MediaType { get; }

        /// <summary>Determines whether two MediaKey instances are equal.</summary>
        /// <param name="other">The other MediaKey to compare.</param>
        /// <returns>True if equal, false otherwise.</returns>
        public bool Equals(MediaKey? other)
        {
            return other is not null && ItemId.Equals(other.ItemId) && MediaType == other.MediaType;
        }

        /// <summary>Determines whether this instance and a specified object are equal.</summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>True if equal, false otherwise.</returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as MediaKey);
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A hash code for the current MediaKey.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(ItemId, MediaType);
        }

        /// <summary>Returns a string representation of the MediaKey.</summary>
        /// <returns>String representation.</returns>
        public override string ToString()
        {
            return $"{MediaType}:{ItemId}";
        }
    }
}