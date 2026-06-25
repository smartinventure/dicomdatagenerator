using System.Text.Json;
using DicomDataGenerator.Models;

namespace DicomDataGenerator.Services
{
    /// <summary>Loads the 79-tag seed (seeddata/dicom-tags.json) used by the UI and value generation.</summary>
    public class TagCatalog
    {
        public IReadOnlyList<TagInfo> Tags { get; private set; } = Array.Empty<TagInfo>();

        public void Load(string seedRootPath)
        {
            var path = Path.Combine(seedRootPath, "dicom-tags.json");
            if (!File.Exists(path))
            {
                return;
            }
            var json = File.ReadAllText(path);
            Tags = JsonSerializer.Deserialize<List<TagInfo>>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))
                   ?? new List<TagInfo>();
        }
    }
}
