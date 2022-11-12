using System;
using System.IO;
using System.Linq;
using JsonToSQL;
using Xunit;

namespace JsonToSQLUnitTest;

public class UnitTest1
{
    private JsonConvert defaultJsonConvert() => new JsonConvert("JsonDb", "DSICH", "dbo", true, true);


    [InlineData("JsonDb", "DSICH", "dbo", true, true)]
    [InlineData("JsonDb", "DSICH", "", false, true)]
    [InlineData("JsonDb", "DSICH", "...", false, false)]
    [Theory]
    public void ConstructorTest(string db, string tbl, string schema, bool dbOk, bool tblOk)
    {
        var converter = new JsonConvert(db, tbl, schema, tblOk, dbOk);
        string actual = converter.ToSQL(Program.dsich_json);
        Assert.False(String.IsNullOrEmpty(actual));
    }

    [InlineData(true, "JsonDb", "DSICH", "dbo", true, true)]        // all options
    [InlineData(false, "JsonDb", "DSICH", "dbo", false, false)]     // mixtured options
    [InlineData(false, "JsonDb", "DSICH", "dbo", true, false)]
    [InlineData(true, "JsonDb", "DSICH", "dbo", false, true)]
    [Theory]
    public void GenerateDropStatementTest(bool expected, string db, string tbl, string schema, bool dbOk, bool tblOk)
    {
        //Json string to SQL script
        var converter = new JsonConvert(db, tbl, schema, tblOk, dbOk);
        bool actual = (converter.ToSQL(Program.dsich_json)).Contains("DROP");
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ContentEmptyExceptionTest()
    {
        var converter = defaultJsonConvert();
        Assert.ThrowsAny<Exception>(() => converter.ToSQL(""));
    }

}