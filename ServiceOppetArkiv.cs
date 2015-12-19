using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;

namespace KEMT
{
    class ServiceOppetArkiv
    {

        protected MainWindow mw;
        protected string type, url;


        public ServiceOppetArkiv(MainWindow mainWindow)
        {
            this.mw = mainWindow;
        }


        public void ffmpeg_dl(String filename, String m3u8, String title)
        {
            mw.c_println("Downloading " + title + "...");
            KakaduaUtil.run_and_wait("ffmpeg/ffmpeg.exe", "-y -i \"" + m3u8 + "\" -c copy \"" + filename + ".ts\"");
        }


        public void generate(string type, string url)
        {
            this.type = type;
            this.url = url;

            Boolean reset = false;
            string show;
            mw.tb_console.Text = ""; //Reset the console


            if (url.Contains("/")) //Check that it is a URL and not an id
            {
                if (url.Contains("/video/")) //Check if episode URL
                {
                    mw.c_println("Converting from episode URL to series URL...");
                    string t1 = KakaduaUtil.get_between(KakaduaUtil.file_get_contents_utf8(url), "<dd class=\"svtoa-dd\">", "</dd>");
                    url = "http://www.oppetarkiv.se" + KakaduaUtil.get_between(t1, "<a href=\"", "\"");
                    mw.c_println(url);
                }

                string[] t2 = url.Split('/'); //Get the show id from the URL
                show = t2[5];
            }
            else
            {
                show = url;
            }


            List<Dictionary<string, string>> episodes; //Get a dictionary with all the episodes
            episodes = getEpisodes(show);
            mw.c_println("Loaded " + episodes.Count + " episodes in total. Starting generation...");


            show = KakaduaUtil.urldecode(show); //Decode the id so it can be used as name

            if (!Directory.Exists("files/" + show)) { DirectoryInfo di = Directory.CreateDirectory("files/" + show); } //Create directory for the show

            int j = 0; //Increment with every loop
            foreach (Dictionary<string, string> episode in episodes)
            {
                string print = "";

                dynamic json = JsonConvert.DeserializeObject(KakaduaUtil.file_get_contents_utf8(episode["url"] + "?output=json")); //Get info about the episode, Using the JSON.net library

                string title = json.context.title;
                string thumb = episode["cover"];
                string year = episode["year"];
                string date = episode["aired"];
                string m3u8 = json.video.videoReferences[1].url;
                string[] temp = KakaduaUtil.file_get_contents_utf8(m3u8).Split(new[] { '\r', '\n' });
                m3u8 = temp[temp.Length - 2];
                string plot = "";

                j++;
                string num;
                if (j > 99) { num = j.ToString("000"); } else { num = j.ToString("00"); } //Format the number of loops (episodes)
                
                string folder = "files\\" + KakaduaUtil.make_filename_valid(show);
                string filename = folder + "\\" + num + " - " + KakaduaUtil.make_filename_valid(title);

                string xml = "<?xml version=\"1.0\" encoding=\"utf -8\" standalone=\"yes\"?>" + //The nfo file
                             "\n<episodedetails>" +
                                "\n\t<plot>" + plot + "</plot>" +
                                "\n\t<title>" + title + "</title>" +
                                "\n\t<originaltitle>" + title + "</originaltitle>" +
                                "\n\t<year>" + year + "</year>" +
                                "\n\t<aired>" + date + "</aired>" +
                             "\n</episodedetails>";
                
                if (!File.Exists(filename + ".nfo") || reset == false)
                { //Generate the files if they don't exist or it should reset old files
                    if (!Directory.Exists(folder)) { DirectoryInfo di = Directory.CreateDirectory(folder); }
                    if (type == "strm") { System.IO.File.WriteAllText(@filename + ".strm", m3u8); }
                    if (type == "dl") { ffmpeg_dl(filename, m3u8, title); }
                    System.IO.File.WriteAllText(@filename + ".nfo", xml);
                    KakaduaUtil.download_file(thumb, @filename + "-thumb.jpg");
                    print += "Added: ";
                }
                else { print += "Skipped: "; }
                
                print += num + " - " + title;

                mw.c_println(print);

            }

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

            int page, i;
            page = i = 1;

            Regex r = new Regex("article(.*?)/article>");
            string raw = WebUtility.HtmlDecode(KakaduaUtil.file_get_contents_utf8("http://www.oppetarkiv.se/etikett/titel/" + title + "/?sida=" + page + "&sort=tid_stigande&embed=true"));

            List<Dictionary<string, string>> ret = new List<Dictionary<string, string>>();
            while (!raw.Contains("error"))
            { //Loop every page with episodes

                if (page == 1) { mw.c_println("Loading episodes..."); }
                else { mw.c_println("Loading more episodes..."); }

                foreach (Match episodeGroup in r.Matches(Regex.Escape(raw)))
                { //For every episode on the current page

                    string episode = Regex.Unescape(episodeGroup.Groups[0].Value);

                    Dictionary<string, string> temp = new Dictionary<string, string>(); //Create a dictionary for the episode and to list
                    temp.Add("title", WebUtility.HtmlDecode(KakaduaUtil.get_between(episode, "alt=\"", "\"").Replace("\\", "")));
                    temp.Add("cover", "http:" + KakaduaUtil.get_between(episode, "oaImg\" src=\"", "\""));
                    temp.Add("year", KakaduaUtil.get_between(episode, "datetime=\"", "-"));
                    temp.Add("aired", KakaduaUtil.get_between(episode, "datetime=\"", "T"));
                    temp.Add("url", "http://www.oppetarkiv.se" + KakaduaUtil.get_between(episode, " href=\"", "\""));
                    ret.Add(temp);

                    mw.c_println(i + " - " + temp["title"]);

                    i++;

                }//foreach END

                page++;

                raw = Regex.Escape(KakaduaUtil.file_get_contents_utf8("http://www.oppetarkiv.se/etikett/titel/" + title + "/?sida=" + page + "&sort=tid_stigande&embed=true"));

            }//while END

            return ret;

        }//load_show_oppetarkiv END
    }
}