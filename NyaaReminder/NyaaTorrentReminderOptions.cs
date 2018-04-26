using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using Newtonsoft.Json;
using qtReminder.Models;

namespace qtReminder.Nyaa
{
    public static class TorrentReminderOptions
    {
        /// <summary>
        /// Will return a populated ReminderOptions if the file exists.
        /// If the file does not exist, it will create an empty one and return.
        /// </summary>
        public static ReminderOptions LoadReminders(string filename)
        {
            if (!File.Exists(filename))
            {
                var reminderOptions = new ReminderOptions();
                reminderOptions.SubscribedAnime = new List<AnimeChannel>();
                return reminderOptions;
            }

            using (var fs = File.OpenText(filename))
            {
                string jsonString = fs.ReadToEnd();
                return JsonConvert.DeserializeObject<ReminderOptions>(jsonString);
            }
        }

        public static void SaveReminders(string filename, ReminderOptions options, bool verbose = false)
        {
            using (var fs = File.Open(filename, FileMode.Create))
            using(var sw = new StreamWriter(fs))
            {
                string json = JsonConvert.SerializeObject(options, Formatting.None);
                sw.Write(json);
                if(verbose) Console.WriteLine($"{StringHelper.GetDateTimeString()} Successfully saved options to {filename}.");
            }
        }
    }
    
    public class ReminderOptions
    {
        [JsonProperty]
        public string LatestChecked;
        [JsonProperty]
        public List<AnimeChannel> SubscribedAnime;
    }
}