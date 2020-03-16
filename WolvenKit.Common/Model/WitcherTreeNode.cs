using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WolvenKit.Common
{
    public class WitcherTreeNode : FolderBrowserNode, IFileExplorerItem
    {
        public WitcherTreeNode()
        {
            Directories = new Dictionary<string, WitcherTreeNode>();
            Files = new Dictionary<string, List<IWitcherFile>>();
            Name = "";

            
            DisplayType = typeof(WitcherTreeNode).Name;
        }

        public new string DisplayName => Name;

        public new string FullPath
        {
            get
            {
                var path = "";
                var current = this;
                while (true)
                {
                    path = Path.Combine(current.Name, path);
                    current = current.Parent;
                    if (current == null)
                        break;
                }

                return path ?? "";
            }
        }

        public string Name { get; set; }
        public WitcherTreeNode Parent { get; set; }
        public Dictionary<string, WitcherTreeNode> Directories { get; set; }
        public Dictionary<string, List<IWitcherFile>> Files { get; set; }

        public new ObservableCollection<WitcherTreeNode> DisplayChildren => new ObservableCollection<WitcherTreeNode>( Directories.Values);

        public List<IWitcherFile> FilesList => Files.SelectMany(_ => _.Value).ToList();
        public List<IFileExplorerItem> Browseables { 
            get
            {
                List<IFileExplorerItem> ret = new List<IFileExplorerItem>(Directories.Values.ToList());
                ret.AddRange(FilesList);
                return ret;
            }
        }
    }
}
