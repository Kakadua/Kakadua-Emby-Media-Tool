using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace KEMT
{
    class ServiceTV4
    {

        protected MainWindow mw;
        protected string type, url;

        public ServiceTV4(MainWindow mainWindow)
        {
            this.mw = mainWindow;
        }

        public Boolean generate(string type, string url)
        {
            if(type == "strm")
            {
                switch (MessageBox.Show("Keep in mind that videos is only availiable on TV4 Play for a limited time. After this your strm files will stop working. Are you sure you want to generate them instead of downloading the videos?", mw.Title, MessageBoxButton.YesNo, MessageBoxImage.Question))
                { //Ask if explorer should be opened
                    case MessageBoxResult.No:
                        return true;
                }
            }

            mw.c_println("Getting the nid of the show...");
            string raw = KakaduaUtil.file_get_contents_utf8(url);
            string title = KakaduaUtil.get_between(raw, "data-nid=\"", "\"");
            string nid = Uri.EscapeUriString(title);
            mw.c_println(nid);
            List<Dictionary<string, string>> episodes; //Get a dictionary with all the episodes
            episodes = getEpisodes(nid);
            mw.c_println("Loaded " + episodes.Count + " videos in total. Starting generation...");

            if (!Directory.Exists("files/" + KakaduaUtil.make_filename_valid(title))) { DirectoryInfo di = Directory.CreateDirectory("files/" + KakaduaUtil.make_filename_valid(title)); } //Create directory for the show

            int j = 0; //Increment with every loop
            foreach (Dictionary<string, string> episode in episodes)
            {
                string print = "";
                Boolean reset = false;
                j++;
                string num;
                if (j > 99) { num = j.ToString("000"); } else { num = j.ToString("00"); } //Format the number of loops (episodes)

                string folder = "files\\" + KakaduaUtil.make_filename_valid(title);
                string filename = folder + "\\" + num + " - " + KakaduaUtil.make_filename_valid(episode["title"]);

                string xml = "<?xml version=\"1.0\" encoding=\"utf -8\" standalone=\"yes\"?>" + //The nfo file
                             "\n<episodedetails>" +
                                "\n\t<plot>" + episode["plot"] + "</plot>" +
                                "\n\t<title>" + episode["title"] + "</title>" +
                                "\n\t<originaltitle>" + episode["title"] + "</originaltitle>" +
                                "\n\t<aired>" + episode["date"] + "</aired>" +
                             "\n</episodedetails>";

                if (!File.Exists(filename + ".nfo") || reset == false)
                { //Generate the files if they don't exist or it should reset old files
                    if (!Directory.Exists(folder)) { DirectoryInfo di = Directory.CreateDirectory(folder); }
                    if (type == "strm") { System.IO.File.WriteAllText(@filename + ".strm", episode["m3u8"]); }
                    if (type == "dl") { ffmpeg_dl(filename, episode["m3u8"], episode["title"]); }
                    System.IO.File.WriteAllText(@filename + ".nfo", xml);
                    KakaduaUtil.download_file(episode["img"], @filename + "-thumb.jpg");
                    print += "Added: ";
                }
                else { print += "Skipped: "; }

                print += num + " - " + title;

                mw.c_println(print);
            }//foreach END

            mw.c_print("Done");

            switch (MessageBox.Show("Do you want to show the files?", mw.Title, MessageBoxButton.YesNo, MessageBoxImage.Question))
            { //Ask if explorer should be opened
                case MessageBoxResult.Yes:
                    Process.Start("explorer.exe", "files\\");
                    break;
            }
            return true;
        }

        public void ffmpeg_dl(String filename, String m3u8, String title)
        {
            mw.c_println("Downloading " + title + "...");
            KakaduaUtil.run_and_wait("ffmpeg/ffmpeg.exe", "-y -i \"" + m3u8 + "\" -c copy \"" + filename + ".ts\"");
        }



        private List<Dictionary<string, string>> getEpisodes(string nid)
        {

            dynamic json = JsonConvert.DeserializeObject(KakaduaUtil.file_get_contents_utf8("http://webapi.tv4play.se/play/video_assets?type=episode&is_premium=false&node_nids=" + nid + "&platform=tablet&per_page=1000&start=0"));

            List<Dictionary<string, string>> ret = new List<Dictionary<string, string>>();
            int i = 0;
            foreach (var video in json.results)
            {
               // mw.c_println((string)video.title);
                Dictionary<string, string> temp = new Dictionary<string, string>(); //Create a dictionary for the episode and to list
                temp.Add("title", (string)video.title);
                temp.Add("videoId", (string)video.id);
                temp.Add("plot", (string)video.description);
                temp.Add("date", KakaduaUtil.explode("T", (string)video.broadcast_date_time)[0]);
                temp.Add("img", (string)video.image);
                temp.Add("m3u8", KakaduaUtil.get_between(KakaduaUtil.get_between(KakaduaUtil.file_get_contents_utf8("https://prima.tv4play.se/api/web/asset/" + temp["videoId"] + "/play?protocol=hls"), "<mediaFormat>mp4", "</item>"), "<url>", "</url>"));
                ret.Add(temp);

                mw.c_println(i + " - " + temp["title"]);

                i++;
            }//foreach END

            ret.Reverse();
            return ret;
        }
    }
}