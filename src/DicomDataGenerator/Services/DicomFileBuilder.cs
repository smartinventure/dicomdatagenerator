using DicomDataGenerator.Models;
using FellowOakDicom;
using FellowOakDicom.IO.Buffer;
using FellowOakDicom.Imaging;

namespace DicomDataGenerator.Services
{
    /// <summary>
    /// Builds one valid DICOM instance (fo-dicom DicomFile) from a context. Identity tags are always
    /// written; all other tags only when selected, and modality-specific technical tags only where they
    /// apply. Tiny pixel data is added unless metadata-only is requested. Sequence (SQ) tags are skipped.
    /// </summary>
    public class DicomFileBuilder
    {
        private static readonly HashSet<string> CtLike = new(StringComparer.OrdinalIgnoreCase) { "CT", "DX", "CR", "MG", "XA", "PT" };

        public DicomFile Build(InstanceBuildContext c, Random rng)
        {
            var sel = c.SelectedTags;
            var modality = c.Machine.Modality;
            var ds = new DicomDataset();

            void Str(DicomTag tag, string? value) { if (!string.IsNullOrEmpty(value)) ds.AddOrUpdate(tag, value); }
            void SelStr(string kw, DicomTag tag, string? value) { if (sel.Contains(kw)) Str(tag, value); }
            void SelDate(string kw, DicomTag tag, DateTime value) { if (sel.Contains(kw)) ds.AddOrUpdate(tag, value); }
            void SelInt(string kw, DicomTag tag, int value) { if (sel.Contains(kw)) ds.AddOrUpdate(tag, value); }
            void SelDs(string kw, DicomTag tag, double value) { if (sel.Contains(kw)) ds.AddOrUpdate(tag, value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)); }

            var studyDt = c.StudyDate.ToDateTime(c.StudyTime);
            var seriesDt = c.SeriesDateTime.UtcDateTime;
            var acqDt = c.AcquisitionDateTime.UtcDateTime;

            // --- Always-on identity (needed for a valid, easy2BI-usable file) ---
            ds.AddOrUpdate(DicomTag.SOPClassUID, c.Machine.SopClassUid);
            ds.AddOrUpdate(DicomTag.SOPInstanceUID, c.SopUid);
            ds.AddOrUpdate(DicomTag.StudyInstanceUID, c.StudyUid);
            ds.AddOrUpdate(DicomTag.SeriesInstanceUID, c.SeriesUid);
            ds.AddOrUpdate(DicomTag.Modality, modality);
            ds.AddOrUpdate(DicomTag.PatientID, c.PatientId);
            ds.AddOrUpdate(DicomTag.PatientName, $"{c.PatientLast}^{c.PatientFirst}");
            ds.AddOrUpdate(DicomTag.PatientSex, c.PatientSex);
            ds.AddOrUpdate(DicomTag.StudyDate, studyDt);
            ds.AddOrUpdate(DicomTag.StudyTime, studyDt);

            // --- Study level ---
            SelStr("SpecificCharacterSet", DicomTag.SpecificCharacterSet, "ISO_IR 100");
            SelStr("AccessionNumber", DicomTag.AccessionNumber, c.AccessionNumber);
            SelStr("StudyDescription", DicomTag.StudyDescription, c.StudyDescription);
            SelStr("StudyID", DicomTag.StudyID, c.StudyId);
            SelStr("ReferringPhysicianName", DicomTag.ReferringPhysicianName, c.ReferringPhysician);
            SelStr("NameOfPhysiciansReadingStudy", DicomTag.NameOfPhysiciansReadingStudy, c.ReferringPhysician);
            SelStr("RequestingPhysician", DicomTag.RequestingPhysician, c.ReferringPhysician);
            SelStr("RequestingService", DicomTag.RequestingService, "Radiology");
            SelStr("RequestedProcedureDescription", DicomTag.RequestedProcedureDescription, c.StudyDescription);
            SelStr("RequestedProcedurePriority", DicomTag.RequestedProcedurePriority, "ROUTINE");
            SelStr("IssuerOfPatientID", DicomTag.IssuerOfPatientID, "easy2BI");
            SelStr("TypeOfPatientID", DicomTag.TypeOfPatientID, "TEXT");
            SelStr("RetrieveAETitle", DicomTag.RetrieveAETitle, "ORTHANC");
            if (sel.Contains("PatientAge")) Str(DicomTag.PatientAge, $"{Math.Clamp(c.PatientAgeYears, 0, 999):000}Y");
            SelDate("PatientBirthDate", DicomTag.PatientBirthDate, c.PatientBirthDate.ToDateTime(TimeOnly.MinValue));
            SelInt("NumberOfStudyRelatedSeries", DicomTag.NumberOfStudyRelatedSeries, c.StudySeriesCount);

