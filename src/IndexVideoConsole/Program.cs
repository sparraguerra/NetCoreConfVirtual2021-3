using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text.Json;
using System.Threading.Tasks;

namespace IndexVideoConsole
{
    class Program
    { 
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .AddCommandLine(args)
                    .Build();

            await IndexVideoAsync(configuration);
        }

        private static async Task IndexVideoAsync(IConfiguration configuration)
        {
            var apiUrl = configuration["AzureVideoIndexer:ApiUrl"];
            var apiKey = configuration["AzureVideoIndexer:ApiKey"];
            var accountId = configuration["AzureVideoIndexer:AccountId"];
            var accountLocation = configuration["AzureVideoIndexer:AccountLocation"];

            var httpHandler = new SocketsHttpHandler();
            httpHandler.SslOptions.EnabledSslProtocols |= SslProtocols.Tls12;
            using var client = new HttpClient(httpHandler);

            string queryParams = CreateQueryString(
                new Dictionary<string, string>()
                {
                    {"allowEdit", "true"},
                });

            var getAccountsRequest = new HttpRequestMessage(HttpMethod.Get, $"{apiUrl}/auth/{accountLocation}/Accounts/{accountId}/AccessToken?{queryParams}");
            getAccountsRequest.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);
            getAccountsRequest.Headers.Add("x-ms-client-request-id", Guid.NewGuid().ToString());
            var result = await client.SendAsync(getAccountsRequest);
            var accessToken = (await result.Content.ReadAsStringAsync()).Trim('"');

            queryParams = CreateQueryString(
                new Dictionary<string, string>()
                {
                    {"name", "Sample video name 1"}, 
                    {"description", "This is a sample video description"}, 
                    {"privacy", configuration["VideoContent:Privacy"]},
                    {"indexingPreset", configuration["VideoContent:IndexingPreset"]},
                    {"personModelId", configuration["VideoContent:PersonModelId"]}
                });

            var videoPath = configuration["VideoContent:Path"];
            var videoLanguage = configuration["VideoContent:Language"];
            var uploadVideoRequest = new HttpRequestMessage(HttpMethod.Post, $"{apiUrl}/{accountLocation}/Accounts/{accountId}/Videos?{queryParams}")
            {
                Content = new MultipartFormDataContent
                {
                    new StreamContent(File.Open(videoPath, FileMode.Open))
                }
            };
            
            uploadVideoRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);  
            uploadVideoRequest.Headers.Add("x-ms-client-request-id", Guid.NewGuid().ToString()); 
            var uploadRequestResult = await client.SendAsync(uploadVideoRequest);
            Console.WriteLine("Response id to log: " + result.Headers.GetValues("x-ms-request-id").FirstOrDefault());
            var uploadResultJson = await uploadRequestResult.Content.ReadAsStringAsync();

            string videoId = JsonDocument.Parse(uploadResultJson).RootElement.GetProperty("id").GetString();
            Console.WriteLine($"Uploaded, Account id: {accountId}, Video ID: {videoId}");
            
            while (true)
            {
                await Task.Delay(10000); 

                queryParams = CreateQueryString(
                    new Dictionary<string, string>()
                    {
                        {"accessToken", accessToken},
                        {"language", videoLanguage},
                    });

                var videoGetIndexRequestResult = await client.GetAsync($"{apiUrl}/{accountLocation}/Accounts/{accountId}/Videos/{videoId}/Index?{queryParams}");
                var videoGetIndexResult = await videoGetIndexRequestResult.Content.ReadAsStringAsync();

                string processingState = JsonDocument.Parse(videoGetIndexResult).RootElement.GetProperty("state").GetString();

                Console.WriteLine("State: " + processingState);

                if (processingState == "Processed" || processingState == "Failed")
                {
                    Console.WriteLine("Full completed index JSON: ");
                    Console.WriteLine(videoGetIndexResult);
                    break;
                }
            }

            Console.ReadKey();
        }

        private static string CreateQueryString(IDictionary<string, string> parameters)
        {
            var queryParameters = System.Web.HttpUtility.ParseQueryString(string.Empty);
            foreach (var parameter in parameters)
            {
                queryParameters[parameter.Key] = parameter.Value;
            }

            return queryParameters.ToString();
        }
    }
}
