using System;
using System.IO;
using JsonToSQL;
using Xunit;

namespace JsonToSQLUnitTest;

public class UnitTest1
{

    [Fact]
    public void ConvertJsonStringToSQL()
    {
        //Json string to SQL script
        var converter = new JsonConvert("JsonDb", "DSICH", "dbo", true, true);

        string json = File.ReadAllText(Program.DSICH_JSON_PATH);

        string sqlScript = converter.ToSQL(json);

        Assert.Equal(json, json);
    }

    // [Fact]
    // public void ConvertJsonStreamToSQL()
    // {
    //     //Json stream to SQL script
    //     var converter = new JsonConvert("JsonDb", "DSICH", "dbo", true, true);

    //     string jsonFilePath = "E:\\json1.txt";

    //     using (FileStream fs = File.OpenRead(jsonFilePath))
    //     {
    //         MemoryStream ms = new MemoryStream();
    //         ms.SetLength(fs.Length);
    //         fs.Read(ms.GetBuffer(), 0, (int)fs.Length);

    //         string sqlScript = converter.ToSQL(ms);
    //     }

    //     Assert.Equal(null, null);
    // }
}