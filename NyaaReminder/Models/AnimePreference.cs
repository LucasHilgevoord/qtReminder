using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace qtReminder.Models
{
    public class NyaaTorrent
    {
        public int Leechers;
        public string Link;
        public int Seeders;
        public string Size;
        public string Title;
    }

    /// <summary>
    ///     Anime someone can subscribe to, their preference.
    /// </summary>
    public class AnimePreference
    {
        public AnimePreference(string name)
        {
            Initialize(name, new[] {"HorribleSubs", "Erai-raws"}, Quality.x720);
        }

        [JsonConstructor]
        public AnimePreference()
        {
        }

        /// <summary>
        ///     The name of the anime being subscribed to
        /// </summary>
        [JsonProperty("Name")]
        public string Name { get; private set; }

        /// <summary>
        ///     The subgroup that is being subscribed to.
        /// </summary>
        [JsonProperty("Subgroup")]
        public string[] Subgroups { get; private set; }

        /// <summary>
        ///     The minimum quality _before announcing_
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public Quality MinQuality { get; set; }

        private void Initialize(string name, string[] subgroups, Quality quality)
        {
            Name = name;
            Subgroups = subgroups;
            MinQuality = quality;
        }
    }

    public enum Quality
    {
        /// <summary>
        /// 480p quality
        /// </summary>
        x480 = 0,
        
        /// <summary>
        /// 720p quality
        /// </summary>
        x720 = 1,
        
        /// <summary>
        /// 1080p quality
        /// </summary>
        x1080 = 2,
        
        Unknown = -1
    }
    
    public static class QualityStrings
    {
        private static Dictionary<Quality, string> strings = new Dictionary<Quality,string>()
        {
            {Quality.x480, "480p"},
            {Quality.x720, "720p"},
            {Quality.x1080, "1080p"},
            {Quality.Unknown, "lmao idk"}
        };

        public static string GetQualityString(Quality quality)
        {
            return strings[quality];
        }
    }
}