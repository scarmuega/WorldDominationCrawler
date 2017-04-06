using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace WorldDominationCrawler {
    internal static class HtmlFetcher {
        public static async Task<string> GetHtml(string url) {
            using (var client = new HttpClient())
            {
                //TODO: cache optimization to avoid fetching the same url twice
                return await client.GetStringAsync(url);   
            }
        }
    }
}
