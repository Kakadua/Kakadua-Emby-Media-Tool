using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
            if (System.IO.File.ReadAllText("API_KEY") != null && System.IO.File.ReadAllText("API_KEY") != "") { tb_yt_key.Text = System.IO.File.ReadAllText("API_KEY"); }
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
