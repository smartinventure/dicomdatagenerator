using DicomDataGenerator.Models;

namespace DicomDataGenerator.Services
{
    /// <summary>
    /// Builds made-up physician names ("Last^First^^Dr." DICOM PN form). The name language follows the
    /// patient-name language selection so referrers match the patients (e.g. German patients ⇒ German Dr.).
    /// </summary>
    public static class ReferringPhysicianPool
    {
        public static List<string> Build(NameProvider names, int count, Random rng, bool useEnglish, bool useGerman)
        {
            var opts = DoctorOptions(useEnglish, useGerman);
            var pool = new List<string>(Math.Max(1, count));
            for (var i = 0; i < Math.Max(1, count); i++)
            {
                pool.Add(Format(names.Next(opts, rng)));
            }
            return pool;
        }

        /// <summary>One made-up physician name (used e.g. for the reading physician, kept distinct from the referrer).</summary>
        public static string BuildOne(NameProvider names, Random rng, bool useEnglish, bool useGerman)
            => Format(names.Next(DoctorOptions(useEnglish, useGerman), rng));

        private static NameOptions DoctorOptions(bool useEnglish, bool useGerman)
        {
            var anyLanguage = useEnglish || useGerman;
            return new NameOptions
            {
                Random = true,
                UseEnglish = anyLanguage ? useEnglish : true,
                UseGerman = useGerman,
                SexMale = true,
                SexFemale = true,
                Weighting = "even"
            };
        }

        private static string Format(GeneratedName n) => $"{n.Last}^{n.First}^^Dr.";
    }
}
