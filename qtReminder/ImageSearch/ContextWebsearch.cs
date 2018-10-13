using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace qtReminder.ImageSearch
{
    public static class ContextWebsearch
    {
        public static async Task<string[]> FindImages(string query, int maxResults = 50)
        {
            using (var client = new HttpClient())
            {
                var result = await client.GetStringAsync(
                    $"https://contextualwebsearch.com/api/Search/ImageSearchAPI?q={query}&count={maxResults}&autoCorrect=false");

                var json = JObject.Parse(result);

                var list = json["value"].Children().ToList();

                var results = new List<string>();
                foreach (var i in list)
                {
                    results.Add(i["url"].ToString());
                }

                return results.ToArray();
            }
        }
    }
}