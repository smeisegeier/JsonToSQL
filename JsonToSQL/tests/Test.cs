using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonToSQL;

namespace JsonToSql
{
    public class Test
    {
        public static void Main(string[] args)
        {
            string json = File.ReadAllText("tests/DSICH.json");
            string dbName = "TestDb";

            var converter = new JsonConvert();
            converter.DatabaseName = dbName;
            string result = converter.ToSQL(json);
            Console.WriteLine(result);
        }
    }
}