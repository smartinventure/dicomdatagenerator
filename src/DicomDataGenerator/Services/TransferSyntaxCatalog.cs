using FellowOakDicom;

namespace DicomDataGenerator.Services
{
    /// <summary>
    /// Transfer syntaxes the generator can write. Limited to the uncompressed/native syntaxes that fo-dicom
    /// handles without external image codecs, so every generated file stays valid. Retired Big Endian is excluded.
    /// </summary>
    public static class TransferSyntaxCatalog
    {
        public static readonly DicomTransferSyntax[] Supported =
        {
            DicomTransferSyntax.ExplicitVRLittleEndian,   // 1.2.840.10008.1.2.1 (default)
            DicomTransferSyntax.ImplicitVRLittleEndian,   // 1.2.840.10008.1.2
            DicomTransferSyntax.DeflatedExplicitVRLittleEndian // 1.2.840.10008.1.2.1.99
        };

        public static readonly DicomTransferSyntax Default = DicomTransferSyntax.ExplicitVRLittleEndian;

        /// <summary>Resolves a UID string to a supported transfer syntax, falling back to the default.</summary>
        public static DicomTransferSyntax Resolve(string? uid)
        {
            if (string.IsNullOrWhiteSpace(uid))
            {
                return Default;
            }
            return Supported.FirstOrDefault(ts => ts.UID.UID == uid) ?? Default;
        }

        /// <summary>UI metadata: each supported syntax as { uid, name }.</summary>
        public static IEnumerable<object> ForUi()
            => Supported.Select(ts => new { uid = ts.UID.UID, name = ts.UID.Name });
    }
}
