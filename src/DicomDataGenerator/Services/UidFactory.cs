using System.Text.RegularExpressions;
using FellowOakDicom;

namespace DicomDataGenerator.Services
{
    /// <summary>
    /// Builds valid DICOM UIDs from a user-supplied root plus a per-run stamp and hierarchy counters
    /// (study.series.instance). Falls back to a fo-dicom generated UID if a candidate is invalid/too long.
    /// </summary>
    public class UidFactory
    {
        private readonly string _root;
        private readonly long _runStamp;

        public UidFactory(string root, long? runStamp = null)
        {
            _root = Sanitize(root);
            _runStamp = runStamp ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public string StudyUid(int studyIdx) => Make($"{_root}.{_runStamp}.{studyIdx}");
        public string SeriesUid(int studyIdx, int seriesIdx) => Make($"{_root}.{_runStamp}.{studyIdx}.{seriesIdx}");
        public string SopUid(int studyIdx, int seriesIdx, int instanceIdx) => Make($"{_root}.{_runStamp}.{studyIdx}.{seriesIdx}.{instanceIdx}");
        public string DeviceUid() => DicomUIDGenerator.GenerateDerivedFromUUID().UID;

        private static string Make(string candidate)
            => IsValid(candidate) ? candidate : DicomUIDGenerator.GenerateDerivedFromUUID().UID;

        public static bool IsValid(string uid)
            => uid.Length is > 0 and <= 64 && Regex.IsMatch(uid, @"^[0-9]+(\.[0-9]+)*$");

        private static string Sanitize(string root)
        {
            // Keep digits and dots; collapse junk. Default to a generic example root if empty.
            var cleaned = Regex.Replace(root ?? string.Empty, @"[^0-9.]", "").Trim('.');
            return string.IsNullOrEmpty(cleaned) ? "1.2.826.0.1.3680043.8.498" : cleaned;
        }
    }
}
