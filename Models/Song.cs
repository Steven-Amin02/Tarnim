namespace Tarnim.Models
{
    /// <summary>
    /// Represents a Christian Hymn/Song.
    /// </summary>
    public class Song
    {
        /// <summary>
        /// Database Primary Key (Auto-increment).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Song number as per the hymnal book.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Title of the song (Arabic).
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Full lyrics of the song, with newline characters for verse separation.
        /// </summary>
        public string Lyrics { get; set; } = string.Empty;

        /// <summary>
        /// Normalized title for search (without diacritics).
        /// </summary>
        public string NormalizedTitle { get; set; } = string.Empty;

        /// <summary>
        /// Normalized lyrics for search (without diacritics).
        /// </summary>
        public string NormalizedLyrics { get; set; } = string.Empty;

        /// <summary>
        /// Optional: Musical Key (e.g., "Fm", "G").
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// Optional: Category (e.g., "Praise", "Hymn").
        /// </summary>
        public string? Category { get; set; }
    }
}

