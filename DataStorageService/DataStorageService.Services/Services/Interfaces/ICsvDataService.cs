using DataStorageService.Services.Models;

namespace DataStorageService.Services.Services.Interfaces;

public interface ICsvDataService
{
    public Task Upload(Stream file, string fileName, string rootPath, CancellationToken cancellationToken);

    public Task Delete(string fileName, string rootPath, CancellationToken cancellationToken);
    
    public Task<List<FileInfoData>> GetFiles(string rootPath, CancellationToken cancellationToken);

    Task<List<dynamic>> GetData(GetDataPayload payload, string rootPath,
        CancellationToken cancellationToken);
}
