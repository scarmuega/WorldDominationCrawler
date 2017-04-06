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

        public static void PrintStatus(CrawlStats stats, CrawlJob currentJob)
        {
            Console.SetCursorPosition(0, _InitialCursorTop);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Fetched:\t{0:D4}\t(waiting {1:D4})", stats.FetchCount, stats.FetchPending);
            Console.WriteLine("Parsed:\t\t{0:D4}\t(waiting {1:D4})", stats.ParseCount, stats.ParsePending);
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Faulted:\t{0:D4}", stats.ErrorCount);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Current: (depth: {0:D2}) {1}", currentJob.Depth, currentJob.Url.Truncate(55), currentJob.PageHrefs.Count());
            Console.ResetColor();
            _InitialCursorTop = Console.CursorTop - 4;
        }
    }
}