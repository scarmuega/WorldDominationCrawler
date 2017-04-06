using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TransformBlock = System.Threading.Tasks.Dataflow.TransformBlock<WorldDominationCrawler.CrawlJob, WorldDominationCrawler.CrawlJob>;
using TransformManyBlock = System.Threading.Tasks.Dataflow.TransformManyBlock<WorldDominationCrawler.CrawlJob, WorldDominationCrawler.CrawlJob>;

namespace WorldDominationCrawler
{
    internal class CrawlPipeline
    {
        private const int DEFAULT_FETCH_WORKERS = 8;
        private const int DEFAULT_PARSE_WORKERS = 2;
        private const int DEFAULT_PARSE_DELAY = 200;
        private const int DEFAULT_MAX_DEPTH = 3;
        private const int DEFAULT_MAX_LINKS_PER_NODE = 10;

        private CrawlJob _RootJob;
        private CrawlStats _Stats;
        private ReportData _Report;

        public CrawlPipeline(string url)
        {
            _RootJob = new CrawlJob(url, 0);
            _Report = new ReportData(_RootJob);
            _Stats = new CrawlStats();
        }

        public CrawlStats Stats
        {
            get { return _Stats; }
        }

        public ReportData Report
        {
            get { return _Report; }
        }

        private async Task<CrawlJob> _FetchHtmlStep(CrawlJob job)
        {
            try
            {
                job.Html = await HtmlFetcher.GetHtml(job.Url);
                _Stats.FetchCount += 1;
            }
            catch (Exception ex)
            {
                job.Exception = ex;
                _Stats.ErrorCount += 1;
            }
            
            return job;
        }

        private CrawlJob _ParseHtmlStep(CrawlJob job, CrawlOptions options)
        {
            try
            {
                job.PageTitle = HtmlParser.GetTitle(job.Html);
                job.PageHrefs = HtmlParser.GetHrefs(job.Url, job.Html); 
                _Stats.ParseCount += 1;
            }
            catch (Exception ex)
            {
                job.Exception = ex;
                _Stats.ErrorCount += 1;
            }

            //delay a little bit, just to make stats interesting
            System.Threading.Thread.Sleep(options.ParseDelay.GetValueOrDefault(DEFAULT_PARSE_DELAY));

            return job;
        }

        private CrawlJob _ReportJobStep(CrawlJob job, TransformBlock fetchBlock, TransformBlock parseBlock)
        {
            _Report.TrackJob(job);
            
            _Stats.FetchPending = fetchBlock.InputCount;
            _Stats.ParsePending = parseBlock.InputCount;
            ConsoleMonitor.PrintStatus(_Stats, job);

            return job;
        }

        private IEnumerable<CrawlJob> _SpanNewJobsStep(CrawlJob parentJob, TransformBlock fetchBlock, CrawlOptions options)
        {
            var newJobs = parentJob
                    .PageHrefs
                    .Select((href) => new CrawlJob(href, parentJob.Depth+1))
                    .Take(options.MaxLinksPerNode.GetValueOrDefault(DEFAULT_MAX_LINKS_PER_NODE));

            if (parentJob.Depth == 0 && newJobs.Count() == 0)
            {
                fetchBlock.Complete();
            }

            return newJobs;
        }

        private void _RequeueJobStep(CrawlJob newJob, TransformBlock fetchBlock, TransformBlock parseBlock, CrawlOptions options)
        {
            if (newJob.Depth <= options.MaxDepth.GetValueOrDefault(DEFAULT_MAX_DEPTH)) fetchBlock.Post(newJob);
            if (fetchBlock.InputCount == 0 && parseBlock.InputCount == 0) fetchBlock.Complete();
        }

        public async Task<ReportData> RunAsync(CrawlOptions options)
        {
            _Stats.Reset();
            _Report.Reset();

            var fetchHtmlBlock = new TransformBlock((job) => _FetchHtmlStep(job), new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = options.FetchWorkers.GetValueOrDefault(DEFAULT_FETCH_WORKERS) });
            var parseHtmlBlock = new TransformBlock((job) => _ParseHtmlStep(job, options), new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = options.ParseWorkers.GetValueOrDefault(DEFAULT_PARSE_WORKERS) });
            var reportJobBlock = new TransformBlock((job) => _ReportJobStep(job, fetchHtmlBlock, parseHtmlBlock));
            var spawnNewJobsBlock = new TransformManyBlock((job) => _SpanNewJobsStep(job, fetchHtmlBlock, options));
            var requeueJobs = new ActionBlock<CrawlJob>((job) => _RequeueJobStep(job, fetchHtmlBlock, parseHtmlBlock, options));

            fetchHtmlBlock.LinkTo(parseHtmlBlock, new DataflowLinkOptions { PropagateCompletion = true });
            parseHtmlBlock.LinkTo(reportJobBlock, new DataflowLinkOptions { PropagateCompletion = true });
            reportJobBlock.LinkTo(spawnNewJobsBlock, new DataflowLinkOptions { PropagateCompletion = true });
            spawnNewJobsBlock.LinkTo(requeueJobs, new DataflowLinkOptions { PropagateCompletion = true });

            fetchHtmlBlock.Post(_RootJob);

            await requeueJobs.Completion;

            return _Report;
        }
    }
}
