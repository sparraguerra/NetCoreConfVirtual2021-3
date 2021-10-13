using AzureVideoIndexer.ForMediaService;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading.Tasks;

namespace VideoIndexerFunctions
{
    public  class UploadVideo
    {
        private readonly IOptions<ForMediaServiceOptions> mediaServiceOptions;
        private readonly IForMediaService mediaService;

        public UploadVideo(IOptions<ForMediaServiceOptions> mediaServiceOptions)
        {            
            this.mediaServiceOptions = mediaServiceOptions;
            mediaService = new ForMediaService(this.mediaServiceOptions);
        }
    
        [FunctionName("UploadVideo")]
        public async Task Run(
            [BlobTrigger("upload-videos-code/{name}", Connection = "AzureWebJobsStorage")] Stream myblob,
            [Blob("upload-videos-code/{name}", FileAccess.Read)] CloudBlockBlob blob, string name, ILogger log)
        {
            string url = blob.StorageUri.PrimaryUri.ToString();
            await mediaService.UploadVideo(url, name);
            log.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n Size:  {myblob.Length} Bytes");
        }
    }
}
