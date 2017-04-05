using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace WorldDominationCrawler {
    internal static class HtmlFetcher {
        public static string GetHtml(string url) {
            using (var client = new HttpClient())
            {
                try
                {
                    Task<string> fetchTask = client.GetStringAsync(url);
                    fetchTask.Wait();
                    return fetchTask.Result;
                }
                catch (Exception err)
                {
                    Console.WriteLine("error on {0}", url);
                    Console.WriteLine(err.Message);
                    return String.Empty;
                }
            }
        }
    }
}
