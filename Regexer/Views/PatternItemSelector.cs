using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Regexer
{
    internal class PatternItemSelector: DataTemplateSelector
    {
        public DataTemplate IrremovableItemTemplate { get; set; } = new DataTemplate();
        public DataTemplate RemovableItemTemplate { get; set; } = new DataTemplate();

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is PatternItem patternItem)
            {
                if (patternItem.IsRemovable)
                {
                    return RemovableItemTemplate;
                }
                else
                {
                    return IrremovableItemTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}
