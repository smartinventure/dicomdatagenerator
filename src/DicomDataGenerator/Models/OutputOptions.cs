namespace DicomDataGenerator.Models
{
    /// <summary>Where generated instances go.</summary>
    public record OutputOptions
    {
        /// <summary>"folder" or "pacs".</summary>
        public string Target { get; init; } = "folder";

        public string FolderPath { get; init; } = string.Empty;

        /// <summary>"flat" or "nested" (Patient ▸ Study ▸ Series ▸ instance.dcm).</summary>
        public string Layout { get; init; } = "nested";
    }
}
