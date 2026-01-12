using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Tarnim.Data;
using Tarnim.Models;
using Tarnim.Utils;

namespace Tarnim.Services
{
    /// <summary>
    /// Service for importing songs from JSON file into the SQLite database.
    /// </summary>
    public class ImportService
    {
        private readonly DatabaseService _db;

        public ImportService(DatabaseService db)
        {
            _db = db;
        }

        /// <summary>
        /// Imports songs from a JSON file into the database.
        /// Clears existing songs before import.
        /// Normalizes Arabic text for search indexing.
        /// </summary>
        /// <param name="jsonFilePath">Path to the song.json file.</param>
        /// <returns>Number of songs imported.</returns>
        public int ImportFromJson(string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
                throw new FileNotFoundException("JSON file not found.", jsonFilePath);

            string jsonContent = File.ReadAllText(jsonFilePath);
            var songs = JsonSerializer.Deserialize<List<JsonSongEntry>>(jsonContent);

            if (songs == null || songs.Count == 0)
                return 0;

            // Clear existing songs before re-import
            _db.ClearAllSongs();

            int count = 0;
            foreach (var entry in songs)
            {
                string title = entry.song_title ?? string.Empty;
                string lyrics = entry.song_lyrics ?? string.Empty;

                var song = new Song
                {
                    Number = int.Parse(entry.song_number ?? "0"),

                    Title = title,
                    Lyrics = lyrics,
                    // Normalize Arabic text for search
                    NormalizedTitle = ArabicNormalizer.Normalize(title),
                    NormalizedLyrics = ArabicNormalizer.Normalize(lyrics),
                    Key = null,
                    Category = null
                };
                _db.InsertSong(song);
                count++;
            }

            return count;
        }

        /// <summary>
        /// Internal class to match the JSON structure from song.json.
        /// </summary>
        private class JsonSongEntry
        {
            public string? song_number { get; set; }
            public string? song_title { get; set; }
            public string? song_lyrics { get; set; }
        }

    }
}

