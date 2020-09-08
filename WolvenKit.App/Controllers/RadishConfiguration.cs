﻿using Catel.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Xml.Serialization;
using WolvenKit.Common;
using WolvenKit.Radish.Model;

namespace WolvenKit.App
{
    public class RadishConfiguration : ObservableObject
    {
        public RadishConfiguration()
        {
            
        }
        public static string ConfigurationPath
        {
            get
            {
                var path = AppDomain.CurrentDomain.BaseDirectory;
                var filename = Path.GetFileNameWithoutExtension(path);
                var dir = Path.GetDirectoryName(path);
                return Path.Combine(dir ?? "", filename + "_radishconfig.xml");
            }
        }


        [Browsable(false)]
        public List<RadishWorkflow> Workflows { get; set; } = new List<RadishWorkflow>();
        [Browsable(false)]
        public string RadishProjectPath { get; set; }

        //
        private string _oldmodname;
        public string GetOldModname() => _oldmodname;
        #region modname
        private string _modname;
        public string modname
        {
            get => _modname;
            set
            {
                if (_modname != value)
                {
                    _oldmodname = _modname;
                    _modname = value;
                    RaisePropertyChanged(() => modname, _oldmodname, value);
                }
            }
        }
        #endregion
        #region idspace
        private string _idspace;
        public string idspace
        {
            get => _idspace;
            set
            {
                if (_idspace != value)
                {
                    var oldValue = _idspace;
                    _idspace = value;
                    RaisePropertyChanged(() => idspace, oldValue, value);
                }
            }
        }
        #endregion
        #region DIR_ENCODER
        private string _DIR_ENCODER;
        public string DIR_ENCODER
        {
            get => _DIR_ENCODER;
            set
            {
                if (_DIR_ENCODER != value)
                {
                    var oldValue = _DIR_ENCODER;
                    _DIR_ENCODER = value;
                    RaisePropertyChanged(() => DIR_ENCODER, oldValue, value);
                }
            }
        }
        #endregion
        #region DIR_MODKIT
        private string _DIR_MODKIT;
        public string DIR_MODKIT
        {
            get => _DIR_MODKIT;
            set
            {
                if (_DIR_MODKIT != value)
                {
                    var oldValue = _DIR_MODKIT;
                    _DIR_MODKIT = value;
                    RaisePropertyChanged(() => DIR_MODKIT, oldValue, value);
                }
            }
        }
        #endregion
        #region DIR_W3
        private string _DIR_W3;
        public string DIR_W3
        {
            get => _DIR_W3;
            set
            {
                if (_DIR_W3 != value)
                {
                    var oldValue = _DIR_W3;
                    _DIR_W3 = value;
                    RaisePropertyChanged(() => DIR_W3, oldValue, value);
                }
            }
        }
        #endregion

        public void Save()
        {
            var ser = new XmlSerializer(typeof(RadishConfiguration));
            var stream = new FileStream(RadishConfiguration.ConfigurationPath, FileMode.Create, FileAccess.Write);
            ser.Serialize(stream, this);
            stream.Close();
        }

        public static RadishConfiguration Load()
        {
            if (File.Exists(ConfigurationPath) && new FileInfo(ConfigurationPath).Length != 0)
            {
                var ser = new XmlSerializer(typeof(RadishConfiguration));
                var stream = new FileStream(ConfigurationPath, FileMode.Open, FileAccess.Read);
                var config = (RadishConfiguration)ser.Deserialize(stream);
                stream.Close();
                return config;
            }

            // Defaults
            return new RadishConfiguration
            {

            };

        }






    }
}
