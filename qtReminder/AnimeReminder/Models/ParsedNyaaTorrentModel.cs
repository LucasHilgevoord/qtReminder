namespace qtReminder.AnimeReminder.Models
{
    public class ParsedNyaaTorrentModel
    {
        public string AnimeTitle { get; set; }
        public string SubGroup { get; set; }
        public Quality Quality { get; set; }
        public int Episode { get; set; }
        
        public NyaaTorrentModel NyaaTorrentModel { get; set; }
    }
}