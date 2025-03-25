using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Utils
{
    public static class StringUtils
    {
        public static string NormalizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string normalized = input.Normalize(NormalizationForm.FormD);
            StringBuilder result = new StringBuilder();

            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    result.Append(c);
                }
            }

            return result.ToString().ToLower().Replace(" ", "_"); ;
        }
    }
}
