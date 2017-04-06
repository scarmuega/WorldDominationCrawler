using System;

namespace WorldDominationCrawler
{
    internal struct CrawlOptions
    {
        public int? FetchWorkers;
        public int? ParseWorkers;
        public int? ParseDelay;
        public int? MaxDepth;
        public int? MaxLinksPerNode;
    }
}