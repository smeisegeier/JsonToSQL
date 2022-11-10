using System.Diagnostics;
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
            Uri json = new Uri("tests/DSICH.json", UriKind.Relative);
            var converter = new JsonConvert("JsonDb", "DSICH", "dbo", true, true);
            string result = converter.ToSQL(json);
            Console.WriteLine(result);

        }
    }
}