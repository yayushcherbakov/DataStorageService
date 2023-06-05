# DataStorageService
Данный проект представляет собой HTTP сервис для работы с импортируемыми данными в формате CSV и предоставляет следующий функционал:
### Загрузка данных:
Сервис позволяет загружать файлы в формате CSV с помощью эндпоинта [POST] /api/CsvData/Upload
### Получение списка файлов c информацией о колонках:
Сервис предоставляет возможность получить список загруженных файлов и информацию о колонках в каждом файле c помощью эндпоина [GET] /api/CsvData/GetFilesInfo
### Получение данных из конкретного файла с фильтрацией и сортировкой:
Сервис позволяет получать данные из конкретного загруженного файла с возможностью применения фильтрации и сортировки по одному или нескольким столбцам c помощью эндпоинта [POST] /api/CsvData/GetData при помощи следующей схемы: 
#### {
      "fileName": "string",
      "filters": [
       {
          "columnName": "string",
          "filterAction": 0,
          "value": "string"
       }
      ],
      "sorting": [
        {
          "columnName": "string",
         "orderDirection": 0
        }
      ]
     }
### Дополнительный функционал:
#### 1. Дополнительный эндпойнт [DELETE] /api/CsvData/Delete для удаление ранее загруженного файла
#### 2. Dockerfile для запуска сервиса в Docker
#### 3. Документация к коду
### Технологии и язык программирования:
Платформа: .NET 7.0

Язык: С# 10

Frameworks: Microsoft.AspNetCore.App, Microsoft.NetCore.App

Packages: CsvHelper/30.0.1, Swashbuckle.AspNetCore.App/6.2.3, Microsoft.Extensions.DependencyInjection.Absractions/7.0.0
