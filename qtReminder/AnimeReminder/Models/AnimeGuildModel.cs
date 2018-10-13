using System.Collections.Generic;

namespace qtReminder.AnimeReminder.Models
{
    public class AnimeGuildModel
    {
        public int Id { get; set; }
        public int AnimeID { get; set; }
        public AniListTitle AnimeTitle { get; set; }
        public string[] WantedSubgroupTitle { get; set; }
        public ulong[] SubscribedUsers { get; set; }
        public ulong Guild { get; set; }
        public ulong Channel { get; set; }
        public int LastAnnouncedEpisode { get; set; }
        public Quality MinAnnounceQuality { get; set; }
    }
}