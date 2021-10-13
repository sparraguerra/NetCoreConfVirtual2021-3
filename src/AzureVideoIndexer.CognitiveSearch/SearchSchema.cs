using Azure.Search.Documents.Indexes.Models;
using System;
using System.Collections.Generic;

namespace AzureVideoIndexer.CognitiveSearch
{
    [Serializable]
    public class SearchSchema
    {
        public SearchSchema()
        {
        }

        public Dictionary<string, SearchField> Fields = new Dictionary<string, SearchField>();
    }

    public static partial class Extensions
    {
        public static SearchSchema AddFields(this SearchSchema schema, IEnumerable<SearchField> fields)
        {
            foreach (var field in fields)
            {
                schema.Fields[field.Name] = field;
            }
            return schema;
        }
    }
}
