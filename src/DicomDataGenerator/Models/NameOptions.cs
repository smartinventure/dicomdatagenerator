namespace DicomDataGenerator.Models
{
    /// <summary>Patient-name generation options.</summary>
    public record NameOptions
    {
        /// <summary>If false, every patient uses FixedLast/FixedFirst.</summary>
        public bool Random { get; init; } = true;
        public string FixedLast { get; init; } = "Doe";
        public string FixedFirst { get; init; } = "John";

        public bool UseEnglish { get; init; } = true;
        public bool UseGerman { get; init; } = false;

        /// <summary>"even" or "weighted" (rank 1 ≈ 10× rank N).</summary>
        public string Weighting { get; init; } = "even";

        // Allowed sexes (random ⇒ 50/50 among the selected). First name matches the chosen sex.
        public bool SexMale { get; init; } = true;
        public bool SexFemale { get; init; } = true;
    }
}
