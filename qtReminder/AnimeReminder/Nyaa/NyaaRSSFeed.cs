using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Net;

namespace qtReminder.AnimeReminder.Nyaa
{
    public class NyaaRSSFeed
    {
        private static readonly string nyaaRssLink = "https://nyaa.si/?page=rss";

        public static async Task<string> GetRSSFeed()
        {
            using (var client = new HttpClient())
            {
                var result = await client.GetAsync(nyaaRssLink);
                if (!result.IsSuccessStatusCode) throw new HttpException(result.StatusCode);
                return (await result.Content.ReadAsStringAsync());
            }
        }

        /// <summary>
        /// Used for debug pruposes
        /// </summary>
        public static async Task<string> GetFakeRSSFeed()
        {
            using (var file = File.OpenText("FakeRSS.rss"))
            {
                return await file.ReadToEndAsync();
            }
        }
    }
}