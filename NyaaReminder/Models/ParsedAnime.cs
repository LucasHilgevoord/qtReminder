namespace qtReminder.Models
{
    /// <summary>
    ///     An anime parsed from a Nyaa Torrent
    /// </summary>
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

    /// <summary>
    ///     A mix of a parsed nyaa anime torrent, and an anime channel associated to it.
    /// </summary>
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