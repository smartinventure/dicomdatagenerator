namespace DicomDataGenerator.Models
{
    /// <summary>A name with a selection weight (higher = more frequent).</summary>
    public record WeightedName(string Name, double Weight);
}
