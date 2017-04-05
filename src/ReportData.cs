using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using JobKey = System.Tuple<int, string>;

namespace WorldDominationCrawler
{
    internal class ReportData
    {
        private CrawlJob _RootJob;
        private Dictionary<JobKey, CrawlJob> _Jobs;

        public ReportData(CrawlJob rootJob)
        {
            _RootJob = rootJob;
            _Jobs = new Dictionary<JobKey, CrawlJob>();
        }

        public void TrackJob(CrawlJob job)
        {
            var key = job.GetJobKey();
            if (_Jobs.ContainsKey(key)) return;
            _Jobs.Add(job.GetJobKey(), job);
        }

        private object _BuildJobToTree(CrawlJob job)
        {
            return new {
                url = job.Url,
                title = job.PageTitle,
                children = job.PageHrefs
                    .Select((url) => new JobKey(job.Depth+1, url))
                    .Where((key) => _Jobs.ContainsKey(key))
                    .Select((key) => _Jobs[key])
                    .Select((childJob) => this._BuildJobToTree(childJob)),
            };
        }

        public string ToJson()
        {
            var tree = this._BuildJobToTree(_RootJob);
            return JsonConvert.SerializeObject(tree, Formatting.Indented);
        }
    }
}