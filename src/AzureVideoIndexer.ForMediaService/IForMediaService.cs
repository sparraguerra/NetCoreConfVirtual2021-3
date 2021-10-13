using AzureVideoIndexer.ForMediaService;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureVideoIndexer.ForMediaService
{
    public interface IForMediaService
    {
        Task UploadVideo(string url, string name);
        Task<SearchResult> SearchVideosAsync(string query, TextScope textScope);
        Task<string> GetInsightsAsync(string videoId);


    }
}