            // --- Series level ---
            SelDate("SeriesDate", DicomTag.SeriesDate, seriesDt);
            SelDate("SeriesTime", DicomTag.SeriesTime, seriesDt);
            SelStr("Manufacturer", DicomTag.Manufacturer, c.Machine.Manufacturer);
            SelStr("ManufacturerModelName", DicomTag.ManufacturerModelName, c.Machine.Model);
            SelStr("InstitutionName", DicomTag.InstitutionName, c.InstitutionName);
            SelStr("InstitutionAddress", DicomTag.InstitutionAddress, c.InstitutionAddress);
            SelStr("InstitutionalDepartmentName", DicomTag.InstitutionalDepartmentName, "Radiology");
            SelStr("StationName", DicomTag.StationName, c.Machine.StationName);
            SelStr("OperatorsName", DicomTag.OperatorsName, $"TECH^{c.Machine.StationName}");
            SelStr("SeriesDescription", DicomTag.SeriesDescription, c.SeriesDescription);
            SelStr("ProtocolName", DicomTag.ProtocolName, c.ProtocolName);
            SelStr("BodyPartExamined", DicomTag.BodyPartExamined, c.BodyPart);
            SelInt("SeriesNumber", DicomTag.SeriesNumber, c.SeriesNumber);
            if (sel.Contains("Laterality")) Str(DicomTag.Laterality, c.Laterality);
            SelStr("DeviceSerialNumber", DicomTag.DeviceSerialNumber, c.Machine.DeviceSerialNumber);
            SelStr("DeviceUID", DicomTag.DeviceUID, c.Machine.DeviceUid);
            SelStr("DeviceID", DicomTag.DeviceID, c.Machine.StationName);
            SelStr("SoftwareVersions", DicomTag.SoftwareVersions, "1.0");
            SelStr("TimezoneOffsetFromUTC", DicomTag.TimezoneOffsetFromUTC, "+0000");
            SelInt("NumberOfSeriesRelatedInstances", DicomTag.NumberOfSeriesRelatedInstances, c.SeriesInstanceCount);
            if (modality == "MR" && c.FieldStrength.HasValue) SelDs("MagneticFieldStrength", DicomTag.MagneticFieldStrength, c.FieldStrength.Value);
            SelDate("PerformedProcedureStepStartDate", DicomTag.PerformedProcedureStepStartDate, seriesDt);
            SelDate("PerformedProcedureStepStartTime", DicomTag.PerformedProcedureStepStartTime, seriesDt);
            SelDate("PerformedProcedureStepEndDate", DicomTag.PerformedProcedureStepEndDate, seriesDt);
            SelDate("PerformedProcedureStepEndTime", DicomTag.PerformedProcedureStepEndTime, seriesDt);
            SelStr("PerformedProcedureStepID", DicomTag.PerformedProcedureStepID, $"PPS{c.SeriesNumber}");
            SelStr("PerformedProcedureStepDescription", DicomTag.PerformedProcedureStepDescription, c.SeriesDescription);
            SelDate("ScheduledProcedureStepStartDate", DicomTag.ScheduledProcedureStepStartDate, studyDt);
            SelDate("ScheduledProcedureStepStartTime", DicomTag.ScheduledProcedureStepStartTime, studyDt);
            SelStr("ScheduledProcedureStepDescription", DicomTag.ScheduledProcedureStepDescription, c.StudyDescription);
            SelStr("ScheduledProcedureStepLocation", DicomTag.ScheduledProcedureStepLocation, c.Machine.StationName);

