using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureVideoIndexer.CognitiveSearch
{
    public class CognitiveSearchService : ICognitiveSearchService
    {
        //private const string specialCharacters = "+ - & | ! ( ) { } [ ] ^ " ~ * ? : \ /"
        public SearchClient SearchClient { get; }
        public SearchIndexClient IndexClient { get; }
        public SearchIndexerClient IndexerClient { get; }
        public SearchSchema Schema { get; set; }
        public SearchModel Model { get; set; }
        public string IndexName { get; }
        public string IndexerName { get; }

        public CognitiveSearchService(IOptions<CognitiveSearchServiceOptions> configuration)
          : this(configuration.Value)
        {
        }

        public CognitiveSearchService(CognitiveSearchServiceOptions configuration)
        {
            var endpoint = configuration.Endpoint ?? throw new ArgumentException("endpoint");
            var apiKey = configuration.ApiKey ?? throw new ArgumentException("apiKey");
            IndexName = configuration.IndexName ?? throw new ArgumentException("indexName");
            IndexerName = configuration.IndexerName ?? throw new ArgumentException("indexerName");
            IndexClient = new SearchIndexClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
            IndexerClient = new SearchIndexerClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
            SearchClient = IndexClient.GetSearchClient(IndexName);
            Schema = new SearchSchema().AddFields(IndexClient.GetIndex(IndexName).Value?.Fields);
            Model = new SearchModel(Schema);
        }

        public async Task<SearchResults<SearchDocument>> SearchAsync(string searchText, SearchFacet[] searchFacets = null, string[] selectFilter = null, int currentPage = 1)
        {
            try
            {
                SearchOptions options = GenerateSearchOptions(searchFacets, selectFilter, currentPage);                
                var result = await SearchClient.SearchAsync<SearchDocument>(searchText, options);
                return result.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error querying index: {0}\r\n", ex.Message.ToString());
            }
            return null;
        }

        public async Task<SuggestResults<SearchDocument>> SuggestAsync(string searchText, bool fuzzy)
        { 
            try
            {
                SuggestOptions options = new SuggestOptions()
                {
                    UseFuzzyMatching = fuzzy
                };
                 
                var result = await SearchClient.SuggestAsync<SearchDocument>(searchText, "sg", options);
                return result.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error querying index: {0}\r\n", ex.Message.ToString());
            }
            return null;
        }

        public async Task<AutocompleteResults> AutocompleteAsync(string searchText, bool fuzzy)
        { 
            try
            {
                AutocompleteOptions options = new AutocompleteOptions()
                {
                    Mode = AutocompleteMode.OneTermWithContext,
                    UseFuzzyMatching = fuzzy,
                    Size = 8
                }; 
                var result = await SearchClient.AutocompleteAsync(searchText, "sg", options);
                return result.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error querying index: {0}\r\n", ex.Message.ToString());
            }
            return null;
        }

        public async Task<SearchResults<SearchDocument>> LookUpAsync(string id)
        {
            try
            {
                SearchOptions options = new SearchOptions()
                {
                    SearchMode = SearchMode.All,
                    Size = 1,
                    Skip = 0,
                    IncludeTotalCount = true,
                    QueryType = SearchQueryType.Full
                };

                var result = await SearchClient.SearchAsync<SearchDocument>(id, options);
                return result.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error querying index: {0}\r\n", ex.Message.ToString());
            }
            return null;
        }

        public async Task<SearchResults<SearchDocument>> GetFacetsAsync(string searchText, string[] facetNames, int maxCount = 30)
        {
            var facets = new List<string>();

            foreach (var facet in facetNames)
            {
                facets.Add($"{facet}, count:{maxCount}");
            }
             
            try
            {
                SearchOptions options = new SearchOptions()
                {
                    SearchMode = SearchMode.Any,
                    Size = 10,
                    QueryType = SearchQueryType.Full
                };

                options.Select.Add("id");

                if (facets?.Any() == true)
                {
                    facets.ForEach(f => options.Facets.Add(f));
                }
                 
                var result = await SearchClient.SearchAsync<SearchDocument>(searchText, options);
                return result.Value; 
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error querying index: {0}\r\n", ex.Message.ToString());
            }
            return null;
        }

        public async Task RunIndexerAsync()
        {
            try
            {
                _ = await IndexerClient.RunIndexerAsync(IndexerName); 
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error running indexer: {0}\r\n", ex.Message.ToString());
            }
        }

        private SearchOptions GenerateSearchOptions(SearchFacet[] searchFacets = null, string[] selectFilter = null, int currentPage = 1)
        {
            SearchOptions options = new SearchOptions()
            {
                SearchMode = SearchMode.All,
                Size = 10,
                Skip = (currentPage - 1) * 10,
                IncludeTotalCount = true,
                QueryType = SearchQueryType.Full
            };
            if (Model.SelectFilter?.Any() == true)
            {
                Model.SelectFilter.ToList().ForEach(f => options.Select.Add(f));
            }
            if (Model.Facets?.Any() == true)
            {
                Model.Facets.ForEach(f => options.Facets.Add(f.Name));
            }

            string filter = null;
            var filterStr = string.Empty;

            if (searchFacets != null)
            {
                foreach (var item in searchFacets)
                {
                    var facet = Model.Facets.Where(f => f.Name == item.Key).FirstOrDefault();

                    filterStr = string.Join(",", item.Value);

                    // Construct Collection(string) facet query
                    if (facet.Type == SearchFieldDataType.Collection(SearchFieldDataType.String))
                    {
                        if (string.IsNullOrEmpty(filter))
                            filter = $"{item.Key}/any(t: search.in(t, '{filterStr}', ','))";
                        else
                            filter += $" and {item.Key}/any(t: search.in(t, '{filterStr}', ','))";
                    }
                    // Construct string facet query
                    else if (facet.Type == SearchFieldDataType.String)
                    {
                        if (string.IsNullOrEmpty(filter))
                            filter = $"{item.Key} eq '{filterStr}'";
                        else
                            filter += $" and {item.Key} eq '{filterStr}'";
                    }
                    // Construct DateTime facet query
                    else if (facet.Type == SearchFieldDataType.DateTimeOffset)
                    {
                        // TODO: Date filters
                    }
                }
            }

            options.Filter = filter;
            return options;
        }
    }
}
