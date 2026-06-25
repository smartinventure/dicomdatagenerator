namespace DicomDataGenerator.Models
{
    /// <summary>One DICOM tag from the seed list (seeddata/dicom-tags.json).</summary>
    public record TagInfo
    {
        public required string Group { get; init; }      // "0008"
        public required string Element { get; init; }    // "0020"
        public required string Keyword { get; init; }    // "StudyDate"
        public required string Name { get; init; }       // "Study Date"
        public required string Vr { get; init; }         // "DA"
        public required string Level { get; init; }      // Study | Series | Image
        public bool Core { get; init; }
    }
}
