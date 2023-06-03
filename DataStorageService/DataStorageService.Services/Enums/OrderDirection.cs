using System.ComponentModel.DataAnnotations;

namespace DataStorageService.Services.Enums;

public enum OrderDirection
{
    [Display(Name = "Ascending")]
    Asc,
    [Display(Name = "Descending")]
    Desc
}
