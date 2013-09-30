using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace CQA.Helpers
{
    public class StringHelper
    {
        public static string CreateSeoFriendlyString(string text)
        {
            //As it is now it does not solve every case
            //text = Regex.Replace(text, @"ä", "a");
            //text = Regex.Replace(text, @"ö", "o");
            //text = Regex.Replace(text, @"", "o");
            //text = Regex.Replace(text, @"ł", "l");
            text = Regex.Replace(text, @"\s", "-");

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}