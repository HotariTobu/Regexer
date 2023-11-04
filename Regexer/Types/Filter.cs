using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Regexer
{
    internal class Filter
    {
        public string WhitePattern
        {
            get => "";
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    WhiteRegex = null;
                }
                else
                {
                    WhiteRegex = new Regex(value, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                }
            }
        }
        public string BlackPattern
        {
            get => "";
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    BlackRegex = null;
                }
                else
                {
                    BlackRegex = new Regex(value, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                }
            }
        }

        private Regex? WhiteRegex { get; set; }
        private Regex? BlackRegex { get; set; }

        public bool IsPass(string text)
        {
            if (WhiteRegex?.IsMatch(text) == false)
            {
                return false;
            }
            if (BlackRegex?.IsMatch(text) == true)
            {
                return false;
            }
            return true;
        }
    }
}
