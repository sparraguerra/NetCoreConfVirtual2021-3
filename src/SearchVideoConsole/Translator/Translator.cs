using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SearchVideoConsole
{
    public class Translator
    {
        private const string TranslatorRoute = "/translate?api-version=3.0&to=";
        private readonly string subscriptionKey;
        private readonly string endpoint;
        private readonly string location;

        public Translator(string subscriptionKey, string endpoint, string location)
        {
            this.subscriptionKey = subscriptionKey ?? throw new ArgumentException(null, nameof(subscriptionKey));
            this.endpoint = endpoint ?? throw new ArgumentException(null, nameof(endpoint));
            this.location = location ?? throw new ArgumentException(null, nameof(location));
        }        

        public async Task<string> TranslateTextAsync(string inputText, string translationLanguage)
        {
            object[] body = new object[] { new { Text = inputText } };
            var requestBody = JsonConvert.SerializeObject(body);

            using HttpClient client = new();
            using HttpRequestMessage request = new();

            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri($"{endpoint}{TranslatorRoute}{translationLanguage}");
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            request.Headers.Add("Ocp-Apim-Subscription-Region", location);

            HttpResponseMessage response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string message = await response.Content.ReadAsStringAsync();
                dynamic json = JObject.Parse(message);
                throw new Exception(response.ReasonPhrase);
            }

            string result = await response.Content.ReadAsStringAsync();
            TranslationResult[] deserializedOutput = JsonConvert.DeserializeObject<TranslationResult[]>(result); 
            return deserializedOutput[0].Translations.First().Text;
        }
    }    
}
