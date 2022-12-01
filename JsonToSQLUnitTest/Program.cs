using JsonToSQL;

namespace JsonToSQLUnitTest
{
    public static class Program
    {
        public const string DSICH_JSON_PATH = "assets/DSICH.json";
        public static string dsich_json => File.ReadAllText(Program.DSICH_JSON_PATH);


        public static void Main(string[] args)
        {
            Uri json = new Uri(DSICH_JSON_PATH, UriKind.Relative);
            var converter = new JsonConvert("JsonDb", "DSICH", "meta", true, true, true, true);

            string result = converter.ToSQL(json); // "" -> Newtonsoft.Json.JsonReaderException

            Console.WriteLine(result);
        }
    }
}