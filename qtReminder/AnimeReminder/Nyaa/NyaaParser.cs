using System.Collections.Generic;
using System.Text.RegularExpressions;
using qtReminder.AnimeReminder.Models;

namespace qtReminder.AnimeReminder.Nyaa
{
    public static class NyaaParser
    {
        public static ParsedNyaaTorrentModel[] 
            ParseNyaaTorrents(NyaaTorrentModel[] nyaatorrentmodels)
        {
            var list = new List<ParsedNyaaTorrentModel>();
            foreach (var torrentmodel in nyaatorrentmodels)
            {
                list.Add(ParseRawModel(torrentmodel));
            }

            return list.ToArray();
        }

        public static ParsedNyaaTorrentModel ParseRawModel(NyaaTorrentModel model)
        {
            var title = "";
            var subgroup = "";
            var episode = -1;
            var quality = Quality.Unknown;
                
            subgroup = FindSubgroup(model.Title, out var pos);
            episode = FindEpisode(model.Title, out var endPos);
            title = FindTitle(model.Title, pos, endPos);
            quality = FindQuality(model.Title);
            
            return new ParsedNyaaTorrentModel
            {
                AnimeTitle = title ?? "",
                SubGroup = subgroup ?? "",
                Episode = episode,
                NyaaTorrentModel = model,
                Quality = quality
            };
        }

        /// <summary>
        /// Parse the subgroup using brackets which subgroups mostly use.
        /// also returns the pos of the next bracket.
        /// </summary>
        /// <param name="torrentTitle"></param>
        /// <param name="pos"></param>
        /// <returns>the subgroup? what else</returns>
        private static string FindSubgroup(string torrentTitle, out int pos)
        {
            // get the first character of the title
            var firstChar = torrentTitle[0];
            pos = 0;
            var dict = new Dictionary<char, char> {{'{', '}'}, {'[', ']'}, {'(', ')'}};
            if (!dict.TryGetValue(firstChar, out var secondChar)) return null; // Can't parse if the ending char is not found.

            int i = 1;
            for (; i < torrentTitle.Length; i++) {
                if (torrentTitle[i] != secondChar) continue;
                pos = i+1;
                break;
            }

            return pos == 0 ? null : torrentTitle.Substring(1, i-1);
        }

        /// <summary>
        /// try to find the last occurence of a REAL number
        /// most, if not all, nyaa torrents only have a real number
        /// when it's referencing the volume, or episode.
        /// so we will be using this information to parse the number.
        /// if the title could not be found, -1 will be used for both the pos AND the episode.
        /// </summary>
        public static int FindEpisode(string torrentTitle, out int pos)
        {
            int wordEndPos = torrentTitle.Length-1;
            bool valid = false;
            for (pos = torrentTitle.Length-1; pos >= 0; pos--)
            {
                if (pos == 0) break; // consider job failed if the current pos = 0....
                
                if (char.IsWhiteSpace(torrentTitle[pos]))
                {
                    if (!char.IsWhiteSpace(torrentTitle[pos - 1])) wordEndPos = pos;
                    valid = true;
                }
                else if (!char.IsDigit(torrentTitle[pos])) valid = false;

                if (!valid || !char.IsWhiteSpace(torrentTitle[pos - 1])) continue;
                
                // this is a full bred number!!!!
                // get the substring and turn it into an int.

                var su = torrentTitle.Substring(pos, wordEndPos - pos);
                int.TryParse(su, out var ep);
                return ep;
            }

            return (pos = 0);
        }

        /// <summary>
        /// This function is actually really simple.
        /// It just looks for certain popular occurences of the title.
        /// For example: 1080p, *x1080 returns 1080p... of course...
        /// </summary>
        private static Quality FindQuality(string torrentTitle)
        {
            if (Regex.IsMatch(torrentTitle, @"([^\s]+.x1080)|(1080p)")) return Quality.ThousandEightyP;
            if (Regex.IsMatch(torrentTitle, @"([^\s]+.x720)|(720p)")) return Quality.SevenTwentyP;
            if (Regex.IsMatch(torrentTitle, @"([^\s]+.x480)|(480p)")) return Quality.FourEightyP;
            return Quality.Unknown;
        }

        /// <summary>
        /// Will try to return a title of the anime.
        /// If the endPos is 0, it will look for the title until it finds
        /// a special character, like "(" or "["
        /// Otherwise it will just find the title from the episode count.
        /// </summary>
        private static string FindTitle(string torrentTitle, int startPos, int endPos)
        {
            if (endPos > 0 && endPos > startPos)
            {
                // TODO : IF THE STRING CONTAINS A SINGLE NUMBER, SET IT TO endPos-1
                var i = endPos-1; 
                for (; i > startPos; i--)
                {
                    // look for something that is neither whitespace or a dash
                    // and then break out of the loop..
                    char c = torrentTitle[i];
                    if (!char.IsWhiteSpace(c) && c != '-') break;
                }

                if((++i) + startPos >= torrentTitle.Length) i--;
                if ((i - startPos) <= 0) i = torrentTitle.Length;
                return torrentTitle.Substring(startPos, i - startPos ).Trim();
            }
            else
            {
                var i = 0;
                for (; i < torrentTitle.Length; i++)
                {
                    char c = torrentTitle[i]; // just look for those special characters lol.
                    if (c == '[' || c == '(' || c == '{') break;
                }

                if((++i) + startPos >= torrentTitle.Length) i--;
                if ((i - startPos) <= 0) i = torrentTitle.Length;
                string title = torrentTitle.Substring(startPos, i - startPos);
                return (title.Trim());
            }
        }
    }
}