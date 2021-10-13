using AzureVideoIndexer.CosmosDb;
using System;

namespace AzureVideoIndexer.Core
{
    public class InsightModel : Entity
    {
        public string AccountId { get; set; }
        public string ExternalId { get; set; }
        public string MetaData { get; set; }
        public Uri PublishedUrl { get; set; }
        public string Language { get; set; }
        public bool IsAdult { get; set; }
        public string Name { get; set; }
        public string Transcript { get; set; }
        public string Ocr { get; set; }
        public string Keywords { get; set; }
        public Topic[] Topics { get; set; }
        public Face[] Faces { get; set; }
        public Label[] Labels { get; set; }
        public Brand[] Brands { get; set; }
        public Named[] NamedLocations { get; set; }
        public Named[] NamedPeople { get; set; }
        public Speaker[] Speakers { get; set; }
    }

    public partial class Brand
    {
        public long Id { get; set; }
        public string ReferenceType { get; set; }
        public string Name { get; set; }
        public string ReferenceId { get; set; }
        public Uri ReferenceUrl { get; set; }
        public string Description { get; set; }
    }

    public partial class Face
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public object Description { get; set; }
        public object Title { get; set; }
    }

    public partial class Keyword
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public long? SpeakerId { get; set; }
    }

    public partial class Label
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ReferenceId { get; set; }
    }

    public partial class Named
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public object ReferenceId { get; set; }
        public object ReferenceUrl { get; set; }
        public object Description { get; set; }
    }

    public partial class Ocr
    {
        public long Id { get; set; }
        public string Text { get; set; }
    }

    public partial class Speaker
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public partial class Topic
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ReferenceId { get; set; }
        public string ReferenceType { get; set; }
        public string IptcName { get; set; }
        public string IabName { get; set; }
        public Uri ReferenceUrl { get; set; }
    }

}
