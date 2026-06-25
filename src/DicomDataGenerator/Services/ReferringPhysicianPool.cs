using DicomDataGenerator.Models;

namespace DicomDataGenerator.Services
{
    /// <summary>Builds a pool of made-up referring physicians ("Last^First^^Dr." DICOM PN form).</summary>
    public static class ReferringPhysicianPool
    {
        private static readonly NameOptions DoctorNames = new()
        {
            Random = true, UseEnglish = true, UseGerman = true, SexMale = true, SexFemale = true, Weighting = "even"
        };

        public static List<string> Build(NameProvider names, int count, Random rng)
        {
            var pool = new List<string>(Math.Max(1, count));
            for (var i = 0; i < Math.Max(1, count); i++)
            {
                var n = names.Next(DoctorNames, rng);
                pool.Add($"{n.Last}^{n.First}^^Dr.");
            }
            return pool;
        }
    }
}
