using CsvHelper;
using DataStorageService.Services.Constants;
using DataStorageService.Services.Enums;
using DataStorageService.Services.Models;
using DataStorageService.Services.Services.Interfaces;

namespace DataStorageService.Services.Services;

internal class CsvDataService : ICsvDataService
{
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

    private async Task<List<string>> GetCsvHeaders(CsvReader csv)
    {
        if (!await csv.ReadAsync() || !csv.ReadHeader() || csv.HeaderRecord == null)
        {
            throw new ApplicationException(ErrorMessages.CsvHeadersNotFound);
        }

        return csv.HeaderRecord.ToList();
    }

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

    private static object GetPropertyValue(dynamic target, string propertyName)
    {
        return ((IDictionary<string, object>)target)[propertyName];
    }

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
