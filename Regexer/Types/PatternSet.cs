using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regexer
{
    public record PatternSet
    {
        public string Name { get; init; } = "";
        public string SearchPattern { get; init; } = "";
        public string ReplacePattern { get; init; } = "";
        public bool IsIgnoreCase { get; init; }
        public bool IsEscape { get; init; }
    }
}
