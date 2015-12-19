using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Threading;

class KakaduaUtil{




    public static Boolean unzip(String zip, String extract)
    {
        try
        {
            //Needs to reference System.IO.Compression.FileSystem
            System.IO.Compression.ZipFile.ExtractToDirectory(zip, extract);
            return true;
        }catch(Exception e){
            return false;
        }
    }


    /// <summary>
    /// Check if a string contains a substring, same as String.Contains()
    /// </summary>
    /// <returns>true or false</returns>
    /// <param name="str">The string you want to check</param>
    /// <param name="substr">The string you want to check for</param>
    /// <example>
    /// The following example demonstrates the use of this method.
    /// 
    /// <code language="cs">
    /// String my_string = "Kakadua means Cockatoo in Swedish";
    /// 
    /// if(string_contain(my_string, "Cockatoo")){
    ///     //This code will run
    /// } else{
    ///     //This code will not run
    /// }
    /// </code>
    /// </example>
    public static Boolean string_contain(String str, String substr)
    {
        return str.Contains(substr);
    }




    /// <summary>
    /// Download a file
    /// </summary>
    /// <returns>true if downloaded succesfully otherwise false</returns>
    /// <param name="url">The URL of the file you want to download</param>
    /// <param name="local">The local filename, Can include path if needed</param>
    /// <example>
    /// The following example demonstrates the use of this method.
    /// 
    /// <code language="cs">
    /// //Save image
    /// String url = "http://kakadua.net/someImage.png";
    /// 
    /// KakaduaUtil.download_file(url, "kakadua.png");
    /// KakaduaUtil.download_file(url, "img/kakadua.png");
    /// </code>
    /// </example>
    public static Boolean download_file(String url, String local)
    {
        try
        {
            WebClient client = new WebClient();
            client.DownloadFile(url, local);
            return true;
        }catch(Exception e)
        {
            return false;
        }
    }
    



    /// <summary>
    /// Make a String valid for use as filename
    /// </summary>
    /// <returns>A String containing a valid filename</returns>
    /// <param name="name">The String you want to make valid</param>
    /// <example>
    /// The following example demonstrates the use of this method.
    /// 
    /// <code language="cs">
    /// String name = "Kakadua?.txt"; //Invalid filename
    /// name = KakaduaUtil.make_filename_valid(name); //Now its valid, "Kakadua.txt"
    /// System.IO.File.WriteAllText(name, "Kakadua means Cockatoo in swedish"); //Write to file
    /// </code>
    /// </example>
    public static String make_filename_valid(string name)
    {
        var builder = new StringBuilder();
        var invalid = System.IO.Path.GetInvalidFileNameChars();
        foreach (var cur in name)
        {
            if (!invalid.Contains(cur))
            {
                builder.Append(cur);
            }
        }
        return builder.ToString();
    }
    



    /// <summary>
    /// Remove special characters WIP
    /// </summary>
    /// <returns>A String without special characters</returns>
    /// <param name="str">The String you want to remove characters from</param>
    /// <example>
    /// The following example demonstrates the use of this method.
    /// 
    /// <code language="cs">
    /// String text1 = "ÅÄÖ";
    /// String text2 = "AAO";
    /// 
    /// if(text1 == text2){
    ///     //This code will never run
    /// }
    /// 
    /// if(KakaduaUtil.remove_special_char(text1) == text2){
    ///     //This code will run
    /// }
    /// </code>
    /// </example>
    public static String remove_special_char(string str)
    {
        str = Regex.Replace(str, "[áàäâãå]", "a");
        str = Regex.Replace(str, "[ÁÀÄÂÃÅ]", "A");
        str = Regex.Replace(str, "[óòöôõ]", "o");
        str = Regex.Replace(str, "[ÓÒÖÔÕ]", "O");
        str = Regex.Replace(str, "[éèëê]", "e");
        str = Regex.Replace(str, "[ÉÈËÊ]", "E");
        str = Regex.Replace(str, "[úùüû]", "u");
        str = Regex.Replace(str, "[ÚÙÜÛ]", "U");
        str = Regex.Replace(str, "[íìïî]", "i");
        str = Regex.Replace(str, "[ÍÌÏÎ]", "I");
        return str;
    }
    



