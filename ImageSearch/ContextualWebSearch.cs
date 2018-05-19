using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace qtReminder.ImageSearch
{
    public class ContextualWebSearch
    {
        private async Task<string> SearchImageJson(string query, int maxResults = 10)
        {
            if(string.IsNullOrWhiteSpace(query)) throw new Exception("query has not been given.");
            if(maxResults <= 0) throw new Exception("maxResults cannot be less or equal to 0.");
            if(maxResults > 10) throw new Exception("maxResults cannot be more than 10.");

            using (var httpClient = new HttpClient())
            {
                var uriBuilder = new UriBuilder("https://contextualwebsearch-websearch-v1.p.mashape.com");

                var httpUtility = HttpUtility.ParseQueryString(uriBuilder.Query);
                httpUtility.Add("q", query);
                httpUtility.Add("count", maxResults.ToString());

                uriBuilder.Query = httpUtility.ToString();
                
                httpClient.DefaultRequestHeaders.Add("X-Mashape-Key", "xxx");
                httpClient.DefaultRequestHeaders.Add("X-Mashape-Host", "contextualwebsearch-websearch-v1.p.mashape.com");

                return await httpClient.GetStringAsync(uriBuilder.Uri);
            }
        }

        private async Task<string[]> GetImageURLs(string query, int maxResults = 10)
        {
            if(string.IsNullOrWhiteSpace(query)) throw new ArgumentNullException("query has not been given.");
            if(maxResults <= 0) throw new IndexOutOfRangeException("maxResults cannot be less or equal to 0.");
            if(maxResults > 10) throw new IndexOutOfRangeException("maxResults cannot be more than 10.");
            
            
        }
    }
}