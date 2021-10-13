using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace SearchVideoConsole
{
    public partial class VideoIndexer
    {
        private readonly HttpClient client;

        private readonly string accountId;
        private readonly string subscriptionKey;
        private readonly string endpoint;
        private readonly string location;

        public VideoIndexer(string accountId, string subscriptionKey, string endpoint, string location)
        {   
            this.accountId = accountId ?? throw new ArgumentException(null, nameof(accountId));
            this.subscriptionKey = subscriptionKey ?? throw new ArgumentException(null, nameof(subscriptionKey));
            this.endpoint = endpoint ?? throw new ArgumentException(null, nameof(endpoint));
            this.location = location ?? throw new ArgumentException(null, nameof(location));
            this.client = GetClient();
        }
        public static HttpClient GetClient()
        {
            SocketsHttpHandler handler = new();
            handler.SslOptions.EnabledSslProtocols |= SslProtocols.Tls12;
            return new HttpClient(handler);
        }
 
        public async Task<string> GetAccountAccessTokenAsync()
        {
            string queryParams = CreateQueryString(
               new Dictionary<string, string>()
               {
                    {"allowEdit", "true"},
               });

            return await GetAccessTokenAsync($"{endpoint}/auth/{location}/Accounts/{accountId}/AccessToken?{queryParams}");
        }

        public async Task<string> GetVideoAccessTokenAsync(string videoId)
        {
            string queryParams = CreateQueryString(
                new Dictionary<string, string>()
                {
                    {"allowEdit", "true"},
                });

            return await GetAccessTokenAsync($"{endpoint}/auth/{location}/Accounts/{accountId}/Videos/{videoId}/AccessToken?{queryParams}");
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
            Uri requestUri = new($"{endpoint}/{location}/Accounts/{accountId}/Videos/Search?{queryParams}");

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

            Uri requestUri = new($"{endpoint}/{location}/Accounts/{accountId}/Videos/{videoId}/Index?{queryParams}");

            HttpResponseMessage insightsRequestResult = await client.GetAsync(requestUri);

            if (!insightsRequestResult.IsSuccessStatusCode)
            {
                throw new Exception(insightsRequestResult.ReasonPhrase);
            }

            return await insightsRequestResult.Content.ReadAsStringAsync();
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
