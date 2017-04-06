using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace WorldDominationCrawler
{
    internal static class CrawlPipeline
    {
        private const int MAX_DEPTH = 2;
        private const int MAX_ITEMS_PER_NODE = 5;

        public static async Task<ReportData> RunAsync(string rootUrl, int fetchWorkers, int parseWorkers)
        {
            var rootJob = new CrawlJob(rootUrl, 0);
            var report = new ReportData(rootJob);
            int fetchCount = 0, parseCount = 0;

            var fetchHtmlBlock = new TransformBlock<CrawlJob, CrawlJob>((job) => {
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

            var monitorStatusBlock = new TransformBlock<CrawlJob, CrawlJob>((job) => {
                ConsoleMonitor.PrintStatus(fetchCount, fetchHtmlBlock.InputCount, parseCount, parseHtmlBlock.InputCount, job);
                return job;
            });

            var addToReportBlock = new TransformBlock<CrawlJob, CrawlJob>((job) => {
                report.TrackJob(job);
                return job;
            });

            var spawnNewJobsBlock = new TransformManyBlock<CrawlJob, CrawlJob>((parentJob) => {
                return parentJob
                    .PageHrefs
                    .Select((href) => new CrawlJob(href, parentJob.Depth+1))
                    .Take(MAX_ITEMS_PER_NODE);
            });

            var requeueJobs = new ActionBlock<CrawlJob>((newJob) => {
                if (newJob.Depth <= MAX_DEPTH) fetchHtmlBlock.Post(newJob);
                if (fetchHtmlBlock.InputCount == 0 && parseHtmlBlock.InputCount == 0) fetchHtmlBlock.Complete();
            });

            fetchHtmlBlock.LinkTo(parseHtmlBlock, new DataflowLinkOptions { PropagateCompletion = true });
            parseHtmlBlock.LinkTo(monitorStatusBlock, new DataflowLinkOptions { PropagateCompletion = true });
            monitorStatusBlock.LinkTo(addToReportBlock, new DataflowLinkOptions { PropagateCompletion = true });
            addToReportBlock.LinkTo(spawnNewJobsBlock, new DataflowLinkOptions { PropagateCompletion = true });
            spawnNewJobsBlock.LinkTo(requeueJobs, new DataflowLinkOptions { PropagateCompletion = true });

            fetchHtmlBlock.Post(rootJob);

            await requeueJobs.Completion;

            return report;
        }
    }
}
