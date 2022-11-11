# JsonToSQL
Converts the Json to SQL script.

Here are the primary features of JsonToSQL library:

1. Convert all kinds of your Json to SQL script.
2. Super-fast conversion.
3. The generated SQL script is fully compatible to MS SQL Server and Azure SQL DB.
4. The outcome SQL script also maintains the relationships between the tables if JSON has nested objects.
5. Library is free to use, including for commercial purpose.
6. Open source, and anyone can pull source code and customise as per their need.

Please use the below link to find the more details about library and know about on how to use,

https://www.dotnet4techies.com/2018/07/convert-json-to-sql-format-using-csharp.html

Thank You!

# changes in fork 


![GitHub languages](https://img.shields.io/github/languages/count/smeisegeier/JsonToSQL?style=plastic) languages count  
![tag](https://img.shields.io/github/v/tag/smeisegeier/JsonToSQL?style=plastic) version



## usage

- constructor for `JsonConvert` now takes more parameters
  - `defaultTableName` - if json parser wont find multiple tables, this default will be used
  - `databaseName` - name of the database
  - `schemaName` - name of used schema (default dbo)
  - `hasDropTableStatement` - is drop table if exists required? (default true)
  - `HasCreateDbStatement` - is create db if not exists required? (default false)
- `ToSQL()` now also accepts Uri parameter for json file path

## example

```c#
    Uri json = new Uri(DSICH_JSON_PATH, UriKind.Relative);
    var converter = new JsonConvert("JsonDb", "Test", "dbo", true, true);
    string result = converter.ToSQL(json);
```

## dependencies

- &gt;= net6
- Newtonsoft Json

## roadmap

- [x] migrate solution to newer net version, port to visual studio code
- [x] add options for precise sql code generation
- [ ] validate results
  
