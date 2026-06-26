using DicomDataGenerator.Models;
using FellowOakDicom;
using Microsoft.Extensions.Logging;

namespace DicomDataGenerator.Services
{
    /// <summary>
    /// Orchestrates one generation run (single in-memory job): studies → series → instances, written
    /// to a folder (flat or nested) or sent to a PACS via C-STORE. Exposes live status + cancel.
    /// </summary>
    public class GenerationService
    {
        private readonly NameProvider _names;
        private readonly ModalityCatalog _catalog;
        private readonly DicomFileBuilder _builder;
        private readonly PacsSender _pacs;
        private readonly ILogger<GenerationService> _logger;

        private int _running;
        private CancellationTokenSource? _cts;
        private GenerationStatus _status = new();

        public GenerationService(NameProvider names, ModalityCatalog catalog, DicomFileBuilder builder, PacsSender pacs, ILogger<GenerationService> logger)
        {
            _names = names;
            _catalog = catalog;
            _builder = builder;
            _pacs = pacs;
            _logger = logger;
        }

        public GenerationStatus Status => _status;

        public bool IsRunning => _running == 1;

        public void Cancel() => _cts?.Cancel();

        public EstimateResult Estimate(GenerationRequest req)
        {
            int sMin = req.Series.Random ? Math.Min(req.Series.Min, req.Series.Max) : Math.Max(0, req.Series.Value);
            int sMax = req.Series.Random ? Math.Max(req.Series.Min, req.Series.Max) : Math.Max(0, req.Series.Value);
            int iMin = req.Images.Random ? Math.Min(req.Images.Min, req.Images.Max) : Math.Max(0, req.Images.Value);
            int iMax = req.Images.Random ? Math.Max(req.Images.Min, req.Images.Max) : Math.Max(0, req.Images.Value);
            return new EstimateResult
            {
                Studies = req.Studies,
                SeriesMin = req.Studies * sMin,
                SeriesMax = req.Studies * sMax,
                InstancesMin = req.Studies * sMin * iMin,
                InstancesMax = req.Studies * sMax * iMax
            };
        }

        public bool TryStart(GenerationRequest req)
        {
            if (Interlocked.CompareExchange(ref _running, 1, 0) != 0)
            {
                return false;
            }
            _cts = new CancellationTokenSource();
            _status = new GenerationStatus { State = "running", StartedUtc = DateTimeOffset.UtcNow, Verified = req.Verify };
            var token = _cts.Token;
            _ = Task.Run(() => RunAsync(req, token));
            return true;
        }

