namespace qtReminder.Models
{
    public struct ParsedAnime
    {
        public string Title;
        public Quality Quality;
        public int Episode;
        public string Fangroup;

        public ParsedAnime(string Title, int Episode, string Fangroup, Quality Quality)
        {
            this.Title = Title;
            this.Quality = Quality;
            this.Episode = Episode;
            this.Fangroup = Fangroup;
        }
    }

    public struct ParsedAnimeChannel
    {
        public ParsedAnime ParsedAnime;
        public AnimeChannel AnimeChannel;

        public ParsedAnimeChannel(ParsedAnime p, AnimeChannel a)
        {
            ParsedAnime = p;
            AnimeChannel = a;
        }
    }
}