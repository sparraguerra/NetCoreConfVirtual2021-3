using Azure.Search.Documents.Indexes.Models;
using System.Collections.Generic;
using System.Linq;

namespace AzureVideoIndexer.CognitiveSearch
{
    public class SearchModel
    {
        private readonly string[] facets = new string[]
        {
            // Add UI facets here in order
            //"people", 
            //"locations",
            //"organizations",
            //"keyphrases"
            //"organizations",
            //"persons",
            //"locations"
        };

        private readonly string[] tags = new string[]
        {
            // Add tags fields here in order
            //"people", 
            //"locations",
            //"organizations",
            //"keyphrases"
            //"keyPhrases",
            //"organizations",
            //"persons",
            //"locations"
        };

        private readonly string[] resultFields = new string[]
        {
            //"id",
            //"metadata_storage_name",
            //"persons",
            //"locations",
            //"organizations",
            //"keyPhrases",
            //"name",
            //"text"
            // Add fields needed to display results cards

            // NOTE: if you customize the resultFields, be sure to include metadata_storage_name,
            // id as those fields are needed for the UI to work properly
            //"people",
            //"locations",
            //"organizations",
            //"keyphrases"
        };

        public List<SearchField> Facets { get; set; }
        public List<SearchField> Tags { get; set; }

        public string[] SelectFilter { get; set; }

        public Dictionary<string, string[]> SearchFacets { get; set; }

        public SearchModel(SearchSchema schema)
        {
            Facets = new List<SearchField>();
            Tags = new List<SearchField>();
            SelectFilter = resultFields;

            if (facets?.Any() == true)
            {
                // add field to facets if in facets arr
                foreach (var field in facets)
                {
                    if (schema.Fields[field]?.IsFacetable == true)
                    {
                        Facets.Add(schema.Fields[field]);
                    }
                }
            }
            else
            {
                foreach (var field in schema.Fields.Where(f => f.Value?.IsFacetable == true))
                {
                    Facets.Add(field.Value);
                }
            }

            if (tags?.Any() == true)
            {
                foreach (var field in tags)
                {
                    if (schema.Fields[field]?.IsFacetable == true)
                    {
                        Tags.Add(schema.Fields[field]);
                    }
                }
            }
            else
            {
                foreach (var field in schema.Fields.Where(f => f.Value?.IsFacetable == true))
                {
                    Tags.Add(field.Value);
                }
            }
        }
    }
}
