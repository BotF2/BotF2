// File:ClientApp.xaml.cs
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
using FMOD;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
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

        [NonSerialized]
        public static string newline = Environment.NewLine;
        public static DateTime starttime = DateTime.Now;
        public string _text;
        public string blank = " ";
        public string separator = " ;";
        public bool _gamelog_bool = true;
        public bool _ConsoleWriteline_bool = true;
        #endregion

        #region Constructors
        public ClientApp()
        {
        }
        #endregion

        #region Properties and Indexers
        public static new ClientApp Current => (ClientApp)Application.Current;

        public static Version ClientVersion
        {
            get
            {
                string _text = "Step_0213: Current Version = " + Current.Version;
                //string _text2 = "";
                if (File.Exists("SupremacyClient.exe"))
                {
                    _text += " of > SupremacyClient.exe ";
                    _text += new FileInfo("SupremacyClient.exe").Length.ToString() + " ";
                    _text += new FileInfo("SupremacyClient.exe").LastWriteTime.ToString();
                }
                //_text = "Step_0200: Current Version = " + Current.Version + _text2;
                //if (_ConsoleWriteline_bool) 
                    Console.WriteLine(_text);  // "Current Version = "
                GameLog.Client.General.InfoFormat(_text);



                
                _text = "Step_0220: Time running = " + (DateTime.Now - starttime).ToString();
                Console.WriteLine(_text);
                GameLog.Client.GeneralDetails.DebugFormat(_text);

                return Current.Version;
            }
        }

        public Version Version => Assembly.GetEntryAssembly().GetName().Version;

        public string ClientHint => ClientVersion + newline + ResourceManager.GetString("HINT_FOR_RUNNING");

        public bool IsShuttingDown { get; private set; }

        public IClientCommandLineArguments CommandLineArguments => CmdLineArgs;

        //public static string _text { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Processes all UI messages currently in the message queue.
        /// </summary>
        public void DoEvents()
        {
            // Create new nested message pump.
            DispatcherFrame nestedFrame = new DispatcherFrame();

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
                _ = exitOperation.Abort();
            }
        }

        protected void LoadBaseResources()
        {
            if (Current.IsShuttingDown)
            {
                return;
            }

            try
            {
                Current.Resources = LoadComponent(
                                    new Uri(
                                        "/SupremacyClient;Component/themes/Default.xaml",
                                        UriKind.RelativeOrAbsolute))
                                as ResourceDictionary;


                //int _dicCount = MergedDictionaries.Count;
                string _text = Environment.NewLine + "a;b;c;d;e;(Headline for Excel);g;888888" + Environment.NewLine;
                int _allValue = 0;
                string _text0 = Current.Resources.MergedDictionaries[0].Source.ToString();

                foreach (ResourceDictionary item in Current.Resources.MergedDictionaries)
                {
                    string _text1 = item.Source.ToString();
                    //Console.WriteLine(_text1);  // Output of all keys of MergedDictionaries
                    _allValue += 1000;  // 1.000 step each file

                    foreach (object key in item.Keys)
                    {
                        string _text2 = key.ToString();
                        //Console.WriteLine(_text1 + "-" + _text2);
                        _allValue += +1;
                        _text += _text0 + ";" + _text1 + ";" + _allValue + ";" + _text2 + Environment.NewLine;

                    }
                }
                //Console.WriteLine(this._text);
                //GameLog.Client.UIDetails.DebugFormat(_text);
            }
            catch
            {
                GameLog.Client.GameData.DebugFormat("Current.Resources = LoadComponent");
            }
        }

        public bool LoadDefaultResources()
        {
            if (Current.IsShuttingDown)
            {
                return false;
            }

            GameLog.Client.UI.DebugFormat("LoadDefaultResources...");
            ResourceDictionary themeDictionary = null;
            try
            {
                Uri themeUri = new Uri(
                    "/SupremacyClient;Component/themes/Generic/Theme.xaml",
                    UriKind.RelativeOrAbsolute);
                themeDictionary = LoadComponent(themeUri) as ResourceDictionary;
                GameLog.Client.UI.DebugFormat("Load...", themeDictionary.ToString());
            }
            catch
            {
                GameLog.Client.GameData.DebugFormat("var themeUri = new Uri");
            }

            if (themeDictionary == null)
            {
                return false;
            }

            LoadBaseResources();
            if (Current.Resources == null)
            {
                return false;
            }

            Current.Resources.MergedDictionaries.Add(themeDictionary);
            return true;
        }

        public bool LoadThemeResources(string theme)
        {
            if (Current.IsShuttingDown)
            {
                return false;
            }

            // individual UI
            Uri themeUri = new Uri(
                string.Format(
                    "/SupremacyClient;Component/themes/{0}/Theme.xaml",
                    theme),
                UriKind.RelativeOrAbsolute);

            ResourceDictionary themeDictionary = null;
            try
            {
                GameLog.Client.UIDetails.DebugFormat("trying to load {0}", themeUri.OriginalString);
                themeDictionary = LoadComponent(themeUri) as ResourceDictionary;
            }
            catch
            {
                GameLog.Client.General.DebugFormat("themeDictionary = LoadComponent(themeUri) as ResourceDictionary;");
            }

            if (themeDictionary == null)
            {
                return false;
            }

            LoadBaseResources();
            if (Current.Resources == null)
            {
                return false;
            }

            Current.Resources.MergedDictionaries.Add(themeDictionary);
            return true;
        }

        // ToDo: Currently this function only prints "theme" data, but it could be made globaly accessible
        //       if need arise to trace some other XAML files and ResourceDirecotries 
        private void PrintXAMLdata(ResourceDictionary resourceDirectory, Uri parentUri, int level = 1)
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
                    Style style = (Style)entry.Value;

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
                PrintXAMLdata(childResDirectory, parentUri, level++);
            }
        }

        public bool LoadThemeResourcesShipyard(string themeShipyard)
        {
            if (Current.IsShuttingDown)
            {
                return false;
            }

            // individual UI
            Uri themeUriShipyard = new Uri(
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
            {
                return false;
            }

            Current.Resources.MergedDictionaries.Add(themeDictionaryShipyard);

            return true;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            IsShuttingDown = true;
            if (!CmdLineArgs.AllowMultipleInstances)
            {
                _singleInstanceMutex.ReleaseMutex();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Dispatcher schedulerDispatcher = Scheduler.Dispatcher.Dispatcher;
            if (schedulerDispatcher != Dispatcher)
            {
                throw new InvalidOperationException("DispatcherScheduler is not bound to the main application Dispatcher.");
            }

            Licenser.LicenseKey = "DGF20-AUTJ7-3K8MD-DNNA";

            Console.WriteLine("Step_0154: Next: DesiredAnimationFrameRate and checking for local en.txt - mostly not existing e.g. 'de.txt'");   // File.IO.error next

            Timeline.DesiredFrameRateProperty.OverrideMetadata(
               typeof(Timeline),
               new FrameworkPropertyMetadata(ClientSettings.Current.DesiredAnimationFrameRate));

            _ = LoadDefaultResources();

            Bootstrapper bootstrapper = new Bootstrapper();
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
            if (state is DispatcherFrame frame)
            {
                frame.Continue = false;
            }
            return null;
        }
        #endregion

        [UsedImplicitly]
        private static class EntryPoint
        {
            //private static string _text;

            [STAThread, UsedImplicitly]
            private static void Main(string[] args)
            {
                GameLog.Initialize();

                var _text = GetTimeString(); // DateTime.Now;
                //_text = "Output_" + time.Year + "_" + time.Month + "_" + time.Day + "-" + time.Hour + "_" + time.Minute + "_" + time.Second + ".txt";
                Console.WriteLine(_text);  // "Current Version = "
                //GameLog.Client.General.InfoFormat(_text);

                //Add dll subdirectories to current process PATH variable
                string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + appDir + "\\lib");

                // Windows Version
                string subKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows NT\CurrentVersion";
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine;
                Microsoft.Win32.RegistryKey skey = key.OpenSubKey(subKey);
                string name = skey.GetValue("ProductName").ToString();
                string mjVersion = skey.GetValue("CurrentMajorVersionNumber").ToString();
                string mnVersion = skey.GetValue("CurrentMinorVersionNumber").ToString();
                string cBuild = skey.GetValue("CurrentBuild").ToString();
                string ubr = skey.GetValue("UBR").ToString();
                _text = "Step_0110: Windows Version = " + $"{name} [Version {mjVersion}.{mnVersion}.{cBuild}.{ubr}]";
                Console.WriteLine(_text);  // "Current Version = "
                //GameLog.Client.General.InfoFormat(_text);


                if (!CheckNetFxVersion())
                {
                    _ = MessageBox.Show(
                            "Rise of the UFP requires Microsoft .NET Framework 4.8 or greater"
                            + newline
                            + "It must be installed before running the game.",
                            "Rise of the UFP",
                            MessageBoxButton.OK,
                            MessageBoxImage.Hand);
                    return;
                }

                // maybe we don't need this anymore because it is shown at Start Screen as a requirement

                _text = "Step_0130: No Check for Microsoft XNA Framework V3.1 but it must be installed > see folder";
                Console.WriteLine(_text);
                //GameLog.Core.General.InfoFormat(_text);

                

                //if (!CheckXNAFramework31())
                //{
                //    _ = MessageBox.Show(
                //            "Rise of the UFP requires Microsoft XNA Framework V3.1 "
                //            + Environment.NewLine
                //            + "It must be installed before running the game.",
                //            "Rise of the UFP",
                //            MessageBoxButton.OK,
                //            MessageBoxImage.Hand);
                //    return;
                //}


                //Error - txt - First Run.txt
                string file = appDir + "\\" + "Error-txt - First Run.txt";
                string fileResourceFolder = appDir + "\\Resources\\" + "Error-txt - First Run.txt";

                // File delivered by Dropbox is more up to date
                if (File.Exists(fileResourceFolder))
                {
                    //File.Copy(fileResourceFolder, file, true);
                }
                else
                {
                    if (!File.Exists(file))
                    {
                        StreamWriter streamWriter = new StreamWriter(file);
                        streamWriter.WriteLine("IF this file doesn't exist before it is re-created out of the code ! ");
                        streamWriter.WriteLine(" ");
                        streamWriter.WriteLine("if the game crash after first start...");
                        streamWriter.WriteLine("...make sure you have the requirements installed:");
                        streamWriter.WriteLine(" ");
                        streamWriter.WriteLine("a) XNA Framework 3.1 Redistributable (make sure 3.1 ..... 4.0 or higher doesn't work)");
                        streamWriter.WriteLine("Microsoft XNA Framework 3.1 https://www.microsoft.com/en-us/download/details.aspx?id=15163");
                        streamWriter.WriteLine(" ");
                        streamWriter.WriteLine("b) is the program at a folder with write access ?");
                        streamWriter.WriteLine("- not program folders");
                        streamWriter.WriteLine("- maybe c:/User/YourUserName/RiseoftheUFP");
                        streamWriter.WriteLine(" ");
                        streamWriter.WriteLine("c) if still problems > see /Resources/FIXES FOR COMMON PROBLEMS.txt");
                        streamWriter.WriteLine(" ");
                        streamWriter.WriteLine("d) https://appuals.com/how-to-fix-folder-keeps-reverting-to-read-only-on-windows-10/ > file access in windows 11 might be controled by your security app. Try putting SupremacyClient.exe in an exclusion from read-only protection in the security app.)");
                        streamWriter.WriteLine(" ");
                        streamWriter.WriteLine("e) for more info see http://botf2.square7.ch/wiki/index.php?title=Manual#The_Game_Overview");
                        streamWriter.WriteLine(" ");

                        streamWriter.Close();
                    }
                }

                //if (!CheckXNAFramework31())   // ToDo - check for XNA Framework available ?
                //{
                //    MessageBox.Show(
                //            "Rise of the UFP requires Microsoft XNA Framework 3.1 Redistributable"
                //            + Environment.NewLine
                //            + "(make sure 3.1..... 4.0 or higher doesn't work)"
                //            + Environment.NewLine
                //            + Environment.NewLine
                //            + "It must be installed before running the game.",
                //            "Rise of the UFP",
                //            MessageBoxButton.OK,
                //            MessageBoxImage.Hand);

                //    if (File.Exists(file))  // "file" from before
                //    {
                //        ProcessStartInfo processStartInfo = new ProcessStartInfo
                //        {
                //            UseShellExecute = true,
                //            FileName = file
                //        };
                //    }
                //    return;
                //}



                try
                {
                    Console.WriteLine("Step_0132: NEXT: ShowSplashScreen...");
                    ShowSplashScreen();

                    string _soundfileSplashScreen = "Resources\\SoundFX\\Menu\\LoadingSplash.wav";


                    if (File.Exists(_soundfileSplashScreen)&&ClientSettings.Current.EnableSoundStartSplashScreen)
                    {
                        GameLog.Client.General.Debug("Step_0205: Playing LoadingSplash.wav");
                        //var soundPlayer = new SoundPlayer("Resources/SoundFX/Menu/LoadingSplash.ogg");
                        System.Media.SoundPlayer player = new System.Media.SoundPlayer(_soundfileSplashScreen);
                        if (File.Exists("Resources/SoundFX/Menu/LoadingSplash.wav"))
                        {
                            player.Play();
                        }
                        else
                        {
                            GameLog.Client.Audio.InfoFormat("Step_0133: Resources/SoundFX/Menu/LoadingSplash.wav not found...");

                            /*Important*/
                            GameLog.Client.Audio.WarnFormat("wav format is needed for several files which are played by System.Media (out of WPF = xaml files), not by the code's own sound player: ");
                            GameLog.Client.Audio.InfoFormat("wav format is needed for LoadingSplash.wav");
                            GameLog.Client.Audio.InfoFormat("wav format is needed for REDALERT.WAV");
                            GameLog.Client.Audio.InfoFormat("wav format is needed for ButtonClick.wav");
                            GameLog.Client.Audio.InfoFormat("wav format is needed for GameContextMenu.wav");
                            GameLog.Client.Audio.InfoFormat("wav format is needed for GameContextMenuItem.wav (if used)");
                        }
                    }

                    if (File.Exists("Resources\\Data\\Civilizations.xml"))
                    {
                        StartClient(args);
                    }
                    else
                    {
                        _ = MessageBox.Show("Resources\\Data\\Civilizations.xml is missing" + Environment.NewLine + Environment.NewLine +
                            "Make sure you have the folder \\Resources !!" + Environment.NewLine + "(only delivered within an original game release)", "WARNING",
                            MessageBoxButton.OK);
                    }
                }
                catch (Exception e)
                {
                    while (e.InnerException != null)
                    {
                        e = e.InnerException;
                    }
                    MessageBox.Show("Look into Log.txt for the crash");
                    throw e;
                }
            }

            private static object GetTimeString()
            {
                var time = DateTime.Now;
                string Year = time.Year.ToString(); Year = CheckDateString(Year);
                string Month = time.Month.ToString(); Month = CheckDateString(Month);
                string Day = time.Day.ToString(); Day = CheckDateString(Day);
                string Hour = time.Hour.ToString(); Hour = CheckDateString(Hour);
                string Minute = time.Minute.ToString(); Minute = CheckDateString(Minute);
                string Second = time.Second.ToString(); Second = CheckDateString(Second);
                return "Output_" + Year + "_" + Month + "_" + Day + "-" + Hour + "_" + Minute + "_" + Second + ".txt";
            }

            private static string CheckDateString(string _string)
            {
                if (_string.Length == 1)
                    _string = "0" + _string; 

                return _string;
            }
        }

#pragma warning disable IDE0051 // Remove unused private members
        private static bool CheckXNAFramework31()
#pragma warning restore IDE0051 // Remove unused private members
        {
            string _text;

            //RegRead HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\XNA\Framework\v3.1

            bool found = false;

            //string dxsetup = "%programfiles(x86)%\\Microsoft XNA\\XNA Game Studio\\v3.1\\Redist\\DX Redist\\DXSETUP.exe";
            //string dxsetup = "_check_XNA31.bat";
            //if (File.Exists("_check_XNA31.bat"))
            //{
            //    GameLog.Client.General.Info("for XNA 3.1: trying to copy into XNA31_ok.info from C:\\Program Files (x86)\\Microsoft XNA\\XNA Game Studio\\v3.1\\Redist\\DX Redist\\DXSETUP.exe");
            //    Process.Start(dxsetup);
            //}
            //_workingDirectory = PathHelper.GetWorkingDirectory();

            string xna_check = PathHelper.GetWorkingDirectory() + "\\Resources\\XNA31_ok.info";
            string xna_copy = PathHelper.GetWorkingDirectory() + "\\Resources\\XNA31_check.bat";

            if (File.Exists(xna_check))
            {
                _text = "XNA31_ok.info found, so Microsoft XNA Framework V3.1 seems to be installed. ";
                GameLog.Client.General.Info(_text);
                //MessageBox.Show(_text, "WARNING", MessageBoxButton.OK);
                //string msg = "XNA31_ok.info found, so Microsoft XNA Framework V3.1 seems to be installed. ";
            }
            else
            {
                if (File.Exists(xna_copy))
                {
                    GameLog.Client.General.Info("for XNA 3.1: trying to copy into XNA31_ok.info from C:\\Program Files (x86)\\Microsoft XNA\\XNA Game Studio\\v3.1\\Redist\\DX Redist\\DXSETUP.exe");
                    _ = Process.Start(xna_copy);
                    Thread.Sleep(1000);
                }
                else
                {
                    _text = "Did not found file " + xna_copy + newline + "to check for XNA 3.1"
                        + newline + newline + "*** if already installed copy any file to folder \\Resources and named it 'XNA31_ok.info'"
                        + newline + newline + "*** or rename the fake file 'XNA31_ok_OFF.info' to 'XNA31_ok.info'"
                        + newline + newline + "For Coders: Make sure you have fill the \\Resources folder"
                        ;
                    _ = MessageBox.Show(_text, "WARNING", MessageBoxButton.OK);


                }
            }

            // Second check after xna_copy was done
            if (!File.Exists(xna_check))
            {
                _text = "Sorry, Microsoft XNA Framework V3.1 might not be installed - but it is necessary. "
                    + newline + newline + "Version 3.1 is ABSOLUTELY needed, any newer Version can be installed, but additional !"
                    + newline + newline + "Install out of folder \\Resources\\XNA31_NEEDED_INSTALL\\ or"
                    + newline + newline + "Download it at www.microsoft.com/download/details.aspx?id=15163"
                    + newline + newline + "*** if already installed copy any file to folder \\Resources AND game-folder and named it 'XNA31_ok.info'"
                    + newline + newline + "Download  it at www.microsoft.com/download/details.aspx?id=15163"
                    + newline + newline + "Press OK for going on, but don't wonder if the game crashes ..or maybe not...."
                    ;
                GameLog.Client.General.Info(_text);
                _ = MessageBox.Show(_text, "CHECK", MessageBoxButton.OK);
            }
            else { found = true; }

            return found;
        }

        private static bool CheckNetFxVersion()
        {
            //Adapted from https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed#net_d
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
            int version;
            string _text;

            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
            {
                if ((ndpKey == null) || (ndpKey.GetValue("Release") == null))
                {
                    return false;
                }

                version = (int)ndpKey.GetValue("Release");
            }

            //if (version >= 461808) { 
            //{
            //    GameLog.Client.General.Info(".NET Framework 4.7.2 or later found");
            //} else if
            if (version >= 528040)
            {
                _text = ".NET Framework 4.8 or later found";
            }
            else if (version >= 461808)
            {
                _text = ".NET Framework 4.7.2 or later found";
            }
            else if (version >= 461308)
            {
                _text = ".NET Framework 4.7.1 found";
            }
            else if (version >= 460798)
            {
                _text = ".NET Framework 4.7 found";
            }
            else if (version >= 394802)
            {
                _text = ".NET Framework 4.6.2 found";
            }
            else if (version >= 394254)
            {
                _text = ".NET Framework 4.6.1 found"; // no need to check older ones
                                                                           //} else if (version >= 393295) {
                                                                           //    GameLog.Client.General.Info(".NET Framework 4.6 found"); 
                                                                           //} else if (version >= 379893) {
                                                                           //    GameLog.Client.General.Info(".NET Framework 4.5.2 found");
                                                                           //} else if (version >= 378675) {
                                                                           //    GameLog.Client.General.Info(".NET Framework 4.5.1 found");
                                                                           //} else if (version >= 378389) {
                                                                           //    GameLog.Client.General.Info(".NET Framework 4.5 found");
            }
            else
            {
                _text = ".NET Framework is less than 4.6.1";
            }
            _text = "Step_0120: " + _text;
            Console.WriteLine(_text);
            GameLog.Client.General.Info(_text);

            return version >= 394802;
        }

        private static void StartClient(string[] args)
        {
            ClientCommandLineArguments.Parse(CmdLineArgs, args);

            /* If an instance of the game is already running, then exit. */
            if (!CmdLineArgs.AllowMultipleInstances)
            {
                _singleInstanceMutex = new Mutex(true, "{CC4FD558-0934-451d-A387-738B5DB5619C}", out bool mutexIsNew);
                if (!mutexIsNew)
                {
                    _ = MessageBox.Show("An instance of Supremacy is already running.");
                    Environment.Exit(Environment.ExitCode);
                }
            }

            if (!string.IsNullOrWhiteSpace(CmdLineArgs.Traces))
            {
                List<string> traces = CmdLineArgs.Traces.Split(',').ToList();
                traces.ForEach(t =>
                    GameLog.SetRepositoryToDebug(t)
                );
            }

            if (CmdLineArgs.TraceLevel != PresentationTraceLevel.None)
            {
                PresentationTraceSources.Refresh();
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            try
            {
                Console.WriteLine("Step_0152: opening Error.txt (just in case an Error appears)");
                //var file = "Error.txt";
                //StreamWriter errorFile = new StreamWriter(file);

                FileStream errorFile = File.Open("Error.txt", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                Console.SetError(new StreamWriter(errorFile));
                //Console.SetError(errorFile);

                //Console.SetError
                //errorFile.WriteLine("Hello");
                //errorFile.WriteLine(DateTime.Now.ToString());
                //if (ClientSettings.Current.EnableOutputToTXT) Console.WriteLine("Time111: " + DateTime.Now);
                if (true) Console.WriteLine("Time_0103: " + DateTime.Now);
                //just starts an empty file 
                // System.Diagnostics.Process.Start("Error.txt");
            }
            catch
            {
                Console.WriteLine("Step_0163: error.txt could not be created. ");
                //_ = MessageBox.Show(
                //    "The error log could not be created.  You may still run the game,\n"
                //    + "but error details cannot be logged.",
                //    "Warning",
                //    MessageBoxButton.OK,
                //    MessageBoxImage.Warning);
            }

            //try
            //{
            //    // Diary like the old game had curves of Federation growing 
            //    var errorFile = File.Open("./lib/_diary.txt", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            //    Console.SetError(new StreamWriter(errorFile));
            //    //just starts an empty file 
            //    // System.Diagnostics.Process.Start("./lib/_diary.txt");
            //}
            //catch
            //{
            //    MessageBox.Show(
            //        "./lib/_diary.txt could not be created.  You may still run the game,\n"
            //        + "but diary details cannot be logged.",
            //        "Warning",
            //        MessageBoxButton.OK,
            //        MessageBoxImage.Warning);
            //}

            //try
            //{
            //    var errorFile = File.Open("./lib/_last_options.txt", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            //    Console.SetError(new StreamWriter(errorFile));
            //    //just starts an empty file 
            //    // System.Diagnostics.Process.Start("Error.txt");
            //}
            //catch
            //{
            //    MessageBox.Show(
            //        "./lib/_last_options.txt could not be created.  You may still run the game,\n"
            //        + "but _last_options.txt cannot be logged.",
            //        "Warning",
            //        MessageBoxButton.OK,
            //        MessageBoxImage.Warning);
            //}

            VfsWebRequestFactory.EnsureRegistered();

            //if (ClientSettings.Current.EnableOutputToTXT) Console.WriteLine("Time112: " + DateTime.Now.ToString());
            //if (true) Console.WriteLine("Time112: " + DateTime.Now.ToString());

            ClientApp app = new ClientApp();
            app.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            app.InitializeComponent();
            _ = app.Run();
            //if (ClientSettings.Current.EnableOutputToTXT) Console.WriteLine("Time113: " + DateTime.Now.ToString());
            //if (true) Console.WriteLine("Time113: " + DateTime.Now.ToString());
        }

        private static void ShowSplashScreen()
        {
            _splashScreen = new SplashScreen("resources/images/backgrounds/splash.png");  // hardcoded, file out of Code
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
            public string _text = "";

            //_text = "Step_0200: UnityBootstrapper";
            //        Console.WriteLine(_text);
            //        GameLog.Core.GalaxyGeneratorDetails.DebugFormat(_text);

            protected override IModuleCatalog GetModuleCatalog()
            {
                return new ConfigurationModuleCatalog().AddModule(ClientModule.ModuleName, typeof(ClientModule).AssemblyQualifiedName);
            }

            protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
            {
                RegionAdapterMappings baseMappings = base.ConfigureRegionAdapterMappings();
                baseMappings.RegisterMapping(
                    typeof(GameScreenStack),
                    Container.Resolve<GameScreenStackRegionAdapter>());
                return baseMappings;
            }

            protected override void ConfigureContainer()
            {
                base.ConfigureContainer();
                _ = Container.RegisterInstance(ResourceManager.VfsService, new ContainerControlledLifetimeManager());
                _ = Container.RegisterInstance<IApplicationSettingsService>(new ApplicationSettingsService(), new ContainerControlledLifetimeManager());
                _ = Container.RegisterInstance<IClientApplication>(Current, new ContainerControlledLifetimeManager());
                _ = Container.RegisterInstance<IDispatcherService>(new DefaultDispatcherService(Dispatcher.CurrentDispatcher), new ContainerControlledLifetimeManager());
                _ = Container.RegisterType<IUnhandledExceptionHandler, ClientUnhandledExceptionHandler>(new ContainerControlledLifetimeManager());
                _ = Container.RegisterType<INavigationCommandsProxy, NavigationCommandsProxy>(new ContainerControlledLifetimeManager());
                _ = Container.RegisterType<IAppContext, Context.AppContext>(new ContainerControlledLifetimeManager());
                _ = Container.RegisterType<IResourceManager, ClientResourceManager>(new ContainerControlledLifetimeManager());
                _ = Container.RegisterType<IGameErrorService, GameErrorService>(new ContainerControlledLifetimeManager());
                _ = Container.RegisterInstance<IAudioEngine>(FMODAudioEngine.Instance, new ContainerControlledLifetimeManager());
                _ = Container.RegisterType<IMusicPlayer, MusicPlayer>(new ContainerControlledLifetimeManager());
                _ = Container.RegisterType<ISoundPlayer, SoundPlayer>(new ContainerControlledLifetimeManager());
            }

            protected override DependencyObject CreateShell()
            {
                if (_shell != null)
                {
                    return _shell;
                }

                _shell = Container.Resolve<ClientWindow>();
                _shell.SourceInitialized += OnGameWindowSourceInitialized;
                Application.Current.MainWindow = _shell;
                _shell.Show();
                _ = Container.RegisterInstance<IGameWindow>(_shell, new ContainerControlledLifetimeManager());
                return _shell;
            }
        }
        #endregion
    }
}
