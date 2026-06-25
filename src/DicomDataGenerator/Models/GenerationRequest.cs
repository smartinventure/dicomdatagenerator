namespace DicomDataGenerator.Models
{
    /// <summary>Full configuration for one generation run (from the web UI).</summary>
    public record GenerationRequest
    {
        public int Studies { get; init; } = 1;
        public CountSpec Series { get; init; } = new();
        public CountSpec Images { get; init; } = new() { Min = 1, Max = 100 };

        public NameOptions Names { get; init; } = new();

        public string InstitutionName { get; init; } = "easy2BI Test Clinic";
        public string InstitutionAddress { get; init; } = string.Empty;

        public bool ReferringRandom { get; init; } = true;
        public string ReferringFixed { get; init; } = "Dr. Smith";
        public int ReferringPoolSize { get; init; } = 10;

        public string UidRoot { get; init; } = "1.2.826.0.1.3680043.8.498";

        public List<ModalityConfig> Modalities { get; init; } = new();

        /// <summary>Keywords of the DICOM tags to populate (others are skipped, except mandatory identity tags).</summary>
        public List<string> SelectedTags { get; init; } = new();

        public int PixelSize { get; init; } = 8;       // Rows = Columns
        public bool NoPixelData { get; init; } = false;

        public DateOnly StudyDateFrom { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-3));
        public DateOnly StudyDateTo { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow);
        public int PatientAgeMin { get; init; } = 1;
        public int PatientAgeMax { get; init; } = 95;

        /// <summary>How patient birth dates (and ages) are produced. Defaults to deriving from the age range.</summary>
        public BirthDateSpec BirthDate { get; init; } = new();

        public OutputOptions Output { get; init; } = new();
        public PacsOptions Pacs { get; init; } = new();

        /// <summary>Optional seed for reproducible runs (0 = random).</summary>
        public int RandomSeed { get; init; } = 0;
    }
}
