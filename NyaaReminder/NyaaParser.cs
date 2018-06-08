using System.Text.RegularExpressions;
using qtReminder.Models;

namespace qtReminder.Nyaa
{
    public static class NyaaParser
    {
        /// <summary>
        ///     Parse the XML title.
        /// </summary>
        /// <param name="title">title of the torrent.</param>
        /// <returns>cool parsed shit</returns>
        public static ParsedAnime ParseTitle(string title)
        {
            // TODO: Improve the parser.
            // right now it only works with this format:
            // [Subgroup] Title of the anime - episode [quality].filetype 
            // But honestly it should work on every. I have no idea how will do this though.
            
            string anime_title, fangroup;
            var quality = Quality.Unknown;

            var fangroupAndQuality = Regex.Match(title, "\\[(.*?)\\]"); // get everything between '[ ]' 

            fangroup = fangroupAndQuality.Groups[1].Value;
            int titleStart = fangroupAndQuality.Index + fangroupAndQuality.Length, titleEnd;

            // theoratically the second isntance of [] should have the quality in it.
            // unless an anime comes out that has that in it. 
            var qualitystring = fangroupAndQuality.NextMatch().Groups[1].Value; 

            var episodeResultCollection = Regex.Matches(title, "([^-]+$)", RegexOptions.Multiline); // This gets everything after the -, I think.
            // Regex is fucking hard, it's like {}}[($.^?1)%_$#^#$^}} and it returns what you want
            var episodeResult = episodeResultCollection[episodeResultCollection.Count - 1];

            titleEnd = episodeResult.Index - 2; // - 3 because the title also include " - ", and that needs to go.
            var episodeString = episodeResult.Value.Trim().Split(" ")[0];

            if (!int.TryParse(episodeString, out var episode)) episode = -1;

            var length = titleEnd - titleStart;
            if (length < 0) length = 1;
            if (title.Length <= titleStart + length) return new ParsedAnime("", -1, "", Quality.Unknown);
            anime_title = title.Substring(titleStart, length).Trim();


            // beeeeh make a dictionary for this,, weeeeeeh
            // cry baby, show me your tears.
            switch (qualitystring.ToLower())
            {
                case "1080p":
                    quality = Quality.x1080;
                    break;
                case "720p":
                    quality = Quality.x720;
                    break;
                case "480p":
                    quality = Quality.x480;
                    break;
            }

            return new ParsedAnime(anime_title, episode, fangroup, quality);
        }
    }
}