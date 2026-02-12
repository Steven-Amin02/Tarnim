using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Tarnim.Models;
using Tarnim.Services;

namespace Tarnim
{
    /// <summary>
    /// Settings window for customizing app appearance.
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly SettingsService _settingsService;
        private AppSettings _tempSettings;
        private bool _isInitializing = true;

        public AppSettings? ResultSettings { get; private set; }

        public SettingsWindow(SettingsService settingsService)
        {
            InitializeComponent();
            _settingsService = settingsService;

            // Create a copy of current settings for editing
            _tempSettings = new AppSettings
            {
                LyricsFontSize = settingsService.CurrentSettings.LyricsFontSize,
                Theme = settingsService.CurrentSettings.Theme,
                BackgroundColor = settingsService.CurrentSettings.BackgroundColor,
                PanelColor = settingsService.CurrentSettings.PanelColor,
                AccentColor = settingsService.CurrentSettings.AccentColor
            };

            // Initialize UI with current settings
            FontSizeSlider.Value = _tempSettings.LyricsFontSize;
            FontSizeText.Text = _tempSettings.LyricsFontSize.ToString();
            FontPreview.FontSize = _tempSettings.LyricsFontSize;

            // Select the correct color radio button
            SelectColorRadio(_tempSettings.BackgroundColor);

            // Select the correct theme radio button
            if (_tempSettings.Theme == ThemeMode.Light)
                ThemeLight.IsChecked = true;
            else
                ThemeDark.IsChecked = true;

            // Apply initial theme to window
            ApplyThemeToWindow(_tempSettings.Theme);

            // Subscribe to Loaded event to reapply theme after Visual Tree is ready
            Loaded += SettingsWindow_Loaded;

            _isInitializing = false;
        }

        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyThemeToWindow(_tempSettings.Theme);
        }

        private void SelectColorRadio(string hexColor)
        {
            if (ColorRoyalDark.Parent is WrapPanel panel)
            {
                foreach (var child in panel.Children)
                {
                    if (child is RadioButton radio && radio.Tag?.ToString() == hexColor)
                    {
                        radio.IsChecked = true;
                        break;
                    }
                }
            }
        }

        private void FontSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (FontSizeText != null && FontPreview != null)
            {
                int fontSize = (int)FontSizeSlider.Value;
                FontSizeText.Text = fontSize.ToString();
                FontPreview.FontSize = fontSize;
                _tempSettings.LyricsFontSize = fontSize;
            }
        }

        private void ColorRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radio && radio.Tag is string hexColor)
            {
                _tempSettings.BackgroundColor = hexColor;
            }
        }

        private void ThemeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;

            if (ThemeDark != null && ThemeLight != null)
            {
                _tempSettings.Theme = ThemeDark.IsChecked == true ? ThemeMode.Dark : ThemeMode.Light;
                ApplyThemeToWindow(_tempSettings.Theme);
            }
        }

        /// <summary>
        /// Applies theme colors to the Settings window in real-time.
        /// </summary>
        private void ApplyThemeToWindow(ThemeMode theme)
        {
            if (theme == ThemeMode.Light)
            {
                ApplyThemeResources(
                    windowBackground: "#f8fafc",
                    titleBarBackground: "#ffffff",
                    surface: "#ffffff",
                    surfaceAlt: "#e2e8f0",
                    primaryText: "#0f172a",
                    secondaryText: "#64748b",
                    accent: "#d97706",
                    border: "#cbd5e1");
            }
            else
            {
                ApplyThemeResources(
                    windowBackground: "#0f172a",
                    titleBarBackground: "#0f172a",
                    surface: "#1e293b",
                    surfaceAlt: "#334155",
                    primaryText: "#f8fafc",
                    secondaryText: "#94a3b8",
                    accent: "#fbbf24",
                    border: "#475569");
            }

            ApplyThemeExceptions();
        }

        private void ApplyThemeResources(
            string windowBackground,
            string titleBarBackground,
            string surface,
            string surfaceAlt,
            string primaryText,
            string secondaryText,
            string accent,
            string border)
        {
            UpdateBrushResource("SettingsWindowBackgroundBrush", windowBackground);
            UpdateBrushResource("SettingsTitleBarBackgroundBrush", titleBarBackground);
            UpdateBrushResource("SettingsSurfaceBrush", surface);
            UpdateBrushResource("SettingsSurfaceAltBrush", surfaceAlt);
            UpdateBrushResource("SettingsPrimaryTextBrush", primaryText);
            UpdateBrushResource("SettingsSecondaryTextBrush", secondaryText);
            UpdateBrushResource("SettingsAccentBrush", accent);
            UpdateBrushResource("SettingsBorderBrush", border);
        }

        private void UpdateBrushResource(string key, string colorHex)
        {
            if (TryFindResource(key) is SolidColorBrush brush)
            {
                brush.Color = (Color)ColorConverter.ConvertFromString(colorHex);
            }
        }

        /// <summary>
        /// Applies small non-resource exceptions explicitly by control name/type.
        /// </summary>
        private void ApplyThemeExceptions()
        {
            if (TryFindResource("SettingsAccentBrush") is SolidColorBrush accentBrush)
            {
                if (WindowTitleIcon is TextBlock)
                {
                    WindowTitleIcon.Foreground = accentBrush;
                }

                if (SettingsHeaderIcon is TextBlock)
                {
                    SettingsHeaderIcon.Foreground = accentBrush;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ResultSettings = _tempSettings;
            _settingsService.UpdateSettings(_tempSettings);
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ResultSettings = null;
            DialogResult = false;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
    }
}
