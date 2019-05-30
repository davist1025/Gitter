using INI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GitMoji
{
    public class GitMoji
    {
        /** TODOs:
         * - Update "emoji.json" file.
         * - Save images.
         * - MySQL integration?
         **/

        public static INIFile Config;

        public GitMoji()
        {
            Config = INIFile.Instance;
            Config.Load("config.ini");

            var jsonStrData = GetEmojiData().Result;
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonStrData);
            var fetchedEmojis = new List<EmojiData>();
            var loadedEmojis = new List<EmojiData>();

            Console.WriteLine("Fetching emoji list...");
            foreach (var kv in dictionary)
            {
                var emojiData = new EmojiData()
                {
                    Name = kv.Key,
                    Url = new Uri(kv.Value)
                };
                fetchedEmojis.Add(emojiData);
            }

            if (!File.Exists("emojis.json"))
            {
                Console.WriteLine("Generating 'emojis.json' file...");
                var stringBuilder = new StringBuilder();
                var stringWriter = new StringWriter(stringBuilder);
                using (var jw = new JsonTextWriter(stringWriter))
                {
                    jw.Formatting = Formatting.Indented;

                    jw.WriteStartObject();
                    foreach (var emojiData in fetchedEmojis)
                    {
                        jw.WritePropertyName(emojiData.Name);
                        jw.WriteValue(emojiData.Url);
                    }
                    jw.WriteEndObject();
                }

                using (var sw = new StreamWriter("emojis.json"))
                    sw.Write(stringBuilder.ToString());
            } 
            else
            {
                Console.WriteLine("Loading 'emojis.json'...");
                using (var sr = new StreamReader("emojis.json"))
                {
                    var json = sr.ReadToEnd();
                    dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                    foreach (var keyValue in dictionary)
                    {
                        var emojiData = new EmojiData()
                        {
                            Name = keyValue.Key,
                            Url = new Uri(keyValue.Value)
                        };
                        loadedEmojis.Add(emojiData);
                    }
                }

                Console.WriteLine("Checking fetched->loaded differences...");
                if (loadedEmojis.Count != fetchedEmojis.Count)
                    Console.WriteLine("Change detected!");
                else
                    Console.WriteLine("No changes were made to the API.");
            }

            Console.WriteLine("Finished! Press any key to exit.");
            Console.ReadKey();
        }

        private async Task<string> GetEmojiData()
        {
            string data = string.Empty;
            using (var wc = new WebClient())
            {
                wc.Headers.Add(HttpRequestHeader.UserAgent, "davist-GitMoji-App"); // 403 otherwise.
                data = await wc.DownloadStringTaskAsync("https://api.github.com/emojis");
            }
            return data;
        }

        static void Main(string[] args)
        {
            new GitMoji();
        }

        public class EmojiData
        {
            public string Name { get; set; }
            public Uri Url { get; set; }
        }
    }
}
