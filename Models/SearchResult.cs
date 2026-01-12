namespace Tarnim.Models;

/// <summary>
/// Represents a search result with contextual snippet information.
/// </summary>
public class SearchResult
{
    /// <summary>
    /// The matched song.
    /// </summary>
    public Song Song { get; set; } = null!;

    /// <summary>
    /// The line containing the matched text.
    /// </summary>
    public string MatchingLine { get; set; } = string.Empty;

    /// <summary>
    /// The search query that was used.
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Line number where the match was found (1-indexed).
    /// </summary>
    public int MatchLineNumber { get; set; }
}
