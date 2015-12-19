using System;
using System.Windows;
using System.Windows.Controls;

namespace KEMT
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            try { if (System.IO.File.ReadAllText("API_KEY") != null && System.IO.File.ReadAllText("API_KEY") != "") { tb_yt_key.Text = System.IO.File.ReadAllText("API_KEY"); } } catch(Exception e) {}
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }


        private void update(object sender, RoutedEventArgs e)
        {
            System.IO.File.WriteAllText("API_KEY", tb_yt_key.Text);
            MessageBox.Show("Settings have been updated.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void get_key(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://developers.google.com/youtube/registering_an_application");
        }
    }
}
