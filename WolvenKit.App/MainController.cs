using System;
using System.IO;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WolvenKit.Common.Services;

namespace WolvenKit.App
{
    using Bundles;
    using Cache;
    using CR2W;
    using W3Strings;
    using Common;

    public class MainController : ILocalizedStringSource, INotifyPropertyChanged
    {
        private static MainController mainController;
        public Configuration Configuration { get; private set; }
        public W3Mod ActiveMod { get; set; }

        public const string ManagerCacheDir = "ManagerCache";
        public string VLCLibDir = "C:\\Program Files\\VideoLAN\\VLC";
        public string InitialModProject = "";
        public string InitialWKP = "";

        public event PropertyChangedEventHandler PropertyChanged;

        private string _projectstatus = "Idle";
        public string ProjectStatus
        {
            get => _projectstatus;
            set => SetField(ref _projectstatus, value, nameof(ProjectStatus));
        }

        private string _loadstatus = "Loading...";
        public string loadStatus
        {
            get => _loadstatus;
            set => SetField(ref _loadstatus, value, nameof(loadStatus));
        }

        private bool _loaded = false;
        public bool Loaded
        {
            get => _loaded;
            set => SetField(ref _loaded, value, nameof(Loaded));
        }


        private KeyValuePair<string, Logtype> _logMessage = new KeyValuePair<string, Logtype>("", Logtype.Normal);
        public KeyValuePair<string, Logtype> LogMessage
        {
            get => _logMessage;
            set => SetField(ref _logMessage, value, nameof(LogMessage));
        }

        /// <summary>
        /// Shows wheteher there are unsaved changes in the project.
        /// </summary>
        public bool ProjectUnsaved = false;

        private MainController() { }

        #region Archive Managers
        private SoundManager soundManager;
        private SoundManager modsoundmanager;
        private BundleManager bundleManager;
        private BundleManager modbundleManager;
        private TextureManager textureManager;
        private CollisionManager collisionManager;
        private TextureManager modTextureManager;
        private W3StringManager w3StringManager;

        //Public getters
        public W3StringManager W3StringManager => w3StringManager;
        public BundleManager BundleManager => bundleManager;
        public BundleManager ModBundleManager => modbundleManager;
        public SoundManager SoundManager => soundManager;
        public SoundManager ModSoundManager => modsoundmanager;
        public TextureManager TextureManager => textureManager;
        public TextureManager ModTextureManager => modTextureManager;
        public CollisionManager CollisionManager => collisionManager;

        #endregion

        public string GetLocalizedString(uint val)
        {
            return W3StringManager.GetString(val);
        }

        public static MainController Get()
        {
            if (mainController == null)
            {
                mainController = new MainController();
                mainController.Configuration = Configuration.Load();
            }
            return mainController;
        }

