using System.Globalization;
using System.Text.RegularExpressions;
using DicomDataGenerator.Models;

namespace DicomDataGenerator.Services
{
    /// <summary>
    /// Loads and parses the name seed lists into weighted pools. Ranked lists get a linear weight
    /// (rank 1 ≈ 10× the last); the English surnames HTML uses its database-% column.
    /// </summary>
    public class SeedDataLoader
    {
        private readonly string _root;

        public SeedDataLoader(string seedRootPath)
        {
            _root = seedRootPath;
        }

        public IReadOnlyList<WeightedName> GermanMaleFirst { get; private set; } = Array.Empty<WeightedName>();
        public IReadOnlyList<WeightedName> GermanFemaleFirst { get; private set; } = Array.Empty<WeightedName>();
        public IReadOnlyList<WeightedName> GermanSurnames { get; private set; } = Array.Empty<WeightedName>();
        public IReadOnlyList<WeightedName> EnglishMaleFirst { get; private set; } = Array.Empty<WeightedName>();
        public IReadOnlyList<WeightedName> EnglishFemaleFirst { get; private set; } = Array.Empty<WeightedName>();
        public IReadOnlyList<WeightedName> EnglishSurnames { get; private set; } = Array.Empty<WeightedName>();

        public void Load()
        {
            GermanMaleFirst = ParseRanked(Path.Combine(_root, "de", "firstnames-male-de.txt"), titleCase: false);
            GermanFemaleFirst = ParseRanked(Path.Combine(_root, "de", "firstnames-female-de.txt"), titleCase: false);
            GermanSurnames = ParsePlain(Path.Combine(_root, "de", "surnames-de.txt"), titleCase: false);
            EnglishMaleFirst = ParsePlain(Path.Combine(_root, "en", "firstnames-male-en.txt"), titleCase: true);
            EnglishFemaleFirst = ParsePlain(Path.Combine(_root, "en", "firstnames-female-en.txt"), titleCase: true);
            EnglishSurnames = ParseSurnamesHtml(Path.Combine(_root, "en", "surnames-en.html"));
        }

        // German first names: lines like "  1. Emilia" (blank lines interspersed).
        public static List<WeightedName> ParseRanked(string path, bool titleCase)
        {
            var names = new List<string>();
            if (File.Exists(path))
            {
                foreach (var raw in File.ReadLines(path))
                {
                    var m = Regex.Match(raw.Trim(), @"^\d+\.\s*(.+)$");
                    if (m.Success)
                    {
                        names.Add(Clean(m.Groups[1].Value, titleCase));
                    }
                }
            }
            return RankWeighted(names);
        }

        // Plain one-name-per-line lists (German surnames; English first names in ALL CAPS).
        public static List<WeightedName> ParsePlain(string path, bool titleCase)
        {
            var names = new List<string>();
            if (File.Exists(path))
            {
                foreach (var raw in File.ReadLines(path))
                {
                    var n = raw.Trim();
                    if (n.Length > 0)
                    {
                        names.Add(Clean(n, titleCase));
                    }
                }
            }
            return RankWeighted(names);
        }

        // English surnames: HTML table rows <td>rank</td><td>SURNAME</td><td>pct</td><td>cum</td>.
        public static List<WeightedName> ParseSurnamesHtml(string path)
        {
            var result = new List<WeightedName>();
            if (!File.Exists(path))
            {
                return result;
            }

            var html = File.ReadAllText(path);
            var rx = new Regex(@"<td>\s*\d+\s*</td>\s*<td>\s*([^<]+?)\s*</td>\s*<td>\s*([\d.]+)\s*</td>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            foreach (Match m in rx.Matches(html))
            {
                var name = Clean(m.Groups[1].Value, titleCase: true);
                var weight = double.TryParse(m.Groups[2].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var w) ? w : 1.0;
                if (name.Length > 0)
                {
                    result.Add(new WeightedName(name, Math.Max(weight, 0.0001)));
                }
            }
            return result.Count > 0 ? result : result;
        }

        private static List<WeightedName> RankWeighted(List<string> names)
        {
            var n = names.Count;
            var list = new List<WeightedName>(n);
            for (var i = 0; i < n; i++)
            {
                // rank 1 (i=0) ≈ 10×, last ≈ 1×, linear.
                var weight = n <= 1 ? 1.0 : 1.0 + 9.0 * (n - 1 - i) / (n - 1);
                list.Add(new WeightedName(names[i], weight));
            }
            return list;
        }

        private static string Clean(string s, bool titleCase)
        {
            s = s.Trim();
            if (titleCase)
            {
                s = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.ToLowerInvariant());
            }
            return s;
        }
    }
}
