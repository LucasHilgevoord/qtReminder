namespace qtReminder.Models
{
    public class NyaaAnime
    {
        public string Title;
        public string Link;
        public string Size;
        public int Seeders;
        public int Leechers;

        public NyaaAnime()
        {
            
        }
    }
    
    public class Anime
    {
        public string Name { get; private set; }
        public string Subgroup { get; private set; }
        public Quality MinQuality { get; private set; }

        public Anime(string name)
        {
            Initialize(name, "HorribleSubs", Quality.x720);
        }

        private void Initialize(string name, string subgroup, Quality quality)
        {
            Name = name;
            Subgroup = subgroup;
            MinQuality = quality;
        }

        [Newtonsoft.Json.JsonConstructor]
        public Anime(string name, string subgroup, Quality quality)
        {
            Initialize(name, subgroup, quality);
        }
    }

    public enum Quality
    {
        x480 = 0,
        x720,
        x1080,
        Unknown = -1
    }
}