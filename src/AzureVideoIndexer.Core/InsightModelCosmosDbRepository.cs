using AzureVideoIndexer.Core;
using AzureVideoIndexer.CosmosDb;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureVideoIndexer.Core
{
    public class InsightModelCosmosDbRepository : CosmosDbRepository<InsightModel>
    {
        private readonly string collectionName;

        public InsightModelCosmosDbRepository(ICosmosDbClientFactory cosmosDbClientFactory, string collectionName)
            : base(cosmosDbClientFactory, collectionName)
        {
            this.collectionName = collectionName;
        }

        public override string CollectionName => collectionName;

    }
}
