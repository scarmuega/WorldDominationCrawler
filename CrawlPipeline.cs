using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace WorldDominationCrawler
{
    internal static class CrawlPipeline
    {
        public static Task Start(string rootUrl, int fetchWorkers, int parseWorkers)
        {

            int fetchCount = 0;
            int parseCount = 0;

            var urlToHtmlBlock = new TransformBlock<CrawlJob, CrawlJob>((job) => {
                job.Html = HtmlFetcher.GetHtml(job.Url);
                fetchCount += 1;
                return job;
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = fetchWorkers });

            var parseHtmlBlock = new TransformBlock<CrawlJob, CrawlJob>((job) => {
                job.PageTitle = HtmlParser.GetTitle(job.Html);
                job.PageHrefs = HtmlParser.GetHrefs(job.Url, job.Html); 
                parseCount += 1;
                return job;
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = parseWorkers });

            var reportStatusBlock = new TransformBlock<CrawlJob, CrawlJob>((job) => {
                ConsoleMonitor.PrintStatus(fetchCount, urlToHtmlBlock.InputCount, parseCount, parseHtmlBlock.InputCount, job);
                return job;
            });

            var spawnNewJobsBlock = new TransformManyBlock<CrawlJob, CrawlJob>((parentJob) => {
                return parentJob
                    .PageHrefs
                    .Select((href) => new CrawlJob(href, parentJob.Depth+1));
            });

            var requeueJobs = new ActionBlock<CrawlJob>((newJob) => {
                if (newJob.Depth >= 3) return;
                urlToHtmlBlock.Post(newJob);
            });

            urlToHtmlBlock.LinkTo(parseHtmlBlock);
            parseHtmlBlock.LinkTo(reportStatusBlock);
            reportStatusBlock.LinkTo(spawnNewJobsBlock);
            spawnNewJobsBlock.LinkTo(requeueJobs);

            urlToHtmlBlock.Post(new CrawlJob(rootUrl, 0));

            return Task.WhenAll(
                urlToHtmlBlock.Completion,
                parseHtmlBlock.Completion,
                reportStatusBlock.Completion,
                spawnNewJobsBlock.Completion,
                requeueJobs.Completion);
        }
    }
}
