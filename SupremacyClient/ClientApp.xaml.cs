// ClientApp.xaml.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

// What are "Practices.Unity" and "Practices.Composite"?
//      - Looks like this application uses
//          - (old?) PRISM guidelines (Composite Application Library),
//          - Unity for dependency injection
//          - Bootstrapper for the initialization of an application built using the Composite Application Library
//
//      - StackOverflow             https://stackoverflow.com/questions/6273357/what-is-prism-for-wpf
//      - Article:                  https://visualstudiomagazine.com/articles/2012/07/01/creating-modularity-with-wpf-prism-and-unity.aspx
//      - Article:                  https://visualstudiomagazine.com/articles/2013/04/01/unity-vs-mef.aspx
//      - Original docu (Composite):https://msdn.microsoft.com/en-us/library/ff647752.aspx
//      - New MS docu(Composite):   https://msdn.microsoft.com/en-us/library/ff921125
//      - MS docu Unity:            https://msdn.microsoft.com/en-us/library/dn507457(v=pandp.30).aspx
//      - MS docu Unity:            https://msdn.microsoft.com/en-us/library/ff647202.aspx
//      - DI pattern                https://en.wikipedia.org/wiki/Dependency_injection#Examples
//      - Bootstrapper              https://msdn.microsoft.com/en-us/library/ff921139.aspx
//      - Tutorial:                 https://www.codeproject.com/Articles/37164/Introduction-to-Composite-WPF-CAL-Prism-Part
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.Presentation.Regions;
using Microsoft.Practices.Composite.UnityExtensions;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.Win32;
using Supremacy.Annotations;
using Supremacy.Client.Audio;
using Supremacy.Client.Commands;
using Supremacy.Client.Context;
using Supremacy.Client.Services;
using Supremacy.Resources;
using Supremacy.Utility;
using Supremacy.VFS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Xceed.Wpf.DataGrid;
using Scheduler = System.Concurrency.Scheduler;

namespace Supremacy.Client
{
    /// <summary>
    /// Interaction logic for MyApp.xaml
    /// </summary>
    public partial class ClientApp : IClientApplication
    {
        #region Fields
        private static readonly ClientCommandLineArguments CmdLineArgs = new ClientCommandLineArguments();
        private static readonly DispatcherOperationCallback ExitFrameCallback = ExitFrame;

        private static SplashScreen _splashScreen;
        private static Mutex _singleInstanceMutex;
        
        private bool _isShuttingDown;
        #endregion

        #region Constructors
        public ClientApp()
        {
        }
        #endregion

        #region Properties and Indexers
        public new static ClientApp Current
        {
            get { return (ClientApp)Application.Current; }
        }

        public static Version ClientVersion
        {
            get
            {
                GameLog.Client.General.InfoFormat("Current Version = {0}", Current.Version);
                return Current.Version;
            }
        }

        public Version Version
        {
            get { return Assembly.GetEntryAssembly().GetName().Version; }
        }

        public bool IsShuttingDown
        {
            get { return _isShuttingDown; }
        }

