namespace DataStorageService.Services.Models;

public record FileInfoData(string FileName, long FileSize, List<string> Headers);
