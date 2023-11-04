using Encodable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regexer
{
    internal static class PatternSetExtension
    {
        public static Data Encode(this PatternSet patternItem)
        {
            Data nameData = patternItem.Name.Encode();
            Data searchPatternData = patternItem.SearchPattern.Encode();
            Data replacePatternData = patternItem.ReplacePattern.Encode();
            Data isIgnoreCaseData = patternItem.IsIgnoreCase.Encode();
            Data isEscapeData = patternItem.IsEscape.Encode();
            return new Data(nameData, searchPatternData, replacePatternData, isIgnoreCaseData, isEscapeData);
        }

        public static PatternSet Decode(this PatternSet _, Data data, Type? __ = null)
        {
            List<Data> contents = data.Contents.ToList();
            return new PatternSet
            {
                Name = contents[0].Decode<string>(),
                SearchPattern = contents[1].Decode<string>(),
                ReplacePattern = contents[2].Decode<string>(),
                IsIgnoreCase = contents[3].Decode<bool>(),
                IsEscape = contents[4].Decode<bool>(),
            };
        }
    }
}