        public IClientCommandLineArguments CommandLineArguments
        {
            get { return CmdLineArgs; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Processes all UI messages currently in the message queue.
        /// </summary>
        public void DoEvents()
        {
            // Create new nested message pump.
            var nestedFrame = new DispatcherFrame();

            // Dispatch a callback to the current message queue, when getting called, 
            // this callback will end the nested message loop.
            // The priority of this callback should be lower than the that of UI event messages.
            DispatcherOperation exitOperation = Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background,
                ExitFrameCallback,
                nestedFrame);

            // Pump the nested message loop.  The nested message loop will 
            // immediately process the messages left inside the message queue.
            Dispatcher.PushFrame(nestedFrame);

            // If the "ExitFrame" callback doesn't get finished, Abort it.
            if (exitOperation.Status != DispatcherOperationStatus.Completed)
            {
                exitOperation.Abort();
            }
        }

        protected void LoadBaseResources()
        {
            if (Current.IsShuttingDown)
                return;

            try
            {
                Current.Resources = LoadComponent(
                                    new Uri(
                                        "/SupremacyClient;Component/themes/Default.xaml",
                                        UriKind.RelativeOrAbsolute))
                                as ResourceDictionary;
            }
            catch
            {
                GameLog.Client.GameData.DebugFormat("Current.Resources = LoadComponent");
            }
        }

        public bool LoadDefaultResources()
        {
            if (Current.IsShuttingDown)
                return false;

            ResourceDictionary themeDictionary = null;
            try
            {
                var themeUri = new Uri(
                    "/SupremacyClient;Component/themes/Generic/Theme.xaml",
                    UriKind.RelativeOrAbsolute);
                themeDictionary = LoadComponent(themeUri) as ResourceDictionary;
            }
            catch
            {
                GameLog.Client.GameData.DebugFormat("var themeUri = new Uri");
            }

            if (themeDictionary == null)
                return false;

            LoadBaseResources();
            if (Current.Resources == null)
                return false;

            Current.Resources.MergedDictionaries.Add(themeDictionary);
            return true;
        }

        public bool LoadThemeResources(string theme)
        {
            if (Current.IsShuttingDown)
                return false;

            // individual UI
            var themeUri = new Uri(
                string.Format(
                    "/SupremacyClient;Component/themes/{0}/Theme.xaml",
                    theme),
                UriKind.RelativeOrAbsolute);
            
            ResourceDictionary themeDictionary = null;
            try
            {
                themeDictionary = LoadComponent(themeUri) as ResourceDictionary;
            }
            catch
            {
                GameLog.Client.General.Debug("themeDictionary = LoadComponent(themeUri) as ResourceDictionary;");
            }

            if (themeDictionary == null)
                return false;

            LoadBaseResources();
            if (Current.Resources == null)
                return false;

            Current.Resources.MergedDictionaries.Add(themeDictionary);
            return true;
        }

        // ToDo: Currently this function only prints "theme" data, but it could be made globaly accessible
        //       if need arise to trace some other XAML files and ResourceDirecotries 
        private void printXAMLdata(ResourceDictionary resourceDirectory, Uri parentUri, int level = 1)
        {
            // How to enumerate loaded images with absolute paths
            // https://docs.microsoft.com/en-us/windows/uwp/controls-and-patterns/resourcedictionary-and-xaml-resource-references
            // https://msdn.microsoft.com/en-us/library/system.windows.resourcedictionary(v=vs.110).aspx
            // https://stackoverflow.com/questions/3783620/how-would-i-access-this-wpf-xaml-resource-programmatically
            // https://stackoverflow.com/questions/2631604/get-absolute-file-path-from-image-in-wpf
            // https://msdn.microsoft.com/en-us/library/aa350178(v=vs.110).aspx

            // https://stackoverflow.com/questions/6369184/print-the-source-filename-and-linenumber-in-c-sharp
            // https://stackoverflow.com/questions/171970/how-can-i-find-the-method-that-called-the-current-method
            // http://www.csharp-examples.net/reflection-calling-method-name/

            string currentSource = "NOT SET";
            if (resourceDirectory.Source != null)
            {
                currentSource = resourceDirectory.Source.ToString();
            }

            GameLog.Client.GameData.DebugFormat("");
            GameLog.Client.GameData.DebugFormat("");
            GameLog.Client.GameData.DebugFormat("////////////////////////////// XAML TRACE //////////////////////////////");  
            GameLog.Client.GameData.DebugFormat(" Current source: {0}", currentSource);
            GameLog.Client.GameData.DebugFormat(" Root source:    {0}", parentUri.ToString());
            GameLog.Client.GameData.DebugFormat("");

            // Iterate and print all key/data pairs
            foreach (DictionaryEntry entry in resourceDirectory)
            {
                GameLog.Client.GameData.DebugFormat(" Key:   {0}", entry.Key);
                GameLog.Client.GameData.DebugFormat(" Value: {0}", entry.Value);
                GameLog.Client.GameData.DebugFormat("");

                if (entry.Value.GetType() == typeof(Style))
                {
                    var style = (Style)entry.Value;

                    foreach (DictionaryEntry resourceEntry in style.Resources)
                    {
                        GameLog.Client.GameData.DebugFormat(" Resource data, Key:   {0}", resourceEntry.Key);
                        GameLog.Client.GameData.DebugFormat(" Resource data, Value: {0}", resourceEntry.Value);
                        GameLog.Client.GameData.DebugFormat("");
                    }
                }
            }

            // Iterate merged directories for current directory if any, 
            // and recursively print all their data 
            foreach (ResourceDictionary childResDirectory in resourceDirectory.MergedDictionaries)
            {
                printXAMLdata(childResDirectory, parentUri, level++);
            }
        }

        public bool LoadThemeResourcesShipyard(string themeShipyard)
        {
            if (Current.IsShuttingDown)
                return false;

            // individual UI
            var themeUriShipyard = new Uri(
                string.Format(
                    "/SupremacyClientComponents;Component/Themes/{0}/ShipyardDockView.xaml",
                    themeShipyard),
                UriKind.RelativeOrAbsolute);

            ResourceDictionary themeDictionaryShipyard = null;
            try
            {
                themeDictionaryShipyard = LoadComponent(themeUriShipyard) as ResourceDictionary;
            }
            catch
            {
                GameLog.Client.GameData.DebugFormat("themeDictionaryShipyard = LoadComponent(themeUriShipyard)");
            }

            if (themeDictionaryShipyard == null)
                return false;

            Current.Resources.MergedDictionaries.Add(themeDictionaryShipyard);

            return true;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _isShuttingDown = true;
            if (!CmdLineArgs.AllowMultipleInstances)
            {
                _singleInstanceMutex.ReleaseMutex();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var schedulerDispatcher = Scheduler.Dispatcher.Dispatcher;
            if (schedulerDispatcher != Dispatcher)
                throw new InvalidOperationException("DispatcherScheduler is not bound to the main application Dispatcher.");

            Licenser.LicenseKey = "DGF20-AUTJ7-3K8MD-DNNA";

            Timeline.DesiredFrameRateProperty.OverrideMetadata(
               typeof(Timeline),
               new FrameworkPropertyMetadata(ClientSettings.Current.DesiredAnimationFrameRate));

            LoadDefaultResources();

            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }

        private static void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            ServiceLocator.Current.GetInstance<IUnhandledExceptionHandler>().HandleError(e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ServiceLocator.Current.GetInstance<IUnhandledExceptionHandler>().HandleError((Exception)e.ExceptionObject);
        }

        private static object ExitFrame(object state)
        {
            // Exit the nested message loop.
            var frame = state as DispatcherFrame;
            if (frame != null)
            {
                frame.Continue = false;
            }
            return null;
        }
        #endregion

        [UsedImplicitly]
        private static class EntryPoint
        {
            [STAThread, UsedImplicitly]
            private static void Main(string[] args)
            {
                GameLog.Initialize();

                //Add dll subdirectories to current process PATH variable
                var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + appDir + "\\lib");

                if (!CheckNetFxVersion())
                {
                    MessageBox.Show(
                            "Birth of the Federation 2 requires Microsoft .NET Framework 4.6.2 or greater"
                            + Environment.NewLine
                            + "It must be installed before running the game.",
                            "Birth of the Federation 2",
                            MessageBoxButton.OK,
                            MessageBoxImage.Hand);
                    return;
                }

                try
                {
                    ShowSplashScreen();

                    var _soundfileSplashScreen = "Resources\\SoundFX\\Menu\\LoadingSplash.ogg";
                    if (File.Exists(_soundfileSplashScreen))
                    {
                        GameLog.Client.General.Debug("Playing LoadingSplash.ogg");
                        System.Media.SoundPlayer player = new System.Media.SoundPlayer(_soundfileSplashScreen);
                        player.Play();
                    }
                      
                    if (File.Exists("Resources\\Data\\Civilizations.xml"))
                        StartClient(args);
                    else
                        MessageBox.Show("Resources\\Data\\Civilizations.xml is missing" + Environment.NewLine + Environment.NewLine + 
                            "Make sure you have the folder \\Resources !!" + Environment.NewLine + "(only delivered within an original game release)","WARNING",
                            MessageBoxButton.OK);    
                }
                catch (Exception e)
                {
                    while (e.InnerException != null)
                        e = e.InnerException;
                    throw e;
                }
            }

        }

        private static bool CheckNetFxVersion()
        {
            //Adapted from https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed#net_d
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
            int version;

            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
            {
                if ((ndpKey == null) || (ndpKey.GetValue("Release") == null))
                {
                    return false;
                }

                version = (int)ndpKey.GetValue("Release");
            }

            if (version >= 461808) {
                GameLog.Client.General.Info(".NET Framework 4.7.2 or later found");
            } else if (version >= 461308) {
                GameLog.Client.General.Info(".NET Framework 4.7.1 found");
            } else if (version >= 460798) {
                GameLog.Client.General.Info(".NET Framework 4.7 found");
            } else if (version >= 394802) {
                GameLog.Client.General.Info(".NET Framework 4.6.2 found");
            } else if (version >= 394254) {
                GameLog.Client.General.Info(".NET Framework 4.6.1 found");
            } else if (version >= 393295) {
                GameLog.Client.General.Info(".NET Framework 4.6 found");
            } else if (version >= 379893) {
                GameLog.Client.General.Info(".NET Framework 4.5.2 found");
            } else if (version >= 378675) {
                GameLog.Client.General.Info(".NET Framework 4.5.1 found");
            } else if (version >= 378389) {
                GameLog.Client.General.Info(".NET Framework 4.5 found");
            }
            else
            {
                GameLog.Client.General.Info(".NET Framework is less than 4.5");
            }

            return version >= 394802;
        }

        private static void StartClient(string[] args)
        {
            ClientCommandLineArguments.Parse(CmdLineArgs, args);

            /* If an instance of the game is already running, then exit. */
            if (!CmdLineArgs.AllowMultipleInstances)
            {
                bool mutexIsNew;
                _singleInstanceMutex = new Mutex(true, "{CC4FD558-0934-451d-A387-738B5DB5619C}", out mutexIsNew);
                if (!mutexIsNew)
                {
                    MessageBox.Show("An instance of Supremacy is already running.");
                    Environment.Exit(Environment.ExitCode);
                }
            }

            if (!string.IsNullOrWhiteSpace(CmdLineArgs.Traces)) {
                List<string> traces = CmdLineArgs.Traces.Split(',').ToList();
                traces.ForEach(t =>
                    GameLog.SetRepositoryToDebug(t)
                );
            }

            if (CmdLineArgs.TraceLevel != PresentationTraceLevel.None)
                PresentationTraceSources.Refresh();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException; 

            try
            {
                var errorFile = File.Open("Error.txt", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                Console.SetError(new StreamWriter(errorFile));
            }
            catch
            {
                MessageBox.Show(
                    "The error log could not be created.  You may still run the game,\n"
                    + "but error details cannot be logged.",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            VfsWebRequestFactory.EnsureRegistered();

            var app = new ClientApp();
            app.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            app.InitializeComponent();
            app.Run();
        }

        private static void ShowSplashScreen()
        {
            _splashScreen = new SplashScreen("resources/images/backgrounds/splash.png");
            _splashScreen.Show(false);
        }

        private static void OnGameWindowSourceInitialized(object sender, EventArgs e)
        {
            ((Window)sender).SourceInitialized -= OnGameWindowSourceInitialized;
            _splashScreen.Close(TimeSpan.Zero);
        }

#region Bootstrapper Class
        private class Bootstrapper : UnityBootstrapper
        {
            // What is Unity Bootstrapper? --> https://msdn.microsoft.com/en-us/library/ff921139.aspx

            private ClientWindow _shell;

            protected override IModuleCatalog GetModuleCatalog()
            {
                return new ConfigurationModuleCatalog().AddModule(ClientModule.ModuleName, typeof(ClientModule).AssemblyQualifiedName);
            }

            protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
            {
                var baseMappings = base.ConfigureRegionAdapterMappings();
                baseMappings.RegisterMapping(
                    typeof(GameScreenStack),
                    Container.Resolve<GameScreenStackRegionAdapter>());
                return baseMappings;
            }

            protected override void ConfigureContainer()
            {
                base.ConfigureContainer();
                Container.RegisterInstance(ResourceManager.VfsService, new ContainerControlledLifetimeManager());
                Container.RegisterInstance<IApplicationSettingsService>(new ApplicationSettingsService(), new ContainerControlledLifetimeManager());
                Container.RegisterInstance<IClientApplication>(Current, new ContainerControlledLifetimeManager());
                Container.RegisterInstance<IDispatcherService>(new DefaultDispatcherService(Dispatcher.CurrentDispatcher), new ContainerControlledLifetimeManager());
                Container.RegisterType<IUnhandledExceptionHandler, ClientUnhandledExceptionHandler>(new ContainerControlledLifetimeManager());
                Container.RegisterType<INavigationCommandsProxy, NavigationCommandsProxy>(new ContainerControlledLifetimeManager());
                Container.RegisterType<IAppContext, Context.AppContext>(new ContainerControlledLifetimeManager());
                Container.RegisterType<IResourceManager, ClientResourceManager>(new ContainerControlledLifetimeManager());
                Container.RegisterType<IGameErrorService, GameErrorService>(new ContainerControlledLifetimeManager());
                Container.RegisterInstance<IAudioEngine>(FMODAudioEngine.Instance, new ContainerControlledLifetimeManager());
                Container.RegisterType<IMusicPlayer, MusicPlayer>(new ContainerControlledLifetimeManager());
                Container.RegisterType<ISoundPlayer, SoundPlayer>(new ContainerControlledLifetimeManager());
            }

            protected override DependencyObject CreateShell()
            {
                if (_shell != null)
                    return _shell;
                _shell = Container.Resolve<ClientWindow>();
                _shell.SourceInitialized += OnGameWindowSourceInitialized;
                Application.Current.MainWindow = _shell;
                _shell.Show();
                Container.RegisterInstance<IGameWindow>(_shell, new ContainerControlledLifetimeManager());
                return _shell;
            }
        }
#endregion
    }
}
 