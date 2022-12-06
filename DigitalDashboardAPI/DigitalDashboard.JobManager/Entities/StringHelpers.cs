using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DigitalDashboard.JobManager.Entities
{
    public static class StringHelpers
    {
        public static string ToTitleCase(this string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return title;
            }
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower());
        }

        public static string RemoveSpaces(this string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return inputString;
            }
            return inputString.Replace(" ", "");
        }

        public static string RemoveSpecialCharacters(this string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return inputString;
            }
            return Regex.Replace(inputString, "[^a-zA-Z0-9]+", "", RegexOptions.Compiled);
        }
    }
}
