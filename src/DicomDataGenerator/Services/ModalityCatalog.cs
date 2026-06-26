using DicomDataGenerator.Models;

namespace DicomDataGenerator.Services
{
    /// <summary>
    /// Modalities offered to the UI. The common imaging modalities carry their correct SOP Class UID and a
    /// manufacturer/model pool; the remaining (non-retired) DICOM modality codes are still selectable and
    /// fall back to Secondary Capture Image Storage. Retired modality codes are intentionally excluded.
    /// </summary>
    public class ModalityCatalog
    {
        private const string SecondaryCapture = "1.2.840.10008.5.1.4.1.1.7";

        private record Entry(string SopClassUid, (string Manufacturer, string Model)[] Devices);

        private static readonly Dictionary<string, Entry> Catalog = new(StringComparer.OrdinalIgnoreCase)
        {
            ["CT"] = new("1.2.840.10008.5.1.4.1.1.2", new[] { ("SIEMENS", "SOMATOM Definition"), ("GE MEDICAL SYSTEMS", "Revolution CT"), ("Philips", "Brilliance 64"), ("CANON", "Aquilion ONE") }),
            ["MR"] = new("1.2.840.10008.5.1.4.1.1.4", new[] { ("SIEMENS", "MAGNETOM Skyra"), ("GE MEDICAL SYSTEMS", "SIGNA Pioneer"), ("Philips", "Ingenia 3.0T") }),
            ["US"] = new("1.2.840.10008.5.1.4.1.1.6.1", new[] { ("GE MEDICAL SYSTEMS", "LOGIQ E10"), ("Philips", "EPIQ 7"), ("SIEMENS", "ACUSON Sequoia") }),
            ["CR"] = new("1.2.840.10008.5.1.4.1.1.1", new[] { ("AGFA", "CR 30-X"), ("FUJIFILM", "FCR Prima") }),
            ["DX"] = new("1.2.840.10008.5.1.4.1.1.1.1", new[] { ("SIEMENS", "Ysio Max"), ("GE MEDICAL SYSTEMS", "Discovery XR656") }),
            ["MG"] = new("1.2.840.10008.5.1.4.1.1.1.2", new[] { ("HOLOGIC", "Selenia Dimensions"), ("GE MEDICAL SYSTEMS", "Senographe Pristina") }),
            ["NM"] = new("1.2.840.10008.5.1.4.1.1.20", new[] { ("SIEMENS", "Symbia Intevo"), ("GE MEDICAL SYSTEMS", "Discovery NM630") }),
            ["PT"] = new("1.2.840.10008.5.1.4.1.1.128", new[] { ("SIEMENS", "Biograph mCT"), ("GE MEDICAL SYSTEMS", "Discovery MI") }),
            ["XA"] = new("1.2.840.10008.5.1.4.1.1.12.1", new[] { ("SIEMENS", "Artis zee"), ("Philips", "Azurion 7") }),
            ["RF"] = new("1.2.840.10008.5.1.4.1.1.12.2", new[] { ("SIEMENS", "Luminos dRF"), ("Philips", "ProxiDiagnost") }),
        };

        /// <summary>Non-retired DICOM modality codes (PS3.16 / dicomlibrary.com). Retired codes excluded.</summary>
        private static readonly string[] NonRetired =
        {
            "AR", "ASMT", "AU", "BDUS", "BI", "BMD", "CR", "CT", "DG", "DOC", "DX", "ECG", "EPS", "ES", "FID",
            "GM", "HC", "HD", "IO", "IOL", "IVOCT", "IVUS", "KER", "KO", "LEN", "LS", "MG", "MR", "NM", "OAM",
            "OCT", "OP", "OPM", "OPT", "OPV", "OSS", "OT", "PLAN", "PR", "PT", "PX", "REG", "RESP", "RF", "RG",
            "RTDOSE", "RTIMAGE", "RTPLAN", "RTRECORD", "RTSTRUCT", "RWV", "SEG", "SM", "SMR", "SR", "SRF",
            "STAIN", "TG", "US", "VA", "XA", "XC"
        };

        public IReadOnlyList<string> SupportedModalities =>
            NonRetired.Union(Catalog.Keys, StringComparer.OrdinalIgnoreCase).OrderBy(k => k, StringComparer.Ordinal).ToList();

        public string SopClassUid(string modality)
            => Catalog.TryGetValue(modality, out var e) ? e.SopClassUid : SecondaryCapture;

        /// <summary>Creates a deterministic machine for (modality, 1-based index).</summary>
        public MachineInfo CreateMachine(string modality, int index, UidFactory uids, Random rng)
        {
            var devices = Catalog.TryGetValue(modality, out var e) ? e.Devices : new[] { ("ACME", "Generic Imager") };
            var (manufacturer, model) = devices[(index - 1) % devices.Length];
            return new MachineInfo
            {
                Modality = modality,
                SopClassUid = SopClassUid(modality),
                StationName = $"{modality}{index:00}",
                Manufacturer = manufacturer,
                Model = model,
                DeviceSerialNumber = $"SN{rng.Next(100000, 999999)}",
                DeviceUid = uids.DeviceUid()
            };
        }
    }
}
