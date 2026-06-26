const { createApp, ref, reactive, computed, onMounted } = Vue;

const MODALITY_NAMES = {
  CR: 'Computed Radiography (digitised X-ray plates)',
  CT: 'Computed Tomography',
  DX: 'Digital Radiography (direct digital X-ray)',
  MG: 'Mammography',
  MR: 'Magnetic Resonance Imaging',
  NM: 'Nuclear Medicine',
  PT: 'Positron Emission Tomography (PET)',
  US: 'Ultrasound',
  XA: 'X-Ray Angiography'
};

const todayIso = () => new Date().toISOString().slice(0, 10);
const yearsAgoIso = (y) => { const d = new Date(); d.setFullYear(d.getFullYear() - y); return d.toISOString().slice(0, 10); };

const CountSpec = {
  props: ['spec'],
  template: `
    <div>
      <div class="form-check form-check-inline">
        <input class="form-check-input" type="checkbox" v-model="spec.Random" :id="rid" />
        <label class="form-check-label" :for="rid">random</label>
      </div>
      <input v-if="!spec.Random" type="number" min="0" class="form-control mt-1" v-model.number="spec.Value" />
      <div v-else class="input-group mt-1">
        <span class="input-group-text">min</span>
        <input type="number" min="0" class="form-control" v-model.number="spec.Min" />
        <span class="input-group-text">max</span>
        <input type="number" min="0" class="form-control" v-model.number="spec.Max" />
      </div>
    </div>`,
  computed: { rid() { return 'rnd-' + Math.random().toString(36).slice(2, 8); } }
};

