using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using System.Threading.Tasks;

namespace AzureVideoIndexer.CognitiveSearch
{
    public interface ICognitiveSearchService
    {
        SearchClient SearchClient { get; }
        SearchIndexClient IndexClient { get; }
        SearchIndexerClient IndexerClient { get; }
        SearchSchema Schema { get; set; }
        SearchModel Model { get; set; }
        Task<SearchResults<SearchDocument>> SearchAsync(string searchText, SearchFacet[] searchFacets = null, string[] selectFilter = null, int currentPage = 1);
        Task<SuggestResults<SearchDocument>> SuggestAsync(string searchText, bool fuzzy);
        Task<AutocompleteResults> AutocompleteAsync(string searchText, bool fuzzy);
        Task<SearchResults<SearchDocument>> LookUpAsync(string id); 
        Task<SearchResults<SearchDocument>> GetFacetsAsync(string searchText, string[] facetNames, int maxCount = 30);
        Task RunIndexerAsync();
    }
}
