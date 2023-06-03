using DataStorageService.Services.Enums;

namespace DataStorageService.Services.Models;

public record ColumnOrder(string ColumnName, OrderDirection OrderDirection);
