namespace DicomDataGenerator.Models
{
    /// <summary>A physical imaging device at the site (one per machine of a modality).</summary>
    public record MachineInfo
    {
        public required string Modality { get; init; }
        public required string SopClassUid { get; init; }
        public required string StationName { get; init; }      // e.g. CT01
        public required string Manufacturer { get; init; }
        public required string Model { get; init; }
        public required string DeviceSerialNumber { get; init; }
        public required string DeviceUid { get; init; }
    }
}
