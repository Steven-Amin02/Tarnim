using System.Collections.Generic;
using Tarnim.Data;
using Tarnim.Models;
using Tarnim.Utils;

namespace Tarnim.Services
{
    /// <summary>
    /// Service for searching songs in the database.
    /// Handles multiple search scenarios: number lookup, text search, and mixed queries.
    /// </summary>
    public class SearchService
    {
        private readonly DatabaseService _db;

        public SearchService(DatabaseService db)
        {
            _db = db;
        }

        /// <summary>
        /// Main search method that handles all search scenarios.
        /// </summary>
        /// <param name="query">User's search input (can be number, Arabic text, or mixed).</param>
        /// <returns>List of matching songs.</returns>
        public List<Song> Search(string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                // Return all songs if query is empty
                return _db.GetAllSongs();
            }

            query = query.Trim();

            // Scenario 1: Pure numeric query (song number lookup)
            if (ArabicNormalizer.IsNumericOnly(query))
            {
                if (int.TryParse(query, out int songNumber))
                {
                    return _db.GetSongsByNumber(songNumber);
                }
            }

            // Scenario 2: Text search (Arabic or mixed)
            // Normalize the query for matching against normalized database content
            string normalizedQuery = ArabicNormalizer.Normalize(query);

            if (string.IsNullOrWhiteSpace(normalizedQuery))
            {
                return new List<Song>();
            }

            // Use FTS5 search
            var results = _db.SearchByText(normalizedQuery);

            // If no FTS results but query contains a number, also try number lookup
            if (results.Count == 0 && ContainsNumber(query, out int extractedNumber))
            {
                return _db.GetSongsByNumber(extractedNumber);
            }

            return results;
        }

        /// <summary>
        /// Search with snippet context - returns the line where the match occurred.
        /// </summary>
        public List<SearchResult> SearchWithSnippets(string? query)
        {
            var songs = Search(query);
            var results = new List<SearchResult>();

            if (string.IsNullOrWhiteSpace(query))
            {
                // Return all songs without snippets
                foreach (var song in songs)
                {
                    results.Add(new SearchResult
                    {
                        Song = song,
                        Query = query ?? "",
                        MatchingLine = GetFirstLine(song.Lyrics),
                        MatchLineNumber = 1
                    });
                }
                return results;
            }

            string normalizedQuery = ArabicNormalizer.Normalize(query.Trim());

            foreach (var song in songs)
            {
                var result = new SearchResult
                {
                    Song = song,
                    Query = query
                };

                // Find the line containing the match
                var lines = song.Lyrics.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    string normalizedLine = ArabicNormalizer.Normalize(lines[i]);
                    if (normalizedLine.Contains(normalizedQuery, System.StringComparison.OrdinalIgnoreCase))
                    {
                        result.MatchingLine = lines[i].Trim();
                        result.MatchLineNumber = i + 1;
                        break;
                    }
                }

                // If no specific line match, use title or first line
                if (string.IsNullOrEmpty(result.MatchingLine))
                {
                    string normalizedTitle = ArabicNormalizer.Normalize(song.Title);
                    if (normalizedTitle.Contains(normalizedQuery, System.StringComparison.OrdinalIgnoreCase))
                    {
                        result.MatchingLine = $"🏷️ {song.Title}";
                    }
                    else
                    {
                        result.MatchingLine = GetFirstLine(song.Lyrics);
                    }
                    result.MatchLineNumber = 0;
                }

                results.Add(result);
            }

            return results;
        }

        /// <summary>
        /// Get first non-empty line from lyrics.
        /// </summary>
        private string GetFirstLine(string lyrics)
        {
            if (string.IsNullOrWhiteSpace(lyrics)) return "";
            var lines = lyrics.Split('\n');
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    return trimmed;
            }
            return "";
        }

        /// <summary>
        /// Search by song number only.
        /// </summary>
        public List<Song> SearchByNumber(int number)
        {
            return _db.GetSongsByNumber(number);
        }

        /// <summary>
        /// Search by text in title or lyrics.
        /// </summary>
        public List<Song> SearchByText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<Song>();

            string normalizedQuery = ArabicNormalizer.Normalize(text);
            return _db.SearchByText(normalizedQuery);
        }

        /// <summary>
        /// Get all songs (for initial listing).
        /// </summary>
        public List<Song> GetAllSongs()
        {
            return _db.GetAllSongs();
        }

        /// <summary>
        /// Get a specific song by its database ID.
        /// </summary>
        public Song? GetSongById(int id)
        {
            return _db.GetSongById(id);
        }

        /// <summary>
        /// Check if query contains a number and extract it.
        /// </summary>
        private bool ContainsNumber(string query, out int number)
        {
            number = 0;
            var match = System.Text.RegularExpressions.Regex.Match(query, @"\d+");
            if (match.Success)
            {
                return int.TryParse(match.Value, out number);
            }
            return false;
        }
    }
}
