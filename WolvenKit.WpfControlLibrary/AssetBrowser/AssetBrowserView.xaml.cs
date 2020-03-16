using System;
using System.Windows.Controls;
using WolvenKit.App.ViewModels;

namespace WolvenKit.WpfControlLibrary.AssetBrowser
{
    

    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class AssetBrowserControl : UserControl
    {
        public delegate void MyControlEventHandler(object sender, MyControlEventArgs args);
        public event MyControlEventHandler OnButtonClick;

        private void Init(object sender, EventArgs e)
        {

        }

        public AssetBrowserControl(IViewModel viewModel)
        {
            // TODO: inject
            this.DataContext = viewModel;
        }
    }


    public class MyControlEventArgs : EventArgs
    {
        private bool _IsOK;

        public MyControlEventArgs(bool result)
        {
            _IsOK = result;

        }

        public bool IsOK
        {
            get { return _IsOK; }
            set { _IsOK = value; }
        }
    }
}
