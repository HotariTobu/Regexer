using Encodable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regexer
{
    public static class PatternItemExtension
    {
        public static Data Encode(this PatternItem patternItem)
        {
            Data isRemovableData = patternItem.IsRemovable.Encode();
            Data patternData = patternItem.Pattern.Encode();
            return new Data(isRemovableData, patternData);
        }

        public static PatternItem Decode(this PatternItem _, Data data, Type? __ = null)
        {
            List<Data> contents = data.Contents.ToList();
            return new PatternItem
            {
                IsRemovable = contents[0].Decode<bool>(),
                Pattern = contents[1].Decode<string>(),
            };
        }
    }
}
