using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    ///     <MyNamespace:HighlightableTextBox/>
    ///
    /// </summary>
    internal class HighlightableTextBox : TextBox
    {
        static HighlightableTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HighlightableTextBox), new FrameworkPropertyMetadata(typeof(HighlightableTextBox)));
        }

        #region == HighlightBrush ==

        public static readonly DependencyProperty HighlightBrushProperty = DependencyProperty.Register("HighlightBrush", typeof(Brush), typeof(HighlightableTextBox), new FrameworkPropertyMetadata());
        public Brush HighlightBrush { get => (Brush)GetValue(HighlightBrushProperty); set => SetValue(HighlightBrushProperty, value); }

        #endregion

        #region == Ranges ==

        public static readonly DependencyProperty RangesProperty = DependencyProperty.Register("Ranges", typeof(IEnumerable<Range>), typeof(HighlightableTextBox), new FrameworkPropertyMetadata(
            (d, e) => (d as HighlightableTextBox)?.UpdateRects()));
        public IEnumerable<Range> Ranges { get => GetValue(RangesProperty) as IEnumerable<Range>; set => SetValue(RangesProperty, value); }

        #endregion
        #region == Rects ==

        private static readonly DependencyPropertyKey RectsPropertyKey = DependencyProperty.RegisterReadOnly("Rects", typeof(ObservableCollection<Rect>), typeof(HighlightableTextBox), new FrameworkPropertyMetadata());
        public static readonly DependencyProperty RectsProperty = RectsPropertyKey.DependencyProperty;
        public ObservableCollection<Rect> Rects { get => GetValue(RectsProperty) as ObservableCollection<Rect>; private set => SetValue(RectsPropertyKey, value); }

        private void UpdateRects()
        {
            List<Rect> rects = new();

            foreach (Range range in Ranges)
            {
                Rect rect1 = GetRectFromCharacterIndex(range.Start.Value);
                Rect rect2 = GetRectFromCharacterIndex(range.End.Value);
                Rect rect3 = new Rect(rect1.TopLeft, rect2.BottomRight);
                rect3.X += HorizontalOffset;
                rect3.Y += VerticalOffset;
                rects.Add(rect3);
            }

            Rects = new ObservableCollection<Rect>(rects);
        }

        #endregion

        #region == NegativeHorizontalOffset ==

        public static readonly DependencyProperty NegativeHorizontalOffsetProperty = DependencyProperty.Register("NegativeHorizontalOffset", typeof(double), typeof(HighlightableTextBox), new FrameworkPropertyMetadata());
        public double NegativeHorizontalOffset { get => (double)GetValue(NegativeHorizontalOffsetProperty); set => SetValue(NegativeHorizontalOffsetProperty, value); }

        #endregion
        #region == NegativeVerticalOffset ==

        public static readonly DependencyProperty NegativeVerticalOffsetProperty = DependencyProperty.Register("NegativeVerticalOffset", typeof(double), typeof(HighlightableTextBox), new FrameworkPropertyMetadata());
        public double NegativeVerticalOffset { get => (double)GetValue(NegativeVerticalOffsetProperty); set => SetValue(NegativeVerticalOffsetProperty, value); }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_ContentHost") is ScrollViewer contentHost)
            {
                contentHost.ScrollChanged += ContentHost_ScrollChanged;
            }
        }

        private void ContentHost_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            NegativeHorizontalOffset = -HorizontalOffset;
            NegativeVerticalOffset = -VerticalOffset;
        }
    }
}
