namespace qtReminder.Models
{
    /// <summary>
    ///     An anime parsed from a Nyaa Torrent
    /// </summary>
    public struct ParsedAnime
    {
        public readonly string Title;
        public readonly Quality Quality;
        public readonly int Episode;
        public readonly string Fangroup;
        public readonly string Link;

        public ParsedAnime(string Title, int Episode, string Fangroup, Quality Quality, string Link)
        {
            this.Title = Title;
            this.Quality = Quality;
            this.Episode = Episode;
            this.Fangroup = Fangroup;
            this.Link = Link;
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