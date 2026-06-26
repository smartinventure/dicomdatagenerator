namespace DicomDataGenerator.Services
{
    /// <summary>Small curated value pools for coded/descriptive tags, so generated data looks realistic.</summary>
    public static class ValuePools
    {
        public static readonly string[] Laterality = { "", "L", "R" };

        public static readonly string[] PatientPositions = { "HFS", "FFS", "HFP", "FFP" };

        public static readonly string[] MrScanningSequences = { "SE", "GR", "IR", "EP" };
        public static readonly string[] MrAcquisitionTypes = { "2D", "3D" };
        public static readonly string[] MrCoils = { "HEAD_8CH", "BODY_18CH", "SPINE_32CH", "KNEE_15CH", "SHOULDER_16CH" };
        public static readonly double[] MrFieldStrengths = { 1.5, 3.0 };

        public static string Pick(string[] pool, Random rng) => pool[rng.Next(pool.Length)];

        public static string Protocol(string modality, string bodyPart, Random rng)
        {
            var suffix = modality switch
            {
                "CT" => rng.Next(2) == 0 ? "w/o contrast" : "w/ contrast",
                "MR" => Pick(new[] { "T1", "T2", "FLAIR", "DWI", "STIR" }, rng),
                "US" => "B-mode",
                _ => "routine"
            };
            return $"{modality} {bodyPart} {suffix}".Trim();
        }

        public static string StudyDescription(string modality, string bodyPart) => $"{modality} {bodyPart}";

        public static string SeriesDescription(string modality, string bodyPart, Random rng) => Protocol(modality, bodyPart, rng);
    }
}
