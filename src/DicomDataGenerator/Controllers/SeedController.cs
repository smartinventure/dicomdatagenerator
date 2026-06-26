using DicomDataGenerator.Services;
using Microsoft.AspNetCore.Mvc;

namespace DicomDataGenerator.Controllers
{
    /// <summary>UI metadata: the DICOM tag seed list and the supported modalities.</summary>
    [ApiController]
    [Route("api/seed")]
    public class SeedController : ControllerBase
    {
        private readonly TagCatalog _tags;
        private readonly ModalityCatalog _modalities;

        public SeedController(TagCatalog tags, ModalityCatalog modalities)
        {
            _tags = tags;
            _modalities = modalities;
        }

        [HttpGet("tags")]
        public IActionResult Tags() => Ok(_tags.Tags);

        [HttpGet("modalities")]
        public IActionResult Modalities() => Ok(_modalities.SupportedModalities);

        [HttpGet("bodyparts")]
        public IActionResult BodyParts() => Ok(BodyPartCatalog.All);
    }
}