createApp({
  components: { 'count-spec': CountSpec },
  setup() {
    const req = reactive({
      Studies: 1,
      Series: { Value: 1, Random: false, Min: 1, Max: 5 },
      Images: { Value: 1, Random: false, Min: 1, Max: 100 },
      Names: { Random: true, FixedLast: 'Doe', FixedFirst: 'John', UseEnglish: true, UseGerman: false, Weighting: 'even', SexMale: true, SexFemale: true },
      InstitutionName: 'Radiology Clinic',
      InstitutionAddress: '',
      BodySiteRandom: true,
      BodySiteFixed: 'BRAIN',
      ReferringRandom: true,
      ReferringFixed: 'Smith^John^^Dr.',
      ReferringPoolSize: 10,
      UidRoot: '1.2.826.0.1.3680043.8.498',
      PixelSize: 8,
      NoPixelData: false,
      Verify: false,
      TransferSyntaxRandom: false,
      TransferSyntaxFixed: '1.2.840.10008.1.2.1',
      StudyDateFrom: yearsAgoIso(3),
      StudyDateTo: todayIso(),
      PatientAgeMin: 1,
      PatientAgeMax: 95,
      BirthDate: { Mode: 'age', Fixed: null, From: null, To: null },
      Output: { Target: 'folder', FolderPath: '', Layout: 'nested' },
      Pacs: { Host: 'localhost', Port: 4242, CalledAet: 'ORTHANC', CallingAet: 'DICOMGEN' },
      RandomSeed: 0
    });

    const modalityRows = ref([]);
    const customModality = reactive({ enabled: false, modality: '', machines: 1 });
    const bodySites = ref([]); // {value, checked}
    const transferSyntaxes = ref([]); // {uid, name, checked}
    const tags = ref([]); // {keyword,name,group,element,level,core,checked}
    const error = ref('');
    const estimateText = ref('');
    const status = ref(null);
    const running = ref(false);
    const completedModal = ref(false);
    let pollTimer = null;

    const fs = reactive({ open: false, current: '', parent: null, entries: [] });

    const tagsByLevel = computed(() => {
      const g = { Study: [], Series: [], Image: [] };
      for (const t of tags.value) (g[t.level] || (g[t.level] = [])).push(t);
      return g;
    });
    const selectedTagCount = computed(() => tags.value.filter(t => t.checked).length);
    const selectedBodyCount = computed(() => bodySites.value.filter(b => b.checked).length);
    const pct = computed(() => {
      if (!status.value || !status.value.instancesTotalEstimate) return 0;
      return Math.min(100, Math.round(100 * status.value.instancesDone / status.value.instancesTotalEstimate));
    });

    const modalityName = (m) => MODALITY_NAMES[m] || m;

    const getJson = async (url) => { const r = await fetch(url); if (!r.ok) throw new Error(await r.text()); return r.json(); };
    const postJson = async (url, body) => {
      const r = await fetch(url, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
      if (!r.ok) { let m = r.statusText; try { m = (await r.json()).message || m; } catch {} throw new Error(m); }
      return r.status === 202 ? {} : r.json().catch(() => ({}));
    };

    const setAllTags = (v) => tags.value.forEach(t => t.checked = v);
    const setLevel = (lvl, v) => tags.value.filter(t => t.level === lvl).forEach(t => t.checked = v);
    const setAllBody = (v) => bodySites.value.forEach(b => b.checked = v);

    const suggestBirthRange = () => {
      if (!req.BirthDate.From) req.BirthDate.From = yearsAgoIso(90);
      if (!req.BirthDate.To) req.BirthDate.To = todayIso();
    };

    const buildRequest = () => ({
      ...req,
      BirthDate: {
        Mode: req.BirthDate.Mode,
        Fixed: req.BirthDate.Fixed || null,
        From: req.BirthDate.From || null,
        To: req.BirthDate.To || null
      },
      Modalities: [
        ...modalityRows.value.filter(m => m.enabled).map(m => ({ Modality: m.modality, Machines: m.machines })),
        ...(customModality.enabled && customModality.modality.trim()
          ? [{ Modality: customModality.modality.trim().toUpperCase(), Machines: customModality.machines }]
          : [])
      ],
      BodySites: bodySites.value.filter(b => b.checked).map(b => b.value),
      TransferSyntaxes: transferSyntaxes.value.filter(t => t.checked).map(t => t.uid),
      SelectedTags: tags.value.filter(t => t.checked).map(t => t.keyword)
    });

    const estimate = async () => {
      error.value = '';
      try {
        const e = await postJson('/api/generate/estimate', buildRequest());
        estimateText.value = `≈ ${e.studies} studies, ${e.seriesMin}–${e.seriesMax} series, ${e.instancesMin}–${e.instancesMax} instances (files).`;
      } catch (ex) { error.value = ex.message; }
    };

    const generate = async () => {
      error.value = ''; estimateText.value = '';
      if (req.Output.Target === 'folder' && !req.Output.FolderPath) { error.value = 'Choose an output folder.'; return; }
      try {
        await postJson('/api/generate', buildRequest());
        running.value = true;
        poll();
      } catch (ex) { error.value = ex.message; }
    };

    const poll = () => {
      clearInterval(pollTimer);
      pollTimer = setInterval(async () => {
        try {
          status.value = await getJson('/api/generate/status');
          if (['done', 'cancelled', 'error', 'idle'].includes(status.value.state)) {
            running.value = false;
            clearInterval(pollTimer);
            completedModal.value = true;   // confirm-only modal; Generate stays disabled until confirmed
          }
        } catch { /* keep polling */ }
      }, 700);
    };

    const dismissCompleted = () => { completedModal.value = false; };

    const cancel = async () => { try { await postJson('/api/generate/cancel', {}); } catch {} };

    const openFs = () => { fs.open = true; loadFs(''); };
    const loadFs = async (path) => {
      try {
        const res = await getJson('/api/fs/list' + (path ? ('?path=' + encodeURIComponent(path)) : ''));
        if (Array.isArray(res)) { fs.entries = res; fs.current = ''; fs.parent = null; }
        else { fs.entries = res.directories; fs.current = res.path; fs.parent = res.parent; }
      } catch (ex) { error.value = ex.message; }
    };
    const pickFs = () => { if (fs.current) { req.Output.FolderPath = fs.current; fs.open = false; } };

    onMounted(async () => {
      try {
        const mods = await getJson('/api/seed/modalities');
        modalityRows.value = mods.map(m => ({ modality: m, enabled: m === 'CT' || m === 'MR', machines: 1 }));
        const defaultBody = ['BRAIN', 'HEAD', 'NECK', 'CHEST', 'ABDOMEN', 'PELVIS', 'SPINE', 'LSPINE', 'SHOULDER', 'KNEE', 'HIP', 'HAND', 'FOOT', 'HEART', 'LIVER'];
        const bp = await getJson('/api/seed/bodyparts');
        bodySites.value = bp.map(v => ({ value: v, checked: defaultBody.includes(v) }));
        const ts = await getJson('/api/seed/transfersyntaxes');
        transferSyntaxes.value = ts.map(x => ({ uid: x.uid, name: x.name, checked: true }));
        const t = await getJson('/api/seed/tags');
        tags.value = t.map(x => ({ keyword: x.keyword, name: x.name, group: x.group, element: x.element, level: x.level, core: x.core, checked: true }));
      } catch (ex) { error.value = 'Failed to load seed data: ' + ex.message; }
    });

    return { req, modalityRows, customModality, bodySites, transferSyntaxes, tags, tagsByLevel, selectedTagCount, selectedBodyCount, error, estimateText, status, running, completedModal, pct, fs,
      setAllTags, setLevel, setAllBody, estimate, generate, cancel, dismissCompleted, openFs, loadFs, pickFs, suggestBirthRange, modalityName };
  }
}).mount('#app');
