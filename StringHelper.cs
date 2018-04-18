using System.Linq;

namespace qtReminder
{
    public static class StringHelper
    {
        public static string FirstLettersToUpper(this string s)
        {
            return s.Split(" ").Aggregate("", (current, ss) => current + (ss.First().ToString().ToUpper() + ss.Substring(1).ToLower() + " ")).TrimEnd();
        }
    }
}