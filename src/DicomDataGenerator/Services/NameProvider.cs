using DicomDataGenerator.Models;

namespace DicomDataGenerator.Services
{
    /// <summary>
    /// Produces patient names from the seed pools: chooses sex (random ⇒ 50/50 among allowed),
    /// language (en/de/both), and picks a sex-matched first name + surname, even or weighted.
    /// </summary>
    public class NameProvider
    {
        private readonly SeedDataLoader _seed;
        // Cache cumulative-weight arrays per pool (built once) for fast weighted picking.
        private readonly Dictionary<IReadOnlyList<WeightedName>, double[]> _cumCache = new();

        public NameProvider(SeedDataLoader seed)
        {
            _seed = seed;
        }

        public GeneratedName Next(NameOptions opts, Random rng)
        {
            var sex = PickSex(opts, rng);

            if (!opts.Random)
            {
                return new GeneratedName(
                    string.IsNullOrWhiteSpace(opts.FixedLast) ? "Doe" : opts.FixedLast.Trim(),
                    string.IsNullOrWhiteSpace(opts.FixedFirst) ? "John" : opts.FixedFirst.Trim(),
                    sex);
            }

            var german = PickLanguageIsGerman(opts, rng);
            var weighted = string.Equals(opts.Weighting, "weighted", StringComparison.OrdinalIgnoreCase);

            var firstPool = german
                ? (sex == "M" ? _seed.GermanMaleFirst : _seed.GermanFemaleFirst)
                : (sex == "M" ? _seed.EnglishMaleFirst : _seed.EnglishFemaleFirst);
            var surnamePool = german ? _seed.GermanSurnames : _seed.EnglishSurnames;

            var first = Pick(firstPool, weighted, rng) ?? (sex == "M" ? "John" : "Jane");
            var last = Pick(surnamePool, weighted, rng) ?? "Doe";
            return new GeneratedName(last, first, sex);
        }

        private static string PickSex(NameOptions opts, Random rng)
        {
            var male = opts.SexMale;
            var female = opts.SexFemale;
            if (male && female) return rng.Next(2) == 0 ? "M" : "F";
            if (male) return "M";
            if (female) return "F";
            return rng.Next(2) == 0 ? "M" : "F"; // none selected → 50/50
        }

        private static bool PickLanguageIsGerman(NameOptions opts, Random rng)
        {
            if (opts.UseEnglish && opts.UseGerman) return rng.Next(2) == 0;
            if (opts.UseGerman) return true;
            return false; // english (or default)
        }

        private string? Pick(IReadOnlyList<WeightedName> pool, bool weighted, Random rng)
        {
            if (pool.Count == 0) return null;
            if (!weighted) return pool[rng.Next(pool.Count)].Name;

            var cum = GetCumulative(pool);
            var total = cum[^1];
            var target = rng.NextDouble() * total;
            // binary search for first cum >= target
            int lo = 0, hi = cum.Length - 1;
            while (lo < hi)
            {
                var mid = (lo + hi) / 2;
                if (cum[mid] < target) lo = mid + 1; else hi = mid;
            }
            return pool[lo].Name;
        }

        private double[] GetCumulative(IReadOnlyList<WeightedName> pool)
        {
            if (_cumCache.TryGetValue(pool, out var cached)) return cached;
            var cum = new double[pool.Count];
            double running = 0;
            for (var i = 0; i < pool.Count; i++)
            {
                running += pool[i].Weight;
                cum[i] = running;
            }
            _cumCache[pool] = cum;
            return cum;
        }
    }
}
