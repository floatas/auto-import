using System.Text.RegularExpressions;

namespace AutoImport
{
    public static class StringHelpers
    {
        public static string RemoveSpecialCharacters(string str)
        {
            return Regex.Replace(str, @"\t|\n|\r", "");
        }
    }

}

