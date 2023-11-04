using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regexer
{
    internal class TreeNode: SharedWPF.ViewModelBase
    {
        public bool IsDirectory { get; init; }
        #region == FromPath ==

        public string FromName => Path.GetFileName(FromPath);
        public string FromPath { get; init; } = "";

        #endregion
        #region == ToName ==

        private string _ToName = "";
        public string ToName
        {
            get => _ToName;
            set
            {
                _ToName = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region == IsChecked ==

        private bool? _IsChecked = true;
        public bool? IsChecked
        {
            get => _IsChecked;
            set
            {
                if (_IsChecked != value)
                {
                    _IsChecked = value;
                    OnIsCheckedChanged();
                    RaisePropertyChanged();
                }
            }
        }

        private void OnIsCheckedChanged()
        {
            if (IsChecked != null)
            {
                foreach (TreeNode treeNode in Nodes)
                {
                    treeNode.Parent = null;
                    treeNode.IsChecked = IsChecked;
                    treeNode.Parent = this;
                }
            }

            if (Parent != null)
            {
                int count = Parent.Nodes.Count(TreeNode => TreeNode.IsChecked == true);
                if (count == Parent.Nodes.Count)
                {
                    Parent.IsChecked = true;
                }
                else if (count == 0)
                {
                    Parent.IsChecked = false;
                }
                else
                {
                    Parent.IsChecked = null;
                }
            }
        }

        #endregion
        
        public TreeNode? Parent;
        #region == Nodes ==

        private readonly ObservableCollection<TreeNode> _Nodes = new ObservableCollection<TreeNode>();
        public ObservableCollection<TreeNode> Nodes => _Nodes;

        #endregion

        #region == FileCount ==

        public int FileCount
        {
            get
            {
                int count = 0;

                foreach (TreeNode child in Nodes)
                {
                    if (child.IsDirectory && child.IsChecked != false)
                    {
                        count += child.FileCount;
                    }
                    else if (child.IsChecked == true)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        #endregion
    }
}
