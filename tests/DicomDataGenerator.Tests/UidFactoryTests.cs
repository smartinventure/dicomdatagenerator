using DicomDataGenerator.Services;

namespace DicomDataGenerator.Tests;

public class UidFactoryTests
{
    [Fact]
    public void StudySeriesSop_AreValidAndNested()
    {
        var f = new UidFactory("1.2.826.0.1.3680043.8.498", runStamp: 1000);
        var study = f.StudyUid(1);
        var series = f.SeriesUid(1, 2);
        var sop = f.SopUid(1, 2, 3);
        Assert.True(UidFactory.IsValid(study));
        Assert.True(UidFactory.IsValid(series));
        Assert.True(UidFactory.IsValid(sop));
        Assert.StartsWith(study, series);
        Assert.StartsWith(series, sop);
    }

    [Fact]
    public void InvalidRoot_IsSanitized_StillValid()
    {
        var f = new UidFactory("not-a-uid!!", runStamp: 1);
        Assert.True(UidFactory.IsValid(f.StudyUid(1)));
    }

    [Theory]
    [InlineData("1.2.3", true)]
    [InlineData("1.2.x", false)]
    [InlineData("", false)]
    public void IsValid_ChecksFormat(string uid, bool expected)
        => Assert.Equal(expected, UidFactory.IsValid(uid));

    [Fact]
    public void IsValid_RejectsTooLong()
        => Assert.False(UidFactory.IsValid(new string('1', 65)));
}