        /// <summary>
        /// Initializes the archive managers in an async thread
        /// </summary>
        /// <returns></returns>
        public async Task Initialize()
        {
            try
            {
                loadStatus = "Loading string manager";
                #region Load string manager
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                if (w3StringManager == null)
                {
                    try
                    {
                        if (File.Exists(Path.Combine(ManagerCacheDir, "string_cache.bin")) && new FileInfo(Path.Combine(ManagerCacheDir, "string_cache.bin")).Length > 0)
                        {
                            using (var file = File.Open(Path.Combine(ManagerCacheDir, "string_cache.bin"), FileMode.Open))
                            {
                                w3StringManager = ProtoBuf.Serializer.Deserialize<W3StringManager>(file);
                            }
                        }
                        else
                        {
                            w3StringManager = new W3StringManager();
                            w3StringManager.Load(Configuration.TextLanguage, Path.GetDirectoryName(Configuration.ExecutablePath));
                            Directory.CreateDirectory(ManagerCacheDir);
                            using (var file = File.Open(Path.Combine(ManagerCacheDir, "string_cache.bin"), FileMode.Create))
                            {
                                ProtoBuf.Serializer.Serialize(file, w3StringManager);
                            }
                        }
                    }
                    catch (System.Exception)
                    {
                        if (File.Exists(Path.Combine(ManagerCacheDir, "string_cache.bin")))
                            File.Delete(Path.Combine(ManagerCacheDir, "string_cache.bin"));
                        w3StringManager = new W3StringManager();
                        w3StringManager.Load(Configuration.TextLanguage, Path.GetDirectoryName(Configuration.ExecutablePath));
                    }
                }

                var i = sw.ElapsedMilliseconds;
                sw.Stop();
                #endregion

                loadStatus = "Loading bundle manager!";
                #region Load bundle manager
                if (bundleManager == null)
                {
                    try
                    {
                        if (File.Exists(Path.Combine(ManagerCacheDir, "bundle_cache.json")))
                        {
                            using (StreamReader file = File.OpenText(Path.Combine(ManagerCacheDir, "bundle_cache.json")))
                            {
                                JsonSerializer serializer = new JsonSerializer();
                                serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                                serializer.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                                serializer.TypeNameHandling = TypeNameHandling.Auto;
                                bundleManager = (BundleManager)serializer.Deserialize(file, typeof(BundleManager));
                            }
                        }
                        else
                        {
                            bundleManager = new BundleManager();
                            bundleManager.LoadAll(Path.GetDirectoryName(Configuration.ExecutablePath));
                            File.WriteAllText(Path.Combine(ManagerCacheDir, "bundle_cache.json"), JsonConvert.SerializeObject(bundleManager, Formatting.None, new JsonSerializerSettings()
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                                TypeNameHandling = TypeNameHandling.Auto
                            }));
                        }
                    }
                    catch (System.Exception)
                    {
                        if (File.Exists(Path.Combine(ManagerCacheDir, "bundle_cache.json")))
                            File.Delete(Path.Combine(ManagerCacheDir, "bundle_cache.json"));
                        bundleManager = new BundleManager();
                        bundleManager.LoadAll(Path.GetDirectoryName(Configuration.ExecutablePath));
                    }
                }
                #endregion
                loadStatus = "Loading mod bundle manager!";
                #region Load mod bundle manager
                if (modbundleManager == null)
                {
                    modbundleManager = new BundleManager();
                    modbundleManager.LoadModsBundles(Path.GetDirectoryName(Configuration.ExecutablePath));
                }
                #endregion

                loadStatus = "Loading texture manager!";
                #region Load texture manager
                if (textureManager == null)
                {
                    try
                    {
                        if (File.Exists(Path.Combine(ManagerCacheDir, "texture_cache.json")))
                        {
                            using (StreamReader file = File.OpenText(Path.Combine(ManagerCacheDir, "texture_cache.json")))
                            {
                                JsonSerializer serializer = new JsonSerializer();
                                serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                                serializer.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                                serializer.TypeNameHandling = TypeNameHandling.Auto;
                                textureManager = (TextureManager)serializer.Deserialize(file, typeof(TextureManager));
                            }
                        }
                        else
                        {
                            textureManager = new TextureManager();
                            textureManager.LoadAll(Path.GetDirectoryName(Configuration.ExecutablePath));
                            File.WriteAllText(Path.Combine(ManagerCacheDir, "texture_cache.json"), JsonConvert.SerializeObject(textureManager, Formatting.None, new JsonSerializerSettings()
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                                TypeNameHandling = TypeNameHandling.Auto
                            }));
                        }
                    }
                    catch (System.Exception)
                    {
                        if (File.Exists(Path.Combine(ManagerCacheDir, "texture_cache.json")))
                            File.Delete(Path.Combine(ManagerCacheDir, "texture_cache.json"));
                        textureManager = new TextureManager();
                        textureManager.LoadAll(Path.GetDirectoryName(Configuration.ExecutablePath));
                    }
                }
                #endregion

