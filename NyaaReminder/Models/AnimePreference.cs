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
        x480 = 0,
        
        x720 = 1,
        
        x1080 = 2,
        
        Unknown = -1
    }
}