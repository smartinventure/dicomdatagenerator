namespace DicomDataGenerator.Models
{
    /// <summary>A fixed count, or a random count within [Min,Max].</summary>
    public record CountSpec
    {
        public int Value { get; init; } = 1;
        public bool Random { get; init; } = false;
        public int Min { get; init; } = 1;
        public int Max { get; init; } = 5;

        public int Next(Random rng) => Random ? rng.Next(System.Math.Min(Min, Max), System.Math.Max(Min, Max) + 1) : System.Math.Max(0, Value);
    }
}
