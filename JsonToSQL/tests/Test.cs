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

            var converter = new JsonConvert("JsonDb", "DSICH");
            string result = converter.ToSQL(json);
            Console.WriteLine(result);
        }
    }
}