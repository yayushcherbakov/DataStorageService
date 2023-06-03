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

    public async Task<List<FileInfoData>> GetFiles(string rootPath, CancellationToken cancellationToken)
    {
        var uploadPath = Path.Combine(rootPath, DataImportConstants.UploadsFolder);
        var files = Directory.GetFiles(uploadPath);
        var filesInfo = new List<FileInfoData>();

        foreach (var filePath in files)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture);

            if (!await csv.ReadAsync() || !csv.ReadHeader())
            {
                continue;
            }

            if (csv.HeaderRecord == null)
            {
                continue;
            }

            var fileInfo = new FileInfo(filePath);
            var headers = csv.HeaderRecord.ToList();
            filesInfo.Add(new FileInfoData(fileInfo.Name, fileInfo.Length, headers));
        }

        return filesInfo;
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


    private IEnumerable<dynamic> ApplyFilters(IEnumerable<dynamic> records, List<Filter> filters)
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
                    ((IDictionary<string, object>)record)[filter.ColumnName].Equals(filter.Value)),        
                FilterAction.NotEqual => records.Where(record =>
                    !((IDictionary<string, object>)record)[filter.ColumnName].Equals(filter.Value)),

                FilterAction.Contains => records.Where(record =>
                    ((string)((IDictionary<string, object>)record)[filter.ColumnName]).Contains(filter.Value)),
                FilterAction.NotContains => records.Where(record =>
                    !((string)((IDictionary<string, object>)record)[filter.ColumnName]).Contains(filter.Value)),
                
                _ => throw new AggregateException(nameof(FilterAction))
            };
        }

        return records;
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
                ((IDictionary<string, object>)record)[firstColumnOrder.ColumnName]),
            OrderDirection.Desc => records.OrderByDescending(record =>
                ((IDictionary<string, object>)record)[firstColumnOrder.ColumnName]),
            _ => throw new AggregateException(nameof(OrderDirection))
        };
        
        foreach (var columnOrder in columnOrders.Skip(1))
        {
            orderedRecords = columnOrder.OrderDirection switch
            {
                OrderDirection.Asc => orderedRecords.ThenBy(record =>
                    ((IDictionary<string, object>)record)[columnOrder.ColumnName]),
                OrderDirection.Desc => orderedRecords.ThenByDescending(record =>
                    ((IDictionary<string, object>)record)[columnOrder.ColumnName]),
                _ => throw new AggregateException(nameof(OrderDirection))
            };
        }

        return orderedRecords;
    }
}
