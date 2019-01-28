using System;
using System.Collections.Generic;
using System.Linq;
using qtReminder.AnimeReminder.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace qtReminder.AnimeReminder.AniList
{
    public class AnilistRequest
    {
        public static async Task<AniListModel[]> FindAnime(string searchQuery)
        {
            using (var client = new HttpClient())
            {
                var query = $@"query ($search:String, $perPage:Int, $page:Int) {{
    Page(page:$page, perPage:$perPage) {{
        pageInfo {{
            perPage 
            total
        }}

    media(search:$search, type: ANIME) {{
        id
        title {{
        romaji
        english
        }}
        
        nextAiringEpisode {{
            airingAt
            episode
        }}
    
    }}
}}}}
";
                string variables = $@"
""search""     : ""{searchQuery}"",
""perPage""    : ""10"",
""page""       : ""1""
".Replace(Environment.NewLine,"");
                
                string content = $@"{{""query"": ""{query.Replace(Environment.NewLine, " ")}"", ""variables"":{{{variables}}}}}";

                var m = new HttpRequestMessage
                {
                    Content = new StringContent(content, Encoding.UTF8,
                        "application/json")
                };

                var sadsd = await m.Content.ReadAsStringAsync();

                var result = (await client.PostAsync("https://graphql.anilist.co", m.Content));
                string json = await result.Content.ReadAsStringAsync();

                JObject animeResults = JObject.Parse(json);
                var list = animeResults["data"]["Page"]["media"].Children().ToList();
                
                List<AniListModel> aniListModel = new List<AniListModel>();
                foreach (var l in list)
                {
                    aniListModel.Add(l.ToObject<AniListModel>());
                }

                return aniListModel.ToArray();
            }
        }
    }
}