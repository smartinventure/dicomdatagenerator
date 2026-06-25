using DicomDataGenerator.Models;
using DicomDataGenerator.Services;

namespace DicomDataGenerator.Tests;

public class PatientChronologyTests
{
    private static readonly DateOnly Study = new(2024, 6, 15);

    [Fact]
    public void AgeMode_DerivesBirthFromAgeRange()
    {
        var spec = new BirthDateSpec { Mode = "age" };
        for (var i = 0; i < 50; i++)
        {
            var (birth, age) = PatientChronology.Resolve(spec, 30, 30, Study, new Random(i));
            Assert.Equal(30, age);
            Assert.Equal(Study.AddYears(-30), birth);
        }
    }

    [Fact]
    public void FixedMode_UsesGivenDate_AndComputesAge()
    {
        var spec = new BirthDateSpec { Mode = "fixed", Fixed = new DateOnly(1980, 1, 1) };
        var (birth, age) = PatientChronology.Resolve(spec, 1, 95, Study, new Random(1));
        Assert.Equal(new DateOnly(1980, 1, 1), birth);
        Assert.Equal(44, age);
    }

    [Fact]
    public void RandomMode_StaysWithinRange_AndAgeNonNegative()
    {
        var from = new DateOnly(1950, 1, 1);
        var to = new DateOnly(2000, 12, 31);
        var spec = new BirthDateSpec { Mode = "random", From = from, To = to };
        for (var i = 0; i < 200; i++)
        {
            var (birth, age) = PatientChronology.Resolve(spec, 1, 95, Study, new Random(i));
            Assert.True(birth >= from && birth <= to);
            Assert.True(age >= 0);
        }
    }

    [Fact]
    public void RandomMode_DefaultsToLast90Years_WhenRangeOmitted()
    {
        var spec = new BirthDateSpec { Mode = "random" };
        var (birth, _) = PatientChronology.Resolve(spec, 1, 95, Study, new Random(3));
        Assert.True(birth >= Study.AddYears(-90));
        Assert.True(birth <= Study);
    }

    [Fact]
    public void RandomMode_NeverBornAfterStudy_EvenIfToInFuture()
    {
        var spec = new BirthDateSpec { Mode = "random", From = Study.AddYears(-5), To = Study.AddYears(10) };
        for (var i = 0; i < 100; i++)
        {
            var (birth, age) = PatientChronology.Resolve(spec, 1, 95, Study, new Random(i));
            Assert.True(birth <= Study);
            Assert.True(age >= 0);
        }
    }
}
