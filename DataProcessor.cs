using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;
using Azure.Storage.Blobs;
using System;
using Azure.Storage.Queues;

namespace project_r_data_collector
{
    public class DataProcessor
    {
        [FunctionName(nameof(DataProcessor))]
        public void Run([BlobTrigger("rawdatafiles/{name}", Connection = "dataprocessor")]string myBlob, string name, ILogger log)
        {
            log.LogInformation($"Data Processor trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(myBlob);

            HtmlNode scriptNode = htmlDoc.DocumentNode.SelectSingleNode("//script[@id='__NEXT_DATA__']");
            string scriptContent = "";

            if (scriptNode == null)
            {
                log.LogWarning($"Completed Data Processor trigger function Processed blob\n Name:{name} \n with no content");
            } else
            {
                scriptContent = scriptNode.InnerText;
            }

            if (scriptContent.Length == 0)
            {
                log.LogWarning($"Completed Data Processor trigger function Processed blob\n Name:{name} \n with no content");
            }

            log.LogInformation($"Completed Data Processor trigger function Processed blob\n Name:{name} \n with new Size: {scriptContent.Length} Bytes");

            try
            {
                var serviceClient = GetBlobServiceClient();
                var blobClient = serviceClient.GetBlobContainerClient("jsonfiles").GetBlobClient($"{name}.json");
                blobClient.Upload(BinaryData.FromString(scriptContent), overwrite: true);

                log.LogInformation($"Completed Data upload for blob\n Name:{name}");

                serviceClient.GetBlobContainerClient("rawdatafiles").GetBlobClient(name).Delete();

                log.LogInformation($"Completed Data deletion for blob\n Name:{name}");

            } catch (Exception e)
            {
                log.LogWarning(e.Message);
            }
        }

        public BlobServiceClient GetBlobServiceClient()
        {
            BlobServiceClient client = new("BlobEndpoint=https://rawdatastore1.blob.core.windows.net/;QueueEndpoint=https://rawdatastore1.queue.core.windows.net/;FileEndpoint=https://rawdatastore1.file.core.windows.net/;TableEndpoint=https://rawdatastore1.table.core.windows.net/;SharedAccessSignature=sv=2022-11-02&ss=bfqt&srt=co&sp=rwdlacupiytfx&se=2024-06-15T20:57:07Z&st=2024-05-16T12:57:07Z&spr=https&sig=KGcxrYQG%2FKVIo7%2FzeaGYF2trtJy1MZ%2F%2B6WKjegM8lFQ%3D");

            return client;
        }
    }
}
