namespace WorldDominationCrawler
{
    internal class CrawlJob
    {
        public CrawlJob(string url, int depth) {
            this.Url = url;
            this.Depth = depth;
        }

        public string Url { get; }
        public int Depth { get; }
        public string Html { get; set; }
        public string PageTitle { get; set; }
        public string[] PageHrefs { get; set; }
    }
}