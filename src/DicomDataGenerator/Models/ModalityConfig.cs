namespace DicomDataGenerator.Models
{
    /// <summary>A selected modality and how many machines of it the site has.</summary>
    public record ModalityConfig
    {
        public required string Modality { get; init; }  // CT, MR, US, ...
        public int Machines { get; init; } = 1;
    }
}
