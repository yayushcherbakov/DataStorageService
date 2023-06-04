namespace DataStorageService.Services.Constants;

public class ErrorMessages
{
    public static readonly string InvalidFileExtension = "File {0} has an invalid extension.";
    public static readonly string FileNotFound = "File {0} not found.";
    public static readonly string CsvHeadersNotFound = "CSV headers not found";
    public static readonly string CsvColumnNotFound = "The file {0} do not contain column {1}.";
}
