using DicomDataGenerator.Models;
using DicomDataGenerator.Services;

namespace DicomDataGenerator.Tests;

public class NameProviderTests
{
    private static SeedDataLoader BuildSeed()
    {
        var root = Path.Combine(Path.GetTempPath(), "ddg-seed-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "de"));
        Directory.CreateDirectory(Path.Combine(root, "en"));
        File.WriteAllText(Path.Combine(root, "en", "firstnames-male-en.txt"), "JAMES\nROBERT\n");
        File.WriteAllText(Path.Combine(root, "en", "firstnames-female-en.txt"), "MARY\nLINDA\n");
        File.WriteAllText(Path.Combine(root, "en", "surnames-en.html"),
            "<table><tr><td>1</td><td>SMITH</td><td>1.1</td><td>1.1</td></tr></table>");
        File.WriteAllText(Path.Combine(root, "de", "firstnames-male-de.txt"), "1. Noah\n");
        File.WriteAllText(Path.Combine(root, "de", "firstnames-female-de.txt"), "1. Emma\n");
        File.WriteAllText(Path.Combine(root, "de", "surnames-de.txt"), "Müller\n");
        var seed = new SeedDataLoader(root);
        seed.Load();
        return seed;
    }

    [Fact]
    public void Random_MaleOnly_English_PicksMatchingFirstName()
    {
        var np = new NameProvider(BuildSeed());
        var opts = new NameOptions { Random = true, UseEnglish = true, UseGerman = false, SexMale = true, SexFemale = false };
        var rng = new Random(42);
        for (var i = 0; i < 20; i++)
        {
            var n = np.Next(opts, rng);
            Assert.Equal("M", n.Sex);
            Assert.Contains(n.First, new[] { "James", "Robert" });
            Assert.Equal("Smith", n.Last);
        }
    }

    [Fact]
    public void Random_FemaleOnly_PicksFemaleFirstName()
    {
        var np = new NameProvider(BuildSeed());
        var opts = new NameOptions { Random = true, UseEnglish = true, UseGerman = false, SexMale = false, SexFemale = true };
        var n = np.Next(opts, new Random(1));
        Assert.Equal("F", n.Sex);
        Assert.Contains(n.First, new[] { "Mary", "Linda" });
    }

    [Fact]
    public void Fixed_UsesProvidedName()
    {
        var np = new NameProvider(BuildSeed());
        var opts = new NameOptions { Random = false, FixedLast = "Doe", FixedFirst = "Jane", SexMale = false, SexFemale = true };
        var n = np.Next(opts, new Random(1));
        Assert.Equal("Doe", n.Last);
        Assert.Equal("Jane", n.First);
        Assert.Equal("F", n.Sex);
    }
}
