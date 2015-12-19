using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace KEMT
{
    class ServiceYouTube
    {

        protected MainWindow mw;
        protected string type, url;
        protected string API_DIRECTLINK = "http://popeen.com/api/youtube/direct/?v=";


        public ServiceYouTube(MainWindow mainWindow)
        {
            this.mw = mainWindow;
        }

        public void generate(string type, string url)
        {
            Boolean reset = false;
            string username = url;
            mw.tb_console.Text = "";

            if (url.Contains("/")) //Check that it is a URL and not an id
            {
                mw.c_println("Converting from video URL to user...");
                url = KakaduaUtil.get_between(KakaduaUtil.file_get_contents_utf8(url), "/user/", "\"");
                mw.c_println(url);

                username = url;
            }
            else
            {
                username = url;
            }

            List<Dictionary<string, string>> episodes; //Get a dictionary with all the episodes
            episodes = getEpisodes(username);
            mw.c_println("Loaded " + episodes.Count + " videos in total. Starting generation...");
            
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

                mw.c_println(print);

            }//foreach END

            mw.c_print("Done");

            switch (MessageBox.Show("Do you want to show the files?", mw.Title, MessageBoxButton.YesNo, MessageBoxImage.Question))
            { //Ask if explorer should be opened
                case MessageBoxResult.Yes:
                    Process.Start("explorer.exe", "files\\");
                    break;
            }
        }


        private List<Dictionary<string, string>> getEpisodes(string title)
        {
            String API_KEY = System.IO.File.ReadAllText("API_KEY");

            dynamic user = JsonConvert.DeserializeObject(KakaduaUtil.file_get_contents_utf8("https://www.googleapis.com/youtube/v3/channels?part=contentDetails&forUsername=" + title + "&key=" + API_KEY));
            dynamic json = JsonConvert.DeserializeObject(KakaduaUtil.file_get_contents_utf8("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&playlistId=" + user.items[0].contentDetails.relatedPlaylists.uploads + "&key=" + API_KEY + "&maxResults=50"));

            int i = 0;
            List<Dictionary<string, string>> ret = new List<Dictionary<string, string>>();
            Boolean run = true;
            while (run)
            {

                if (i == 0) { mw.c_println("Loading videos..."); } else { mw.c_println("Loading more videos..."); }
                foreach (var video in json.items)
                { //For every episode on the current page
                    Dictionary<string, string> temp = new Dictionary<string, string>(); //Create a dictionary for the episode and to list
                    temp.Add("title", (string)video.snippet.title);
                    temp.Add("videoId", (string)video.snippet.resourceId.videoId);
                    temp.Add("date", KakaduaUtil.explode("T", (string)video.snippet.publishedAt)[0]);
                    temp.Add("aired", KakaduaUtil.explode("-", temp["date"])[0]);
                    try { temp.Add("img", (string)video.snippet.thumbnails.standard.url); } catch (Exception e) { temp.Add("img", (string)video.snippet.thumbnails.medium.url); } //TODO, Real catch that looks for closest image to standard
                    temp.Add("channel", (string)video.snippet.channelTitle);
                    ret.Add(temp);

                    mw.c_println(temp["title"]);

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



    }
}