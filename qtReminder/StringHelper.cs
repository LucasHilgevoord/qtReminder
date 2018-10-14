using System;
using System.Linq;
using F23.StringSimilarity;

namespace qtReminder
{
    public static class StringHelper
    {
        public static string FirstLettersToUpper(this string s)
        {
            return s.Split(" ").Aggregate("",
                (current, ss) => current + ss.First().ToString().ToUpper() + ss.Substring(1).ToLower() + " ").TrimEnd();
        }

        public static string GetDateTimeString()
        {
            var date = DateTime.Now;
            return $"[{date.Day}/{date.Month}/{date.Year.ToString().Substring(2)} {date.Hour}:{date.Minute}]";
        }

        public static double GetSimilarity(this string s1, string s2)
        {
            if (s1 == null || s2 == null) return 0; 
            var sim = new F23.StringSimilarity.Jaccard();
            return sim.Similarity(s1, s2);
        }
    }
}