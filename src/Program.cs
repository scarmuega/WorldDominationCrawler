using System;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;

namespace WorldDominationCrawler
{
    class Program
    {
        static async Task RunAsync(string url, int fetchWorkers, int parseWorkers)
        {
            var reportData = await CrawlPipeline.RunAsync(url, fetchWorkers, parseWorkers);
            var reportGuid = await ReportPublisher.UploadToS3Async(reportData);
            System.IO.File.AppendAllText("data.json", reportData.ToJson());
            Console.WriteLine("Your report GUID is: {0}", reportGuid);
        }

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
                    fetchWorkers = fetchWorkersOption.ParseIntOption().GetValueOrDefault(5);
                    parseWorkers = parseWorkersOption.ParseIntOption().GetValueOrDefault(1);
                }
                catch
                {
                    cliApp.ShowHelp();
                    return 1;
                }

                var task = Program.RunAsync(url, fetchWorkers, parseWorkers);
                task.Wait();
                return 0;
            });
            
            try
            {
                cliApp.Execute(args);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                cliApp.ShowHelp();
            }
        }
    }
}
