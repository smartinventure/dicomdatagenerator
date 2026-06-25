using DicomDataGenerator.Models;
using DicomDataGenerator.Services;
using Microsoft.AspNetCore.Mvc;

namespace DicomDataGenerator.Controllers
{
    /// <summary>Estimate, start, monitor and cancel a generation run.</summary>
    [ApiController]
    [Route("api/generate")]
    public class GenerationController : ControllerBase
    {
        private readonly GenerationService _generation;

        public GenerationController(GenerationService generation)
        {
            _generation = generation;
        }

        [HttpPost("estimate")]
        public IActionResult Estimate([FromBody] GenerationRequest request)
            => Ok(_generation.Estimate(request));

        [HttpPost]
        public IActionResult Start([FromBody] GenerationRequest request)
        {
            if (request.Output.Target == "folder" && string.IsNullOrWhiteSpace(request.Output.FolderPath))
            {
                return BadRequest(new { message = "Choose an output folder." });
            }
            if (!_generation.TryStart(request))
            {
                return Conflict(new { message = "A generation run is already in progress." });
            }
            return Accepted(new { started = true });
        }

        [HttpGet("status")]
        public IActionResult Status() => Ok(_generation.Status);

        [HttpPost("cancel")]
        public IActionResult Cancel()
        {
            _generation.Cancel();
            return Ok(new { cancelling = true });
        }
    }
}
