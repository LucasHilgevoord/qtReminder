using Newtonsoft.Json;

namespace qtReminder.AnimeReminder.Models
{
    public class AniListModel 
    {    
        [JsonProperty("id")]
        public int ID { get; set; }
        [JsonProperty("title")]
        public AniListTitle Title { get; set; }
        [JsonProperty("nextAiringEpisode")]
        public AniListNextAiring? NextAiringEpisode { get; set; }
    }

    public struct AniListTitle
    {
        [JsonProperty("romaji")]
        public string RomajiTitle { get; set; }
        [JsonProperty("english")]
        public string EnglishTitle { get; set; }
    }

    public struct AniListNextAiring
    {
        [JsonProperty("airingAt")]
        public int? AiringAt { get; set; }
        [JsonProperty("episode")]
        public int? Episode { get; set; }
    }
}