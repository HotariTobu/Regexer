using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regexer
{
    internal record Replacement
    {
        public ReplacementType Type { get; set; } = ReplacementType.Normal;
        public string Value { get; init; } = "";
    }
}
