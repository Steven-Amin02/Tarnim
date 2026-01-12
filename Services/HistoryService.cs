using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Tarnim.Data;
using Tarnim.Models;

namespace Tarnim.Services;

/// <summary>
/// Service to manage and persist song history.
/// </summary>
public class HistoryService
{
    private const string HistoryFile = "history.json";
    private const int MaxHistoryItems = 20;
    private List<int> _historySongIds;
    private readonly DatabaseService _db;

    public HistoryService(DatabaseService db)
    {
        _db = db;
        _historySongIds = LoadHistory();
    }

    /// <summary>
    /// Adds a song to history. Moves it to the top if already exists.
    /// </summary>
    public void AddToHistory(int songId)
    {
        _historySongIds.Remove(songId); // Remove if exists
        _historySongIds.Insert(0, songId); // Add to top

        if (_historySongIds.Count > MaxHistoryItems)
        {
            _historySongIds.RemoveAt(_historySongIds.Count - 1);
        }

        SaveHistory();
    }

    /// <summary>
    /// Gets the list of recently viewed songs.
    /// </summary>
    public List<Song> GetHistory()
    {
        var songs = new List<Song>();
        foreach (var id in _historySongIds)
        {
            var song = _db.GetSongById(id);
            if (song != null)
            {
                songs.Add(song);
            }
        }
        return songs;
    }

    /// <summary>
    /// Clears the history.
    /// </summary>
    public void ClearHistory()
    {
        _historySongIds.Clear();
        SaveHistory();
    }

    private List<int> LoadHistory()
    {
        try
        {
            if (File.Exists(HistoryFile))
            {
                var json = File.ReadAllText(HistoryFile);
                return JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
            }
        }
        catch
        {
            // Ignore errors
        }
        return new List<int>();
    }

    private void SaveHistory()
    {
        try
        {
            var json = JsonSerializer.Serialize(_historySongIds);
            File.WriteAllText(HistoryFile, json);
        }
        catch
        {
            // Ignore errors
        }
    }
}

