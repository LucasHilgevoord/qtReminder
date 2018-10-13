using System.Collections.Generic;

namespace qtReminder.AnimeReminder
{
    public static class QualityString
    {
        private static Dictionary<Quality, string> qualityStrings = new Dictionary<Quality, string>()
        {
            {Quality.Unknown, "Unknown" },
            {Quality.ThousandEightyP, "1080p" },
            {Quality.SevenTwentyP, "720p" },
            {Quality.FourEightyP, "480p" }
        };

        public static string GetQualityString(Quality q)
        {
            return qualityStrings[q];
        }
    }
    
    public enum Quality
    {
        Unknown,
        ThousandEightyP,
        SevenTwentyP,
        FourEightyP
    }
}