using SharedWPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Regexer
{
    internal partial class MWVM: ViewModelBase, Encodable.ICodable
    {
        private Replacer Replacer = new();
        private Filter Filter = new();

        #region == SearchPatternError ==

        public bool HasSearchError => SearchPatternError != null;

        private string? _SearchPatternError;
        public string? SearchPatternError
        {
            get => _SearchPatternError;
            set
            {
                _SearchPatternError = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasSearchError));
            }
        }

        #endregion
        #region == ReplacePatternError ==

        public bool HasReplaceError => ReplacePatternError != null;

        private string? _ReplacePatternError;
        public string? ReplacePatternError
        {
            get => _ReplacePatternError;
            set
            {
                _ReplacePatternError = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasReplaceError));
            }
        }

        #endregion
        #region == HasPatternWarning ==

        private bool _HasPatternWarning;
        public bool HasPatternWarning
        {
            get => _HasPatternWarning;
            set
            {
                if (_HasPatternWarning != value)
                {
                    _HasPatternWarning = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region == SearchPattern ==

        public int SearchPatternIndex { get; set; } = -1;

        private string _SearchPattern = "";
        public string SearchPattern
        {
            get => _SearchPattern;
            set
            {
                if (!_SearchPattern.Equals(value))
                {
                    _SearchPattern = value;
                    if (SearchPatternIndex < 0 && value.Length > 0)
                    {
                        SearchPatternItems[0] = new PatternItem { Pattern = value };
                    }
                    UpdateSearchRegcies();
                    RaisePropertyChanged();
                }
            }
        }

        private void UpdateSearchRegcies()
        {
            try
            {
                Replacer.SetSearchRegices(SearchPattern, IsIgnoreCase);
                SearchPatternError = null;
            }
            catch (Exception e)
            {
                SearchPatternError = e.Message;
            }

            UpdateInputText();
        }

        #endregion
        #region == ReplacePattern ==

        public int ReplacePatternIndex { get; set; } = -1;

        private string _ReplacePattern = "";
        public string ReplacePattern
        {
            get => _ReplacePattern;
            set
            {
                if (!_ReplacePattern.Equals(value))
                {
                    _ReplacePattern = value;
                    if (ReplacePatternIndex < 0 && value.Length > 0)
                    {
                        ReplacePatternItems[0] = new PatternItem { Pattern = value };
                    }
                    UpdateReplacements();
                    RaisePropertyChanged();
                }
            }
        }

        private void UpdateReplacements()
        {
            try
            {
                Replacer.SetReplacements(ReplacePattern, IsEscape);
                ReplacePatternError = null;
            }
            catch (Exception e)
            {
                ReplacePatternError = e.Message;
            }

            UpdateOutputText();
        }

        #endregion

        #region == SearchPatternItems ==

        private readonly ObservableCollection<PatternItem> __SearchPatternItems = new ObservableCollection<PatternItem>(Enumerable.Repeat(new PatternItem(), 2));
        public ObservableCollection<PatternItem> SearchPatternItems => __SearchPatternItems;

        #endregion
        #region == ReplacePatternItems ==

        private readonly ObservableCollection<PatternItem> __ReplacePatternItems = new ObservableCollection<PatternItem>(Enumerable.Repeat(new PatternItem(), 2));
        public ObservableCollection<PatternItem> ReplacePatternItems => __ReplacePatternItems;

        #endregion

        #region == IsIgnoreCase ==

        private bool _IsIgnoreCase;
        public bool IsIgnoreCase
        {
            get => _IsIgnoreCase;
            set
            {
                if (_IsIgnoreCase != value)
                {
                    _IsIgnoreCase = value;
                    UpdateSearchRegcies();
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
        #region == IsEscape ==

        private bool _IsEscape;
        public bool IsEscape
        {
            get => _IsEscape;
            set
            {
                if (_IsEscape != value)
                {
                    _IsEscape = value;
                    UpdateReplacements();
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region == InputText ==

        private bool IsUpdatingInputText;

        private string _InputText = "";
        public string InputText
        {
            get => _InputText;
            set
            {
                if (!IsUpdatingInputText)
                {
                    _InputText = value;
                    UpdateInputText();
                }
            }
        }

        private CancellationTokenSource InputTextCancellationTokenSource = new();
        private Task InputTextTask = Task.Run(() => { });
        private async void UpdateInputText()
        {
            IsUpdatingInputText = true;

            InputTextCancellationTokenSource.Cancel();
            InputTextTask.Wait();
            InputTextCancellationTokenSource.Dispose();
            InputTextTask.Dispose();

            List<Range> ranges = new();

            InputTextCancellationTokenSource = new();
            InputTextTask = Task.Run(() =>
              {
                  Regex[] searchRegices = Replacer.SearchRegices;
                  if (!searchRegices.Any())
                  {
                      return;
                  }
                  
                  string[] lines = InputText.Split("\r\n");
                  Match match = searchRegices[0].Match(InputText.RemoveAll('\r'));

                  int lineCount = 0;
                  int index = lines[0].Length + 1;

                  while (match.Success && !InputTextCancellationTokenSource.IsCancellationRequested)
                  {
                      int matchIndex = match.Index;
                      int matchEndIndex = matchIndex + match.Length;

                      while (index < matchIndex)
                      {
                          updateLine();
                      }

                      while (index <= matchEndIndex)
                      {
                          add(matchIndex, index - 1);
                          matchIndex = index;
                          updateLine();
                      }

                      add(matchIndex, matchEndIndex);

                      match = match.NextMatch();

                      void updateLine()
                      {
                          lineCount++;
                          index += lines[lineCount].Length + 1;
                      }

                      void add(int start, int end)
                      {
                          if (start != end)
                          {
                              ranges.Add(new Range(start + lineCount, end + lineCount));
                          }
                      }
                  }
              }, InputTextCancellationTokenSource.Token);
            await InputTextTask;

            if (HighlightRanges.Count > 0 || ranges.Count > 0)
            {
                HighlightRanges = ranges;
            }

            IsUpdatingInputText = false;
            UpdateOutputText();

            GC.Collect();
        }

        #endregion
        #region == HighlightRanges ==

        private List<Range> _HighlightRanges = new();
        public List<Range> HighlightRanges
        {
            get => _HighlightRanges;
            set
            {
                _HighlightRanges = value;
                RaisePropertyChanged();
            }
        }

        #endregion
        #region == SuggestedPatterns ==

        private readonly ObservableCollection<string> _SuggestedPatterns = new ObservableCollection<string>();
        public ObservableCollection<string> SuggestedPatterns => _SuggestedPatterns;

        #endregion

        #region == OutputText ==

        private string _OutputText = "";
        public string OutputText
        {
            get => _OutputText;
            set
            {
                if (!_OutputText.Equals(value))
                {
                    _OutputText = value;
                    RaisePropertyChanged();
                }
            }
        }

        private void UpdateOutputText()
        {
            HasPatternWarning = Replacer.SearchRegices.Length != Replacer.Replacements.Length;

            OutputText = Replacer.Replace(InputText);
        }

        #endregion

        #region == PatternSetName ==

        private string _PatternSetName = "";
        public string PatternSetName
        {
            get => _PatternSetName;
            set
            {
                if (!_PatternSetName.Equals(value))
                {
                    _PatternSetName = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
        #region == PatternSets ==

        private readonly ObservableCollection<PatternSet> __PatternSets = new ObservableCollection<PatternSet>();
        public ObservableCollection<PatternSet> PatternSets => __PatternSets;

        #endregion

        #region == InputPath ==

        private string _InputPath = "";
        public string InputPath
        {
            get => _InputPath;
            set
            {
                if (!_InputPath.Equals(value))
                {
                    _InputPath = value;
                    UpdateInputFiles();
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
        #region == OutputPath ==

        private string _OutputPath = "";
        public string OutputPath
        {
            get => _OutputPath;
            set
            {
                if (!_OutputPath.Equals(value))
                {
                    _OutputPath = value;
                    UpdateOutputFiles();
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region == IsOverwrite ==

        private bool _IsOverwrite;
        public bool IsOverwrite
        {
            get => _IsOverwrite;
            set
            {
                if (_IsOverwrite != value)
                {
                    _IsOverwrite = value;
                    UpdateOutputFiles();
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
        #region == WhiteFilter ==

        private string _WhiteFilter = "";
        public string WhiteFilter
        {
            get => _WhiteFilter;
            set
            {
                if (!_WhiteFilter.Equals(value))
                {
                    _WhiteFilter = value;
                    try
                    {
                        Filter.WhitePattern = value;
                        ClearErrors();
                    }
                    catch
                    {
                        AddError();
                    }
                    UpdateInputFiles();
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
        #region == BlackFilter ==

        private string _BlackFilter = "";
        public string BlackFilter
        {
            get => _BlackFilter;
            set
            {
                if (!_BlackFilter.Equals(value))
                {
                    _BlackFilter = value;
                    try
                    {
                        Filter.BlackPattern = value;
                        ClearErrors();
                    }
                    catch
                    {
                        AddError();
                    }
                    UpdateInputFiles();
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
        #region == IsRename ==

        private bool _IsRename;
        public bool IsRename
        {
            get => _IsRename;
            set
            {
                if (_IsRename != value)
                {
                    _IsRename = value;
                    if (!value)
                    {
                        FilePatternSetIndex = -1;
                        DirectoryPatternSetIndex = -1;
                    }
                    UpdateOutputFiles();
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
        #region == FilePatternSetIndex ==

        private Replacer? FileReplacer;

        private int _FilePatternSetIndex = -1;
        public int FilePatternSetIndex
        {
            get => _FilePatternSetIndex;
            set
            {
                if (_FilePatternSetIndex != value)
                {
                    _FilePatternSetIndex = value;
                    if (value >= 0)
                    {
                        FileReplacer = new Replacer(PatternSets[value]);
                        UpdateOutputFiles();
                    }
                    else
                    {
                        FileReplacer = null;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
        #region == DirectoryPatternSetIndex ==

        private Replacer? DirectoryReplacer;

        private int _DirectoryPatternSetIndex = -1;
        public int DirectoryPatternSetIndex
        {
            get => _DirectoryPatternSetIndex;
            set
            {
                if (_DirectoryPatternSetIndex != value)
                {
                    _DirectoryPatternSetIndex = value;
                    if (value >= 0)
                    {
                        DirectoryReplacer = new Replacer(PatternSets[value]);
                        UpdateOutputFiles();
                    }
                    else
                    {
                        DirectoryReplacer = null;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region == FileTree ==

        private TreeNode _FileTree = new TreeNode();
        public TreeNode FileTreeRoot
        {
            get => _FileTree;
            set
            {
                _FileTree = value;
                RaisePropertyChanged();
            }
        }

        #endregion
        #region == InputFiles ==

        private async void UpdateInputFiles()
        {
            if (string.IsNullOrWhiteSpace(InputPath))
            {
                return;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(InputPath);
            if (directoryInfo.Exists)
            {
                FileTreeRoot = await GetTreeNodeAsync(directoryInfo);
                UpdateOutputFiles();
            }
        }

        private async Task<TreeNode> GetTreeNodeAsync(DirectoryInfo directoryInfo)
        {
            TreeNode parent = new TreeNode
            {
                IsDirectory = true,
                FromPath = directoryInfo.FullName,
            };

            foreach (DirectoryInfo directory in await Task.Run(() => directoryInfo.EnumerateDirectories()))
            {
                TreeNode child = await GetTreeNodeAsync(directory);
                parent.Nodes.Add(child);
                child.Parent = parent;
            }

            foreach (FileInfo file in await Task.Run(() => directoryInfo.EnumerateFiles()))
            {
                if (Filter.IsPass(file.Name))
                {
                    TreeNode child = new TreeNode
                    {
                        FromPath = file.FullName,
                    };
                    parent.Nodes.Add(child);
                    child.Parent = parent;
                }
            }

            return parent;
        }

        #endregion
        #region == OutputFiles ==

        private bool IsUpdatingOutputFiles;

        private async void UpdateOutputFiles()
        {
            if (IsUpdatingOutputFiles)
            {
                return;
            }

            if (IsOverwrite || !IsRename)
            {
                FileReplacer = null;
                DirectoryReplacer = null;
            }

            IsUpdatingOutputFiles = true;
            await Task.Run(() =>
            {
                UpdateTreeNode(FileTreeRoot);
            });
            IsUpdatingOutputFiles = false;
        }

        private void UpdateTreeNode(TreeNode treeNode)
        {
            string name = treeNode.FromName;

            if (treeNode.IsDirectory)
            {
                if (DirectoryReplacer != null)
                {
                    name = DirectoryReplacer.Replace(name);
                }

                foreach (TreeNode child in treeNode.Nodes)
                {
                    UpdateTreeNode(child);
                }
            }
            else
            {
                if (FileReplacer != null)
                {
                    name = FileReplacer.Replace(name);
                }
            }

            treeNode.ToName = name;
        }

        #endregion
        #region == IsReplacingFiles ==

        private bool _IsReplacingFiles;
        public bool IsReplacingFiles
        {
            get => _IsReplacingFiles;
            set
            {
                if (_IsReplacingFiles != value)
                {
                    _IsReplacingFiles = value;
                    if (value)
                    {
                        ProgressValue = 0;
                        ProgressMaximum = FileTreeRoot.FileCount;
                    }
                    else
                    {
                        ProgressValue = 0;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
        #region == ReplaceFiles ==

        public async void ReplaceFiles()
        {
            if (IsReplacingFiles)
            {
                return;
            }

            string path = OutputPath;

            if (IsOverwrite)
            {
                path = InputPath;
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            IsReplacingFiles = true;

            Directory.CreateDirectory(path);

            foreach (TreeNode treeNode in FileTreeRoot.Nodes)
            {
                await replace(treeNode);
            }

            IsReplacingFiles = false;

            async Task replace(TreeNode treeNode)
            {
                if (!IsReplacingFiles)
                {
                    return;
                }

                if (treeNode.IsChecked == false)
                {
                    return;
                }

                int length = path.Length;
                string toPath = Path.Combine(path, treeNode.ToName);

                if (treeNode.IsDirectory)
                {
                    try
                    {
                        Directory.CreateDirectory(toPath);

                        foreach (TreeNode child in treeNode.Nodes)
                        {
                            path = toPath;
                            await replace(child);
                        }

                        path = path.Substring(0, length);
                    }
                    catch (Exception ex)
                    {
                        ex.Log();
                    }
                }
                else
                {
                    await Replacer.ReplaceFile(treeNode.FromPath, toPath);
                    ProgressValue++;
                }
            }
        }

        public void CancelReplace()
        {
            IsReplacingFiles = false;
        }

        #endregion

        #region == ProgressValue ==

        private double _ProgressValue;
        public double ProgressValue
        {
            get => _ProgressValue;
            set
            {
                if (_ProgressValue != value)
                {
                    _ProgressValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
        #region == ProgressMaximum ==

        private double _ProgressMaximum = 1;
        public double ProgressMaximum
        {
            get => _ProgressMaximum;
            set
            {
                if (_ProgressMaximum != value)
                {
                    _ProgressMaximum = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        public void Close()
        {
            SearchPatternItems[0] = new PatternItem { Pattern = "" };
            ReplacePatternItems[0] = new PatternItem { Pattern = "" };
        }
    }
}
