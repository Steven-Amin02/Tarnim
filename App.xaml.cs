using System;
using System.IO;
using System.Windows;
using Tarnim.Data;
using Tarnim.Services;

namespace Tarnim;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Global Database Service instance.
    /// </summary>
    public static DatabaseService? Database { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize the database
        Database = new DatabaseService();

        // Auto-import songs from JSON if database is empty
        if (Database.GetSongCount() == 0)
        {
            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "song.json");
            if (File.Exists(jsonPath))
            {
                try
                {
                    var importService = new ImportService(Database);
                    int count = importService.ImportFromJson(jsonPath);
                    MessageBox.Show($"Successfully imported {count} songs into the database.",
                                    "Import Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing songs: {ex.Message}",
                                    "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Show Splash Screen
        var splash = new SplashScreen();
        splash.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Database?.Dispose();
        base.OnExit(e);
    }
}
