// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AzureVideoIndexer.ForMediaService;
using AzureVideoIndexer.CosmosDb;
using AzureVideoIndexer.Core;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using AzureVideoIndexer.CognitiveSearch;

namespace VideoIndexerFunctions
{
    public class SaveVideoInsights
    {
        private readonly IForMediaService mediaService;
        private readonly ICosmosDbRepository<InsightModel> insightCosmosDbRepository;
        private readonly ICognitiveSearchService cognitiveSearchService;

        public SaveVideoInsights(ICognitiveSearchService cognitiveSearchService, 
                                IForMediaService mediaService, 
                                ICosmosDbRepository<InsightModel> videoInsightCosmosDbRepository)
        {
            insightCosmosDbRepository = videoInsightCosmosDbRepository;
            this.mediaService = mediaService;
            this.cognitiveSearchService = cognitiveSearchService;
        }

        [FunctionName("SaveVideoInsights_Http")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                var projectName = req.Query["id"];

                var result = await mediaService.GetInsightsAsync(projectName);

                dynamic jObj = JObject.Parse(result);   
                dynamic video = jObj.videos[0];

                InsightModel insight = video.ToObject<InsightModel>();
                insight.Name = jObj.name;
                insight.Brands = video.insights.brands?.ToObject<Brand[]>();
                var keywords = video.insights.keywords?.ToObject<Keyword[]>() as Keyword[];
                if (keywords != null)
                {
                    insight.Keywords = string.Join("", keywords.Select(x => x.Text));
                }
                var transcript = video.insights.transcript?.ToObject<Keyword[]>() as Keyword[];
                if (transcript != null)
                {
                    insight.Transcript = string.Join("", transcript.Select(x => x.Text));
                }
                var ocr = video.insights.ocr?.ToObject<Keyword[]>() as Keyword[];
                if (ocr != null)
                {
                    insight.Transcript = string.Join("", ocr.Select(x => x.Text));
                }
                insight.Topics = video.insights.topics?.ToObject<Topic[]>();
                insight.Faces = video.insights.faces?.ToObject<Face[]>();
                insight.Labels = video.insights.labels?.ToObject<Label[]>();
                insight.NamedLocations = video.insights.namedLocations?.ToObject<Named[]>();
                insight.NamedPeople = video.insights.namedPeople?.ToObject<Named[]>();
                insight.Speakers = video.insights.speakers?.ToObject<Speaker[]>();

                _ = await insightCosmosDbRepository.AddAsync(insight);

                await cognitiveSearchService.RunIndexerAsync();

                return new OkResult();
            
            }catch(Exception e)
            {
                log.LogError(e.StackTrace);
                return new BadRequestResult();
            }
        }
    }
}

