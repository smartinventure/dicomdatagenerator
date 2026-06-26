namespace DicomDataGenerator.Models
{
    /// <summary>Live status of the (single) generation job.</summary>
    public class GenerationStatus
    {
        public string State { get; set; } = "idle"; // idle | running | done | cancelled | error
        /// <summary>True when this run was generated with fo-dicom validation (AutoValidate) enabled.</summary>
        public bool Verified { get; set; }
        public int StudiesDone { get; set; }
        public int InstancesDone { get; set; }
        public int InstancesTotalEstimate { get; set; }
        public string? CurrentTarget { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTimeOffset? StartedUtc { get; set; }
        public DateTimeOffset? FinishedUtc { get; set; }
    }
}
