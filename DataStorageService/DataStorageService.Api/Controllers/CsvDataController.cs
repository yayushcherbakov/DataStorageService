using DataStorageService.Services.Models;
using DataStorageService.Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DataStorageService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CsvDataController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ICsvDataService _csvDataService;

        public CsvDataController(IWebHostEnvironment hostEnvironment, ICsvDataService csvDataService)
        {
            _csvDataService = csvDataService;
            _hostEnvironment = hostEnvironment;
        }
        
        [HttpPost("Upload")]
        public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
        {
            await _csvDataService.Upload(file.OpenReadStream(), file.FileName, _hostEnvironment.ContentRootPath, cancellationToken);

            return Ok();
        }
        
        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete(string fileName, CancellationToken cancellationToken)
        {
            await _csvDataService.Delete(fileName, _hostEnvironment.ContentRootPath, cancellationToken);

            return Ok();
        }

        [HttpGet("GetFilesInfo")]
        public async Task<IActionResult> GetFiles(CancellationToken cancellationToken)
        {
            var filesInfo = await _csvDataService.GetFiles(_hostEnvironment.ContentRootPath, cancellationToken);

            return Ok(filesInfo);
        }

        [HttpPost("GetData")]
        public async Task<IActionResult> GetData(GetDataPayload payload, CancellationToken cancellationToken)
        {
            var data = await _csvDataService.GetData(payload, _hostEnvironment.ContentRootPath,
                cancellationToken);
            return Ok(data);
        }
    }
}
