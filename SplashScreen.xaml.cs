using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace Tarnim
{
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();
            this.Loaded += SplashScreen_Loaded;
        }

        private async void SplashScreen_Loaded(object sender, RoutedEventArgs e)
        {
            // Start Fade In Animation
            if (TryFindResource("FadeInStoryboard") is Storyboard fadeIn)
            {
                fadeIn.Begin();
            }

            // Simulate Loading / Wait for delay
            await Task.Delay(3000); // 3 seconds splash

            // Fade Out manually for smooth exit
            DoubleAnimation fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5)
            };

            fadeOut.Completed += (s, args) =>
            {
                // Open Main Window
                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            };

            MainBorder.BeginAnimation(OpacityProperty, fadeOut);
        }
    }
}
