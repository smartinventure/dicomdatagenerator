namespace DicomDataGenerator.Services
{
    /// <summary>DICOM "Body Part Examined" (0018,0015) defined terms used to populate the organ-site picker.</summary>
    public static class BodyPartCatalog
    {
        /// <summary>A practical subset of the DICOM Body Part Examined defined terms (PS3.16), alphabetised.</summary>
        public static readonly string[] All =
        {
            "ABDOMEN", "ABDOMENPELVIS", "ADRENAL", "ANKLE", "AORTA", "ARM", "AXILLA", "BACK", "BLADDER",
            "BRAIN", "BREAST", "BRONCHUS", "BUTTOCK", "CALCANEUS", "CALF", "CAROTID", "CEREBELLUM", "CERVIX",
            "CHEEK", "CHEST", "CHESTABDOMEN", "CHESTABDPELVIS", "CLAVICLE", "COCCYX", "COLON", "DUODENUM",
            "EAR", "ELBOW", "ESOPHAGUS", "EXTREMITY", "EYE", "EYELID", "FACE", "FEMUR", "FINGER", "FOOT",
            "GALLBLADDER", "HAND", "HEAD", "HEADNECK", "HEART", "HIP", "HUMERUS", "ILEUM", "ILIUM", "JAW",
            "JEJUNUM", "KIDNEY", "KNEE", "LARYNX", "LEG", "LIVER", "LSPINE", "LUNG", "MAXILLA", "MEDIASTINUM",
            "MOUTH", "NECK", "NOSE", "ORBIT", "OVARY", "PANCREAS", "PAROTID", "PATELLA", "PELVIS", "PENIS",
            "PHARYNX", "PROSTATE", "RADIUS", "RECTUM", "RIB", "SCALP", "SCAPULA", "SHOULDER", "SKULL", "SPINE",
            "SPLEEN", "SSPINE", "STERNUM", "STOMACH", "TSPINE", "TESTIS", "THIGH", "THUMB", "THYROID", "TIBIA",
            "TOE", "TONGUE", "TRACHEA", "ULNA", "URETER", "URETHRA", "UTERUS", "VAGINA", "WHOLEBODY", "WRIST", "ZYGOMA"
        };
    }
}
