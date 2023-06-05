using System.ComponentModel.DataAnnotations;

namespace DataStorageService.Services.Enums;

/// <summary>
/// Order direction enum.
/// </summary>
public enum OrderDirection
{
    [Display(Name = "Ascending")]
    Asc,
    [Display(Name = "Descending")]
    Desc
}
