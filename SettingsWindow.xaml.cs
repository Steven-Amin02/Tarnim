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
            if (_tempSettings.Theme == Models.ThemeMode.Light)
                ThemeLight.IsChecked = true;
            else
                ThemeDark.IsChecked = true;

            // Apply initial theme to window (partial - Visual Tree may not be ready)
            ApplyThemeToWindow(_tempSettings.Theme);

            // Subscribe to Loaded event to reapply theme after Visual Tree is ready
            this.Loaded += SettingsWindow_Loaded;

            _isInitializing = false;
        }

        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Reapply theme after Visual Tree is fully loaded
            // This ensures all elements are styled correctly on first open
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
                _tempSettings.Theme = ThemeDark.IsChecked == true ? Models.ThemeMode.Dark : Models.ThemeMode.Light;
                ApplyThemeToWindow(_tempSettings.Theme);
            }
        }

        /// <summary>
        /// Applies theme colors to the Settings window in real-time
        /// </summary>
        private void ApplyThemeToWindow(Models.ThemeMode theme)
        {
            if (theme == Models.ThemeMode.Light)
            {
                ApplyLightTheme();
            }
            else
            {
                ApplyDarkTheme();
            }
        }

        private void ApplyLightTheme()
        {
            try
            {
                // ☀️ Light Theme Color Palette
                var bgColor = (Color)ColorConverter.ConvertFromString("#f8fafc");        // Slate 50
                var surfaceColor = (Color)ColorConverter.ConvertFromString("#ffffff");   // White
                var surfaceAltColor = (Color)ColorConverter.ConvertFromString("#e2e8f0"); // Slate 200
                var textPrimaryColor = (Color)ColorConverter.ConvertFromString("#0f172a"); // Slate 900
                var textSecondaryColor = (Color)ColorConverter.ConvertFromString("#64748b"); // Slate 500
                var accentColor = (Color)ColorConverter.ConvertFromString("#d97706");    // Amber 600
                var borderColor = (Color)ColorConverter.ConvertFromString("#cbd5e1");    // Slate 300

                var bgBrush = new SolidColorBrush(bgColor);
                var surfaceBrush = new SolidColorBrush(surfaceColor);
                var surfaceAltBrush = new SolidColorBrush(surfaceAltColor);
                var textPrimaryBrush = new SolidColorBrush(textPrimaryColor);
                var textSecondaryBrush = new SolidColorBrush(textSecondaryColor);
                var accentBrush = new SolidColorBrush(accentColor);
                var borderBrush = new SolidColorBrush(borderColor);

                // Main window background
                this.Background = bgBrush;

                // Update title bar
                if (this.FindName("titleBar") is Grid titleBar)
                {
                    titleBar.Background = surfaceBrush;
                    foreach (var child in LogicalTreeHelper.GetChildren(titleBar))
                    {
                        if (child is StackPanel sp)
                        {
                            foreach (var btn in sp.Children)
                            {
                                if (btn is Button button)
                                    button.Foreground = textPrimaryBrush;
                            }
                        }
                        else if (child is TextBlock tb)
                        {
                            tb.Foreground = textPrimaryBrush;
                        }
                    }
                }

                // Update main content Grid
                if (this.Content is Grid mainGrid)
                {
                    mainGrid.Background = bgBrush;
                }

                // Update FontSizeText (number display)
                if (FontSizeText != null)
                {
                    FontSizeText.Background = surfaceAltBrush;
                    FontSizeText.Foreground = accentBrush;
                }

                // Update FontPreview (text sample)
                if (FontPreview != null)
                {
                    FontPreview.Background = surfaceAltBrush;
                    FontPreview.Foreground = textSecondaryBrush;
                }

                // Update Slider
                if (FontSizeSlider != null)
                {
                    FontSizeSlider.Background = surfaceAltBrush;
                }

                // Update Theme Radio Buttons
                UpdateRadioButtonTheme(ThemeDark, surfaceAltBrush, borderBrush, textPrimaryBrush);
                UpdateRadioButtonTheme(ThemeLight, surfaceAltBrush, borderBrush, textPrimaryBrush);

                // Update all other TextBlocks using Dispatcher for reliable Visual Tree access
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        UpdateAllTextBlocks("#0f172a");
                    }
                    catch { }
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying light theme: {ex.Message}");
            }
        }

        private void ApplyDarkTheme()
        {
            try
            {
                // 🌙 Dark Theme Color Palette (Royal Serenity)
                var bgColor = (Color)ColorConverter.ConvertFromString("#0f172a");        // Slate 900
                var surfaceColor = (Color)ColorConverter.ConvertFromString("#1e293b");   // Slate 800
                var surfaceAltColor = (Color)ColorConverter.ConvertFromString("#334155"); // Slate 700
                var textPrimaryColor = (Color)ColorConverter.ConvertFromString("#f8fafc"); // Slate 50
                var textSecondaryColor = (Color)ColorConverter.ConvertFromString("#94a3b8"); // Slate 400
                var accentColor = (Color)ColorConverter.ConvertFromString("#fbbf24");    // Amber 400
                var borderColor = (Color)ColorConverter.ConvertFromString("#475569");    // Slate 600

                var bgBrush = new SolidColorBrush(bgColor);
                var surfaceBrush = new SolidColorBrush(surfaceColor);
                var surfaceAltBrush = new SolidColorBrush(surfaceAltColor);
                var textPrimaryBrush = new SolidColorBrush(textPrimaryColor);
                var textSecondaryBrush = new SolidColorBrush(textSecondaryColor);
                var accentBrush = new SolidColorBrush(accentColor);
                var borderBrush = new SolidColorBrush(borderColor);

                // Main window background
                this.Background = bgBrush;

                // Update title bar
                if (this.FindName("titleBar") is Grid titleBar)
                {
                    titleBar.Background = bgBrush;
                    foreach (var child in LogicalTreeHelper.GetChildren(titleBar))
                    {
                        if (child is StackPanel sp)
                        {
                            foreach (var btn in sp.Children)
                            {
                                if (btn is Button button)
                                    button.Foreground = textPrimaryBrush;
                            }
                        }
                        else if (child is TextBlock tb)
                        {
                            tb.Foreground = textPrimaryBrush;
                        }
                    }
                }

                // Update main content Grid
                if (this.Content is Grid mainGrid)
                {
                    mainGrid.Background = bgBrush;
                }

                // Update FontSizeText (number display)
                if (FontSizeText != null)
                {
                    FontSizeText.Background = surfaceAltBrush;
                    FontSizeText.Foreground = accentBrush;
                }

                // Update FontPreview (text sample)
                if (FontPreview != null)
                {
                    FontPreview.Background = surfaceAltBrush;
                    FontPreview.Foreground = textSecondaryBrush;
                }

                // Update Slider
                if (FontSizeSlider != null)
                {
                    FontSizeSlider.Background = surfaceAltBrush;
                }

                // Update Theme Radio Buttons
                UpdateRadioButtonTheme(ThemeDark, surfaceAltBrush, borderBrush, textPrimaryBrush);
                UpdateRadioButtonTheme(ThemeLight, surfaceAltBrush, borderBrush, textPrimaryBrush);

                // Update all other TextBlocks using Dispatcher for reliable Visual Tree access
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        UpdateAllTextBlocks("#f8fafc");
                    }
                    catch { }
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying dark theme: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates a radio button's visual appearance for theme changes
        /// </summary>
        private void UpdateRadioButtonTheme(RadioButton? radio, SolidColorBrush bgBrush, SolidColorBrush borderBrush, SolidColorBrush textBrush)
        {
            if (radio?.Template?.FindName("border", radio) is Border border)
            {
                border.Background = bgBrush;
                border.BorderBrush = borderBrush;
            }
            // Update text inside radio button
            if (radio?.Content is TextBlock tb)
            {
                tb.Foreground = textBrush;
            }
        }

        /// <summary>
        /// Recursively updates all TextBlock foreground colors
        /// </summary>
        private void UpdateAllTextBlocks(string colorHex)
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(colorHex);
                var brush = new SolidColorBrush(color);

                UpdateTextBlocksRecursive(this, brush);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating text blocks: {ex.Message}");
            }
        }

        private void UpdateTextBlocksRecursive(DependencyObject parent, SolidColorBrush brush)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is TextBlock textBlock)
                {
                    // Skip updating hardcoded theme button text colors
                    if (!IsThemeButton(textBlock))
                    {
                        textBlock.Foreground = brush;
                    }
                }

                // Recurse into child elements
                UpdateTextBlocksRecursive(child, brush);
            }
        }

        private bool IsThemeButton(TextBlock textBlock)
        {
            // Keep theme button text (sun/moon emojis) in their original colors for visibility
            string text = textBlock.Text;
            return text.Contains("🌙") || text.Contains("☀️") || text.Contains("⚙️");
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
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
    }
}