                loadStatus = "Loading collision manager!";
                #region Load collision manager
                if (collisionManager == null)
                {
                    try
                    {
                        if (File.Exists(Path.Combine(ManagerCacheDir, "collision_cache.json")))
                        {
                            using (StreamReader file = File.OpenText(Path.Combine(ManagerCacheDir, "collision_cache.json")))
                            {
                                JsonSerializer serializer = new JsonSerializer();
                                serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                                serializer.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                                serializer.TypeNameHandling = TypeNameHandling.Auto;
                                collisionManager = (CollisionManager)serializer.Deserialize(file, typeof(CollisionManager));
                            }
                        }
                        else
                        {
                            collisionManager = new CollisionManager();
                            collisionManager.LoadAll(Path.GetDirectoryName(Configuration.ExecutablePath));
                            File.WriteAllText(Path.Combine(ManagerCacheDir, "collision_cache.json"), JsonConvert.SerializeObject(collisionManager, Formatting.None, new JsonSerializerSettings()
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                                TypeNameHandling = TypeNameHandling.Auto
                            }));
                        }
                    }
                    catch (System.Exception)
                    {
                        if (File.Exists(Path.Combine(ManagerCacheDir, "collision_cache.json")))
                            File.Delete(Path.Combine(ManagerCacheDir, "collision_cache.json"));
                        collisionManager = new CollisionManager();
                        collisionManager.LoadAll(Path.GetDirectoryName(Configuration.ExecutablePath));
                    }
                }
                #endregion

                loadStatus = "Loading mod texure manager!";
                #region Load mod texture manager
                if (modTextureManager == null)
                {
                    modTextureManager = new TextureManager();
                    modTextureManager.LoadModsBundles(Path.GetDirectoryName(Configuration.ExecutablePath));
                }
                #endregion

                loadStatus = "Loading sound manager!";
                #region Load sound manager
                if (soundManager == null)
                {
                    try
                    {
                        if (File.Exists(Path.Combine(ManagerCacheDir, "sound_cache.json")))
                        {
                            using (StreamReader file = File.OpenText(Path.Combine(ManagerCacheDir, "sound_cache.json")))
                            {
                                JsonSerializer serializer = new JsonSerializer();
                                serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                                serializer.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                                serializer.TypeNameHandling = TypeNameHandling.Auto;
                                soundManager = (SoundManager)serializer.Deserialize(file, typeof(SoundManager));
                            }
                        }
                        else
                        {
                            soundManager = new SoundManager();
                            soundManager.LoadAll(Path.GetDirectoryName(Configuration.ExecutablePath));
                            File.WriteAllText(Path.Combine(ManagerCacheDir, "sound_cache.json"), JsonConvert.SerializeObject(soundManager, Formatting.None, new JsonSerializerSettings()
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                                TypeNameHandling = TypeNameHandling.Auto
                            }));
                        }
                    }
                    catch (System.Exception)
                    {
                        if (File.Exists(Path.Combine(ManagerCacheDir, "sound_cache.json")))
                            File.Delete(Path.Combine(ManagerCacheDir, "sound_cache.json"));
                        soundManager = new SoundManager();
                        soundManager.LoadAll(Path.GetDirectoryName(Configuration.ExecutablePath));
                    }
                }
                #endregion
                loadStatus = "Loading mod sound manager!";
                #region Load mod sound manager
                if (modsoundmanager == null)
                {
                    modsoundmanager = new SoundManager();
                    modsoundmanager.LoadModsBundles(Path.GetDirectoryName(Configuration.ExecutablePath));
                }
                #endregion
                loadStatus = "Loaded";

                mainController.Loaded = true;
            }
            catch (Exception e)
            {
                mainController.Loaded = false;
                Console.WriteLine(e.Message);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public List<IWitcherArchive> GetArchives()
        {
            var managers = new List<IWitcherArchive>();
            if (BundleManager != null) managers.Add(BundleManager);
            if (SoundManager != null) managers.Add(SoundManager);
            if (TextureManager != null) managers.Add(TextureManager);
            if (CollisionManager != null) managers.Add(CollisionManager);
            return managers;
        }

        public List<IWitcherArchive> GetModArchives()
        {
            var modmanagers = new List<IWitcherArchive>();
            if (ModBundleManager != null) modmanagers.Add(ModBundleManager);
            if (ModSoundManager != null) modmanagers.Add(ModSoundManager);
            if (ModTextureManager != null) modmanagers.Add(ModTextureManager);
            //if (ModCollisionManager != null) managers.Add(ModCollisionManager);
            return modmanagers;
        }
    }
}