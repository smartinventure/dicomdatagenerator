namespace DicomDataGenerator.Models
{
    /// <summary>Target PACS for C-STORE output (defaults suit Orthanc4Dev).</summary>
    public record PacsOptions
    {
        public string Host { get; init; } = "localhost";
        public int Port { get; init; } = 4242;
        public string CalledAet { get; init; } = "ORTHANC";
        public string CallingAet { get; init; } = "DICOMGEN";
    }
}
