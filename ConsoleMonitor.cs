using System;
using System.Linq;

namespace WorldDominationCrawler
{
    internal static class ConsoleMonitor
    {
        private static int _InitialCursorTop;

        static ConsoleMonitor()
        {
            _InitialCursorTop = Console.CursorTop;
        }

        public static void PrintStatus(int fetchCount, int fetchPending, int parseCount, int parsePending, CrawlJob currentJob)
        {
            Console.SetCursorPosition(0, _InitialCursorTop);
            Console.WriteLine("fetched {0} (waiting {1})", fetchCount, fetchPending);
            Console.WriteLine("parsed {0} (waiting {1})", parseCount, parsePending);
            Console.WriteLine("processing {0} {1} {2}", currentJob.Depth, currentJob.Url, currentJob.PageHrefs.Count());
            _InitialCursorTop = Console.CursorTop - 3;
        }
    }
}