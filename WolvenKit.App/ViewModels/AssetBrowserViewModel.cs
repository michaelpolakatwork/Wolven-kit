using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WolvenKit.App;
using WolvenKit.App.Commands;
using WolvenKit.Common;

namespace WolvenKit.App.ViewModels
{
    public class AssetBrowserViewModel : ViewModel
    {
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
        public List<IWitcherFile> FileList = new List<IWitcherFile>();
        public List<IWitcherArchive> Managers;

        public List<string> Files { get; set; }
        public WitcherTreeNode ActiveNode { get; set; }
        public WitcherTreeNode RootNode { get; set; }
        #endregion

        #region Commands
        public ICommand AddToDlcCommand { get; }
        public ICommand AddToModCommand { get; }
        #endregion


        public AssetBrowserViewModel()
        {
            Title = "Asset Browser";
            ContentId = "assetBrowser";


            #region Commands
            AddToDlcCommand = new RelayCommand(AddToDlc);
            AddToModCommand = new RelayCommand(AddToMod);
            #endregion

            bool loadmods = false; //FIXME!!!!

            List<IWitcherArchive> archives = loadmods ? MainController.Get().GetModArchives() : MainController.Get().GetArchives();
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

                
            }

        }

        #region Methods
        protected virtual void AddToDlc()
        {

        }

        protected virtual void AddToMod()
        {

        }
        #endregion


    }
}
