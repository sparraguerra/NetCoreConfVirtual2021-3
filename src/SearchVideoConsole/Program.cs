using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace SearchVideoConsole
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

            await SearchVideosAsync(configuration);
        }

        private static async Task SearchVideosAsync(IConfiguration configuration)
        {
            var videoIndexerApiUrl = configuration["AzureVideoIndexer:ApiUrl"];
            var videoIndexerApiKey = configuration["AzureVideoIndexer:ApiKey"];
            var videoIndexerAccountId = configuration["AzureVideoIndexer:AccountId"];
            var videoIndexerAccountLocation = configuration["AzureVideoIndexer:AccountLocation"];
            var traslatorApiUrl = configuration["AzureTranslator:ApiUrl"];
            var traslatorApiKey = configuration["AzureTranslator:ApiKey"];
            var traslatorAccountLocation = configuration["AzureTranslator:AccountLocation"];

            var videoIndexer = new VideoIndexer(videoIndexerAccountId, videoIndexerApiKey, videoIndexerApiUrl, videoIndexerAccountLocation);
            var translator = new Translator(traslatorApiKey, traslatorApiUrl, traslatorAccountLocation);
            var tranlationLanguage = "fr";
            var query = "desarrolladores";
            var textScope = TextScope.Transcript;

            Console.WriteLine($"Searching query: {query}");
            Console.WriteLine($"The text scope to search in: {textScope}");
            
            var searchResult = await videoIndexer.SearchVideosAsync(query, textScope);

            if (searchResult?.Results?.Length > 0)
            {
                
                foreach (var result in searchResult?.Results)
                {
                    Console.WriteLine($"-----------");
                    Console.WriteLine($"Video Name: {result.Name}");
                    Console.WriteLine($"-----------");
                    foreach (var match in result.SearchMatches)
                    {
                        Console.WriteLine($"Match: {match.Text}");
                        Console.WriteLine($"Translation: {await translator.TranslateTextAsync(match.Text, tranlationLanguage)}");

                        Console.WriteLine();
                    }
                }
            }

            Console.WriteLine($"-----------");

            Console.ReadKey();
        }
    }
}
