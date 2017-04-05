using System;
using Microsoft.Extensions.CommandLineUtils;

namespace WorldDominationCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("v0.3");
            CommandLineApplication cliApp = new CommandLineApplication();
            cliApp.Name = "WorldDominationCrawler";

            var urlArgument = cliApp.Argument("url", "The root url to crawl", false);
            var fetchWorkersOption = cliApp.Option("-f | --fetch-workers", "Degree of parallel fetches", CommandOptionType.SingleValue);
            var parseWorkersOption = cliApp.Option("-p | --parse-workers", "Degree of parallel parsers", CommandOptionType.SingleValue);

            cliApp.OnExecute(() => {
                string url;
                int fetchWorkers, parseWorkers;

                if (!String.IsNullOrWhiteSpace(urlArgument.Value))
                {
                    url = urlArgument.Value;
                }
                else
                {
                    cliApp.ShowHelp();
                    return 1;
                }

                try
                {
                    fetchWorkers = fetchWorkersOption.ParseIntOption().GetValueOrDefault(4);
                    parseWorkers = parseWorkersOption.ParseIntOption().GetValueOrDefault(2);
                }
                catch
                {
                    cliApp.ShowHelp();
                    return 1;
                }

                var task = CrawlPipeline.RunAsync(urlArgument.Value, fetchWorkers, parseWorkers);
                System.IO.File.AppendAllText("data.json", task.Result.ToJson());
                return 0;
            });
            cliApp.Execute(args);
            try
            {
                
            }
            catch
            {
                cliApp.ShowHelp();
            }
        }
    }
}
