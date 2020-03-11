using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using WolvenKit.Bundles;
using WolvenKit.Cache;
using WolvenKit.Common;
using WolvenKit.CR2W;

namespace WolvenKit
{
    public partial class frmAssetBrowser : DockContent
    {
        public List<string> Autocompletelist;
        public List<IWitcherFile> FileList = new List<IWitcherFile>();
        public List<IWitcherArchive> Managers;

        public List<string> Files { get; set; }
        public WitcherTreeNode ActiveNode { get; set; }
        public WitcherTreeNode RootNode { get; set; }

        public List<WitcherListViewItem> SelectedPaths
        {
            get
            {
                var ret = new List<WitcherListViewItem>();
                foreach (WitcherListViewItem item in fileListView.SelectedItems)
                {
                    ret.Add(item);
                }
                return ret;
            }
        }

        public event EventHandler<Tuple<List<IWitcherArchive>, List<WitcherListViewItem>, bool>> RequestFileAdd;

        public frmAssetBrowser(List<IWitcherArchive> archives)
        {
            InitializeComponent();

            Managers = archives;
            RootNode = new WitcherTreeNode();
            RootNode.Name = "Root";
            foreach (var arch in archives)
            {
                if (arch == null)
                    continue;
                FileList.AddRange(arch.FileList);
                RootNode.Directories[arch.RootNode.Name] = arch.RootNode;
                arch.RootNode.Parent = RootNode;

                //extensions
                extensionCB.Items.Add(arch.TypeName);
                extensionCB.SelectedIndex = 0;
                var extensions = arch.Extensions;
                extensions.Sort();
                filetypeCB.Items.AddRange(extensions.ToArray());
                filetypeCB.SelectedIndex = 0;
                for (int i = 0; i < arch.AutocompleteSource.Count; i++)
                {
                    SearchBox.AutoCompleteCustomSource.Add(arch.AutocompleteSource[i]);
                }
            }
        }

        private void frmBundleExplorer_Load(object sender, EventArgs e)
        {
        }

        public void OpenNode(WitcherTreeNode node,bool reset = false)
        {
            if (ActiveNode != node || reset)
            {
                ActiveNode = node;

                UpdatePathPanel();

                fileListView.Items.Clear();
                List<WitcherListViewItem> res = new List<WitcherListViewItem>();
                if (node.Parent != null)
                {
                    res.Add(new WitcherListViewItem
                    {
                        Node = node.Parent,
                        Text = "..",
                        IsDirectory = true,
                        ImageKey = "FolderOpened"
                    });
                }

                res.AddRange(node.Directories.OrderBy(x => x.Key).Select(item => new WitcherListViewItem
                {
                    Node = item.Value,
                    Text = item.Key,
                    IsDirectory = true,
                    ImageKey = "FolderOpened"
                }));


                foreach (var item in node.Files.OrderBy(x => x.Key))
                {
                    var lastItem = item.Value[item.Value.Count - 1];
                    var listItem = new WitcherListViewItem
                    {
                        Node = node,
                        FullPath = lastItem.Name,
                        Text = item.Key,
                        IsDirectory = false,
                        ImageKey = GetImageKey(item.Key)
                    };
                    listItem.SubItems.Add(lastItem.Size.ToString());
                    listItem.SubItems.Add(string.Format("{0}%",
                        (100 - (int)(lastItem.ZSize / (float)lastItem.Size * 100.0f))));
                    listItem.SubItems.Add(lastItem.CompressionType);
                    listItem.SubItems.Add(lastItem.Bundle.TypeName);
                    res.Add(listItem);
                }
                fileListView.Items.AddRange(res.ToArray());
            }
        }

        private void UpdatePathPanel()
        {
            var nodes = new List<WitcherTreeNode>();
            var c = ActiveNode;
            while (c != null)
            {
                nodes.Add(c);
                c = c.Parent;
            }

            pathPanel.Controls.Clear();
            var x = 0;
            nodes.Reverse();
            foreach (var link in nodes)
            {
                var button = new Label();
                button.Text = link.Name + " >";
                button.Location = new Point(x, -3);
                button.Padding = new Padding(0);
                button.Margin = new Padding(0);
                var textSize = TextRenderer.MeasureText(button.Text, button.Font);
                button.Width = textSize.Width;
                button.Height = 23;
                button.BackColor = Color.Transparent;
                button.MouseLeave += button_MouseLeave;
                button.MouseEnter += button_MouseEnter;
                button.TextAlign = ContentAlignment.MiddleCenter;
                button.Cursor = Cursors.Hand;
                button.Click += delegate { OpenNode(link); };

                pathPanel.Controls.Add(button);

                x += button.Width;
            }
        }

