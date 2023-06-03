using DataStorageService.Services.Enums;

namespace DataStorageService.Services.Models;

public record Filter(string ColumnName, FilterAction FilterAction, string Value);
