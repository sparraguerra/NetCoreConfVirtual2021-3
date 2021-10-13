using Azure.Identity;
using AzureVideoIndexer.CognitiveSearch;
using AzureVideoIndexer.Core;
using AzureVideoIndexer.CosmosDb;
using AzureVideoIndexer.ForMediaService;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.IO;
using System.Linq;
using System.Reflection;

[assembly: FunctionsStartup(typeof(VideoIndexerFunctions.Startup))]
namespace VideoIndexerFunctions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();

            var configuration = configurationBuilder
                                    .SetBasePath(Directory.GetCurrentDirectory())
                                    .AddJsonFile("local.settings.json",optional: true, reloadOnChange: true)
                                    .AddEnvironmentVariables()
                                    .AddUserSecrets(Assembly.GetExecutingAssembly())
                                    .Build();

            // Get data from secrets
            var connectionString = configuration.GetConnectionString("AppConfiguration");
#if DEBUG
            var keyVaultCredential = new AzureCliCredential();
#else 
            var keyVaultCredential = new DefaultAzureCredential();
#endif

            configuration = configurationBuilder.AddAzureAppConfiguration(options =>
            {
                options.Connect(connectionString)
                       .Select(KeyFilter.Any, LabelFilter.Null)
                       .ConfigureKeyVault(kv =>
                       {
                           kv.SetCredential(keyVaultCredential);
                       });
            }).Build();

            builder.Services.AddSingleton<IConfiguration>(configuration);
            builder.Services.Configure<ForMediaServiceOptions>(configuration.GetSection("MediaServiceOptions"));
            builder.Services.AddSingleton<IForMediaService, ForMediaService>();

            builder.Services.Configure<CosmosDbRepositoryOptions>(configuration.GetSection("CosmosDbRepositoryOptions"));
            builder.Services.AddSingleton<ICosmosDbClientFactory>(s =>
            {
                var configuration = s.GetService<IOptions<CosmosDbRepositoryOptions>>();
                return new CosmosDbClientFactory(configuration);
            });

            builder.Services.AddSingleton<ICosmosDbRepository<InsightModel>, InsightModelCosmosDbRepository>(s =>
            {
                var configuration = s.GetService<IOptions<CosmosDbRepositoryOptions>>();
                var factory = s.GetService<ICosmosDbClientFactory>();
                return new InsightModelCosmosDbRepository(factory, configuration.Value.CollectionNames.ElementAt(0));
            });

            builder.Services.Configure<CognitiveSearchServiceOptions>(configuration.GetSection("CognitiveSearchServiceOptions"));
            builder.Services.AddSingleton<ICognitiveSearchService, CognitiveSearchService>();
        }
    }
}
