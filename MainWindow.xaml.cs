using System;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;

namespace KEMT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window{


        public MainWindow(){
            InitializeComponent();
            tb_console.IsReadOnly = true;

        }




        //--- Print to TextBox console START ---///

        public void c_print(String str){
            tb_console.AppendText(str);
            tb_console.ScrollToEnd();
            KakaduaUtil.update_frame();
        }

        public void c_println(String str){
            c_print(str+"\n");
        }

        //--- Print to TextBox console STOP ---///




        //---Navbar START ---//

        private void open_settings(object sender, RoutedEventArgs e)
        {
            Settings settingsWindow = new Settings();
            settingsWindow.Show();
        }

        private void open_about(object sender, RoutedEventArgs e)
        {
            About aboutWindow = new About();
            aboutWindow.Show();
        }

        private void close(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        //--- Navbar STOP ---//




        //--- Select onClick URL bar START ---///

        private void SelectAddress(object sender, RoutedEventArgs e){
            TextBox tb = (sender as TextBox);
            if(tb != null){
                tb.SelectAll();
            }
        }

        private void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e){
            TextBox tb = (sender as TextBox);
            if(tb != null){
                if(!tb.IsKeyboardFocusWithin){
                    e.Handled = true;
                    tb.Focus();
                }
            }
        }

        //--- Select onClick URL bar STOP ---///

        
            
        
        //--- Controls START ---///

        private void click_dl(object sender, RoutedEventArgs e) //Download button
        {
            if (!File.Exists("ffmpeg/ffmpeg.exe"))//If ffmpeg has not been downloaded
            {
                c_println("Downloading 7zip...");//Download and extract 7zip cli. Needed to extract ffmpeg
                KakaduaUtil.download_file("http://www.7-zip.org/a/7za920.zip", "7za920.zip");
                KakaduaUtil.unzip("7za920.zip", "7zip");
                c_println("Extracting 7zip...");
                File.Move("7za920.zip", "7zip/7za920.zip");

                String ffmpeg = KakaduaUtil.file_get_contents_utf8("http://ffmpeg.zeranoe.com/builds/"); //Get link to latest stable ffmpeg build
                ffmpeg = "http://ffmpeg.zeranoe.com/builds" + KakaduaUtil.get_between(ffmpeg, "<a class=\"latest\" href=\".", "\"");

                c_println("Downloading ffmpeg...");//Download and extract ffmpeg
                KakaduaUtil.download_file(ffmpeg, "ffmpeg.7z");
                c_println("Extracting ffmpeg...");
                KakaduaUtil.run_and_wait("7zip/7za.exe", "e ffmpeg.7z -offmpeg ffmpeg.exe -r");
                File.Move("ffmpeg.7z", "ffmpeg/ffmpeg.7z");
            }//if END


            String url = tb_url.Text;
            if (url.Contains("oppetarkiv.se"))//Öppet Arkiv
            {
                ServiceOppetArkiv oppetArkiv = new ServiceOppetArkiv(this);
                oppetArkiv.generate("dl", url);
            }//Öppet Arkiv END
            else if (url.Contains("youtube"))//Youtube
            {
                String no_key = "You need to set a YouTube API key in the settings";
                if (File.Exists("API_KEY"))
                {
                    if(System.IO.File.ReadAllText("API_KEY") != null && System.IO.File.ReadAllText("API_KEY") != "")
                    {
                        ServiceYouTube youTube = new ServiceYouTube(this);
                        youTube.generate("strm", url);
                    }
                    else { c_println(no_key);  }
                }
                else { c_println(no_key);  }
            }//YouTube END
            else if (url.Contains("tv4play.se"))//TV4 Play
            {
                ServiceTV4 tv4 = new ServiceTV4(this);
                tv4.generate("dl", url);
            }//TV4 Play END
            else
            {
                c_println("Usupported URL / Service");
            }

        }

        private void click_strm(object sender, RoutedEventArgs e) //Generate strm button
        {
            String url = tb_url.Text;
            if (url.Contains("oppetarkiv.se"))//Öppet Arkiv
            {
                ServiceOppetArkiv oppetArkiv = new ServiceOppetArkiv(this);
                oppetArkiv.generate("strm", url);
            }//Öppet Arkiv END
            else if(url.Contains("youtube"))//Youtube
            {
                String no_key = "You need to set a YouTube API key in the settings";
                if (File.Exists("API_KEY"))
                {
                    if (System.IO.File.ReadAllText("API_KEY") != null && System.IO.File.ReadAllText("API_KEY") != "") {
                        ServiceYouTube youTube = new ServiceYouTube(this);
                        youTube.generate("strm", url);
                    }
                    else { c_println(no_key); }
                }
                else { c_println(no_key); }
            }//YouTube END
            else if (url.Contains("tv4play.se"))//TV4 Play
            {
                ServiceTV4 tv4 = new ServiceTV4(this);
                tv4.generate("strm", url);
            }//TV4 Play END
            else
            {
                c_println("Usupported URL / Service");
            }
        }

        //--- Controls START ---///




        //--- Other Clicks START ---///

        private void web_licence(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Kakadua/Kakadua-Emby-Media-Tool/blob/master/LICENSE");
        }

        private void web_github(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Kakadua/Kakadua-Emby-Media-Tool");
        }
        
        //--- Other Clicks STOP ---///

            

    }
}