        private async Task RunAsync(GenerationRequest req, CancellationToken ct)
        {
            var status = _status;
            try
            {
                var rng = req.RandomSeed != 0 ? new Random(req.RandomSeed) : new Random();
                var uids = new UidFactory(req.UidRoot);
                var selected = new HashSet<string>(req.SelectedTags ?? new(), StringComparer.Ordinal);

                var mods = (req.Modalities is { Count: > 0 }) ? req.Modalities : new List<ModalityConfig> { new() { Modality = "CT", Machines = 1 } };
                var machinesByMod = mods.ToDictionary(
                    m => m,
                    m => Enumerable.Range(1, Math.Max(1, m.Machines)).Select(i => _catalog.CreateMachine(m.Modality, i, uids, rng)).ToArray());

                var tsPool = (req.TransferSyntaxes is { Count: > 0 })
                    ? req.TransferSyntaxes.Select(TransferSyntaxCatalog.Resolve).ToArray()
                    : TransferSyntaxCatalog.Supported;
                var tsFixed = TransferSyntaxCatalog.Resolve(req.TransferSyntaxFixed);

                // Physician names follow the patient-name language so referrers/readers match the patients.
                var docEnglish = req.Names.UseEnglish;
                var docGerman = req.Names.UseGerman;
                var referring = ReferringPhysicianPool.Build(_names, req.ReferringPoolSize, rng, docEnglish, docGerman);

                var bodyPool = (req.BodySites is { Count: > 0 }) ? req.BodySites.ToArray() : BodyPartCatalog.All;
                var bodyFixed = string.IsNullOrWhiteSpace(req.BodySiteFixed) ? "BRAIN" : req.BodySiteFixed;

                // Pre-roll counts: accurate progress total, then values stay reproducible for a fixed seed.
                var plan = new List<int[]>(req.Studies);
                long total = 0;
                for (var s = 0; s < req.Studies; s++)
                {
                    var sc = req.Series.Next(rng);
                    var arr = new int[sc];
                    for (var k = 0; k < sc; k++) { arr[k] = req.Images.Next(rng); total += arr[k]; }
                    plan.Add(arr);
                }
                status.InstancesTotalEstimate = (int)Math.Min(total, int.MaxValue);

                var dateSpan = Math.Max(0, req.StudyDateTo.DayNumber - req.StudyDateFrom.DayNumber);
                var seriesGlobal = 0; // for even round-robin modality spread across the whole run

                for (var sIdx = 0; sIdx < req.Studies; sIdx++)
                {
                    ct.ThrowIfCancellationRequested();
                    var studyIdx = sIdx + 1;
                    var name = _names.Next(req.Names, rng);
                    var studyDate = req.StudyDateFrom.AddDays(dateSpan == 0 ? 0 : rng.Next(dateSpan + 1));
                    var studyTime = new TimeOnly(rng.Next(7, 19), rng.Next(60), rng.Next(60));
                    var (birth, age) = PatientChronology.Resolve(req.BirthDate, req.PatientAgeMin, req.PatientAgeMax, studyDate, rng);
                    var patientId = $"PID{studyIdx:000000}";
                    var accession = $"ACC{studyIdx:000000}";
                    var refPhys = req.ReferringRandom ? referring[rng.Next(referring.Count)] : req.ReferringFixed;
                    // Reading physician must differ from the referring physician (requesting may equal it).
                    string readPhys;
                    var guard = 0;
                    do { readPhys = ReferringPhysicianPool.BuildOne(_names, rng, docEnglish, docGerman); }
                    while (readPhys == refPhys && ++guard < 5);
                    var bodyPart = req.BodySiteRandom ? bodyPool[rng.Next(bodyPool.Length)] : bodyFixed;
                    var studyUid = uids.StudyUid(studyIdx);
                    var imgCounts = plan[sIdx];
                    var seriesCount = imgCounts.Length;
                    var studyModality0 = mods[0].Modality;
                    var pacsBatch = req.Output.Target == "pacs" ? new List<DicomFile>() : null;

                    for (var seriesIdx = 1; seriesIdx <= seriesCount; seriesIdx++)
                    {
                        ct.ThrowIfCancellationRequested();
                        var modCfg = req.ModalityRandom ? mods[rng.Next(mods.Count)] : mods[seriesGlobal % mods.Count];
                        seriesGlobal++;
                        var machines = machinesByMod[modCfg];
                        var machine = machines[rng.Next(machines.Length)];
                        if (seriesIdx == 1) studyModality0 = modCfg.Modality;

                        var seriesUid = uids.SeriesUid(studyIdx, seriesIdx);
                        var seriesDt = new DateTimeOffset(studyDate.ToDateTime(studyTime), TimeSpan.Zero).AddMinutes((seriesIdx - 1) * 7);
                        var proto = ValuePools.Protocol(modCfg.Modality, bodyPart, rng);
                        var seriesDesc = ValuePools.SeriesDescription(modCfg.Modality, bodyPart, rng);
                        var lat = ValuePools.Pick(ValuePools.Laterality, rng);
                        double? field = modCfg.Modality == "MR" ? ValuePools.MrFieldStrengths[rng.Next(ValuePools.MrFieldStrengths.Length)] : null;
                        string? coil = modCfg.Modality == "MR" ? ValuePools.Pick(ValuePools.MrCoils, rng) : null;
                        var instCount = imgCounts[seriesIdx - 1];

                        for (var inst = 1; inst <= instCount; inst++)
                        {
                            ct.ThrowIfCancellationRequested();
                            var sopUid = uids.SopUid(studyIdx, seriesIdx, inst);
                            var acqDt = seriesDt.AddSeconds((inst - 1) * 2);
                            var ts = req.TransferSyntaxRandom ? tsPool[rng.Next(tsPool.Length)] : tsFixed;

                            var ctx = new InstanceBuildContext
                            {
                                SelectedTags = selected,
                                PixelSize = req.PixelSize,
                                NoPixelData = req.NoPixelData,
                                PatientLast = name.Last,
                                PatientFirst = name.First,
                                PatientSex = name.Sex,
                                PatientId = patientId,
                                PatientAgeYears = age,
                                PatientBirthDate = birth,
                                StudyUid = studyUid,
                                StudyDate = studyDate,
                                StudyTime = studyTime,
                                AccessionNumber = accession,
                                StudyId = studyIdx.ToString(),
                                StudyDescription = ValuePools.StudyDescription(studyModality0, bodyPart),
                                ReferringPhysician = refPhys,
                                ReadingPhysician = readPhys,
                                InstitutionName = req.InstitutionName,
                                InstitutionAddress = req.InstitutionAddress,
                                StudySeriesCount = seriesCount,
                                SeriesUid = seriesUid,
                                SeriesNumber = seriesIdx,
                                Machine = machine,
                                BodyPart = bodyPart,
                                Laterality = lat,
                                ProtocolName = proto,
                                SeriesDescription = seriesDesc,
                                FieldStrength = field,
                                Coil = coil,
                                SeriesDateTime = seriesDt,
                                SeriesInstanceCount = instCount,
                                SopUid = sopUid,
                                InstanceNumber = inst,
                                AcquisitionDateTime = acqDt
                            };

                            var file = _builder.Build(ctx, rng, req.Verify, ts);
                            if (pacsBatch != null)
                            {
                                pacsBatch.Add(file);
                            }
                            else
                            {
                                var path = SavePath(req.Output, name, studyUid, studyDate, seriesIdx, modCfg.Modality, sopUid);
                                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                                await file.SaveAsync(path).ConfigureAwait(false);
                                status.CurrentTarget = path;
                            }
                            status.InstancesDone++;
                        }
                    }

                    if (pacsBatch is { Count: > 0 })
                    {
                        status.CurrentTarget = $"C-STORE {pacsBatch.Count} → {req.Pacs.Host}:{req.Pacs.Port}";
                        await _pacs.SendAsync(pacsBatch, req.Pacs, ct).ConfigureAwait(false);
                    }
                    status.StudiesDone++;
                }

                status.State = "done";
            }
            catch (OperationCanceledException)
            {
                status.State = "cancelled";
            }
            catch (Exception ex)
            {
                status.State = "error";
                status.Errors.Add(ex.Message);
                _logger.LogError(ex, "Generation failed");
            }
            finally
            {
                status.FinishedUtc = DateTimeOffset.UtcNow;
                Interlocked.Exchange(ref _running, 0);
            }
        }

        private static string SavePath(OutputOptions o, GeneratedName name, string studyUid, DateOnly date, int seriesNum, string modality, string sop)
        {
            if (string.Equals(o.Layout, "flat", StringComparison.OrdinalIgnoreCase))
            {
                return Path.Combine(o.FolderPath, sop + ".dcm");
            }
            var patient = Sanitize($"{name.Last}_{name.First}");
            var study = Sanitize($"{date:yyyyMMdd}_{LastSegment(studyUid)}");
            var series = Sanitize($"{seriesNum:00}_{modality}");
            return Path.Combine(o.FolderPath, patient, study, series, sop + ".dcm");
        }

        private static string LastSegment(string uid)
        {
            var i = uid.LastIndexOf('.');
            return i >= 0 ? uid[(i + 1)..] : uid;
        }

        private static string Sanitize(string s)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return new string(s.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray()).Replace(' ', '_');
        }
    }
}
