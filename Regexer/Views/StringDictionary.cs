using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Regexer
{
    internal class StringDictionary: ResourceDictionary
    {
        #region == Path ==

        public string Path
        {
            get => "";
            set
            {
                try
                {
                    Source = new Uri($"{value}/{ CultureInfo.CurrentCulture.Name }.xaml", UriKind.Relative);
                }
                catch
                {
                    Source = new Uri($"{value}/en-us.xaml", UriKind.Relative);
                }
            }
        }

        #endregion

        public StringDictionary() { }
    }
}
