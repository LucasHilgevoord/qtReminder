namespace qtReminder.AnimeReminder.Models
{
    public class QuoteModel
    {
        public int Id { get; set; }
        public ulong GuildOrigin { get; set; }
        public ulong Author { get; set; }
        public string Message { get; set; }
    }

    public class Quote
    {
        public string Author { get; set; }
        public string Message { get; set; }
    }
}