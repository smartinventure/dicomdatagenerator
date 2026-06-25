namespace DicomDataGenerator.Models
{
    /// <summary>Everything needed to build one DICOM instance (patient/study/series/instance + selection).</summary>
    public record InstanceBuildContext
    {
        public required HashSet<string> SelectedTags { get; init; }
        public int PixelSize { get; init; } = 8;
        public bool NoPixelData { get; init; }

        // Patient
        public required string PatientLast { get; init; }
        public required string PatientFirst { get; init; }
        public required string PatientSex { get; init; }   // M | F
        public required string PatientId { get; init; }
        public int PatientAgeYears { get; init; }
        public DateOnly PatientBirthDate { get; init; }

        // Study
        public required string StudyUid { get; init; }
        public DateOnly StudyDate { get; init; }
        public TimeOnly StudyTime { get; init; }
        public required string AccessionNumber { get; init; }
        public required string StudyId { get; init; }
        public required string StudyDescription { get; init; }
        public required string ReferringPhysician { get; init; }
        public required string InstitutionName { get; init; }
        public string InstitutionAddress { get; init; } = string.Empty;
        public int StudySeriesCount { get; init; }

        // Series
        public required string SeriesUid { get; init; }
        public int SeriesNumber { get; init; }
        public required MachineInfo Machine { get; init; }
        public required string BodyPart { get; init; }
        public string Laterality { get; init; } = string.Empty;
        public required string ProtocolName { get; init; }
        public required string SeriesDescription { get; init; }
        public double? FieldStrength { get; init; }
        public string? Coil { get; init; }
        public DateTimeOffset SeriesDateTime { get; init; }
        public int SeriesInstanceCount { get; init; }

        // Instance
        public required string SopUid { get; init; }
        public int InstanceNumber { get; init; }
        public DateTimeOffset AcquisitionDateTime { get; init; }
    }
}
