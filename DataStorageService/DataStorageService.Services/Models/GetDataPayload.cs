namespace DataStorageService.Services.Models;

public record GetDataPayload(string FileName, List<Filter> Filters, List<ColumnOrder> Sorting);