    /// <summary>
    /// Gets a substring between two other substrings
    /// </summary>
    /// <returns>A string containing your substring, or empty if no match</returns>
    /// <param name="content">The full string you want to look at</param>
    /// <param name="before">The substring before what you want</param>
    /// <param name="rafte">The substring after what you want</param>
    /// <example>
    /// The following example demonstrates the use of this method.
    /// 
    /// <code language="cs">
    /// String text = "Kakadua means Cockatoo in swedish";
    /// 
    /// String meaing = get_between(text, "means ", " in");
    /// 
    /// //meaning == Cockatoo
    /// </code>
    /// </example>
    public static String get_between(String content, String before, String after)
    {
        String[] temp = KakaduaUtil.explode(before, content);
        try
        {
            if (!String.IsNullOrEmpty(temp[1]))
            {
                temp = explode(after, temp[1]);
                return temp[0];
            }
            else
            {
                return "";
            }
        }catch(Exception e)
        {
            return "";
        }
       
    }




    /// <summary>
    /// Run a process and waith for it to finnish
    /// </summary>
    /// <returns></returns>
    /// <param name="file">Path to the file you want to run</param>
    /// <param name="arguments">Any arguments you want to pass to the file</param>
    /// <example>
    /// The following example demonstrates the use of this method.
    /// 
    /// <code language="cs">
    /// String file = "ffmpeg/ffmpeg.exe";    /// 
    /// String arguments = "-i \"http://kakadua.net/stream.m3u8\"  -c copy \"stream.mp4\"";
    /// 
    /// </code>
    /// </example>
    public static void run_and_wait(String file, String arguments)
    {
        var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = file,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        proc.Start();
        proc.WaitForExit();
    }




    /// <summary>
    /// Force the GUI to update
    /// </summary>
    public static void update_frame()
    {
        DispatcherFrame frame = new DispatcherFrame(true);
        Dispatcher.CurrentDispatcher.BeginInvoke
        (
            DispatcherPriority.Background,
            (SendOrPostCallback)delegate (object arg) {
                var f = arg as DispatcherFrame;
                f.Continue = false;
            },
            frame
        );
        Dispatcher.PushFrame(frame);
    }




    // ----- PHP emulators START ----- //




    /// <summary>
    /// Emulates PHPs explode funtion
    /// </summary>
    /// <returns>An array containing the separated Strings</returns>
    /// <param name="separator">The string you want to use as separator</param>
    /// <param name="source">The string you want to explode</param>
    /// <example>
    /// The following example demonstrates the use of this method.
    /// 
    /// <code language="cs">
    /// String text = "Kakadua means Cockatoo";
    /// 
    /// String[] words = explode(" ", text);
    /// 
    /// //words[0] == Kakadua
    /// //words[1] == means
    /// //words[2] == Cockatoo
    /// </code>
    /// </example>
    public static String[] explode(string separator, string source)
    {
        return source.Split(new string[] { separator }, StringSplitOptions.None);
    }




    /// <summary>
    /// Emulates PHPs file_get_contents. WIP
    /// </summary>
    /// <returns>A String containing the file. Encoded in utf-8, If there is a problem it returns the String "error"</returns>
    /// <param name="url">The URL of the file you want to read</param>
    /// <example>
    /// The following example demonstrates the use of this method.
    /// 
    /// <code language="cs">
    /// //Read site
    /// String url = "http://kakadua.net/api/?some=key";
    /// String  kakaduaSite = KakaduaUtil.file_get_contents_utf8(url);
    ///
    /// Console.WriteLine(kakaduaSite);
    /// </code>
    /// </example>
    public static String file_get_contents_utf8(String url)
    {
        try
        {
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.UTF8;
            return client.DownloadString(url);
        }
        catch (Exception e) { return "error"; }
    }




    /// <summary>
    /// Simulate PHPs urldecode
    /// </summary>
    /// <returns>Decode a URL</returns>
    /// <param name="url">The string you want decoded</param>
    /// <example>
    /// The following example demonstrates the use of this method.
    /// 
    /// <code language="cs">
    /// String text = "Kakadua%20means%20Cockatoo%20in%20swedish";
    /// 
    /// String clearText = KakaduaUtil.urldecode(text);
    /// 
    /// //clearText == Kakadua means Cockatoo in swedish
    /// </code>
    /// </example>
    public static String urldecode(String url){
        return Uri.UnescapeDataString(url);
    }




    // ----- PHP emulators STOP ----- //




}
