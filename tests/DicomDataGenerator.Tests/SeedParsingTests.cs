using DicomDataGenerator.Services;

namespace DicomDataGenerator.Tests;

public class SeedParsingTests
{
    [Fact]
    public void ParseRanked_StripsRankNumbers_AndBlanks()
    {
        var f = Path.GetTempFileName();
        File.WriteAllText(f, "\n\n    1. Emilia\n2. Sophia\n3. Emma\n");
        var r = SeedDataLoader.ParseRanked(f, titleCase: false);
        Assert.Equal(3, r.Count);
        Assert.Equal("Emilia", r[0].Name);
        Assert.Equal("Sophia", r[1].Name);
        Assert.True(r[0].Weight > r[2].Weight); // rank 1 weighted higher than last
    }

    [Fact]
    public void ParsePlain_TitleCases_WhenRequested()
    {
        var f = Path.GetTempFileName();
        File.WriteAllText(f, "MARY\nPATRICIA\n");
        var r = SeedDataLoader.ParsePlain(f, titleCase: true);
        Assert.Equal("Mary", r[0].Name);
        Assert.Equal("Patricia", r[1].Name);
    }

    [Fact]
    public void ParseSurnamesHtml_ExtractsNameAndWeight()
    {
        var f = Path.GetTempFileName();
        File.WriteAllText(f,
            "<table><tr><th>Rank</th></tr>" +
            "<tr><td>1</td><td>SMITH</td><td>1.15</td><td>1.15</td></tr>" +
            "<tr><td>2</td><td>JONES</td><td>0.94</td><td>2.09</td></tr></table>");
        var r = SeedDataLoader.ParseSurnamesHtml(f);
        Assert.Equal(2, r.Count);
        Assert.Equal("Smith", r[0].Name);
        Assert.Equal(1.15, r[0].Weight, 3);
        Assert.Equal("Jones", r[1].Name);
    }
}
