﻿using Catel.Data;
using System;
using System.IO;
using System.Windows.Input;
using WolvenKit.App.Commands;
using WolvenKit.App.Model;
using WolvenKit.Common.Model;

using WolvenKit.CR2W;
using WolvenKit.CR2W.SRT;

using WolvenKit.Radish.Model;

namespace WolvenKit.App.ViewModels.Documents
{
    public class DocumentViewModel : CloseableViewModel
    {

        public DocumentViewModel()
        {
            cmd1 = new RelayCommand(cm1, canrm1);

        }

        #region Fields
        public event EventHandler<FileSavedEventArgs> OnFileSaved;

        #endregion

        #region Properties
        public object SaveTarget { get; set; }

        #region File
        public IWolvenkitFile File
        {
            get => GetValue<IWolvenkitFile>(FileProperty);
            set => SetValue(FileProperty, value);
        }
        public static readonly PropertyData FileProperty = RegisterProperty(nameof(File), typeof(IWolvenkitFile));
        #endregion



        #region FileName
        public virtual string FileName => File.FileName;
        #endregion

        #region FormText
        public string FormText
        {
            get => GetValue<string>(FormTextProperty);
            set => SetValue(FormTextProperty, value);
        }
        public static readonly PropertyData FormTextProperty = RegisterProperty(nameof(FormText), typeof(string));
        #endregion

        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title { get { return "Just acknowledge"; } }

        #endregion

        #region Commands
        public ICommand cmd1 { get; }

        #endregion

        #region Commands Implementation
        protected bool canrm1() => RadishController.Get().IsHealthy();
        protected void cm1() { }


        #endregion

        #region Methods
        public void SaveFile()
        {
            if (SaveTarget == null)
            {
                saveToFileName();
            }
            else
            {
                saveToMemoryStream();
            }
        }
        private void saveToMemoryStream()
        {
            using (var mem = new MemoryStream())
            {
                using (var writer = new BinaryWriter(mem))
                {
                    File.Write(writer);

                    if (OnFileSaved != null)
                    {
                        OnFileSaved(this, new FileSavedEventArgs { FileName = FileName, Stream = mem, File = File });
                    }
                }
            }
        }

        private void saveToFileName()
        {
            //try
            //{
            using (var mem = new MemoryStream())
            using (var writer = new BinaryWriter(mem))
            {
                File.Write(writer);
                mem.Seek(0, SeekOrigin.Begin);

                using (var fs = new FileStream(FileName, FileMode.Create, FileAccess.Write))
                {
                    mem.WriteTo(fs);

                    OnFileSaved?.Invoke(this, new FileSavedEventArgs { FileName = FileName, Stream = fs, File = File });
                    fs.Close();
                }
            }
            //}
            //catch (Exception e)
            //{
            //    MainController.Get().QueueLog("Failed to save the file(s)! They are probably in use.\n" + e.ToString());
            //}
        }
        public void LoadFile(string filename, IVariableEditor variableEditor)
        {
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                loadFile(fs, filename, variableEditor);

                fs.Close();
            }
        }

        public void LoadFile(string filename, Stream stream, IVariableEditor variableEditor)
        {
            loadFile(stream, filename, variableEditor);
        }

        private void loadFile(Stream stream, string filename, IVariableEditor variableEditor)
        {
            FormText = Path.GetFileName(filename) + " [" + filename + "]";

            using (var reader = new BinaryReader(stream))
            {
                // switch between cr2wfiles and others (e.g. srt)
                if (Path.GetExtension(filename) == ".srt")

                {

                    File = new Srtfile()

                    {

                        FileName = filename

                    };

                    File.Read(reader);

                }

                else

                {

                    File = new CR2WFile(reader, MainController.Get().Logger)

                    {

                        FileName = filename,

                        EditorController = variableEditor/*UIController.Get()*/,

                        LocalizedStringSource = MainController.Get()

                    };

                }
            }
        }
        #endregion

    }
}
