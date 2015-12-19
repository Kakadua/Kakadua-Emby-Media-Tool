using System;
using System.Collections.Generic;
using System.Windows;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;
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

        
        
        
        private List<Dictionary<string, string>> load_show_oppetarkiv(String title){

            int page, i;
            page = i = 1;

            Regex r = new Regex("article(.*?)/article>");
            String raw = WebUtility.HtmlDecode(KakaduaUtil.file_get_contents_utf8("http://www.oppetarkiv.se/etikett/titel/" + title + "/?sida=" + page + "&sort=tid_stigande&embed=true"));
            
            List<Dictionary<string, string>> ret = new List<Dictionary<string, string>>();
            while(!raw.Contains("error")){ //Loop every page with episodes

                if (page == 1) { c_println("Loading episodes..."); }
                else { c_println("Loading more episodes..."); }

                foreach(Match episodeGroup in r.Matches(Regex.Escape(raw))){ //For every episode on the current page

                    String episode = Regex.Unescape(episodeGroup.Groups[0].Value);
                    
                    Dictionary<string, string> temp = new Dictionary<string, string>(); //Create a dictionary for the episode and to list
                    temp.Add("title", WebUtility.HtmlDecode(KakaduaUtil.get_between(episode, "alt=\"", "\"").Replace("\\", "")));
                    temp.Add("cover", "http:" + KakaduaUtil.get_between(episode, "oaImg\" src=\"", "\""));
                    temp.Add("year", KakaduaUtil.get_between(episode, "datetime=\"", "-"));
                    temp.Add("aired", KakaduaUtil.get_between(episode, "datetime=\"", "T"));
                    temp.Add("url", "http://www.oppetarkiv.se" + KakaduaUtil.get_between(episode, " href=\"", "\""));
                    ret.Add(temp);

                    c_println(i + " - " + temp["title"]);

                    i++;

                }//foreach END

                page++;

                raw = Regex.Escape(KakaduaUtil.file_get_contents_utf8("http://www.oppetarkiv.se/etikett/titel/" + title + "/?sida=" + page + "&sort=tid_stigande&embed=true"));

            }//while END

            return ret;

        }//load_show_oppetarkiv END




        private List<Dictionary<string, string>> load_show_youtube(String title)
        {
            String API_KEY = System.IO.File.ReadAllText("API_KEY");

            dynamic user = JsonConvert.DeserializeObject(KakaduaUtil.file_get_contents_utf8("https://www.googleapis.com/youtube/v3/channels?part=contentDetails&forUsername=" + title + "&key=" + API_KEY));
            dynamic json = JsonConvert.DeserializeObject(KakaduaUtil.file_get_contents_utf8("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&playlistId=" + user.items[0].contentDetails.relatedPlaylists.uploads + "&key=" + API_KEY + "&maxResults=50"));

            int i = 0;
            List<Dictionary<string, string>> ret = new List<Dictionary<string, string>>();
            Boolean run = true;
            while (run)
            {
                
                if (i == 0) { c_println("Loading videos..."); } else { c_println("Loading more videos...");  }
                foreach (var video in json.items)
                { //For every episode on the current page
                      Dictionary<string, string> temp = new Dictionary<string, string>(); //Create a dictionary for the episode and to list
                      temp.Add("title", (string)video.snippet.title);
                      temp.Add("videoId", (string)video.snippet.resourceId.videoId);
                      temp.Add("date", KakaduaUtil.explode("T", (string)video.snippet.publishedAt)[0]);
                      temp.Add("aired", KakaduaUtil.explode("-", temp["date"])[0]);
                      try { temp.Add("img", (string)video.snippet.thumbnails.standard.url);  }catch(Exception e) { temp.Add("img", (string)video.snippet.thumbnails.medium.url); } //TODO, Real catch that looks for closest image to standard
                      temp.Add("channel", (string)video.snippet.channelTitle);
                      ret.Add(temp);

                      c_println(temp["title"]);
                      
                    i++;

                }//foreach END

                if (json.nextPageToken == null) { run = false; }
                else
                {
                    String nextPage = (string)json.nextPageToken;
                    json = JsonConvert.DeserializeObject(KakaduaUtil.file_get_contents_utf8("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&playlistId=" + user.items[0].contentDetails.relatedPlaylists.uploads + "&key=" + API_KEY + "&maxResults=50&pageToken=" + nextPage));
                }
            }//while END

            ret.Reverse();
            return ret;
        }


        public void ffmpeg_dl(String filename, String m3u8, String title){
            c_println("Downloading " + title + "...");
            KakaduaUtil.run_and_wait("ffmpeg/ffmpeg.exe", "-y -i \"" + m3u8 + "\" -c copy \"" + filename + ".ts\"");
        }




        //--- Button/Text Clicks START ---///

        private void click_dl(object sender, RoutedEventArgs e)
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
            if (url.Contains("oppetarkiv.se"))
            {
                oppet_arkiv("dl", url);
            }
            else if (url.Contains("youtube"))
            {
                String no_key = "You need to set a YouTube API key in the settings";
                if (File.Exists("API_KEY"))
                {
                    if(System.IO.File.ReadAllText("API_KEY") != null && System.IO.File.ReadAllText("API_KEY") != ""){ youtube("dl", url); }
                    else { c_println(no_key);  }
                }
                else { c_println(no_key);  }
            }
            else
            {
                c_println("Usupported URL / Service");
            }

        }

        private void click_strm(object sender, RoutedEventArgs e)
        {
            String url = tb_url.Text;
            if (url.Contains("oppetarkiv.se"))
            {
                oppet_arkiv("strm", url);
            }
            else if(url.Contains("youtube"))
            {
                String no_key = "You need to set a YouTube API key in the settings";
                if (File.Exists("API_KEY"))
                {
                    if (System.IO.File.ReadAllText("API_KEY") != null && System.IO.File.ReadAllText("API_KEY") != "") { youtube("strm", url); }
                    else { c_println(no_key); }
                }
                else { c_println(no_key); }
            }
            else
            {
                c_println("Usupported URL / Service");
            }
        }

        private void web_licence(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Kakadua/Kakadua-Emby-Media-Tool/blob/master/LICENSE");
        }

        private void web_github(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Kakadua/Kakadua-Emby-Media-Tool");
        }

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

        //--- Button/Text Clicks STOP ---///


        
        private void youtube(String type, String url)
        {
            Boolean reset = false;
            String username = url;
            tb_console.Text = "";




            if (url.Contains("/")) //Check that it is a URL and not an id
            {
                c_println("Converting from video URL to user...");
                url = KakaduaUtil.get_between(KakaduaUtil.file_get_contents_utf8(url), "/user/", "\"");
                c_println(url);

                username = url;
            }
            else
            {
                username = url;
            }




            String API_DIRECTLINK = "http://popeen.com/api/youtube/direct/?v=";

            List<Dictionary<string, string>> episodes; //Get a dictionary with all the episodes
            episodes = load_show_youtube(username);
            c_println("Loaded " + episodes.Count + " videos in total. Starting generation...");


            if (!Directory.Exists("files/" + username)) { DirectoryInfo di = Directory.CreateDirectory("files/" + username); } //Create directory for the show

            int j = 0; //Increment with every loop
            foreach (Dictionary<string, string> episode in episodes)
            {
                String print = "";
                j++;
                String num;
                if (j > 99) { num = j.ToString("000"); } else { num = j.ToString("00"); } //Format the number of loops (episodes)

                String folder = "files\\" + KakaduaUtil.make_filename_valid(username);
                String filename = folder + "\\" + num + " - " + KakaduaUtil.make_filename_valid(episode["title"]);


                String xml = "<?xml version=\"1.0\" encoding=\"utf -8\" standalone=\"yes\"?>" + //The nfo file
                             "\n<episodedetails>" +
                                "\n\t<plot></plot>" +
                                "\n\t<title>" + episode["title"] + "</title>" +
                                "\n\t<originaltitle>" + episode["title"] + "</originaltitle>" +
                                "\n\t<aired>" + episode["date"] + "</aired>" +
                             "\n</episodedetails>";

                if (!File.Exists(filename + ".nfo") || reset == false)
                { //Generate the files if they don't exist or it should reset old files
                    if (!Directory.Exists(folder)) { DirectoryInfo di = Directory.CreateDirectory(folder); }
                    if (type == "strm") { System.IO.File.WriteAllText(@filename + ".strm", API_DIRECTLINK + episode["videoId"]); }
                    if (type == "dl") { KakaduaUtil.download_file(API_DIRECTLINK + episode["videoId"], filename + ".mp4"); }
                    System.IO.File.WriteAllText(@filename + ".nfo", xml);
                    KakaduaUtil.download_file(episode["img"], @filename + "-thumb.jpg");
                    print += "Added: ";
                }
                else { print += "Skipped: "; }


                print += num + " - " + episode["title"];

                c_println(print);

            }//foreach END

            c_print("Done");

            switch (MessageBox.Show("Do you want to show the files?", Title, MessageBoxButton.YesNo, MessageBoxImage.Question))
            { //Ask if explorer should be opened
                case MessageBoxResult.Yes:
                    Process.Start("explorer.exe", "files\\");
                    break;
            }
        }//youtube END


        private void oppet_arkiv(String type, String url){ //Generate the files and download video if wanted

            Boolean reset = false;
            String show;
            tb_console.Text = ""; //Reset the console


            if (url.Contains("/")) //Check that it is a URL and not an id
            {
                if (url.Contains("/video/")) //Check if episode URL
                {
                    c_println("Converting from episode URL to series URL...");
                    String t1 = KakaduaUtil.get_between(KakaduaUtil.file_get_contents_utf8(url), "<dd class=\"svtoa-dd\">", "</dd>");
                    url = "http://www.oppetarkiv.se" + KakaduaUtil.get_between(t1, "<a href=\"", "\"");
                    c_println(url);
                }

                string[] t2 = url.Split('/'); //Get the show id from the URL
                show = t2[5];
            }
            else
            {
                show = url;
            }
            

            List<Dictionary<string, string>> episodes; //Get a dictionary with all the episodes
            episodes = load_show_oppetarkiv(show);
            c_println("Loaded " + episodes.Count + " episodes in total. Starting generation...");


            show = KakaduaUtil.urldecode(show); //Decode the id so it can be used as name

            if (!Directory.Exists("files/" + show)) { DirectoryInfo di = Directory.CreateDirectory("files/" + show); } //Create directory for the show

            int j = 0; //Increment with every loop
            foreach (Dictionary<string, string> episode in episodes){
                String print = "";

                dynamic json = JsonConvert.DeserializeObject(KakaduaUtil.file_get_contents_utf8(episode["url"] + "?output=json")); //Get info about the episode, Using the JSON.net library

                string title = json.context.title;
                string thumb = episode["cover"];
                string year = episode["year"];
                string date = episode["aired"];
                string m3u8 = json.video.videoReferences[1].url;
                String[] temp = KakaduaUtil.file_get_contents_utf8(m3u8).Split(new[] { '\r', '\n' });
                m3u8 = temp[temp.Length - 2];
                string plot = "";


                j++;
                String num;
                if (j > 99) { num = j.ToString("000"); } else { num = j.ToString("00"); } //Format the number of loops (episodes)


                String folder = "files\\" + KakaduaUtil.make_filename_valid(show);
                String filename = folder + "\\" + num + " - " + KakaduaUtil.make_filename_valid(title);

                String xml = "<?xml version=\"1.0\" encoding=\"utf -8\" standalone=\"yes\"?>" + //The nfo file
                             "\n<episodedetails>" +
                                "\n\t<plot>" + plot + "</plot>" + 
                                "\n\t<title>" + title + "</title>" +
                                "\n\t<originaltitle>" + title + "</originaltitle>" +
                                "\n\t<year>" + year + "</year>" +
                                "\n\t<aired>" + date + "</aired>" +
                             "\n</episodedetails>";


                if (!File.Exists(filename + ".nfo") || reset == false){ //Generate the files if they don't exist or it should reset old files
                    if (!Directory.Exists(folder)) { DirectoryInfo di = Directory.CreateDirectory(folder); }
                    if (type == "strm") { System.IO.File.WriteAllText(@filename + ".strm", m3u8); }
                    if (type == "dl") { ffmpeg_dl(filename, m3u8, title); }
                    System.IO.File.WriteAllText(@filename + ".nfo", xml);
                    KakaduaUtil.download_file(thumb, @filename + "-thumb.jpg");
                    print += "Added: ";
                }
                else{ print += "Skipped: "; }


                print += num + " - " + title;

                c_println(print);

            }

            c_print("Done");            

            switch(MessageBox.Show("Do you want to show the files?", Title, MessageBoxButton.YesNo, MessageBoxImage.Question)){ //Ask if explorer should be opened
                case MessageBoxResult.Yes:
                    Process.Start("explorer.exe", "files\\");
                    break;
            }
        }//oppet_arkiv END

    }
}
