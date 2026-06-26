using DicomDataGenerator.Models;
using DicomDataGenerator.Services;
using FellowOakDicom;

namespace DicomDataGenerator.Tests;

public class DicomFileBuilderTests
{
    private static InstanceBuildContext Ct(HashSet<string> selected, int pixel = 4, bool noPixel = false) => new()
    {
        SelectedTags = selected,
        PixelSize = pixel,
        NoPixelData = noPixel,
        PatientLast = "Smith",
        PatientFirst = "John",
        PatientSex = "M",
        PatientId = "PID1",
        PatientAgeYears = 40,
        PatientBirthDate = new DateOnly(1985, 1, 1),
        StudyUid = "1.2.3.1",
        StudyDate = new DateOnly(2020, 1, 1),
        StudyTime = new TimeOnly(10, 0),
        AccessionNumber = "ACC1",
        StudyId = "1",
        StudyDescription = "CT HEAD",
        ReferringPhysician = "Ref^Doc^^Dr.",
        ReadingPhysician = "Read^Doc^^Dr.",
        InstitutionName = "Radiology-Site-1",
        StudySeriesCount = 1,
        SeriesUid = "1.2.3.1.1",
        SeriesNumber = 1,
        Machine = new MachineInfo
        {
            Modality = "CT", SopClassUid = "1.2.840.10008.5.1.4.1.1.2", StationName = "CT01",
            Manufacturer = "SIEMENS", Model = "SOMATOM", DeviceSerialNumber = "SN1", DeviceUid = "1.2.9"
        },
        BodyPart = "HEAD",
        Laterality = "",
        ProtocolName = "CT HEAD",
        SeriesDescription = "CT HEAD",
        FieldStrength = null,
        Coil = null,
        SeriesDateTime = new DateTimeOffset(2020, 1, 1, 10, 0, 0, TimeSpan.Zero),
        SeriesInstanceCount = 1,
        SopUid = "1.2.3.1.1.1",
        InstanceNumber = 1,
        AcquisitionDateTime = new DateTimeOffset(2020, 1, 1, 10, 0, 0, TimeSpan.Zero)
    };

    [Fact]
    public void Build_WritesIdentity_SelectedTags_AndTinyPixelData()
    {
        var sel = new HashSet<string> { "Manufacturer", "BodyPartExamined", "StationName", "PatientAge", "PatientBirthDate" };
        var file = new DicomFileBuilder().Build(Ct(sel), new Random(1), verify: true, DicomTransferSyntax.ExplicitVRLittleEndian);
        var ds = file.Dataset;

        Assert.Equal("CT", ds.GetSingleValue<string>(DicomTag.Modality));
        Assert.Equal("Smith^John", ds.GetSingleValue<string>(DicomTag.PatientName));
        Assert.Equal("M", ds.GetSingleValue<string>(DicomTag.PatientSex));
        Assert.Equal("SIEMENS", ds.GetSingleValue<string>(DicomTag.Manufacturer));
        Assert.Equal("HEAD", ds.GetSingleValue<string>(DicomTag.BodyPartExamined));
        Assert.Equal("040Y", ds.GetSingleValue<string>(DicomTag.PatientAge));
        Assert.Equal("19850101", ds.GetSingleValue<string>(DicomTag.PatientBirthDate));
        Assert.True(ds.Contains(DicomTag.PixelData));
        Assert.Equal((ushort)4, ds.GetSingleValue<ushort>(DicomTag.Rows));
    }

    [Fact]
    public void Build_OmitsUnselectedOptionalTags()
    {
        var file = new DicomFileBuilder().Build(Ct(new HashSet<string>()), new Random(1), verify: true, DicomTransferSyntax.ExplicitVRLittleEndian);
        Assert.False(file.Dataset.Contains(DicomTag.Manufacturer)); // not selected
        Assert.True(file.Dataset.Contains(DicomTag.SOPInstanceUID)); // identity always present
    }

    [Fact]
    public void Build_RoundTripsThroughFoDicom()
    {
        var file = new DicomFileBuilder().Build(Ct(new HashSet<string> { "StudyDescription" }), new Random(1), verify: true, DicomTransferSyntax.ExplicitVRLittleEndian);
        using var ms = new MemoryStream();
        file.Save(ms);
        ms.Position = 0;
        var reopened = DicomFile.Open(ms);
        Assert.Equal("1.2.3.1.1.1", reopened.Dataset.GetSingleValue<string>(DicomTag.SOPInstanceUID));
        Assert.Equal("CT HEAD", reopened.Dataset.GetSingleValue<string>(DicomTag.StudyDescription));
    }

    [Fact]
    public void Build_MetadataOnly_HasNoPixelData()
    {
        var file = new DicomFileBuilder().Build(Ct(new HashSet<string>(), noPixel: true), new Random(1), verify: true, DicomTransferSyntax.ExplicitVRLittleEndian);
        Assert.False(file.Dataset.Contains(DicomTag.PixelData));
    }

    [Fact]
    public void Build_TranscodesToRequestedTransferSyntax()
    {
        var file = new DicomFileBuilder().Build(Ct(new HashSet<string>()), new Random(1), verify: true, DicomTransferSyntax.ImplicitVRLittleEndian);
        Assert.Equal(DicomTransferSyntax.ImplicitVRLittleEndian, file.Dataset.InternalTransferSyntax);
        using var ms = new MemoryStream();
        file.Save(ms);
        ms.Position = 0;
        var reopened = DicomFile.Open(ms);
        Assert.Equal(DicomTransferSyntax.ImplicitVRLittleEndian, reopened.FileMetaInfo.TransferSyntax);
    }
}
