using System;
using JobKey = System.Tuple<int, string>;

namespace WorldDominationCrawler
{
    internal class CrawlJob
    {
        public CrawlJob(string url, int depth) {
            this.Url = url;
            this.Depth = depth;
            this.PageHrefs = new string[0];
        }

        public string Url { get; }
        public int Depth { get; }
        public string Html { get; set; }
        public string PageTitle { get; set; }
        public string[] PageHrefs { get; set; }
        public Exception Exception { get; set; }

        public JobKey GetJobKey()
        {
            return new JobKey(this.Depth, this.Url);
        }
    }
}