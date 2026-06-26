using DicomDataGenerator.Models;
using DicomDataGenerator.Services;
using Microsoft.AspNetCore.Mvc;

namespace DicomDataGenerator.Controllers
{
    /// <summary>PACS connectivity checks (C-ECHO).</summary>
    [ApiController]
    [Route("api/pacs")]
    public class PacsController : ControllerBase
    {
        private readonly PacsSender _pacs;

        public PacsController(PacsSender pacs)
        {
            _pacs = pacs;
        }

        [HttpPost("test")]
        public async Task<IActionResult> Test([FromBody] PacsOptions pacs, CancellationToken cancellationToken)
        {
            try
            {
                var ok = await _pacs.EchoAsync(pacs, cancellationToken).ConfigureAwait(false);
                return ok
                    ? Ok(new { ok = true, message = $"C-ECHO succeeded ({pacs.Host}:{pacs.Port}, AET {pacs.CalledAet})." })
                    : Ok(new { ok = false, message = "Associated, but C-ECHO was not accepted (check Called AET)." });
            }
            catch (Exception ex)
            {
                return Ok(new { ok = false, message = $"Connection failed: {ex.Message}" });
            }
        }
    }
}
