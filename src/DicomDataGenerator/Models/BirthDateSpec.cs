namespace DicomDataGenerator.Models
{
    /// <summary>
    /// How each patient's birth date (and resulting age) is determined.
    /// <list type="bullet">
    /// <item><c>age</c> — derive the birth date from a random age in [PatientAgeMin, PatientAgeMax] (default).</item>
    /// <item><c>fixed</c> — every patient uses <see cref="Fixed"/>.</item>
    /// <item><c>random</c> — a random date in [<see cref="From"/>, <see cref="To"/>]; the UI suggests the last ~90 years.</item>
    /// </list>
    /// </summary>
    public record BirthDateSpec
    {
        public string Mode { get; init; } = "age";
        public DateOnly? Fixed { get; init; }
        public DateOnly? From { get; init; }
        public DateOnly? To { get; init; }
    }
}
