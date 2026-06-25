using DicomDataGenerator.Models;

namespace DicomDataGenerator.Services
{
    /// <summary>
    /// Resolves a patient's birth date and age for a given study date, honouring the chosen
    /// <see cref="BirthDateSpec"/> mode. A birth date is never allowed to fall after the study date,
    /// so the resulting age is always &gt;= 0.
    /// </summary>
    public static class PatientChronology
    {
        private const int DefaultFixedAgeYears = 40;
        private const int DefaultRandomSpanYears = 90;

        public static (DateOnly Birth, int Age) Resolve(BirthDateSpec? spec, int ageMin, int ageMax, DateOnly studyDate, Random rng)
        {
            switch ((spec?.Mode ?? "age").ToLowerInvariant())
            {
                case "fixed":
                {
                    var birth = spec?.Fixed ?? studyDate.AddYears(-DefaultFixedAgeYears);
                    return (birth, AgeAt(birth, studyDate));
                }
                case "random":
                {
                    var to = spec?.To ?? studyDate;
                    if (to > studyDate) to = studyDate;                 // can't be born after the scan
                    var from = spec?.From ?? to.AddYears(-DefaultRandomSpanYears);
                    if (from > to) from = to.AddYears(-DefaultRandomSpanYears);
                    var birth = RandomDate(from, to, rng);
                    return (birth, AgeAt(birth, studyDate));
                }
                default: // "age"
                {
                    var lo = Math.Max(0, ageMin);
                    var hi = Math.Max(lo, ageMax);
                    var age = rng.Next(lo, hi + 1);
                    return (studyDate.AddYears(-age), age);
                }
            }
        }

        private static int AgeAt(DateOnly birth, DateOnly at)
        {
            var age = at.Year - birth.Year;
            if (at < birth.AddYears(age))
            {
                age--;
            }
            return Math.Max(0, age);
        }

        private static DateOnly RandomDate(DateOnly from, DateOnly to, Random rng)
        {
            var span = to.DayNumber - from.DayNumber;
            return span <= 0 ? from : from.AddDays(rng.Next(0, span + 1));
        }
    }
}
