using System;

namespace WorldDominationCrawler
{
    internal class CrawlStats
    {
        public int FetchCount;
        public int ParseCount;
        public int FetchPending;
        public int ParsePending;
        public int ErrorCount;

        public void Reset()
        {
            this.FetchCount = 0;
            this.ParseCount = 0;
            this.FetchPending = 0;
            this.ParsePending = 0;
            this.ErrorCount = 0;
        }
    }
}