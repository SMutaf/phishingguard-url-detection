using Microsoft.AspNetCore.Mvc;
using PhishingGuard.Core.DTOs;
using PhishingGuard.Service.Interfaces;
using System.Threading.Tasks;

namespace PhishingGuard.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScanController : ControllerBase
    {
        private readonly IPhishingScanService _scanService;

        public ScanController(IPhishingScanService scanService)
        {
            _scanService = scanService;
        }

        // POST api/scan
        [HttpPost]
        public async Task<IActionResult> Scan([FromBody] ScanRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Url))
            {
                return BadRequest("URL boş olamaz.");
            }

            var result = await _scanService.ScanUrlAsync(request);
            return Ok(result);
        }
    }
}