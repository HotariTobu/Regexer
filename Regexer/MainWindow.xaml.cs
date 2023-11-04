using Encodable;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Regexer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static MainWindow()
        {
            CodingProperties.Prefix="__";
            CodingProperties.AddCoder<PatternItem>(PatternItemExtension.Encode, PatternItemExtension.Decode);
            CodingProperties.AddCoder<PatternSet>(PatternSetExtension.Encode, PatternSetExtension.Decode);
        }

        private static Uri VMUri = new Uri(Path.GetFullPath("vm_data"));

        private MWVM VM = new MWVM();

        public MainWindow()
        {
            InitializeComponent();

            load();

            async void load()
            {
                if (File.Exists(VMUri.OriginalString))
                {
                    VM = await VMUri.LoadAsync<MWVM>();
                }

                DataContext = VM;
            }
        }

        private async void Window_Closed(object sender, EventArgs e)
        {
            VM.Close();
            await VM.SaveAsync(VMUri);
        }

        private async void HighlightableTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                VM.SuggestedPatterns.Clear();

                await foreach (string pattern in textBox.SelectedText.SuggestPatterns())
                {
                    VM.SuggestedPatterns.Add(pattern);
                }
            }
        }

        private void OutputTextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy)
            {
                addItem(VM.SearchPatternItems, VM.SearchPattern);
                addItem(VM.ReplacePatternItems, VM.ReplacePattern);

                void addItem(ObservableCollection<PatternItem> items, string item)
                {
                    if (item.Length == 0)
                    {
                        return;
                    }

                    for (int i = 2; i < items.Count; i++)
                    {
                        if (item.Equals(items[i].Pattern))
                        {
                            items.RemoveAt(i);
                            i--;
                        }
                    }
                    items.Insert(2, new PatternItem { IsRemovable = true, Pattern = item });
                }
            }
        }

        private void StorePatternSetButton_Click(object sender, RoutedEventArgs e)
        {
            string patternSetName = VM.PatternSetName;
            string searchPattern = VM.SearchPattern;

            if (patternSetName.Length == 0 || searchPattern.Length == 0)
            {
                return;
            }

            VM.PatternSets.Insert(0, new PatternSet() {
                Name = patternSetName,
                SearchPattern = searchPattern,
                ReplacePattern = VM.ReplacePattern,
                IsIgnoreCase = VM.IsIgnoreCase,
                IsEscape = VM.IsEscape,
            });
        }

        private void PatternSetNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.EndsWith('\r'))
            {
                StorePatternSetButton_Click(sender, e);
                e.Handled = true;
            }
        }

        private void PatternSetItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement frameworkElement)
            {
                PatternSet patternSet = (PatternSet)frameworkElement.DataContext;
                VM.PatternSetName = patternSet.Name;
                VM.SearchPattern = patternSet.SearchPattern;
                VM.ReplacePattern = patternSet.ReplacePattern;
                VM.IsIgnoreCase = patternSet.IsIgnoreCase;
                VM.IsEscape = patternSet.IsEscape;
            }
        }

        private void ReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            VM.ReplaceFiles();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            VM.CancelReplace();
        }
    }
}
