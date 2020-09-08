using Catel.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WolvenKit.Common;
using WolvenKit.Common.Wcc;

namespace WolvenKit.App.Model
{
    public class XBMDump
    {
        public string RedName { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string Format { get; set; }
        public string Compression { get; set; }
        public string TextureGroup { get; set; }

    }

    public class ImportableFile : ObservableObject
    {
        public enum EObjectState
        {
            NoTextureGroup, //Orange
            Ready,  //Green
            Error //Read
        }

        #region IsSelected
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    var oldValue = _isSelected;
                    _isSelected = value;
                    RaisePropertyChanged(() => IsSelected, oldValue, value);
                }
            }
        }
        #endregion

        public string Name { get => Path.GetFileName(GetRelativePath()); }

        #region Texturegroup
        private ETextureGroup _textureGroup;
        public ETextureGroup TextureGroup
        {
            get => _textureGroup;
            set
            {
                if (_textureGroup != value)
                {
                    var oldValue = _textureGroup;
                    _textureGroup = value;
                    RaisePropertyChanged(() => TextureGroup, oldValue, value);
                }
            }
        }
        #endregion

        #region Texturegroup
        private Enum _importType;
        public Enum ImportType
        {
            get => _importType;
            set
            {
                if (_importType != value)
                {
                    var oldValue = _importType;
                    _importType = value;
                    RaisePropertyChanged(() => ImportType, oldValue, value);
                }
            }
        }
        #endregion


        private readonly string _relativePath;
        
        private readonly EImportable _type;

        public string GetRelativePath() => _relativePath;
        public EImportable GetImportableType() => _type;

        private EObjectState _state;
        public EObjectState State
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    var oldValue = _state;
                    _state = value;
                    RaisePropertyChanged(() => State, oldValue, value);
                }
            }
        }


        //public EObjectState GetState() => _state;
        //public void SetState(EObjectState value)
        //{
        //    if (_state != value)
        //    {
        //        var oldValue = _state;
        //        _state = value;
        //        RaisePropertyChanged(() => SetState, oldValue, value);
        //    }
        //}

        public ImportableFile(string path, EImportable type, Enum importtype)
        {
            _relativePath = path;
            _type = type;
            _importType = importtype;
        }

        

    }
}
