namespace DicomDataGenerator.Models
{
    /// <summary>Expected output volume for a request (ranges, since series/images can be random).</summary>
    public record EstimateResult
    {
        public int Studies { get; init; }
        public int SeriesMin { get; init; }
        public int SeriesMax { get; init; }
        public int InstancesMin { get; init; }
        public int InstancesMax { get; init; }
    }
}
