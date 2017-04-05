using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace WorldDominationCrawler
{
    internal static class HtmlParser
    {
        private static Regex HrefRegex = new Regex("<a href=\"([^\"]+)\">([^<]+)</a>", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        public static string GetTitle(string html)
        {
            System.Threading.Thread.Sleep(500);
            return "my first page";
        }

        public static string[] GetHrefs(string sourceUrl, string html)
        {
            var sourceUri = new Uri(sourceUrl);
            return HrefRegex
                .Matches(html)
                .Cast<Match>()
                .Select((match) => match.Groups[1].Value)
                .Where((href) => !String.IsNullOrWhiteSpace(href) && !href.StartsWith("#"))
                .Select((relativeUrl) => new Uri(sourceUri, relativeUrl))
                .Where((uri) => uri.Scheme == "http" || uri.Scheme == "https")
                .Select((uri) => uri.AbsoluteUri)
                .ToArray();
        }
    }
}