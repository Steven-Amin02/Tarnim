using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using Tarnim.Models;

namespace Tarnim.Data
{
    /// <summary>
    /// Handles SQLite database connection, initialization, and CRUD operations.
    /// </summary>
    public class DatabaseService : IDisposable
    {
        private readonly string _connectionString;
        private readonly string _dbPath;

        /// <summary>
        /// Initializes the DatabaseService with a path to the database file.
        /// Creates the database file and Songs table if they don't exist.
        /// </summary>
        public DatabaseService(string? dbPath = null)
        {
            _dbPath = dbPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tarnim.db");
            _connectionString = $"Data Source={_dbPath}";
            InitializeDatabase();
        }

        /// <summary>
        /// Creates the Songs table and FTS5 virtual table if they don't already exist.
        /// </summary>
        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string createTableSql = @"
                CREATE TABLE IF NOT EXISTS Songs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Number INTEGER NOT NULL,
                    Title TEXT NOT NULL,
                    Lyrics TEXT NOT NULL,
                    NormalizedTitle TEXT NOT NULL,
                    NormalizedLyrics TEXT NOT NULL,
                    Key TEXT,
                    Category TEXT
                );

                -- Create index on Number for fast lookups by song number
                CREATE INDEX IF NOT EXISTS idx_songs_number ON Songs(Number);
            ";

            using (var command = new SqliteCommand(createTableSql, connection))
            {
                command.ExecuteNonQuery();
            }

            // Create FTS5 virtual table for full-text search
            string createFtsSql = @"
                CREATE VIRTUAL TABLE IF NOT EXISTS Songs_FTS USING fts5(
                    NormalizedTitle,
                    NormalizedLyrics,
                    content='Songs',
                    content_rowid='Id'
                );
            ";

            using (var command = new SqliteCommand(createFtsSql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Opens and returns a new database connection.
        /// </summary>
        public SqliteConnection GetConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Inserts a single song into the database and updates the FTS index.
        /// </summary>
        public void InsertSong(Song song)
        {
            using var connection = GetConnection();

            // Insert into main table
            string insertSql = @"
                INSERT INTO Songs (Number, Title, Lyrics, NormalizedTitle, NormalizedLyrics, Key, Category)
                VALUES (@Number, @Title, @Lyrics, @NormalizedTitle, @NormalizedLyrics, @Key, @Category);
            ";
            using (var command = new SqliteCommand(insertSql, connection))
            {
                command.Parameters.AddWithValue("@Number", song.Number);
                command.Parameters.AddWithValue("@Title", song.Title);
                command.Parameters.AddWithValue("@Lyrics", song.Lyrics);
                command.Parameters.AddWithValue("@NormalizedTitle", song.NormalizedTitle);
                command.Parameters.AddWithValue("@NormalizedLyrics", song.NormalizedLyrics);
                command.Parameters.AddWithValue("@Key", (object?)song.Key ?? DBNull.Value);
                command.Parameters.AddWithValue("@Category", (object?)song.Category ?? DBNull.Value);
                command.ExecuteNonQuery();
            }

            // Get the inserted row ID
            long lastId;
            using (var command = new SqliteCommand("SELECT last_insert_rowid();", connection))
            {
                lastId = (long)command.ExecuteScalar()!;
            }

            // Insert into FTS table
            string insertFtsSql = @"
                INSERT INTO Songs_FTS (rowid, NormalizedTitle, NormalizedLyrics)
                VALUES (@rowid, @NormalizedTitle, @NormalizedLyrics);
            ";
            using (var command = new SqliteCommand(insertFtsSql, connection))
            {
                command.Parameters.AddWithValue("@rowid", lastId);
                command.Parameters.AddWithValue("@NormalizedTitle", song.NormalizedTitle);
                command.Parameters.AddWithValue("@NormalizedLyrics", song.NormalizedLyrics);
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Clears all songs from the database and FTS index.
        /// </summary>
        public void ClearAllSongs()
        {
            using var connection = GetConnection();
            using (var command = new SqliteCommand("DELETE FROM Songs_FTS;", connection))
            {
                command.ExecuteNonQuery();
            }
            using (var command = new SqliteCommand("DELETE FROM Songs;", connection))
            {
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Gets the total count of songs in the database.
        /// </summary>
        public int GetSongCount()
        {
            using var connection = GetConnection();
            using var command = new SqliteCommand("SELECT COUNT(*) FROM Songs;", connection);
            return Convert.ToInt32(command.ExecuteScalar());
        }

        /// <summary>
        /// Gets song by its ID.
        /// </summary>
        public Song? GetSongById(int id)
        {
            using var connection = GetConnection();
            string sql = "SELECT * FROM Songs WHERE Id = @Id;";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapReaderToSong(reader);
            }
            return null;
        }

        /// <summary>
        /// Gets all songs by number (there may be multiple versions).
        /// </summary>
        public List<Song> GetSongsByNumber(int number)
        {
            var songs = new List<Song>();
            using var connection = GetConnection();
            string sql = "SELECT * FROM Songs WHERE Number = @Number ORDER BY Id;";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@Number", number);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                songs.Add(MapReaderToSong(reader));
            }
            return songs;
        }

        /// <summary>
        /// Full-text search using FTS5.
        /// </summary>
        public List<Song> SearchByText(string normalizedQuery)
        {
            var songs = new List<Song>();
            using var connection = GetConnection();

            // Use FTS5 MATCH query with the normalized search term
            string sql = @"
                SELECT s.* FROM Songs s
                INNER JOIN Songs_FTS fts ON s.Id = fts.rowid
                WHERE Songs_FTS MATCH @Query
                ORDER BY rank;
            ";

            using var command = new SqliteCommand(sql, connection);
            // FTS5 query: search in both title and lyrics
            command.Parameters.AddWithValue("@Query", $"\"{normalizedQuery}\"");

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                songs.Add(MapReaderToSong(reader));
            }
            return songs;
        }

        /// <summary>
        /// Gets all songs (for listing).
        /// </summary>
        public List<Song> GetAllSongs()
        {
            var songs = new List<Song>();
            using var connection = GetConnection();
            string sql = "SELECT * FROM Songs ORDER BY Number, Id;";
            using var command = new SqliteCommand(sql, connection);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                songs.Add(MapReaderToSong(reader));
            }
            return songs;
        }

        /// <summary>
        /// Maps a SqliteDataReader row to a Song object.
        /// </summary>
        private static Song MapReaderToSong(SqliteDataReader reader)
        {
            return new Song
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Number = reader.GetInt32(reader.GetOrdinal("Number")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Lyrics = reader.GetString(reader.GetOrdinal("Lyrics")),
                NormalizedTitle = reader.GetString(reader.GetOrdinal("NormalizedTitle")),
                NormalizedLyrics = reader.GetString(reader.GetOrdinal("NormalizedLyrics")),
                Key = reader.IsDBNull(reader.GetOrdinal("Key")) ? null : reader.GetString(reader.GetOrdinal("Key")),
                Category = reader.IsDBNull(reader.GetOrdinal("Category")) ? null : reader.GetString(reader.GetOrdinal("Category"))
            };
        }

        /// <summary>
        /// Deletes the database file (for testing/reset purposes).
        /// </summary>
        public void DeleteDatabaseFile()
        {
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }
        }

        public void Dispose()
        {
            // No persistent connection to dispose
        }
    }
}

