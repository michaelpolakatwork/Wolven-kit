using Catel.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WolvenKit.App.Commands;
using WolvenKit.Common;

namespace WolvenKit.App.Model
{
    public class SearchParameters : ObservableObject
    {


        public SearchParameters()
        {
            CaseSensitiveCommand = new RelayCommand(CaseSensitive);
            WholeWordCommand = new RelayCommand(WholeWord);
            RegexCommand = new RelayCommand(Regex);
        }

        public ICommand CaseSensitiveCommand { get; }
        public ICommand WholeWordCommand { get; }
        public ICommand RegexCommand { get; }

        #region SearchText
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    RaisePropertyChanged("SearchParams");
                }
            }
        }
        #endregion
        #region SearchBundleText
        private string _searchBundleText;
        public string SearchBundleText
        {
            get => _searchBundleText;
            set
            {
                if (_searchBundleText != value)
                {
                    _searchBundleText = value;
                    RaisePropertyChanged("SearchParams");
                }
            }
        }
        #endregion
        #region SearchTypeText
        private string _searchTypeText;
        public string SearchTypeText
        {
            get => _searchTypeText;
            set
            {
                if (_searchTypeText != value)
                {
                    _searchTypeText = value;
                    RaisePropertyChanged("SearchParams");
                }
            }
        }
        #endregion
        #region IsRegex
        private bool _isRegex;
        public bool IsRegex
        {
            get => _isRegex;
            set
            {
                if (_isRegex != value)
                {
                    _isRegex = value;
                    RaisePropertyChanged("SearchParams");
                }
            }
        }
        #endregion
        #region IsWholeWord
        private bool _isWholeWord;
        public bool IsWholeWord
        {
            get => _isWholeWord;
            set
            {
                if (_isWholeWord != value)
                {
                    _isWholeWord = value;
                    RaisePropertyChanged("SearchParams");
                }
            }
        }
        #endregion
        #region IsMatchCase
        private bool _isMatchCase;
        public bool IsMatchCase
        {
            get => _isMatchCase;
            set
            {
                if (_isMatchCase != value)
                {
                    _isMatchCase = value;
                    RaisePropertyChanged("SearchParams");
                }
            }
        }
        #endregion
        #region IsCurrentFolder
        private bool _isCurrentFolder;
        public bool IsCurrentFolder
        {
            get => _isCurrentFolder;
            set
            {
                if (_isCurrentFolder != value)
                {
                    _isCurrentFolder = value;
                    RaisePropertyChanged("SearchParams");
                }
            }
        }
        #endregion
        #region IsRoot
        private bool _isRoot;
        public bool IsRoot
        {
            get => _isRoot;
            set
            {
                if (_isRoot != value)
                {
                    _isRoot = value;
                    RaisePropertyChanged("SearchParams");
                }
            }
        }
        #endregion
        #region IsAllSubfolders
        private bool _isAllSubfolders;
        public bool IsAllSubfolders
        {
            get => _isAllSubfolders;
            set
            {
                if (_isAllSubfolders != value)
                {
                    _isAllSubfolders = value;
                    RaisePropertyChanged("SearchParams");
                }
            }
        }
        #endregion

        protected void CaseSensitive()
        {
            IsMatchCase = !IsMatchCase;
        }
        protected void WholeWord()
        {
            IsWholeWord = !IsWholeWord;
        }
        protected void Regex()
        {
            IsRegex = !IsRegex;
        }



    }
}
