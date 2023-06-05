using CsvHelper;
using DataStorageService.Services.Constants;
using DataStorageService.Services.Enums;
using DataStorageService.Services.Models;
using DataStorageService.Services.Services.Interfaces;

namespace DataStorageService.Services.Services;

/// <summary>
/// Service for CSV data processing.
/// </summary>
internal class CsvDataService : ICsvDataService
{
    /// <summary>
    /// The logic of uploading CSV files to the server.
    /// </summary>
    public async Task Upload(Stream file, string fileName, string rootPath, CancellationToken cancellationToken)
    {
        var fileExtension = Path.GetExtension(fileName);

        if (DataImportConstants.CsvExtensions != fileExtension)
        {
            throw new ApplicationException(string.Format(ErrorMessages.InvalidFileExtension, fileName));
        }

        var uploadPath = Path.Combine(rootPath, DataImportConstants.UploadsFolder);
        Directory.CreateDirectory(uploadPath);

        var filePath = Path.Combine(uploadPath, fileName);
        await using var fileStream = new FileStream(filePath, FileMode.Create);

        await file.CopyToAsync(fileStream, cancellationToken);
    }

    /// <summary>
    /// The logic of the CSV file deletion process from the server.
    /// </summary>
    public async Task Delete(string fileName, string rootPath, CancellationToken cancellationToken)
    {
        var uploadPath = Path.Combine(rootPath, DataImportConstants.UploadsFolder);
        var filePath = Path.Combine(uploadPath, fileName);

        if (!File.Exists(filePath))
        {
            throw new ApplicationException(string.Format(ErrorMessages.FileNotFound, fileName));
        }
        
        File.Delete(filePath);
    }

    /// <summary>
    /// Logic for processing a request for a list of files and information about files from the server.
    /// </summary>
    public async Task<List<FileInfoData>> GetFiles(string rootPath, CancellationToken cancellationToken)
    {
        var uploadPath = Path.Combine(rootPath, DataImportConstants.UploadsFolder);
        var files = Directory.GetFiles(uploadPath);
        var filesInfo = new List<FileInfoData>();

        foreach (var filePath in files)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture);

            var headers = await GetCsvHeaders(csv);

            var fileInfo = new FileInfo(filePath);

            filesInfo.Add(new FileInfoData(fileInfo.Name, fileInfo.Length, headers));
        }

        return filesInfo;
    }

    /// <summary>
    /// Gets file headers.
    /// </summary>
    private async Task<List<string>> GetCsvHeaders(CsvReader csv)
    {
        if (!await csv.ReadAsync() || !csv.ReadHeader() || csv.HeaderRecord == null)
        {
            throw new ApplicationException(ErrorMessages.CsvHeadersNotFound);
        }

        return csv.HeaderRecord.ToList();
    }
    
    /// <summary>
    /// The logic of processing a request to receive data from the server.
    /// </summary>
    public async Task<List<dynamic>> GetData(GetDataPayload payload, string rootPath,
        CancellationToken cancellationToken)
    {
        var uploadPath = Path.Combine(rootPath, DataImportConstants.UploadsFolder);
        var filePath = Path.Combine(uploadPath, payload.FileName);

        if (!File.Exists(filePath))
        {
            throw new ApplicationException(string.Format(ErrorMessages.FileNotFound, payload.FileName));
        }

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture);

        var headers = (await GetCsvHeaders(csv)).ToHashSet();

        var requestedColumnNames = payload.Filters
            .Select(x => x.ColumnName)
            .Union(payload.Sorting
                .Select(x => x.ColumnName))
            .Distinct();

        foreach (var requestedColumnName in requestedColumnNames)
        {
            if (!headers.Contains(requestedColumnName))
            {
                throw new ApplicationException(
                    string.Format(ErrorMessages.CsvColumnNotFound, payload.FileName, requestedColumnName));
            }
        }

        var records = csv.GetRecords<dynamic>();

        if (payload.Filters.Any())
        {
            records = ApplyFilters(records, payload.Filters);
        }

        if (payload.Sorting.Any())
        {
            records = ApplySort(records, payload.Sorting);
        }

        return records.ToList();
    }
    
    /// <summary>
    /// Takes a set of filters as input and applies them to the dataset.
    /// </summary>
    private static IEnumerable<dynamic> ApplyFilters(IEnumerable<dynamic> records, List<Filter> filters)
    {
        if (!filters.Any())
        {
            return records;
        }

        foreach (var filter in filters)
        {
            records = filter.FilterAction switch
            {
                FilterAction.Equal => records.Where(record =>
                    GetPropertyValue(record, filter.ColumnName)
                        .ToString()
                        .Equals(filter.Value)),
                FilterAction.NotEqual => records.Where(record =>
                    !GetPropertyValue(record, filter.ColumnName)
                        .ToString()
                        .Equals(filter.Value)),

                FilterAction.Contains => records.Where(record =>
                    GetPropertyValue(record, filter.ColumnName)
                        .ToString()
                        .Contains(filter.Value)),
                FilterAction.NotContains => records.Where(record =>
                    !GetPropertyValue(record, filter.ColumnName)
                        .ToString()
                        .Contains(filter.Value)),

                _ => throw new AggregateException(nameof(FilterAction))
            };
        }

        return records;
    }

    /// <summary>
    ///Extracts a property value.
    /// </summary>
    private static object GetPropertyValue(dynamic target, string propertyName)
    {
        return ((IDictionary<string, object>)target)[propertyName];
    }

    /// <summary>
    /// Applies the specified sort order to the records and returns the sorted sequence of records.
    /// </summary>
    private IEnumerable<dynamic> ApplySort(IEnumerable<dynamic> records, List<ColumnOrder> columnOrders)
    {
        if (!columnOrders.Any())
        {
            return records;
        }

        var firstColumnOrder = columnOrders.First();

        var orderedRecords = firstColumnOrder.OrderDirection switch
        {
            OrderDirection.Asc => records.OrderBy(record =>
                GetPropertyValue(record, firstColumnOrder.ColumnName)),
            OrderDirection.Desc => records.OrderByDescending(record =>
                GetPropertyValue(record, firstColumnOrder.ColumnName)),
            _ => throw new AggregateException(nameof(OrderDirection))
        };

        foreach (var columnOrder in columnOrders.Skip(1))
        {
            orderedRecords = columnOrder.OrderDirection switch
            {
                OrderDirection.Asc => orderedRecords.ThenBy(record =>
                    GetPropertyValue(record, columnOrder.ColumnName)),
                OrderDirection.Desc => orderedRecords.ThenByDescending(record =>
                    GetPropertyValue(record, columnOrder.ColumnName)),
                _ => throw new AggregateException(nameof(OrderDirection))
            };
        }

        return orderedRecords;
    }
}
