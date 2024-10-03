using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Abot2.Core;
using Abot2.Poco;

namespace project_r_data_collector
{
    public class DataCollector
    {
        [FunctionName("Collect")]
        public async Task RunAsync([TimerTrigger("* */2 * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Collect trigger function executed at: {DateTime.Now}");

            var pageRequester = new PageRequester(new CrawlConfiguration(), new WebContentExtractor());

            var crawledPage = await pageRequester.MakeRequestAsync(new Uri("https://google.com"));
            log.LogInformation("{result}", new
            {
                url = crawledPage.Uri,
                status = Convert.ToInt32(crawledPage.HttpResponseMessage.StatusCode)
            });

        }
    }
}
