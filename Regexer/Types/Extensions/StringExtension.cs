using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regexer
{
    internal static class StringExtension
    {
        public static string RemoveAll(this string text, char c)
        {
            return string.Create(text.Length, text.ToCharArray(), (result, original) =>
            {
                int j = 0;
                for (int i = 0; i < original.Length; i++)
                {
                    if (original[i] != c)
                    {
                        result[j] = original[i];
                        j++;
                    }
                }
            }).TrimEnd('\0');
        }

        public static string ToLowerFirstInvariant(this string text)
        {
            return string.Create(text.Length, text.ToCharArray(), (Span<char> result, char[] original) =>
            {
                result[0] = char.ToLowerInvariant(original[0]);
                for (int i = 1; i < original.Length; i++)
                {
                    result[i] = original[i];
                }
            });
        }

        public static string ToUpperFirstInvariant(this string text)
        {
            return string.Create(text.Length, text.ToCharArray(), (Span<char> result, char[] original) =>
            {
                result[0] = char.ToUpperInvariant(original[0]);
                for (int i = 1; i < original.Length; i++)
                {
                    result[i] = original[i];
                }
            });
        }
    }
}