            // --- Image level ---
            SelStr("ImageType", DicomTag.ImageType, "ORIGINAL\\PRIMARY");
            SelDate("AcquisitionDate", DicomTag.AcquisitionDate, acqDt);
            SelDate("ContentDate", DicomTag.ContentDate, acqDt);
            SelDate("AcquisitionTime", DicomTag.AcquisitionTime, acqDt);
            SelDate("ContentTime", DicomTag.ContentTime, acqDt);
            SelDate("AcquisitionDateTime", DicomTag.AcquisitionDateTime, acqDt);
            SelInt("InstanceNumber", DicomTag.InstanceNumber, c.InstanceNumber);
            if (sel.Contains("ImageLaterality")) Str(DicomTag.ImageLaterality, c.Laterality);
            SelStr("PatientPosition", DicomTag.PatientPosition, "HFS");

            // Modality-specific technical tags (only where they make sense)
            if (modality == "MR")
            {
                if (c.Coil != null) SelStr("ReceiveCoilName", DicomTag.ReceiveCoilName, c.Coil);
                SelStr("TransmitCoilName", DicomTag.TransmitCoilName, "BODY");
                SelDs("RepetitionTime", DicomTag.RepetitionTime, rng.Next(400, 6000));
                SelDs("EchoTime", DicomTag.EchoTime, rng.Next(8, 120));
                SelInt("EchoTrainLength", DicomTag.EchoTrainLength, rng.Next(1, 32));
                SelStr("ScanningSequence", DicomTag.ScanningSequence, ValuePools.Pick(ValuePools.MrScanningSequences, rng));
                SelStr("SequenceVariant", DicomTag.SequenceVariant, "NONE");
                SelStr("MRAcquisitionType", DicomTag.MRAcquisitionType, ValuePools.Pick(ValuePools.MrAcquisitionTypes, rng));
            }
            if (CtLike.Contains(modality))
            {
                SelDs("KVP", DicomTag.KVP, rng.Next(80, 141));
                SelInt("ExposureTime", DicomTag.ExposureTime, rng.Next(100, 2000));
                SelInt("Exposure", DicomTag.Exposure, rng.Next(50, 500));
            }
            if (modality is "CT" or "MR")
            {
                SelDs("SliceThickness", DicomTag.SliceThickness, Math.Round(0.5 + rng.NextDouble() * 5.0, 2));
            }
            if (sel.Contains("ContrastBolusAgent") && rng.Next(2) == 0) Str(DicomTag.ContrastBolusAgent, "Iohexol 350");
            SelStr("AcquisitionContrast", DicomTag.AcquisitionContrast, "UNKNOWN");

            // --- Pixel data (tiny, valid) ---
            if (!c.NoPixelData)
            {
                var size = Math.Max(1, c.PixelSize);
                ds.AddOrUpdate(DicomTag.SamplesPerPixel, (ushort)1);
                ds.AddOrUpdate(DicomTag.PhotometricInterpretation, "MONOCHROME2");
                ds.AddOrUpdate(DicomTag.Rows, (ushort)size);
                ds.AddOrUpdate(DicomTag.Columns, (ushort)size);
                ds.AddOrUpdate(DicomTag.BitsAllocated, (ushort)8);
                ds.AddOrUpdate(DicomTag.BitsStored, (ushort)8);
                ds.AddOrUpdate(DicomTag.HighBit, (ushort)7);
                ds.AddOrUpdate(DicomTag.PixelRepresentation, (ushort)0);
                var pixels = new byte[size * size];
                rng.NextBytes(pixels);
                var pd = DicomPixelData.Create(ds, true);
                pd.AddFrame(new MemoryByteBuffer(pixels));
            }

            return new DicomFile(ds);
        }
    }
}
