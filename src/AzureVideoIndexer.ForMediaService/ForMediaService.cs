using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureVideoIndexer.ForMediaService
{
    public class ForMediaService : IForMediaService
    {
        private readonly string endpoint;
        private readonly string subscriptionKey;
        private readonly string accountId;
        private readonly string location;
        private readonly HttpClient client;

        public ForMediaService (IOptions<ForMediaServiceOptions> configuration)
            : this(configuration.Value)
        {
        }

        public ForMediaService(ForMediaServiceOptions configuration)
        {
            this.accountId = configuration.AccountId ?? throw new ArgumentException(null, nameof(accountId));
            this.subscriptionKey = configuration.ApiKey ?? throw new ArgumentException(null, nameof(subscriptionKey));
            this.endpoint = configuration.ApiUrl ?? throw new ArgumentException(null, nameof(endpoint));
            this.location = configuration.AccountLocation ?? throw new ArgumentException(null, nameof(location));

            var httpHandler = new SocketsHttpHandler();
            httpHandler.SslOptions.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            this.client = new HttpClient(httpHandler);
        }

        public async Task UploadVideo(string url, string name)
        {
            name = name.Split('.')[0];

            var accessToken = await GetAccountAccessTokenAsync();

            var queryParams = CreateQueryString(
                new Dictionary<string, string>()
                {
                    {"name", name },
                    {"videoUrl", url },
                    {"language", "auto" },
                    {"indexingPreset", "Advanced" },
                    {"callbackUrl", $"https://videoindexerfunctions.azurewebsites.net/api/SaveVideoInsights_Http?projectName=test" }
                });

            var uploadVideoRequest = new HttpRequestMessage(HttpMethod.Post, $"{endpoint}/{location}/Accounts/{accountId}/Videos?{queryParams}");

            uploadVideoRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            uploadVideoRequest.Headers.Add("x-ms-client-request-id", Guid.NewGuid().ToString());
            var uploadRequestResult = await client.SendAsync(uploadVideoRequest);
            var uploadResultJson = await uploadRequestResult.Content.ReadAsStringAsync();

        }

        public async Task<SearchResult> SearchVideosAsync(string query, TextScope textScope)
        {
            string accessToken = await GetAccountAccessTokenAsync();

            string queryParams = CreateQueryString(
                    new Dictionary<string, string>()
                    {
                        {"accessToken", accessToken},
                        {"query", query},
                        {"textScope", textScope.ToString()},
                        {"isBase", "true"}
                    });
            Uri requestUri = new Uri($"{endpoint}/{location}/Accounts/{accountId}/Videos/Search?{queryParams}");

            HttpResponseMessage searchRequestResult = await client.GetAsync(requestUri);

            if (!searchRequestResult.IsSuccessStatusCode)
            {
                throw new Exception(searchRequestResult.ReasonPhrase);
            }

            string result = await searchRequestResult.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<SearchResult>(result);
        }

        public async Task<string> GetInsightsAsync(string videoId)
        {
            string accessToken = await GetVideoAccessTokenAsync(videoId);
            string queryParams = CreateQueryString(
                    new Dictionary<string, string>()
                    {
                        {"accessToken", accessToken}
                    });

            Uri requestUri = new Uri($"{endpoint}/{location}/Accounts/{accountId}/Videos/{videoId}/Index?{queryParams}");

            HttpResponseMessage insightsRequestResult = await client.GetAsync(requestUri);

            if (!insightsRequestResult.IsSuccessStatusCode)
            {
                throw new Exception(insightsRequestResult.ReasonPhrase);
            }

            return await insightsRequestResult.Content.ReadAsStringAsync();
        }

        private async Task<string> GetAccountAccessTokenAsync()
        {
            string queryParams = CreateQueryString(
               new Dictionary<string, string>()
               {
                    {"allowEdit", "true"},
               });

            return await GetAccessTokenAsync($"{endpoint}/auth/{location}/Accounts/{accountId}/AccessToken?{queryParams}");
        }

        private async Task<string> GetAccessTokenAsync(string requestUrl)
        {
            client.DefaultRequestHeaders.Add("x-ms-client-request-id", Guid.NewGuid().ToString());
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            var request = await client.GetAsync(new Uri(requestUrl));

            if (!request.IsSuccessStatusCode)
            {
                throw new Exception(request.ReasonPhrase);
            }

            client.DefaultRequestHeaders.Remove("Ocp-Apim-Subscription-Key");

            return (await request.Content.ReadAsStringAsync()).Trim('"');
        }

        private async Task<string> GetVideoAccessTokenAsync(string videoId)
        {
            string queryParams = CreateQueryString(
                new Dictionary<string, string>()
                {
                    {"allowEdit", "true"},
                });

            return await GetAccessTokenAsync($"{endpoint}/auth/{location}/Accounts/{accountId}/Videos/{videoId}/AccessToken?{queryParams}");
        }

        private string CreateQueryString(IDictionary<string, string> parameters)
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
