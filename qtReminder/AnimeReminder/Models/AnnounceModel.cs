using System;
using System.Collections.Generic;
using System.Linq;
using Discord;

namespace qtReminder.AnimeReminder.Models
{
    public class AnnounceModel
    {
        public AnnounceModel()
        {
            QualityLinks = new Dictionary<Quality, SubgroupTorrentLink>();
            
            // pre-initialize the QualityLinks dictionary.
            var qualityArray = Enum.GetValues(typeof(Quality)).Cast<Quality>().ToList();
            foreach (var q in qualityArray)
            {
                if (q == Quality.Unknown) continue;
                QualityLinks.Add(q, default(SubgroupTorrentLink));
            }
        }
        
        public AnimeGuildModel AnimeGuildModel { get; set; }
        public IUserMessage AnnouncedMessage { get; set; }
        public Dictionary<Quality, SubgroupTorrentLink> QualityLinks { get; set; }
        public int Episode { get; set; }
    }

    public struct SubgroupTorrentLink
    {
        public SubgroupTorrentLink(string link, string subGroup)
        {
            Link = link;
            Subgroup = subGroup;
        }
        
        public string Link { get; set; }
        public string Subgroup { get; set; }
    }
}