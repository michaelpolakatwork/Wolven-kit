using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Forms.Integration;
using WeifenLuo.WinFormsUI.Docking;
using System.Windows.Input;

namespace WolvenKit.Forms
{
    using WolvenKit.App.ViewModels;
    using WolvenKit.Common;
    using WpfControlLibrary.AssetBrowser;


    public partial class frmWPFAssetBrowser : DockContent
    {
        private ElementHost ctrlHost;
        private AssetBrowserControl wpfcontrol;
        private IViewModel viewModel;

        public frmWPFAssetBrowser(IViewModel vm)
        {
            viewModel = vm;

            InitializeComponent();
            
        }

        private void AssetBrowserWPF_Load(object sender, EventArgs e)
        {
            // WPF ELementhosting
            ctrlHost = new ElementHost
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(ctrlHost);
            wpfcontrol = new AssetBrowserControl(viewModel);
            wpfcontrol.InitializeComponent();
            ctrlHost.Child = wpfcontrol;
            wpfcontrol.Loaded += new RoutedEventHandler(wpfCtrl_Loaded);
        }

        void wpfCtrl_Loaded(object sender, EventArgs e)
        {
            
        }
    }
}
