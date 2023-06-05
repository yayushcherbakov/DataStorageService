using DataStorageService.Services.Models;

namespace DataStorageService.Services.Services.Interfaces;

/// <summary>
/// The interface that defines the contract for the CSV data service.
/// </summary>
public interface ICsvDataService
{
    /// <summary>
    /// Processes CSV file uploads to the server.
    /// </summary>
    public Task Upload(Stream file, string fileName, string rootPath, CancellationToken cancellationToken);

    /// <summary>
    /// Processes the deletion of the CSV file from the server.
    /// </summary>
    public Task Delete(string fileName, string rootPath, CancellationToken cancellationToken);
    
    /// <summary>
    /// Processes a request for a list of files and file information from the server.
    /// </summary>
    public Task<List<FileInfoData>> GetFiles(string rootPath, CancellationToken cancellationToken);

    /// <summary>
    /// Processes a request to retrieve data from the server.
    /// </summary>
    Task<List<dynamic>> GetData(GetDataPayload payload, string rootPath,
        CancellationToken cancellationToken);
}
