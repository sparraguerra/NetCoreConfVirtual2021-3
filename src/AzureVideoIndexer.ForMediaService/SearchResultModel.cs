using System;

namespace AzureVideoIndexer.ForMediaService
{
    public partial class SearchResult
    {
        public Result[] Results { get; set; }
        public NextPage NextPage { get; set; }
    }

    public partial class NextPage
    {
        public long PageSize { get; set; }
        public long Skip { get; set; }
        public bool Done { get; set; }
    }

    public partial class Result
    {
        public Guid AccountId { get; set; }
        public string Id { get; set; }
        public object Partition { get; set; }
        public object ExternalId { get; set; }
        public object Metadata { get; set; }
        public string Name { get; set; }
        public object Description { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset LastModified { get; set; }
        public DateTimeOffset LastIndexed { get; set; }
        public string PrivacyMode { get; set; }
        public string UserName { get; set; }
        public bool IsOwned { get; set; }
        public bool IsBase { get; set; }
        public bool HasSourceVideoFile { get; set; }
        public string State { get; set; }
        public string ModerationState { get; set; }
        public string ReviewState { get; set; }
        public string ProcessingProgress { get; set; }
        public long DurationInSeconds { get; set; }
        public string ThumbnailVideoId { get; set; }
        public Guid ThumbnailId { get; set; }
        public SearchMatch[] SearchMatches { get; set; }
        public string IndexingPreset { get; set; }
        public string StreamingPreset { get; set; }
        public string SourceLanguage { get; set; }
        public string[] SourceLanguages { get; set; }
        public Guid PersonModelId { get; set; }
    }

    public partial class SearchMatch
    {
        public DateTimeOffset StartTime { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
        public string ExactText { get; set; }
    }
}
