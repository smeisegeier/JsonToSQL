using JsonToSQL;

namespace JsonToSQLUnitTest
{
    public static class Program
    {
        public const string DSICH_JSON_PATH = "assets/DSICH.json";

        public static void Main(string[] args)
        {
            Uri json = new Uri(DSICH_JSON_PATH, UriKind.Relative);
            var converter = new JsonConvert("JsonDb", "DSICH", "dbo", true, true);
            string result = converter.ToSQL(json);
            Console.WriteLine(result);
        }
    }
}