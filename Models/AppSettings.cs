namespace Tarnim.Models
{
    /// <summary>
    /// Theme modes available in the application.
    /// </summary>
    public enum ThemeMode
    {
        Dark,
        Light
    }

    /// <summary>
    /// Application settings for user preferences.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Font size for lyrics display (default: 22).
        /// </summary>
        public int LyricsFontSize { get; set; } = 22;

        /// <summary>
        /// Current theme mode (Dark or Light).
        /// </summary>
        public ThemeMode Theme { get; set; } = ThemeMode.Dark;

        /// <summary>
        /// Background color for the main window (hex format, default: Royal Dark).
        /// </summary>
        public string BackgroundColor { get; set; } = "#0f172a";

        /// <summary>
        /// Secondary background color for panels (hex format).
        /// </summary>
        public string PanelColor { get; set; } = "#1e293b";

        /// <summary>
        /// Accent color for highlights (hex format).
        /// </summary>
        public string AccentColor { get; set; } = "#fbbf24";
    }
}
