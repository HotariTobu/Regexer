using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regexer
{
    public record PatternItem
    {
        public bool IsRemovable { get; init; }
        public string Pattern { get; init; } = "";
    }
}
