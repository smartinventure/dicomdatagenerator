namespace DicomDataGenerator.Models
{
    /// <summary>Full configuration for one generation run (from the web UI).</summary>
    public record GenerationRequest
    {
        public int Studies { get; init; } = 1;
        public CountSpec Series { get; init; } = new();
        public CountSpec Images { get; init; } = new() { Min = 1, Max = 100 };

        public NameOptions Names { get; init; } = new();

        public string InstitutionName { get; init; } = "Radiology Clinic";
        public string InstitutionAddress { get; init; } = string.Empty;

        /// <summary>Organ site (Body Part Examined). Fixed value, or random from <see cref="BodySites"/>.</summary>
        public bool BodySiteRandom { get; init; } = true;
        public string BodySiteFixed { get; init; } = "BRAIN";
        /// <summary>Pool to draw from when <see cref="BodySiteRandom"/> is true (empty ⇒ the full catalog).</summary>
        public List<string> BodySites { get; init; } = new();

        public bool ReferringRandom { get; init; } = true;
        /// <summary>Fixed referrer in DICOM PN form: Family^Given^Middle^Prefix (e.g. Smith^John^^Dr.).</summary>
        public string ReferringFixed { get; init; } = "Smith^John^^Dr.";
        public int ReferringPoolSize { get; init; } = 10;

        public string UidRoot { get; init; } = "1.2.826.0.1.3680043.8.498";

        public List<ModalityConfig> Modalities { get; init; } = new();

        /// <summary>Keywords of the DICOM tags to populate (others are skipped, except mandatory identity tags).</summary>
        public List<string> SelectedTags { get; init; } = new();

        public int PixelSize { get; init; } = 8;       // Rows = Columns
        public bool NoPixelData { get; init; } = false;

        /// <summary>When true, fo-dicom validates every element value against its VR while building.</summary>
        public bool Verify { get; init; } = false;

        /// <summary>Transfer syntax of the written files. Fixed value, or random from <see cref="TransferSyntaxes"/>.</summary>
        public bool TransferSyntaxRandom { get; init; } = false;
        public string TransferSyntaxFixed { get; init; } = "1.2.840.10008.1.2.1";
        /// <summary>Pool to draw from when <see cref="TransferSyntaxRandom"/> is true (empty ⇒ all supported).</summary>
        public List<string> TransferSyntaxes { get; init; } = new();

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
