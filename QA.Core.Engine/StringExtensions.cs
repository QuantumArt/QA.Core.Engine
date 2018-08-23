using System;
using System.Linq;
#pragma warning disable 1591

namespace QA.Core
{
    public static class StringExtensions
    {
        public static string[] SplitString(this string original, params char[] chars)
        {
            if (String.IsNullOrEmpty(original))
            {
                return new string[0];
            }

            var split = from piece in original.Split(chars)
                        let trimmed = piece.Trim()
                        where !String.IsNullOrEmpty(trimmed)
                        select trimmed;
            return split.ToArray();
        }

        public static string[] SplitString(this string original)
        {
            return SplitString(original, ',');
        }
    }
}
