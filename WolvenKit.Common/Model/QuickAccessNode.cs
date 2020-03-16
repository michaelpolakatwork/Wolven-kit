using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WolvenKit.Common
{
    public class QuickAccessNode : FolderBrowserNode
    {
        


        public QuickAccessNode()
        {
            DisplayName = "Quick access";
            DisplayType = typeof(QuickAccessNode).Name;
        }

        public ObservableCollection<QuickAccessNodeItem> QuickAccessItems { get; set; } = new ObservableCollection<QuickAccessNodeItem>();

        public new ObservableCollection<QuickAccessNodeItem>  DisplayChildren => QuickAccessItems;

        public void AddFavorite(WitcherTreeNode node)
        {
            if (node == null)
                return;
            var wrapper = new QuickAccessNodeItem(node);
            if (!QuickAccessItems.Contains(wrapper))
                QuickAccessItems.Add(wrapper);
        }
    }

    public class QuickAccessNodeItem : FolderBrowserNode
    {

        public QuickAccessNodeItem(WitcherTreeNode node)
        {
            Node = node;
            DisplayType = typeof(QuickAccessNodeItem).Name;
        }

        public new string DisplayName => Node.DisplayName;

        public WitcherTreeNode Node { get; set; }
    }
}
