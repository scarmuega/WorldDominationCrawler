using System;
using Microsoft.Extensions.CommandLineUtils;

namespace WorldDominationCrawler
{
    internal static class CliExtensions
    {
        public static int? ParseIntOption(this CommandOption cmdOption)
        {
            if (!cmdOption.HasValue()) return null;
            
            if (String.IsNullOrWhiteSpace(cmdOption.Value())) return null;

            return Int32.Parse(cmdOption.Value());
        }
    }
}