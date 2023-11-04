using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regexer
{
    internal static class RegexHelper
    {
        public static async IAsyncEnumerable<string> SuggestPatterns(this string text)
        {
            if (text.Length == 0)
            {
                yield break;
            }

            yield return text;
            yield return text;
            yield return text;
            yield break;
        }
    }
}
