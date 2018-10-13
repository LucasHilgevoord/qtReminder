using Newtonsoft.Json;

namespace qtReminder.AnimeReminder.Models
{
    public class AniListModel
    {    
        [JsonProperty("id")]
        public int ID { get; set; }
        [JsonProperty("title")]
        public AniListTitle Title { get; set; }
    }

    public struct AniListTitle
    {
        [JsonProperty("romaji")]
        public string RomajiTitle { get; set; }
        [JsonProperty("english")]
        public string EnglishTitle { get; set; }
    }
}