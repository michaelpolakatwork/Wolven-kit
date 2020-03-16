using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using WolvenKit.App.Model;
using WolvenKit.App.Commands;
using WolvenKit.Common;

namespace WolvenKit.App.ViewModels
{
    public class AssetBrowserViewModel : ViewModel
    {
        const string SearchNodeName = "__SearchNode__";

        #region Properties
        #region Title
        private string _title;
        public virtual string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion
        #region ContentId
        private string _contentId;
        public virtual string ContentId
        {
            get
            {
                return _contentId;
            }
            set
            {
                if (_contentId != value)
                {
                    _contentId = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion
        #region SelectedNode
        private WitcherTreeNode _selectedNode = null;
        public WitcherTreeNode SelectedNode
        {
            get => _selectedNode;
            set
            {
                if (_selectedNode != value)
                {
                    _selectedNode = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsContextualRibbonSearchVisible));

                    if (!History.Any())
                        History.Enqueue(value);
                    else if (History.Last() != SelectedNode)
                        History.Enqueue(value);
                    Future.Clear();
                }
            }
        }
        #endregion
        #region SelectedItem
        private IFileExplorerItem _selectedItem = null;
        public IFileExplorerItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion
        #region IsModBrowser
        private bool _isModBrowser;
        public bool IsModBrowser
        {
            get => _isModBrowser;
            set
            {
                if (_isModBrowser != value)
                {
                    _isModBrowser = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion
        #region SearchParams
        private SearchParameters _searchParams;
        public SearchParameters SearchParams
        {
            get => _searchParams;
            set
            {
                if (_searchParams != value)
                {
                    _searchParams = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion
        #region SelectedTabIndex
        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                if (_selectedTabIndex != value)
                {
                    _selectedTabIndex = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion
        #region SearchTabIndex
        private int _searchTabIndex;
        public int SearchTabIndex
        {
            get => _searchTabIndex;
            set
            {
                if (_searchTabIndex != value)
                {
                    _searchTabIndex = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        

        #region Commands
        public ICommand AddToDlcCommand { get; }
        public ICommand AddToModCommand { get; }
        public ICommand PreviewCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand UpCommand { get; }
        public ICommand ForwardsCommand { get; }
        public ICommand BackwardsCommand { get; }
        public ICommand SelectedItemChangedCommand { get; }
        public ICommand OpenNodeCommand { get; }
        public ICommand PinCommand { get; }
        public ICommand ToggleBrowserCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand SelectNoneCommand { get; }
        public ICommand InvertSelectionCommand { get; }

        public ICommand SetSearchSizeCommand { get; }
        public ICommand SearchOpenFileLocationCommand { get; }
        public ICommand SearchCloseCommand { get; }
        #endregion

        public ObservableCollection<FolderBrowserNode> TreeNodes { get; set; } = new ObservableCollection<FolderBrowserNode>();
        public string IsContextualRibbonSearchVisible => SelectedNode.Name == SearchNodeName ? "Visible" : "Hidden";
        public string Breadcrumb => SelectedItem == null ? SelectedNode == null ? "" : _selectedNode.FullPath : _selectedItem.Name;
        public List<string> SearchTypes { get; set; } = new List<string>();
        public List<string> SearchBundles { get; set; } = new List<string>();
        #endregion

        #region Fields
        private List<IWitcherArchive> Managers { get; set; }
        private WitcherTreeNode RootNode { get; set; } = new WitcherTreeNode() { Name = "Root" };
        private QuickAccessNode _favorites { get; set; } = new QuickAccessNode();
        
        

        private Queue<WitcherTreeNode> History { get; set; } = new Queue<WitcherTreeNode>();
        private Queue<WitcherTreeNode> Future { get; set; } = new Queue<WitcherTreeNode>();
        private List<IWitcherFile> FileList { get; set; } = new List<IWitcherFile>();

        #endregion

        #region Constructors
        public static Task<AssetBrowserViewModel> CreateAsync()
        {
            var ret = new AssetBrowserViewModel();
            return ret.InitializeAsync();
        }

        private async Task<AssetBrowserViewModel> InitializeAsync()
        {
            await Task.Run(() => ReloadTree());
            return this;
        }

        private AssetBrowserViewModel()
        {
            Title = "Asset Browser";
            ContentId = "assetBrowser";

            SearchParams = new SearchParameters();
            this.PropertyChanged += MyViewModel_PropertyChanged;

            #region Commands
            AddToDlcCommand = new DelegateCommand<object>(AddToDlc, CanAddToDlc);
            AddToModCommand = new DelegateCommand<object>(AddToMod, CanAddToMod);
            PreviewCommand = new RelayCommand(Preview, CanPreview);
            PinCommand = new RelayCommand(Pin, CanPin);

            SearchCommand = new RelayCommand(Search);
            SetSearchSizeCommand = new DelegateCommand<object>(SetSearchParameter);
            
            UpCommand = new RelayCommand(Up, CanUp);
            ForwardsCommand = new RelayCommand(Forwards, CanForwards);
            BackwardsCommand = new RelayCommand(Backwards, CanBackwards);
            
            SelectedItemChangedCommand = new DelegateCommand<object>(SelectedItemChanged);
            OpenNodeCommand = new DelegateCommand<object>(OpenNode);
            ToggleBrowserCommand = new RelayCommand(ToggleBrowser);
            SelectAllCommand = new RelayCommand(SelectAll);
            SelectNoneCommand = new RelayCommand(SelectNone);
            InvertSelectionCommand = new RelayCommand(InvertSelection);
            SearchOpenFileLocationCommand = new RelayCommand(SearchOpenFileLocation, CanSearchOpenFileLocation);
            SearchCloseCommand = new RelayCommand(SearchClose);
            #endregion

            SearchParams.PropertyChanged += MyViewModel_PropertyChanged;

        }
        #endregion

        #region Events
        void MyViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SearchParams":
                {
                    Search();
                    break;
                }
            }
        }
        #endregion



        #region Commands Implementation
        protected bool CanAddToDlc(object param)
        {
            try
            {
                System.Collections.IList items = (System.Collections.IList)param;
                var n = items.Cast<IFileExplorerItem>().ToList();
                return n.Any();
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected void AddToDlc(object param)
        {
            System.Collections.IList items = (System.Collections.IList)param;
            var n = items.Cast<IFileExplorerItem>().ToList();
            FileAdd(n, true);
        }
        protected bool CanAddToMod(object param)
        {
            try
            {
                System.Collections.IList items = (System.Collections.IList)param;
                var n = items.Cast<IFileExplorerItem>().ToList();
                return n.Any();
            }
            catch (Exception)
            {
                return false;
            }
        }
        protected void AddToMod(object param)
        {
            System.Collections.IList items = (System.Collections.IList)param;
            var n = items.Cast<IFileExplorerItem>().ToList();
            FileAdd(n, false);
        }
        protected bool CanPreview() => SelectedItem is IWitcherFile;
        protected void Preview()
        {
            //throw new NotImplementedException();
        }
        protected bool CanUp() => SelectedNode != null && SelectedNode.Parent != null;
        protected void Up()
        {
            SetCurrentNode(SelectedNode.Parent, true);
        }

        protected bool CanForwards() => Future.Any();
        protected void Forwards()
        {
            SetCurrentNode(Future.Dequeue(), true);
        }

        protected bool CanBackwards() => History.Any() && History.Last() != RootNode;
        protected void Backwards()
        {
            Future.Enqueue(SelectedNode);
            SetCurrentNode(History.Dequeue(), true);
        }
        
        protected void OpenNode(object param)
        {
            if (param is WitcherTreeNode)
            {
                // open directory
                SelectedNode = param as WitcherTreeNode;
            }
            else if (param is IWitcherFile)
            {
                // Add file to Mod
                AddToMod(param);
            }
        }

        protected bool CanPin() => SelectedItem is WitcherTreeNode;
        protected void Pin()
        {
            _favorites.AddFavorite(_selectedItem as WitcherTreeNode);
        }
        protected async void ToggleBrowser()
        {
            IsModBrowser = !IsModBrowser;
            await ReloadTree();
        }
        protected void SelectAll()
        {

        }
        protected void SelectNone()
        {

        }
        protected void InvertSelection()
        {

        }
        protected bool CanSearchOpenFileLocation() => SelectedItem is IFileExplorerItem;
        protected void SearchOpenFileLocation()
        {
            //get breadcrumb
            var splits = SelectedItem.Name.Split(Path.PathSeparator).ToList();
            var node = RootNode;
            while(true)
            {
                if (node.Directories == null || !node.Directories.Any())
                    break;
                node = node.Directories.FirstOrDefault(_ => _.Key == splits.First()).Value;
                splits.Remove(splits.First());
            }

        }
        protected void SearchClose()
        {

        }

        protected void SetSearchParameter(object param)
        {
            // This is terrible, but Fluent:Dropdownbutton has no selected Item property at all ...
            object FocusedInfo = TryGetPrivateProperty<object>(param, "FocusedInfo");
            string ItemName = TryGetPrivateProperty<string>(FocusedInfo, "Item");

            T TryGetPrivateProperty<T>(object obj, string propertyName)
            {
                try
                {
                    PropertyInfo prop = obj.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
                    MethodInfo getter = prop.GetGetMethod(nonPublic: true);
                    T val = (T)getter.Invoke(obj, null);

                    return (T)val;
                }
                catch
                {
                    return default(T);
                }
            }
        }

        protected void Search()
        {
            var searchNode = new WitcherTreeNode() { Name = SearchNodeName };
            var found = SearchFiles();
            //if (found.Length > 1000)
            //    found = found.Take(1000).ToArray();
            
            foreach (var item in found)
            {
                if (searchNode.Files.Keys.Contains(item.Name))
                {
                    searchNode.Files[item.Name].Add(item);
                }
                else
                    searchNode.Files.Add(item.Name, new List<IWitcherFile>() { item });
            }

            SelectedNode = searchNode;
        }


        #endregion

        #region Methods
        protected void SelectedItemChanged(object param)
        {
            if (param is WitcherTreeNode)
                SelectedNode = param as WitcherTreeNode;
            if (param is QuickAccessNodeItem)
                SelectedNode = (param as QuickAccessNodeItem).Node;
        }
        private void FileAdd(List<IFileExplorerItem> items, bool AddAsDLC )
        {
            //ModExplorer.PauseMonitoring();
            if (Process.GetProcessesByName("Witcher3").Length != 0)
            {
                //MessageBox.Show(@"Please close The Witcher 3 before tinkering with the files!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            MainController.Get().ProjectStatus = "Busy";
            var skipping = false;
            foreach (IFileExplorerItem item in items)
            {
                if (item is WitcherTreeNode)
                {

                }
                else if (item is IWitcherFile)
                {
                    skipping = AddToMod(item as IWitcherFile, skipping, AddAsDLC);
                }
            }
            //SaveMod();
            MainController.Get().ProjectStatus = "Ready";
            //ModExplorer.FoldersShown = true;
            //ModExplorer.FilteredFiles = ActiveMod.Files;
            //ModExplorer.UpdateModFileList(true, true);
            //ModExplorer.ResumeMonitoring();
        }

        
        private bool AddToMod(IWitcherFile item, bool skipping, bool AddAsDLC)
        {
            var fullPath = item.Name;
            var NodeName = Path.Combine("Root", item.Bundle.TypeName, Path.GetDirectoryName(item.Name));

            var explorerPath = Path.Combine(NodeName, Path.GetFileName(fullPath));

            var ActiveMod = MainController.Get().ActiveMod;
            bool skip = skipping;
            var depotpath = explorerPath ?? fullPath ?? "";
            foreach (var manager in Managers.Where(manager => depotpath.StartsWith(Path.Combine("Root", manager.TypeName))))
            {
                if (manager.Items.Any(x => x.Value.Any(y => y.Name == fullPath)))
                {
                    var archives = manager.FileList.Where(x => x.Name == fullPath).Select(y => new KeyValuePair<string, IWitcherFile>(y.Bundle.FileName, y));
                    string filename;
                    if (archives.First().Value.Bundle.TypeName == MainController.Get().CollisionManager.TypeName ||
                        archives.First().Value.Bundle.TypeName == MainController.Get().TextureManager.TypeName)
                    {
                        filename = Path.Combine(ActiveMod.FileDirectory, "Raw", AddAsDLC ? Path.Combine("DLC", archives.First().Value.Bundle.TypeName, "dlc", ActiveMod.Name, fullPath) : Path.Combine("Mod", archives.First().Value.Bundle.TypeName, fullPath));
                    }
                    else
                    {
                        filename = Path.Combine(ActiveMod.FileDirectory, AddAsDLC ? Path.Combine("DLC", archives.First().Value.Bundle.TypeName, "dlc", ActiveMod.Name, fullPath) : Path.Combine("Mod", archives.First().Value.Bundle.TypeName, fullPath));
                    }
                    if (archives.Count() > 1)
                    {

                        /*var dlg = new frmExtractAmbigious(archives.Select(x => x.Key).ToList());
                        if (!skip)
                        {
                            var res = dlg.ShowDialog();
                            skip = dlg.Skip;
                            if (res == DialogResult.Cancel)
                            {
                                return skip;
                            }
                        }
                        var selectedBundle = archives.FirstOrDefault(x => x.Key == dlg.SelectedBundle).Value;
                        try
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(filename));
                            if (File.Exists(filename))
                            {
                                File.Delete(filename);
                            }
                            selectedBundle.Extract(filename);
                        }
                        catch { }*/
                        return skip;
                    }

                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(filename));
                        if (File.Exists(filename))
                        {
                            File.Delete(filename);
                        }

                        archives.FirstOrDefault().Value.Extract(filename);
                    }
                    catch (Exception ex)
                    {
                        //AddOutput(ex.ToString(), Logtype.Error);
                    }
                    return skip;
                }
            }
            return skip;
        }
        
        private void SetCurrentNode(WitcherTreeNode node, bool keepFuture)
        {
            Queue<WitcherTreeNode> temp = new Queue<WitcherTreeNode>(Future);
            SelectedNode = node;
            if (keepFuture)
                Future = temp;
        }
        
        

        private IWitcherFile[] SearchFiles()
        {
            var extension = SearchParams.SearchTypeText ?? "ANY";
            var bundletype = SearchParams.SearchBundleText ?? "ANY";
            var searchkeyword = SearchParams.SearchText;

            if (SearchParams.IsRegex)
            {
                try
                {
                    if (SearchParams.IsCurrentFolder)
                    {
                        return FileList.Where(item => item.Bundle.FileName.Contains(SelectedNode.Name) && 
                        new Regex(searchkeyword).IsMatch(item.Name)).ToArray();
                    }

                    return FileList.Where(item => new Regex(searchkeyword).IsMatch(item.Name)).ToArray();
                }
                catch
                {
                    //TODO: Log
                }
            }
            if (SearchParams.IsCurrentFolder)
            {
                return SearchParams.IsMatchCase
                    ? FileList.Where(item => item.Bundle.FileName.Contains(SelectedNode.Name)
                        && item.Name.ToUpper().Contains(searchkeyword.ToUpper())
                        && (item.Name.ToUpper().EndsWith(extension.ToUpper()) || extension.ToUpper() == "ANY")
                        && (item.Bundle.TypeName == bundletype || bundletype.ToUpper() == "ANY"))
                    .Distinct().ToArray()
                    : FileList.Where(item => item.Bundle.FileName.Contains(SelectedNode.Name)
                        && item.Name.Contains(searchkeyword)
                        && (item.Name.EndsWith(extension) || extension.ToUpper() == "ANY")
                        && (item.Bundle.TypeName == bundletype || bundletype.ToUpper() == "ANY"))
                    .Distinct().ToArray();
            }

            return !SearchParams.IsMatchCase
                ? FileList.Where(item => item.Name.ToUpper().Contains(searchkeyword.ToUpper())
                    && (item.Name.ToUpper().EndsWith(extension.ToUpper()) || extension.ToUpper() == "ANY")
                    && (item.Bundle.TypeName == bundletype || bundletype.ToUpper() == "ANY"))
                    .ToArray()
                : FileList.Where(item => item.Name.Contains(searchkeyword)
                    && (item.Name.EndsWith(extension) || extension.ToUpper() == "ANY")
                    && (item.Bundle.TypeName == bundletype || bundletype.ToUpper() == "ANY"))
                    .ToArray();
        }
        
        private async Task ReloadTree()
        {
            RootNode = new WitcherTreeNode() { Name = "Root" };
            TreeNodes.Clear();
            SearchTypes.Clear();
            SearchBundles.Clear();
            FileList.Clear();

            if (IsModBrowser)
                Managers = MainController.Get().GetModArchives();
            else
                Managers = MainController.Get().GetArchives();

            foreach (var arch in Managers)
            {
                if (arch == null)
                    continue;
                RootNode.Directories[arch.RootNode.Name] = arch.RootNode;
                arch.RootNode.Parent = RootNode;

                FileList.AddRange(arch.FileList);

                SearchTypes.AddRange(arch.Extensions);
                SearchBundles.Add(arch.TypeName);
            }
            SearchTypes = SearchTypes.Distinct().ToList();
            SearchTypes.Sort();
            SearchBundles.Sort();

            TreeNodes.Add(_favorites);
            foreach (var item in RootNode.Directories)
            {
                TreeNodes.Add(item.Value);
            }


            SelectedNode = RootNode;
        }

        #endregion
    }
}
