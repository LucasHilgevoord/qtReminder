namespace qtReminder.Models
{
    /// <summary>
    ///     A mix of a Nyaa Torrent and a parsed channel.
    /// </summary>
    internal struct TorrentAndParsedChannel
    {
        public NyaaTorrent NyaaTorrent;
        public ParsedAnimeChannel parsedAnimeChannel;

        public TorrentAndParsedChannel(NyaaTorrent a, ParsedAnimeChannel b)
        {
            NyaaTorrent = a;
            parsedAnimeChannel = b;
        }
    }
}