using System;
using System.Linq;

namespace qtReminder
{
    public static class StringHelper
    {
        public static string FirstLettersToUpper(this string s)
        {
            return s.Split(" ").Aggregate("", (current, ss) => current + (ss.First().ToString().ToUpper() + ss.Substring(1).ToLower() + " ")).TrimEnd();
        }

        public static string GetDateTimeString()
        {
            DateTime date = DateTime.Now;
            return $"[{date.Day}/{date.Month}/{date.Year.ToString().Substring(2)} {date.Hour}:{date.Minute}]";
        }
    }
}