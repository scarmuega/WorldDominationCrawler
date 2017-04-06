using System;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;

namespace WorldDominationCrawler
{
    class Program
    {

        static void PrintTaskInfo(string description)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("TASK: {0}", description);
            Console.ResetColor();
        }

        static async Task RunAsync(string url, CrawlOptions options)
        {
            PrintTaskInfo("running crawl pipeline");
            var crawlPipeline = new CrawlPipeline(url);
            await crawlPipeline.RunAsync(options);
            PrintTaskInfo("uploading report to S3");
            var reportGuid = await ReportPublisher.UploadToS3Async(crawlPipeline.Report);
            var reportUrl = ReportPublisher.GeneratePublicUrl(reportGuid);
            PrintTaskInfo("saving backup data to file");
            System.IO.File.AppendAllText(String.Format("{0}.json", reportGuid), crawlPipeline.Report.ToJson());
            PrintTaskInfo(String.Format("closing up (report GUID: {0})", reportGuid));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("========================");
            Console.WriteLine("View your report here:");
            Console.WriteLine(reportUrl);
            Console.WriteLine("========================");
            Console.ResetColor();
        }

        static CommandLineApplication SetupCliApp()
        {
            CommandLineApplication cliApp = new CommandLineApplication();
            
            cliApp.Name = "dotnet wdcrawler.dll";
            cliApp.Description = "A web crawler written in C# using the Dataflow library";
            cliApp.VersionOption("-v | --version", "wdcrawler.dll v0.4");
            cliApp.HelpOption("-h | --help");

            var urlArg = cliApp.Argument("url", "The seed url to crawl", false);
            var fetchWorkersOpt = cliApp.Option("-f | --fetch-workers", "Degree of parallel fetches", CommandOptionType.SingleValue);
            var parseWorkersOpt = cliApp.Option("-p | --parse-workers", "Degree of parallel parsers", CommandOptionType.SingleValue);
            var maxDepthOpt = cliApp.Option("-d | --max-depth", "Max crawl depth from seed url", CommandOptionType.SingleValue);
            var maxLinksOpt = cliApp.Option("-l | --max-links", "Max links to crawl per node", CommandOptionType.SingleValue);
            var parseWaitOpt = cliApp.Option("-w | --parse-wait", "Add artificial delay to parse step to play with stats", CommandOptionType.SingleValue);

            cliApp.OnExecute(() =>
            {
                var crawlOptions = new CrawlOptions
                {
                    FetchWorkers = fetchWorkersOpt.ParseIntOption(),
                    ParseWorkers = parseWorkersOpt.ParseIntOption(),
                    MaxDepth = maxDepthOpt.ParseIntOption(),
                    MaxLinksPerNode = maxLinksOpt.ParseIntOption(),
                    ParseDelay = parseWaitOpt.ParseIntOption(),
                };

                var task = RunAsync(urlArg.Value, crawlOptions);
                task.Wait();
                return (task.IsFaulted) ? 1 : 0;
            });

            return cliApp;
        }

        static void Main(string[] args)
        {
            var cliApp = SetupCliApp();
            
            try
            {
                cliApp.Execute(args);
            }
            catch (Exception err)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Fatal error: {0}", err.Message);
                Console.ResetColor();
                cliApp.ShowHelp();
            }
        }
    }
}
