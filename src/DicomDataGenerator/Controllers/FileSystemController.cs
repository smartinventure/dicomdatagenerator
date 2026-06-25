using Microsoft.AspNetCore.Mvc;

namespace DicomDataGenerator.Controllers
{
    /// <summary>
    /// Server-side folder browser for the output-folder picker (a browser can't pick a server folder
    /// natively). Local-only tool; lists directories so the user can drill down and pick one.
    /// </summary>
    [ApiController]
    [Route("api/fs")]
    public class FileSystemController : ControllerBase
    {
        [HttpGet("drives")]
        public IActionResult Drives()
        {
            var drives = DriveInfo.GetDrives()
                .Where(d => d.IsReady)
                .Select(d => new { name = d.Name, path = d.RootDirectory.FullName });
            return Ok(drives);
        }

        [HttpGet("list")]
        public IActionResult List([FromQuery] string? path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    return Drives();
                }
                var di = new DirectoryInfo(path);
                if (!di.Exists)
                {
                    return NotFound(new { message = "Directory not found" });
                }
                var dirs = di.EnumerateDirectories()
                    .Where(d => (d.Attributes & FileAttributes.Hidden) == 0)
                    .Select(d => new { name = d.Name, path = d.FullName })
                    .OrderBy(x => x.name, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                return Ok(new { path = di.FullName, parent = di.Parent?.FullName, directories = dirs });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
