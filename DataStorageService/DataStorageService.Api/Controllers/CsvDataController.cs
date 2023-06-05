using DataStorageService.Services.Models;
using DataStorageService.Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DataStorageService.Api.Controllers
{
    /// <summary>
    /// The class is a CSV data controller.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CsvDataController : ControllerBase
    {
        /// <summary>
        /// Interface in ASP.NET Core, designed to provide information about the current web host
        /// environment in which the ASP.NET Core application is running.
        /// </summary>
        private readonly IWebHostEnvironment _hostEnvironment;
        
        /// <summary>
        /// User interface for data processing in CSV format.
        /// </summary>
        private readonly ICsvDataService _csvDataService;

        /// <summary>
        /// A controller class in ASP.NET Core that provides an entry point for handling queries related to read,
        /// write and process CSV data operations.
        /// </summary>
        public CsvDataController(IWebHostEnvironment hostEnvironment, ICsvDataService csvDataService)
        {
            _csvDataService = csvDataService;
            _hostEnvironment = hostEnvironment;
        }
        
        /// <summary>
        /// Processes CSV file uploads to the server.
        /// </summary>
        /// <param name="file">File for upload.</param>
        /// <param name="cancellationToken">Token for the ability to cancel a transaction.</param>
        [HttpPost("Upload")]
        public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
        {
            await _csvDataService.Upload(file.OpenReadStream(), file.FileName, _hostEnvironment.ContentRootPath, cancellationToken);

            return Ok();
        }
        
        /// <summary>
        /// Processes the deletion of the CSV file from the server.
        /// </summary>
        /// <param name="fileName">File to delete.</param>
        /// <param name="cancellationToken">Token for the ability to cancel a transaction.</param>
        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete(string fileName, CancellationToken cancellationToken)
        {
            await _csvDataService.Delete(fileName, _hostEnvironment.ContentRootPath, cancellationToken);

            return Ok();
        }
        
        /// <summary>
        /// Processes a request for a list of files and file information from the server.
        /// </summary>
        /// <param name="cancellationToken">Token for the ability to cancel a transaction.</param>
        [HttpGet("GetFilesInfo")]
        public async Task<IActionResult> GetFiles(CancellationToken cancellationToken)
        {
            var filesInfo = await _csvDataService.GetFiles(_hostEnvironment.ContentRootPath, cancellationToken);

            return Ok(filesInfo);
        }
        
        /// <summary>
        /// Processes a request to retrieve data from the server.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="cancellationToken">Token for the ability to cancel a transaction.</param>
        [HttpPost("GetData")]
        public async Task<IActionResult> GetData(GetDataPayload payload, CancellationToken cancellationToken)
        {
            var data = await _csvDataService.GetData(payload, _hostEnvironment.ContentRootPath,
                cancellationToken);
            return Ok(data);
        }
    }
}
