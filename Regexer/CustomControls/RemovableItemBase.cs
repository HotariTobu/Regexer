using SharedWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Regexer
{
    /// <summary>
    /// このカスタム コントロールを XAML ファイルで使用するには、手順 1a または 1b の後、手順 2 に従います。
    ///
    /// 手順 1a) 現在のプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Regexer"
    ///
    ///
    /// 手順 1b) 異なるプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Regexer;assembly=Regexer"
    ///
    /// また、XAML ファイルのあるプロジェクトからこのプロジェクトへのプロジェクト参照を追加し、
    /// リビルドして、コンパイル エラーを防ぐ必要があります:
    ///
    ///     ソリューション エクスプローラーで対象のプロジェクトを右クリックし、
    ///     [参照の追加] の [プロジェクト] を選択してから、このプロジェクトを参照し、選択します。
    ///
    ///
    /// 手順 2)
    /// コントロールを XAML ファイルで使用します。
    ///
    ///     <MyNamespace:RemovableItemBase/>
    ///
    /// </summary>
    public class RemovableItemBase : ContentControl
    {
        static RemovableItemBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RemovableItemBase), new FrameworkPropertyMetadata(typeof(RemovableItemBase)));
        }

        #region == RemoveButtonMargin ==

        public static readonly DependencyProperty RemoveButtonMarginProperty = DependencyProperty.Register("RemoveButtonMargin", typeof(Thickness), typeof(RemovableItemBase), new FrameworkPropertyMetadata());
        public Thickness RemoveButtonMargin { get => (Thickness)GetValue(RemoveButtonMarginProperty); set => SetValue(RemoveButtonMarginProperty, value); }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_RemoveButton") is Button removeButton)
            {
                removeButton.Click += RemoveButton_Click;
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            VisualStateManager.GoToState(this, "MouseOver", false);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            VisualStateManager.GoToState(this, "Normal", false);
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.FindAncestor<ItemsControl>() is ItemsControl itemsControl)
            {
                object itemsSource = itemsControl.ItemsSource;
                if (itemsSource.GetType().GetMethod("Remove") is MethodInfo removeMethod)
                {
                    removeMethod.Invoke(itemsSource, new object[] { DataContext });
                    e.Handled = true;
                }
                else
                {
                    throw new MissingMethodException("Not Found a method named \"Remove\" in ItemsSource in container ItemsControl of this item.");
                }
            }
            else
            {
                throw new TypeLoadException("Not Found any ItemsControl ancestors of this item.");
            }
        }
    }
}