        private void button_MouseEnter(object sender, EventArgs e)
        {
            var label = (Label) sender;
            label.BackColor = Color.LightBlue;
        }

        private void button_MouseLeave(object sender, EventArgs e)
        {
            var label = (Label) sender;
            label.BackColor = Color.Transparent;
        }

        private void fileListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (fileListView.SelectedItems.Count > 0)
            {
                var item = (WitcherListViewItem) fileListView.SelectedItems[0].Clone();
                if (item.IsDirectory)
                {
                    OpenNode(item.Node);
                }
                else
                {
                    RequestFileAdd.Invoke(this, new Tuple<List<IWitcherArchive>, List<WitcherListViewItem>, bool>(Managers, SelectedPaths, false));
                }
            }
        }

        private Bitmap GetImage(CR2WFile file)
        {
            return file.chunks[0].Type == "CBitmapTexture" ? ImageUtility.Xbm2Bitmap(file.chunks[0]) : null;
        }

        private void pathPanel_Click(object sender, EventArgs e)
        {
            pathPanel.Controls.Clear();
            var textbox = new TextBox
            {
                Location = new Point(16, 2),
                Width = pathPanel.Width,
                Height = pathPanel.Height,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                BorderStyle = BorderStyle.None
            };
            pathPanel.Controls.Add(textbox);
            textbox.Focus();

            textbox.PreviewKeyDown += textbox_PreviewKeyDown;
            textbox.LostFocus += textbox_LostFocus;
            textbox.Margin = new Padding(3);
            textbox.Text = ActiveNode.FullPath;
            textbox.SelectAll();
            textbox.AcceptsReturn = true;
        }

        private void textbox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            var textbox = (TextBox) sender;
            if (e.KeyCode == Keys.Enter)
            {
                var browsePath = textbox.Text;
                OpenPath(browsePath);
            }
        }

        public string GetImageKey(string filename)
        {
            try
            {
                if (treeImages.Images.ContainsKey(Path.GetExtension(filename).Replace(".", "")))
                {
                    return Path.GetExtension(filename).Replace(".", "");
                }
            }
            catch  { }

            return "genericFile";
        }

        public void OpenPath(string browsePath)
        {
            var currentNode = RootNode;
            var parts = browsePath.Split('\\');
            if (parts.Length == 1 && parts[0] == "Root")
            {
                OpenNode(RootNode);
                return;
            }
            if (parts.Length > 0 && parts[0] == "Root")
                parts = parts.Skip(1).ToArray();
            var success = false;
            for (var i = 0; i < parts.Length; i++)
            {
                if (currentNode.Directories.ContainsKey(parts[i]))
                {
                    currentNode = currentNode.Directories[parts[i]];

                    success = true;
                }
                else if (i == parts.Length - 1 && (currentNode.Files.ContainsKey(parts[i]) || parts[i] == ""))
                {
                    success = true;
                }
                else
                {
                    success = false;
                    break;
                }
            }

            if (success || currentNode != RootNode)
            {
                OpenNode(currentNode);
            }
            else
            {
                OpenNode(RootNode);
            }
        }

        private void textbox_LostFocus(object sender, EventArgs e)
        {
            UpdatePathPanel();
        }

        public void Search(string s, int bundleTypeIdx, int fileTypeIdx)
        {
            var extension = "";
            var bundletype = "";
            if (filetypeCB.SelectedIndex != -1)
                extension = filetypeCB.Items[fileTypeIdx].ToString();
            if (extensionCB.SelectedIndex != -1)
                bundletype = extensionCB.Items[bundleTypeIdx].ToString();
            var found = SearchFiles(FileList.ToArray(), s, bundletype, extension);
            if (found.Length > 1000)
                found = found.Take(1000).ToArray();
            fileListView.Items.Clear();
            var results = new List<WitcherListViewItem>();
            foreach (var file in found)
            {
                var listItem = new WitcherListViewItem(file.Item2);
                listItem.SubItems.Add(file.Item2.Size.ToString());
                listItem.SubItems.Add($"{(100 - (int) (file.Item2.ZSize/(float)file.Item2.Size*100.0f))}%");
                listItem.SubItems.Add(file.Item2.CompressionType);
                listItem.SubItems.Add(file.Item2.Bundle.TypeName);
                results.Add(listItem);
            }
            fileListView.Items.AddRange(results.ToArray());
        }

        public List<IWitcherFile> GetFiles(WitcherTreeNode mainnode)
        {
            var bundfiles = new List<IWitcherFile>();
            if (mainnode?.Files != null)
            {
                foreach (var wfile in mainnode.Files)
                {
                    bundfiles.AddRange(wfile.Value);
                }
                bundfiles.AddRange(mainnode.Directories.Values.SelectMany(GetFiles));
            }
            return bundfiles;
        }

        public Tuple<WitcherListViewItem,IWitcherFile>[] SearchFiles(IWitcherFile[] files, string searchkeyword, string bundletype, string extension)
        {
            if (regexCheckbox.Checked)
            {
                try
                {
                    if (currentfolderCheckBox.Checked)
                    {
                        return files.Where(item => item.Bundle.FileName.Contains(ActiveNode.Name) && new Regex(searchkeyword).IsMatch(item.Name)).Select(x => new Tuple<WitcherListViewItem, IWitcherFile>(new WitcherListViewItem(x), x)).ToArray();
                    }

                    return files.Where(item => new Regex(searchkeyword).IsMatch(item.Name)).Select(x => new Tuple<WitcherListViewItem, IWitcherFile>(new WitcherListViewItem(x), x)).ToArray();
                }
                catch
                {
                    //TODO: Log
                }
            }
            if (currentfolderCheckBox.Checked)
            {
                return caseCheckBox.Checked
                    ? files.Where(item => item.Bundle.FileName.Contains(ActiveNode.Name)
                        && item.Name.ToUpper().Contains(searchkeyword.ToUpper())
                        && (item.Name.ToUpper().EndsWith(extension.ToUpper()) || extension.ToUpper() == "ANY")
                        && (item.Bundle.TypeName == bundletype || bundletype.ToUpper() == "ANY"))
                    .Distinct().Select(x => new Tuple<WitcherListViewItem, IWitcherFile>(new WitcherListViewItem(x), x)).ToArray()
                    : files.Where(item => item.Bundle.FileName.Contains(ActiveNode.Name)
                        && item.Name.Contains(searchkeyword)
                        && (item.Name.EndsWith(extension) || extension.ToUpper() == "ANY")
                        && (item.Bundle.TypeName == bundletype || bundletype.ToUpper() == "ANY"))
                    .Distinct().Select(x => new Tuple<WitcherListViewItem, IWitcherFile>(new WitcherListViewItem(x), x)).ToArray();
            }
            return caseCheckBox.Checked
                ? files.Where(item => item.Name.ToUpper().Contains(searchkeyword.ToUpper())
                    && (item.Name.ToUpper().EndsWith(extension.ToUpper()) || extension.ToUpper() == "ANY")
                    && (item.Bundle.TypeName == bundletype || bundletype.ToUpper() == "ANY"))
                    .Select(x => new Tuple<WitcherListViewItem, IWitcherFile>(new WitcherListViewItem(x), x)).ToArray()
                : files.Where(item => item.Name.Contains(searchkeyword)
                    && (item.Name.EndsWith(extension) || extension.ToUpper() == "ANY")
                    && (item.Bundle.TypeName == bundletype || bundletype.ToUpper() == "ANY"))
                    .Select(x => new Tuple<WitcherListViewItem, IWitcherFile>(new WitcherListViewItem(x), x)).ToArray();
        }

        /// <summary>
        /// Recursively collects files into an array, descending depth first from a given root node.
        /// </summary>
        /// <param name="root">Root node.</param>
        /// <returns>Array of collected files.</returns>
        public WitcherListViewItem[] CollectFiles(WitcherTreeNode root)
        {
            var collectedFilesList = new List<WitcherListViewItem>();

            foreach (var fileList in root.Files)
            {
                foreach (var file in fileList.Value)
                {
                    collectedFilesList.Add(new WitcherListViewItem(file));
                }
            }

            foreach (var dr in root.Directories)
            {
                collectedFilesList.AddRange(CollectFiles(dr.Value));
            }

            return collectedFilesList.ToArray();
        }

        public string[] GetExtensions(params string[] filename)
        {
            var extensions = new List<string>();
            foreach (var file in filename.Where(file => !extensions.Contains(file.Split('.').Last())))
            {
                extensions.Add(file.Split('.').Last());
            }
            return extensions.ToArray();
        }

        

        private void SearchButton_Click(object sender, EventArgs e)
        {
            Search(SearchBox.Text, extensionCB.SelectedIndex, filetypeCB.SelectedIndex);
        }

       
        private void fileListView_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Back)
            {
                if (ActiveNode != RootNode)
                    OpenNode(ActiveNode.Parent);
            }
            if (e.KeyCode == Keys.Enter)
            {
                if (fileListView.SelectedItems.Count > 0)
                {
                    var item = (WitcherListViewItem)fileListView.SelectedItems[0];
                    if (item.IsDirectory)
                    {
                        OpenNode(item.Node);
                    }
                    else
                    {
                        RequestFileAdd.Invoke(this, new Tuple<List<IWitcherArchive>, List<WitcherListViewItem>, bool>(Managers, SelectedPaths, false));
                    }
                }
            }
            if (e.KeyCode == Keys.Space)
            {

                if (fileListView.SelectedItems.Count > 0)
                {
                    var item = (WitcherListViewItem)fileListView.SelectedItems[0];
                    if (Path.GetExtension(item.FullPath) == ".xbm" && !item.IsDirectory)
                    {
                        var rawbytes = ExtractFirstFile(item);
                        Bitmap bmp = null;
                        if (item.ExplorerPath.Contains("TextureCache"))
                        {
                            DdsImage ddsImg = new DdsImage(rawbytes);
                            bmp = ddsImg.BitmapImage;
                        }
                        else
                        {    
                            CR2WFile cr2wfile = new CR2WFile(rawbytes);
                            bmp = cr2wfile.chunks[0].Type == "CBitmapTexture" ? ImageUtility.Xbm2Bitmap(cr2wfile.chunks[0]) : null;
                        }
                        if (bmp != null)
                        {
                            var formPreview = new frmTextureFile
                            {

                            };
                            formPreview.LoadImage(bmp);
                            formPreview.Show();
                        }
                    }
                }
            }
        }


        private byte[] ExtractFirstFile(WitcherListViewItem item)
        {
            var depotpath = item.ExplorerPath ?? item.FullPath ?? "";
            byte[] ret = null;
            foreach (var manager in Managers.Where(manager => depotpath.StartsWith(Path.Combine("Root", manager.TypeName))))
            {
                if (manager.Items.Any(x => x.Value.Any(y => y.Name == item.FullPath)))
                {
                    var archives = manager.FileList.Where(x => x.Name == item.FullPath).Select(y => new KeyValuePair<string, IWitcherFile>(y.Bundle.FileName, y));
                    try
                    {
                        using (var ms = new MemoryStream())
                        {
                            archives.FirstOrDefault().Value.Extract(ms);
                            ret = ms.ToArray();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            return ret;
        }

        private void SearchBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Search(SearchBox.Text, extensionCB.SelectedIndex, filetypeCB.SelectedIndex);
            }
        }

        private void btOpen_Click(object sender, EventArgs e)
        {
            RequestFileAdd.Invoke(this, new Tuple<List<IWitcherArchive>, List<WitcherListViewItem>,bool>(Managers, SelectedPaths,false));
        }

        private void AddDLCFile_Click(object sender, EventArgs e)
        {
            RequestFileAdd.Invoke(this, new Tuple<List<IWitcherArchive>, List<WitcherListViewItem>,bool>(Managers, SelectedPaths,true));
        }

        private void copyPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileListView.SelectedItems.Count > 0)
            {
                var item = ((WitcherListViewItem) fileListView.SelectedItems[0]);
                if (item?.IsDirectory == false)
                {
                    Clipboard.SetText(item.FullPath);
                }
            }
        }

        private void fileListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Control)
            {
                fileListView.MultiSelect = true;
                foreach (WitcherListViewItem item in fileListView.Items)
                {
                    item.Selected = true;
                }
            }
        }

        private void clearSearch_Click(object sender, EventArgs e)
        {
            OpenNode(RootNode,true);
        }

        private void detailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileListView.View = View.Details; 
        }

        private void largeIconToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileListView.View = View.LargeIcon;
        }

        private void smallIconToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileListView.View = View.SmallIcon;
        }

        private void listToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileListView.View = View.List;
        }

        private void tileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileListView.View = View.Tile;
        }

        private void Home_Click(object sender, EventArgs e)
        {
            OpenPath("Root");
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var formPreview = new Forms.frmWPFAssetBrowser
            {

            };
            formPreview.Show();
        }
    }
}