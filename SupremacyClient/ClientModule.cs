// File:ClientModule.cs
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Unity;
using Supremacy.Annotations;
using Supremacy.Client.Audio;
using Supremacy.Client.Commands;
using Supremacy.Client.Context;
using Supremacy.Client.Dialogs;
using Supremacy.Client.Events;
using Supremacy.Client.Input;
using Supremacy.Client.OptionsPages;
using Supremacy.Client.Services;
using Supremacy.Client.Views;
using Supremacy.Client.Views.DiplomacyScreen;
using Supremacy.Game;
using Supremacy.Messages;
using Supremacy.Messaging;
using Supremacy.Resources;
using Supremacy.Types;
using Supremacy.UI;
using Supremacy.Utility;
using Supremacy.WCF;
using System;
using System.Concurrency;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Collections.Generic;


namespace Supremacy.Client
{
    public class ClientModule : IModule
    {
        public const string ModuleName = "Supremacy.Client.ClientModule";
        // Non-Serialized see below
        #region Fields
        private const string MusicThemeBasePath = "Resources/Specific_Empires_UI";
        private const string MusicPackFileName = "MusicPacks.xml";

        private readonly IClientApplication _app;
        private readonly IUnityContainer _container;
        private readonly IResourceManager _resourceManager;
        private readonly IRegionManager _regionManager;
        private readonly IRegionViewRegistry _regionViewRegistry;
        private readonly IDispatcherService _dispatcherService;
        private readonly IGameErrorService _errorService;
        private readonly IAppContext _appContext;
        private readonly INavigationCommandsProxy _navigationCommands;
        private readonly IMusicPlayer _musicPlayer;
        private readonly ISoundPlayer _soundPlayer;

        private readonly ClientOptionsDialog _optionsDialog;
        private readonly ClientTracesDialog _tracesDialog;

        //private readonly F07_Dialog _encyclopedia_Dialog;
        private readonly F06_Dialog _f06_Dialog; // F1 - F5 already used
        private readonly F07_Dialog _f07_Dialog;
        private readonly F08_Dialog _f08_Dialog;
        private readonly F09_Dialog _f09_Dialog;
        private readonly F10_Dialog _f10_Dialog;
        private readonly F11_Dialog _f11_Dialog;
        private readonly F12_Dialog _f12_Dialog;
        //private readonly FakeDialog _fakeDialog;

        private readonly CTRL_F01_Dialog _ctrl_f01_Dialog;
        //private readonly CTRL_F02_Dialog _ctrl_f02_Dialog;
        //private readonly CTRL_F03_Dialog _ctrl_f03_Dialog;
        //private readonly CTRL_F04_Dialog _ctrl_f04_Dialog;
        //private readonly CTRL_F05_Dialog _ctrl_f05_Dialog;
        private readonly CTRL_F06_Dialog _ctrl_f06_Dialog;
        //private readonly F07_Dialog _encyclopedia_Dialog;
        private readonly CTRL_F07_Dialog _ctrl_f07_Dialog;  // German version of F07-Dialog
        //private readonly CTRL_F08_Dialog _ctrl_f08_Dialog;
        //private readonly CTRL_F09_Dialog _ctrl_f09_Dialog;
        //private readonly CTRL_F10_Dialog _ctrl_f10_Dialog;
        //private readonly CTRL_F11_Dialog _ctrl_f11_Dialog;
        //private readonly CTRL_F12_Dialog _ctrl_f12_Dialog;

        private readonly DelegateCommand<object> _optionsCommand;
        private readonly DelegateCommand<object> _tracesCommand;
        //private readonly DelegateCommand<object> _f01_Command;  // already used F1 - F5
        private readonly DelegateCommand<object> _f06_Command;
        private readonly DelegateCommand<object> _f07_Command;
        private readonly DelegateCommand<object> _f08_Command;
        private readonly DelegateCommand<object> _f09_Command;
        private readonly DelegateCommand<object> _f10_Command;
        private readonly DelegateCommand<object> _f11_Command;
        private readonly DelegateCommand<object> _f12_Command;

        private readonly DelegateCommand<object> _ctrl_f01_Command;
        //private readonly DelegateCommand<object> _ctrl_f02_Command;
        //private readonly DelegateCommand<object> _ctrl_f03_Command;
        //private readonly DelegateCommand<object> _ctrl_f04_Command;
        //private readonly DelegateCommand<object> _ctrl_f05_Command;
        private readonly DelegateCommand<object> _ctrl_f06_Command;
        private readonly DelegateCommand<object> _ctrl_f07_Command;
        //private readonly DelegateCommand<object> _ctrl_f08_Command;
        //private readonly DelegateCommand<object> _ctrl_f09_Command;
        //private readonly DelegateCommand<object> _ctrl_f10_Command;
        //private readonly DelegateCommand<object> _ctrl_f11_Command;
        //private readonly DelegateCommand<object> _ctrl_f12_Command;

        private readonly DelegateCommand<object> _s0_Command;   // start Single Player Empire 0
        private readonly DelegateCommand<object> _s1_Command;
        private readonly DelegateCommand<object> _s2_Command;
        private readonly DelegateCommand<object> _s3_Command;
        private readonly DelegateCommand<object> _s4_Command;
        private readonly DelegateCommand<object> _s5_Command;
        private readonly DelegateCommand<object> _s6_Command;

        //private readonly DelegateCommand<object> _fakeCommand;
        private readonly DelegateCommand<object> _logTxtCommand;
        private readonly DelegateCommand<object> _errorTxtCommand;
        private readonly DelegateCommand<object> _startSinglePlayerGameCommand;
        private readonly DelegateCommand<object> _continueGameCommand;
        private readonly DelegateCommand<bool> _endGameCommand;
        private readonly DelegateCommand<SavedGameHeader> _loadGameCommand;
        private readonly DelegateCommand<SavedGameHeader> _deleteManualSavedGameCommand;
        private readonly DelegateCommand<SavedGameHeader> _deleteAutoSavedGameCommand;
        private readonly DelegateCommand<object> _showCreditsDialogCommand;
        private readonly DelegateCommand<object> _showSettingsFileCommand;
        private readonly DelegateCommand<object> _showPlayersHistoryFileCommand;
        private readonly DelegateCommand<object> _showAllHistoryFileCommand;
        private readonly DelegateCommand<MultiplayerConnectParameters> _joinMultiplayerGameCommand;
        private readonly DelegateCommand<string> _hostMultiplayerGameCommand;
        private readonly DelegateCommand<bool> _exitCommand;

        //private readonly DelegateCommand<object> _OnMA_Ferengi;

        private readonly DelegateCommand<object> _Hotkey_Alt_D0;
        private readonly DelegateCommand<object> _Hotkey_Alt_D1;
        private readonly DelegateCommand<object> _Hotkey_Alt_D2;
        private readonly DelegateCommand<object> _Hotkey_Alt_D3;
        private readonly DelegateCommand<object> _Hotkey_Alt_D4;
        private readonly DelegateCommand<object> _Hotkey_Alt_D5;
        private readonly DelegateCommand<object> _Hotkey_Alt_D6;
        private readonly DelegateCommand<object> _Hotkey_Alt_D7;
        private readonly DelegateCommand<object> _Hotkey_Alt_D8;
        private readonly DelegateCommand<object> _Hotkey_Alt_D9;


        private readonly DelegateCommand<object> _Hotkey_Alt_F01;
        private readonly DelegateCommand<object> _Hotkey_Alt_F02;
        private readonly DelegateCommand<object> _Hotkey_Alt_F03;
        private readonly DelegateCommand<object> _Hotkey_Alt_F04;
        private readonly DelegateCommand<object> _Hotkey_Alt_F05;
        private readonly DelegateCommand<object> _Hotkey_Alt_F06;
        private readonly DelegateCommand<object> _Hotkey_Alt_F07;
        private readonly DelegateCommand<object> _Hotkey_Alt_F08;
        private readonly DelegateCommand<object> _Hotkey_Alt_F09;
        private readonly DelegateCommand<object> _Hotkey_Alt_F10;
        private readonly DelegateCommand<object> _Hotkey_Alt_F11;
        //private readonly DelegateCommand<object> _Hotkey_Alt_F12;

        private readonly DelegateCommand<object> _Hotkey_Alt_A;
        private readonly DelegateCommand<object> _Hotkey_Alt_B;
        private readonly DelegateCommand<object> _Hotkey_Alt_C;
        private readonly DelegateCommand<object> _Hotkey_Alt_D;
        private readonly DelegateCommand<object> _Hotkey_Alt_E;
        private readonly DelegateCommand<object> _Hotkey_Alt_F;
        private readonly DelegateCommand<object> _Hotkey_Alt_G;
        private readonly DelegateCommand<object> _Hotkey_Alt_H;
        private readonly DelegateCommand<object> _Hotkey_Alt_I;
        private readonly DelegateCommand<object> _Hotkey_Alt_J;
        private readonly DelegateCommand<object> _Hotkey_Alt_K;
        private readonly DelegateCommand<object> _Hotkey_Alt_L;
        private readonly DelegateCommand<object> _Hotkey_Alt_M;
        private readonly DelegateCommand<object> _Hotkey_Alt_N;
        private readonly DelegateCommand<object> _Hotkey_Alt_O;
        private readonly DelegateCommand<object> _Hotkey_Alt_P;
        private readonly DelegateCommand<object> _Hotkey_Alt_Q;
        private readonly DelegateCommand<object> _Hotkey_Alt_R;
        private readonly DelegateCommand<object> _Hotkey_Alt_S;
        private readonly DelegateCommand<object> _Hotkey_Alt_T;
        private readonly DelegateCommand<object> _Hotkey_Alt_U;
        private readonly DelegateCommand<object> _Hotkey_Alt_V;
        private readonly DelegateCommand<object> _Hotkey_Alt_W;
        private readonly DelegateCommand<object> _Hotkey_Alt_X;
        private readonly DelegateCommand<object> _Hotkey_Alt_Y;
        private readonly DelegateCommand<object> _Hotkey_Alt_Z;

        private IGameController _gameController;
        public int localCivID;
        private bool _isExiting;

        [NonSerialized]
        public string localEmpire = "";
        private int startTechLvl = -1;

        public bool _checkLoading = true;
        public bool _gamelog_bool = true;
        public bool _ConsoleWriteline_bool = true;



        public string _text;
        public readonly string newline = Environment.NewLine;


        //private int SpecialWidth1 = 576;
        //private int SpecialHeight1 = 480;

        private string _resultText;
        #endregion

        #region Constructor & Lifetime
        public ClientModule(
            [NotNull] IClientApplication app,
            [NotNull] IUnityContainer container,
            [NotNull] IResourceManager resourceManager,
            [NotNull] IRegionViewRegistry regionViewRegistry,
            [NotNull] IDispatcherService dispatcherService,
            [NotNull] IGameErrorService errorService,
            [NotNull] IMusicPlayer musicPlayer,
            [NotNull] ISoundPlayer soundPlayer)
        {
            _app = app ?? throw new ArgumentNullException("app");
            _container = container ?? throw new ArgumentNullException("container");
            _resourceManager = resourceManager ?? throw new ArgumentNullException("resourceManager");
            _regionViewRegistry = regionViewRegistry ?? throw new ArgumentNullException("regionViewRegistry");
            _dispatcherService = dispatcherService ?? throw new ArgumentNullException("dispatcherService");
            _errorService = errorService ?? throw new ArgumentNullException("errorService");
            _musicPlayer = musicPlayer ?? throw new ArgumentNullException("musicPlayer");
            _soundPlayer = soundPlayer ?? throw new ArgumentNullException("soundPlayer");

            _appContext = _container.Resolve<IAppContext>();
            _regionManager = _container.Resolve<IRegionManager>();
            _navigationCommands = _container.Resolve<INavigationCommandsProxy>();

            _optionsDialog = new ClientOptionsDialog();
            _optionsCommand = new DelegateCommand<object>(
                ExecuteOptionsCommand);

            _tracesDialog = new ClientTracesDialog();
            _tracesCommand = new DelegateCommand<object>(
                ExecuteTracesCommand);

            _f06_Dialog = new F06_Dialog();
            _f06_Command = new DelegateCommand<object>(
                Execute_f06_Command);

            _f07_Dialog = new F07_Dialog();
            _f07_Command = new DelegateCommand<object>(
                Execute_f07_Command);

            _f08_Dialog = new F08_Dialog();
            _f08_Command = new DelegateCommand<object>(
                Execute_f08_Command);

            _f09_Dialog = new F09_Dialog();
            _f09_Command = new DelegateCommand<object>(
                Execute_f09_Command);

            _f10_Dialog = new F10_Dialog();
            _f10_Command = new DelegateCommand<object>(
                Execute_f10_Command);

            _f11_Dialog = new F11_Dialog();
            _f11_Command = new DelegateCommand<object>(
                Execute_f11_Command);

            _f12_Dialog = new F12_Dialog();
            _f12_Command = new DelegateCommand<object>(
                Execute_f12_Command);

            _ctrl_f01_Dialog = new CTRL_F01_Dialog();
            _ctrl_f01_Command = new DelegateCommand<object>(
                Execute_ctrl_f01_Command);
            
            //_ctrl_f02_Dialog = new CTRL_F02_Dialog();
            //_ctrl_f02_Command = new DelegateCommand<object>(
            //    Execute_ctrl_f02_Command);
            
            //_ctrl_f03_Dialog = new CTRL_F03_Dialog();
            //_ctrl_f03_Command = new DelegateCommand<object>(
            //    Execute_ctrl_f03_Command);
            
            //_ctrl_f04_Dialog = new CTRL_F04_Dialog();
            //_ctrl_f04_Command = new DelegateCommand<object>(
            //    Execute_ctrl_f04_Command);
            
            //_ctrl_f05_Dialog = new CTRL_F05_Dialog();
            //_ctrl_f05_Command = new DelegateCommand<object>(
            //    Execute_ctrl_f05_Command);

            _ctrl_f06_Dialog = new CTRL_F06_Dialog();
            _ctrl_f06_Command = new DelegateCommand<object>(
                Execute_ctrl_f06_Command);

            _ctrl_f07_Dialog = new CTRL_F07_Dialog();
            _ctrl_f07_Command = new DelegateCommand<object>(
                Execute_ctrl_f07_Command);

            _s0_Command = new DelegateCommand<object>(Execute_s0_Command); // start Single Player Empire 0
            _s1_Command = new DelegateCommand<object>(Execute_s1_Command);
            _s2_Command = new DelegateCommand<object>(Execute_s2_Command);
            _s3_Command = new DelegateCommand<object>(Execute_s3_Command);
            _s4_Command = new DelegateCommand<object>(Execute_s4_Command);
            _s5_Command = new DelegateCommand<object>(Execute_s5_Command);
            _s6_Command = new DelegateCommand<object>(Execute_s6_Command);

            //_fakeDialog = new FakeDialog();
            //_fakeCommand = new DelegateCommand<object>(ExecuteFakeCommand);

            _logTxtCommand = new DelegateCommand<object>(ExecuteLogTxtCommand);
            _errorTxtCommand = new DelegateCommand<object>(ExecuteErrorTxtCommand);

            _startSinglePlayerGameCommand = new DelegateCommand<object>(ExecuteStartSinglePlayerGameCommand);
            _continueGameCommand = new DelegateCommand<object>(ExecuteContinueGameCommand);
            _endGameCommand = new DelegateCommand<bool>(ExecuteEndGameCommand);
            _exitCommand = new DelegateCommand<bool>(ExecuteExitCommand);
            _loadGameCommand = new DelegateCommand<SavedGameHeader>(ExecuteLoadGameCommand);
            _deleteManualSavedGameCommand = new DelegateCommand<SavedGameHeader>(ExecuteDeleteManualSavedGameCommand);
            _deleteAutoSavedGameCommand = new DelegateCommand<SavedGameHeader>(ExecuteDeleteAutoSavedGameCommand);
            _showCreditsDialogCommand = new DelegateCommand<object>(ExecuteShowCreditsDialogCommand);
            _showSettingsFileCommand = new DelegateCommand<object>(ExecuteShowSettingsFileCommand);
            _showPlayersHistoryFileCommand = new DelegateCommand<object>(ExecuteShowPlayersHistoryFileCommand);
            _showAllHistoryFileCommand = new DelegateCommand<object>(ExecuteShowAllHistoryFileCommand);
            _joinMultiplayerGameCommand = new DelegateCommand<MultiplayerConnectParameters>(ExecuteJoinMultiplayerGameCommand);
            _hostMultiplayerGameCommand = new DelegateCommand<string>(ExecuteHostMultiplayerGameCommand);

            //_OnMA_Ferengi = new DelegateCommand<object>(ExecuteOnMA_Ferengi);

            _Hotkey_Alt_D0 = new DelegateCommand<object>(Execute_Hotkey_Alt_D0);
            _Hotkey_Alt_D1 = new DelegateCommand<object>(Execute_Hotkey_Alt_D1);
            _Hotkey_Alt_D2 = new DelegateCommand<object>(Execute_Hotkey_Alt_D2);
            _Hotkey_Alt_D3 = new DelegateCommand<object>(Execute_Hotkey_Alt_D3);
            _Hotkey_Alt_D4 = new DelegateCommand<object>(Execute_Hotkey_Alt_D4);
            _Hotkey_Alt_D5 = new DelegateCommand<object>(Execute_Hotkey_Alt_D5);
            _Hotkey_Alt_D6 = new DelegateCommand<object>(Execute_Hotkey_Alt_D6);
            _Hotkey_Alt_D7 = new DelegateCommand<object>(Execute_Hotkey_Alt_D7);
            _Hotkey_Alt_D8 = new DelegateCommand<object>(Execute_Hotkey_Alt_D8);
            _Hotkey_Alt_D9 = new DelegateCommand<object>(Execute_Hotkey_Alt_D9);


            _Hotkey_Alt_F01 = new DelegateCommand<object>(Execute_Hotkey_Alt_F01);
            _Hotkey_Alt_F02 = new DelegateCommand<object>(Execute_Hotkey_Alt_F02);
            _Hotkey_Alt_F03 = new DelegateCommand<object>(Execute_Hotkey_Alt_F03);
            _Hotkey_Alt_F04 = new DelegateCommand<object>(Execute_Hotkey_Alt_F04);
            _Hotkey_Alt_F05 = new DelegateCommand<object>(Execute_Hotkey_Alt_F05);
            _Hotkey_Alt_F06 = new DelegateCommand<object>(Execute_Hotkey_Alt_F06);
            _Hotkey_Alt_F07 = new DelegateCommand<object>(Execute_Hotkey_Alt_F07);
            _Hotkey_Alt_F08 = new DelegateCommand<object>(Execute_Hotkey_Alt_F08);
            _Hotkey_Alt_F09 = new DelegateCommand<object>(Execute_Hotkey_Alt_F09);
            _Hotkey_Alt_F10 = new DelegateCommand<object>(Execute_Hotkey_Alt_F10);
            _Hotkey_Alt_F11 = new DelegateCommand<object>(Execute_Hotkey_Alt_F11);

            _Hotkey_Alt_A = new DelegateCommand<object>(Execute_Hotkey_Alt_A);
            _Hotkey_Alt_B = new DelegateCommand<object>(Execute_Hotkey_Alt_B);
            _Hotkey_Alt_C = new DelegateCommand<object>(Execute_Hotkey_Alt_C);
            _Hotkey_Alt_D = new DelegateCommand<object>(Execute_Hotkey_Alt_D);
            _Hotkey_Alt_E = new DelegateCommand<object>(Execute_Hotkey_Alt_E);
            _Hotkey_Alt_F = new DelegateCommand<object>(Execute_Hotkey_Alt_F);
            _Hotkey_Alt_G = new DelegateCommand<object>(Execute_Hotkey_Alt_G);
            _Hotkey_Alt_H = new DelegateCommand<object>(Execute_Hotkey_Alt_H);
            _Hotkey_Alt_I = new DelegateCommand<object>(Execute_Hotkey_Alt_I);
            _Hotkey_Alt_J = new DelegateCommand<object>(Execute_Hotkey_Alt_J);
            _Hotkey_Alt_K = new DelegateCommand<object>(Execute_Hotkey_Alt_K);
            _Hotkey_Alt_L = new DelegateCommand<object>(Execute_Hotkey_Alt_L);
            _Hotkey_Alt_M = new DelegateCommand<object>(Execute_Hotkey_Alt_M);
            _Hotkey_Alt_N = new DelegateCommand<object>(Execute_Hotkey_Alt_N);
            _Hotkey_Alt_O = new DelegateCommand<object>(Execute_Hotkey_Alt_O);
            _Hotkey_Alt_P = new DelegateCommand<object>(Execute_Hotkey_Alt_P);
            _Hotkey_Alt_Q = new DelegateCommand<object>(Execute_Hotkey_Alt_Q);
            _Hotkey_Alt_R = new DelegateCommand<object>(Execute_Hotkey_Alt_R);
            _Hotkey_Alt_S = new DelegateCommand<object>(Execute_Hotkey_Alt_S);
            _Hotkey_Alt_T = new DelegateCommand<object>(Execute_Hotkey_Alt_T);
            _Hotkey_Alt_U = new DelegateCommand<object>(Execute_Hotkey_Alt_U);
            _Hotkey_Alt_V = new DelegateCommand<object>(Execute_Hotkey_Alt_V);
            _Hotkey_Alt_W = new DelegateCommand<object>(Execute_Hotkey_Alt_W);
            _Hotkey_Alt_X = new DelegateCommand<object>(Execute_Hotkey_Alt_X);
            _Hotkey_Alt_Y = new DelegateCommand<object>(Execute_Hotkey_Alt_Y);
            _Hotkey_Alt_Z = new DelegateCommand<object>(Execute_Hotkey_Alt_Z);



        }
        #endregion

        #region Commands
        private static void ExecuteShowCreditsDialogCommand(object parameter)
        {

            // makes a crash, maybe since Data was moved to \bin

            //var creditsPage = Application.LoadComponent(
            //    new Uri(
            //        "/SupremacyClient;Component/Resources/Credits.xaml",
            //        UriKind.RelativeOrAbsolute));

            //MessageDialog.Show(creditsPage, MessageDialogButtons.Close);

            //MessageDialog.Show("Please have a look to Credits.xaml !", MessageDialogButtons.Close);

            string file = ".\\Resources\\Credits_for_Rise_of_the_UFP.pdf";
            try
            {
                if (File.Exists(file))
                {
                    _ = Process.Start(file);
                }
            }
            catch
            {
                _ = MessageDialog.Show("Please have a look to Credits.xaml !", MessageDialogButtons.Close);
            }
        }

        private void ExecuteHostMultiplayerGameCommand(string playerName)
        {
            GameInitData initData = GameInitData.CreateMultiplayerGame(GameOptionsManager.LoadDefaults(), playerName);
            RunGameController(gameController => gameController.RunLocal(initData), true);
        }

        private void ExecuteJoinMultiplayerGameCommand(MultiplayerConnectParameters parameters) => RunGameController(gameController => gameController.RunRemote(parameters.PlayerName, parameters.RemoteHost), true);

        private void ExecuteLoadGameCommand(SavedGameHeader header)
        {
            GameInitData initData = GameInitData.CreateFromSavedGame(header);
            GameLog.Client.General.Debug("doing ExecuteLoadGameCommand ...");
            RunGameController(gameController => gameController.RunLocal(initData), initData.IsMultiplayerGame);
            GameLog.Client.GeneralDetails.Debug("doing gameController.RunLocal(initData) ...");

            startTechLvl = GetStartTechLvl(initData.Options.StartingTechLevel.ToString());
            localEmpire = GetLocalEmpireShortage(initData.LocalPlayerEmpireID, out string localempire);
            GameLog.Client.General.Debug("playing " + localempire + " ( StartLevel " + startTechLvl + " )");
            _navigationCommands.ActivateScreen.Execute(StandardGameScreens.GalaxyScreen);

        }

        private void ExecuteDeleteManualSavedGameCommand(object obj) => _ = SavedGameManager.SaveGameDeleteManualSaved();

        private void ExecuteDeleteAutoSavedGameCommand(object obj) => _ = SavedGameManager.SaveGameDeleteAutoSaved();

        private void ExecuteOptionsCommand(object obj) => _ = _optionsDialog.ShowDialog();
        private void ExecuteTracesCommand(object obj) => _ = _tracesDialog.ShowDialog();

        //private void Execute_f01_Command(object obj) => _ = _f01_Dialog.ShowDialog();  // f1 - F5 already used
        //private void Execute_f02_Command(object obj) => _ = _f02_Dialog.ShowDialog();
        //private void Execute_f03_Command(object obj) => _ = _f03_Dialog.ShowDialog();
        //private void Execute_f04_Command(object obj) => _ = _f04_Dialog.ShowDialog();
        //private void Execute_f05_Command(object obj) => _ = _f05_Dialog.ShowDialog();
        private void Execute_f06_Command(object obj) => _ = _f06_Dialog.ShowDialog();


        private void Execute_f07_Command(object obj) => _ = _f07_Dialog.ShowDialog();// temporary switched to German _f07 > OFF now
        //private void Execute_f07_Command(object obj) => _ = _f07_Dialog.ShowDialog();


        private void Execute_f08_Command(object obj) => _ = _f08_Dialog.ShowDialog();
        private void Execute_f09_Command(object obj) => _ = _f09_Dialog.ShowDialog();
        private void Execute_f10_Command(object obj) => _ = _f10_Dialog.ShowDialog();
        private void Execute_f11_Command(object obj) => _ = _f11_Dialog.ShowDialog();
        private void Execute_f12_Command(object obj) => _ = _f12_Dialog.ShowDialog();


        private void Execute_ctrl_f01_Command(object obj) { _ = _ctrl_f01_Dialog.ShowDialog(); }
        //private void Execute_ctrl_f02_Command(object obj) { _ = _ctrl_f02_Dialog.ShowDialog(); }
        //private void Execute_ctrl_f03_Command(object obj) { _ = _ctrl_f03_Dialog.ShowDialog(); }
        //private void Execute_ctrl_f04_Command(object obj) { _ = _ctrl_f04_Dialog.ShowDialog(); }
        //private void Execute_ctrl_f05_Command(object obj) { _ = _ctrl_f05_Dialog.ShowDialog(); }
        private void Execute_ctrl_f06_Command(object obj) => _ = _ctrl_f06_Dialog.ShowDialog();
        
        
        private void Execute_ctrl_f07_Command(object obj) => _ = _ctrl_f07_Dialog.ShowDialog();// temporary switched to German _f07 > OFF now
        //private void Execute_ctrl_f07_Command(object obj) => _ = _ctrl_f07_Dialog.ShowDialog(); // temporary


        //private void Execute_ctrl_f08_Command(object obj) { _ = _ctrl_f08_Dialog.ShowDialog(); }
        //private void Execute_ctrl_f09_Command(object obj) { _ = _ctrl_f09_Dialog.ShowDialog(); }
        //private void Execute_ctrl_f10_Command(object obj) { _ = _ctrl_f10_Dialog.ShowDialog(); }
        //private void Execute_ctrl_f11_Command(object obj) { _ = _ctrl_f11_Dialog.ShowDialog(); }
        //private void Execute_ctrl_f12_Command(object obj) { _ = _ctrl_f12_Dialog.ShowDialog(); }


        private void Execute_s0_Command(object obj) => ExecuteSP_DirectlyGameCommand(0);
        private void Execute_s1_Command(object obj) => ExecuteSP_DirectlyGameCommand(1);
        private void Execute_s2_Command(object obj) => ExecuteSP_DirectlyGameCommand(2);
        private void Execute_s3_Command(object obj) => ExecuteSP_DirectlyGameCommand(3);
        private void Execute_s4_Command(object obj) => ExecuteSP_DirectlyGameCommand(4);
        private void Execute_s5_Command(object obj) => ExecuteSP_DirectlyGameCommand(5);
        private void Execute_s6_Command(object obj) => ExecuteSP_DirectlyGameCommand(6);
        //private void ExecuteFakeCommand(object obj) => _ = _fakeDialog.ShowDialog();
        private void ExecuteLogTxtCommand(object obj)
        {
            string logFile = Path.Combine(ResourceManager.GetResourcePath(""), "Log.txt");

            if (!string.IsNullOrEmpty(logFile) && File.Exists(logFile))
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = logFile
                };

                try
                {
                    _ = Process.Start(processStartInfo);
                }
                catch
                {
                    _ = MessageBox.Show("Step_0100: Could not load Log.txt");
                }
            }
        }

        private void ExecuteErrorTxtCommand(object obj)
        {
            string errorFile = Path.Combine(ResourceManager.GetResourcePath(""), "Error.txt");

            if (!string.IsNullOrEmpty(errorFile) && File.Exists(errorFile))
            {
                double fileSize = new FileInfo(errorFile).Length;
                _text = "Error.txt is empty - nothing to load"
                    + newline
                    + newline + "> see file > 'Error-txt - First Run.txt'"
                    + newline + "> on start: rename 'SupremacyClient..Settings.xaml' ... will be re-created"
                    + newline
                    + newline + "> NOW please shot down the program... sorry"
                    ;
                // OFF   if (fileSize == 0) { _ = MessageBox.Show(_text); return; }
                // OFF   if (fileSize < 0) { _ = MessageBox.Show("Could not load Error.txt"); return; }

                ProcessStartInfo processStartInfo = new ProcessStartInfo { UseShellExecute = true, FileName = errorFile };

                try { _ = Process.Start(processStartInfo); }
                catch { _ = MessageBox.Show("Could not load Error.txt"+fileSize); }
            }
        }

        private void ExecuteContinueGameCommand(object obj) => _navigationCommands.ActivateScreen.Execute(StandardGameScreens.GalaxyScreen);

        private void ExecuteExitCommand(bool showConfirmation) => Exit(showConfirmation);

        private void ExecuteEndGameCommand(bool showConfirmation) => _ = EndGame(showConfirmation);

        private void ExecuteShowSettingsFileCommand(object obj)
        {
            string file = Path.Combine(
                ResourceManager.GetResourcePath(""),
                "SupremacyClient..Settings.xaml");
            file = file.Replace(".\\", "");
            //string _text1;

            if (!string.IsNullOrEmpty(file) && File.Exists(file))
            {
                FileStream stream = new FileStream(
                    file,
                    FileMode.Open,
                    FileAccess.Read);

                _text = "";

                using (StreamReader reader = new StreamReader(stream))
                {
                    Console.WriteLine("---------------");

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (line == null)
                        {
                            break;
                        }
                        //Console.WriteLine(line);
                        _text += line;
                    }

                }
                //stream.Close;
            }

            string[] coll = _text.Split(' ');
            List<string> _trues = new List<string>(); /*_trues.Clear();*/
            List<string> _false = new List<string>(); /*_false.Clear();*/
            List<string> _rest = new List<string>(); /*_rest.Clear();*/
            //_array = new Dictionary<int, string, string, string>();

            foreach (string item in coll)
            {
                Console.WriteLine(item);
                if (item.Contains("True")) { _trues.Add(item); }// += item + newline;}
                if (item.Contains("False")) { _false.Add(item); }
                if (!item.Contains("True") && !item.Contains("False")) { _rest.Add(item); }
            }

            _resultText = "CONTENT OF SupremacyClient..Settings.xaml " + DateTime.Now + newline;

            _resultText += newline + "VALUES" + newline + "======" + newline;
            foreach (string item in _rest) { _resultText += item + newline; }

            _resultText += newline + "TRUE" + newline + "====" + newline;
            foreach (string item in _trues) { _resultText += item + newline; }

            _resultText += newline + "FALSE" + newline + "=====" + newline;
            foreach (string item in _false) { _resultText += item + newline; }


            _resultText += newline + newline;

            StreamWriter streamWriter = new StreamWriter(file + ".txt");
            streamWriter.Write(_resultText);
            streamWriter.Close();

            string _file = Path.Combine(ResourceManager.GetResourcePath(""), file + ".txt");
            if (!string.IsNullOrEmpty(_file) && File.Exists(_file))
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo { UseShellExecute = true, FileName = _file };

                try { _ = Process.Start(processStartInfo); }
                catch { _ = MessageBox.Show("Could not load Text-File about Settings"); }
            }
        }

        private void ExecuteShowPlayersHistoryFileCommand(object obj)
        {
            if (GameContext.Current == null || GameContext.Current.TurnNumber == 1)
                return;

            string file = Path.Combine(
                ResourceManager.GetResourcePath(""),
                "PlayersHistory");
            file = file.Replace(".\\", "");
            //string _text1;



            //StreamWriter streamWriter = new StreamWriter(file);
            //streamWriter.Write(_resultText);
            //streamWriter.Close();
            file += ".bat";

                //string _file = Path.Combine(ResourceManager.GetResourcePath(""), file + ".txt");
                if (!string.IsNullOrEmpty(file) && File.Exists(file))
                {
                    ProcessStartInfo processStartInfo = new ProcessStartInfo { UseShellExecute = true, FileName = file };

                    try { _ = Process.Start(processStartInfo); }
                    catch { _ = MessageBox.Show("Could not load Text-File about Players History"); }
                }
            
        }

        private void StartFile(string file)
        {
            try { if (File.Exists(file)) _ = Process.Start(file); else _ = MessageDialog.Show(file + " not found !", MessageDialogButtons.Close); }
            catch { _ = MessageDialog.Show(file + " - Error occured !", MessageDialogButtons.Close); }
        }

        private void Execute_Hotkey_Alt_D0(object obj) => StartFile(".\\lib\\Hotkey_Alt_0.bat");
        private void Execute_Hotkey_Alt_D1(object obj) => StartFile(".\\lib\\Hotkey_Alt_1.bat");
        private void Execute_Hotkey_Alt_D2(object obj) => StartFile(".\\lib\\Hotkey_Alt_2.bat");
        private void Execute_Hotkey_Alt_D3(object obj) => StartFile(".\\lib\\Hotkey_Alt_3.bat");
        private void Execute_Hotkey_Alt_D4(object obj) => StartFile(".\\lib\\Hotkey_Alt_4.bat");
        private void Execute_Hotkey_Alt_D5(object obj) => StartFile(".\\lib\\Hotkey_Alt_5.bat");
        private void Execute_Hotkey_Alt_D6(object obj) => StartFile(".\\lib\\Hotkey_Alt_6.bat");
        private void Execute_Hotkey_Alt_D7(object obj) => StartFile(".\\lib\\Hotkey_Alt_7.bat");
        private void Execute_Hotkey_Alt_D8(object obj) => StartFile(".\\lib\\Hotkey_Alt_8.bat");
        private void Execute_Hotkey_Alt_D9(object obj) => StartFile(".\\lib\\Hotkey_Alt_9.bat");


        private void Execute_Hotkey_Alt_F01(object obj) => StartFile(".\\lib\\Hotkey_Alt_F01.bat");
        private void Execute_Hotkey_Alt_F02(object obj) => StartFile(".\\lib\\Hotkey_Alt_F02.bat");
        private void Execute_Hotkey_Alt_F03(object obj) => StartFile(".\\lib\\Hotkey_Alt_F03.bat");
        private void Execute_Hotkey_Alt_F04(object obj) => StartFile(".\\lib\\Hotkey_Alt_F04.bat");
        private void Execute_Hotkey_Alt_F05(object obj) => StartFile(".\\lib\\Hotkey_Alt_F05.bat");
        private void Execute_Hotkey_Alt_F06(object obj) => StartFile(".\\lib\\Hotkey_Alt_F06.bat");
        private void Execute_Hotkey_Alt_F07(object obj) => StartFile(".\\lib\\Hotkey_Alt_F07.bat");
        private void Execute_Hotkey_Alt_F08(object obj) => StartFile(".\\lib\\Hotkey_Alt_F08.bat");
        private void Execute_Hotkey_Alt_F09(object obj) => StartFile(".\\lib\\Hotkey_Alt_F09.bat");
        private void Execute_Hotkey_Alt_F10(object obj) => StartFile(".\\lib\\Hotkey_Alt_F10.bat");
        private void Execute_Hotkey_Alt_F11(object obj) => StartFile(".\\lib\\Hotkey_Alt_F11.bat");

        private void Execute_Hotkey_Alt_A(object obj) => StartFile(".\\lib\\Hotkey_Alt_A.bat");
        private void Execute_Hotkey_Alt_B(object obj) => StartFile(".\\lib\\Hotkey_Alt_B.bat");
        private void Execute_Hotkey_Alt_C(object obj) => StartFile(".\\lib\\Hotkey_Alt_C.bat");
        private void Execute_Hotkey_Alt_D(object obj) => StartFile(".\\lib\\Hotkey_Alt_D.bat");
        private void Execute_Hotkey_Alt_E(object obj) => StartFile(".\\lib\\Hotkey_Alt_E.bat");
        private void Execute_Hotkey_Alt_F(object obj) => StartFile(".\\lib\\Hotkey_Alt_F.bat");
        private void Execute_Hotkey_Alt_G(object obj) => StartFile(".\\lib\\Hotkey_Alt_G.bat");
        private void Execute_Hotkey_Alt_H(object obj) => StartFile(".\\lib\\Hotkey_Alt_H.bat");
        private void Execute_Hotkey_Alt_I(object obj) => StartFile(".\\lib\\Hotkey_Alt_I.bat");
        private void Execute_Hotkey_Alt_J(object obj) => StartFile(".\\lib\\Hotkey_Alt_J.bat");
        private void Execute_Hotkey_Alt_K(object obj) => StartFile(".\\lib\\Hotkey_Alt_K.bat");
        private void Execute_Hotkey_Alt_L(object obj) => StartFile(".\\lib\\Hotkey_Alt_L.bat");
        private void Execute_Hotkey_Alt_M(object obj) => StartFile(".\\lib\\Hotkey_Alt_M.bat");
        private void Execute_Hotkey_Alt_N(object obj) => StartFile(".\\lib\\Hotkey_Alt_N.bat");
        private void Execute_Hotkey_Alt_O(object obj) => StartFile(".\\lib\\Hotkey_Alt_O.bat");
        private void Execute_Hotkey_Alt_P(object obj) => StartFile(".\\lib\\Hotkey_Alt_P.bat");
        private void Execute_Hotkey_Alt_Q(object obj) => StartFile(".\\lib\\Hotkey_Alt_Q.bat");
        private void Execute_Hotkey_Alt_R(object obj) => StartFile(".\\Resources\\Data\\Civilizations_View_List.bat");
        //private void Execute_Hotkey_Alt_R(object obj) { StartFile(".\\lib\\Hotkey_Alt_R.bat"); }
        private void Execute_Hotkey_Alt_S(object obj) => StartFile(".\\lib\\Hotkey_Alt_S.bat");
        private void Execute_Hotkey_Alt_T(object obj) => StartFile(".\\lib\\Hotkey_Alt_T.bat");
        private void Execute_Hotkey_Alt_U(object obj) => StartFile(".\\lib\\Hotkey_Alt_U.bat");
        private void Execute_Hotkey_Alt_V(object obj) => StartFile(".\\lib\\Hotkey_Alt_V.bat");
        private void Execute_Hotkey_Alt_W(object obj) => StartFile(".\\lib\\Hotkey_Alt_W.bat");
        private void Execute_Hotkey_Alt_X(object obj) => StartFile(".\\lib\\Hotkey_Alt_X.bat");
        private void Execute_Hotkey_Alt_Y(object obj) => StartFile(".\\lib\\Hotkey_Alt_Y.bat");
        private void Execute_Hotkey_Alt_Z(object obj) => StartFile(".\\lib\\Hotkey_Alt_Z.bat");







        private void ExecuteShowAllHistoryFileCommand(object obj)
        {
            if (GameContext.Current == null || GameContext.Current.TurnNumber == 1)
            {
                return;
            }

            string file = Path.Combine(
                ResourceManager.GetResourcePath(".\\lib"),
                "AllHistory");
            //file = file.Replace(".\\", "");
            //string _text1;
            _text = "";

            foreach (var civ in GameContext.Current.CivilizationManagers)
            {

                var _hist = civ._civHist_List.ToList();
                _text += newline;

                foreach (var item in _hist)
                {

                    _text +=
                          //newline + "   " + 
                          "Civ+Turn:;_" + item.CivIDHistAndTurn

                        + ";Research;" + item.ResearchHist

                        + ";IntelProd;" + item.IntelProdHist

                        + ";IDef;" + item.IDefHist
                        + ";IAtt;" + item.IAttHist

                        + ";Dil;" + item.DilithiumHist
                        + ";Deut;" + item.DeuteriumHist
                        + ";Dur;" + item.DuraniumHist
                        + ";Morale:;" + item.MoraleHist
                        + ";MoraleG:;" + item.MoraleGlobalHist
                        + ";Col:; " + item.ColoniesHist
                        + ";Pop:; " + item.PopulationHist

                        + ";Credits; " + item.CreditsHist
                        //+ ";Change;" + item.Credits.CurrentChange  // always 0
                        + ";LT; " + item.CreditsHist_LT
                        + ";Maint; " + item.CreditsHist_Maint

                        //+ ";for;" + item.Civilization.CivilizationType + ";"

                        + ";" + item.CivKeyHist
                        + ";" + item.CivIDHist
                        //+ ";" + item.c
                        + newline
                        ;
                    //Console.WriteLine(_text);
                    //GameLog.Core.CivsAndRacesDetails.DebugFormat(_text);
                }
            }

            StreamWriter streamWriter = new StreamWriter(file + ".csv");
            streamWriter.Write(_text);
            streamWriter.Close();
            Thread.Sleep(500);

            //finally
            file += ".txt";
            StreamWriter streamWriter2 = new StreamWriter(file);
            streamWriter2.Write(_text);
            streamWriter2.Close();

            // this blocks following bat file "*.txt" already in usage
            if (!string.IsNullOrEmpty(file) && File.Exists(file))
            {
                if (!string.IsNullOrEmpty(file) && File.Exists(file))
                {
                    ProcessStartInfo processStartInfo = new ProcessStartInfo { UseShellExecute = true, FileName = file };

                    try { _ = Process.Start(processStartInfo); }
                    catch { _ = MessageBox.Show("Could not load Text-File about Players History"); }
                }
            }

            //Thread.Sleep(1500);
            string fileCSV_BAT = Path.Combine(
                ResourceManager.GetResourcePath(".\\lib"),
                "AllHistoryCSV.bat");
            if (!string.IsNullOrEmpty(fileCSV_BAT) && File.Exists(fileCSV_BAT))
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo { UseShellExecute = true, FileName = fileCSV_BAT };

                try { _ = Process.Start(processStartInfo); }
                catch { _ = MessageBox.Show("Could not load Text-File about Players History"); }
            }
        }
        #endregion


        #region Methods
        private void Exit(bool showConfirmation)
        {
            if (_isExiting)
            {
                return;
            }

            _isExiting = true;
            try
            {
                if (!EndGame(showConfirmation))
                {
                    return;
                }
            }
            finally
            {
                _isExiting = false;
            }
            Application.Current.Shutdown();
        }

        private bool EndGame(bool showConfirmation)
        {
            if (showConfirmation && (_appContext.IsGameInPlay || _appContext.IsGameInPlay))
            {
                MessageDialogResult result = MessageDialog.Show(
                    _isExiting ? "Confirm Exit" : "Confirm Quit",
                    "Are you sure you want to " + (_isExiting ? "exit?" : "quit?"),
                    MessageDialogButtons.YesNo);
                if (result != MessageDialogResult.Yes)
                {
                    return false;
                }
            }

            IGameController gameController = Interlocked.CompareExchange(ref _gameController, null, null);

            if (gameController == null)
            {
                return true;
            }

            gameController.Terminate();

            // when current game is terminated, go back to main menu music
            _appContext.ThemeMusicLibrary.Clear();
            _musicPlayer.SwitchMusic("DefaultMusic");

            GameLog.Client.General.Info("Game was exited");

            return true;
        }
        #endregion

        #region Implementation of IModule
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Initialize()
        {
            _text = "Step_0210: Initializing... !";
            Console.WriteLine(_text);
            GameLog.Client.General.InfoFormat(_text);

            RegisterViewsAndServices();
            Console.WriteLine("Step_0220: RegisterViewsAndServices done...");
            RegisterEventHandlers();
            Console.WriteLine("Step_0240: RegisterEventHandlers done...");
            RegisterCommandHandlers();
            Console.WriteLine("Step_0260: RegisterCommandHandlers done...");
            UpdateCommands();
            Console.WriteLine("Step_0280: UpdateCommands done...");

            UIHelpers.IsAutomaticBrowserLaunchEnabled = true;

            if (AutoLoadSavedGame())   // don't show Start Screen
            {
                return;
            }

            _navigationCommands.ActivateScreen.Execute(StandardGameScreens.MenuScreen);

            GameLog.Client.General.InfoFormat("Step_0290: MenuScreen activated... ");
            _soundPlayer.PlayFile("Resources/SoundFX/MenuScreen.ogg");
            Console.WriteLine("Step_0295: Initialize done...");
        }

        private bool AutoLoadSavedGame()
        {
            string savedGameFile = _app.CommandLineArguments.SavedGame;

            if (string.IsNullOrWhiteSpace(savedGameFile))
            {
                return false;
            }

            try
            {
                SavedGameHeader header = SavedGameManager.LoadSavedGameHeader(savedGameFile);
                if (header != null)
                {
                    ClientCommands.LoadGame.Execute(header);
                    return true;
                }
            }
            catch (Exception exception)
            {
                GameLog.Client.General.Error(
                    string.Format(@"Error loading saved game '{0}'.", savedGameFile),
                    exception);
            }

            return false;
        }

        private void RegisterViewsAndServices()
        {
            Console.WriteLine("Step_0214: RegisterViewsAndServices....");

            _ = _container.RegisterInstance(GameOptionsManager.LoadDefaults());

            _ = _container.RegisterType<IScheduler, EventLoopScheduler>(new ContainerControlledLifetimeManager());
            _ = _container.RegisterType<INavigationService, NavigationService>(new ContainerControlledLifetimeManager());

            _ = _container.Resolve<INavigationService>();

            _ = _container.RegisterType<IGameObjectIDService, GameObjectIDService>(new ContainerControlledLifetimeManager());
            _ = _container.RegisterType<ISupremacyCallback, GameClientCallback>(new TransientLifetimeManager());
            _ = _container.RegisterType<IGameClient, GameClient>(new TransientLifetimeManager());
            _ = _container.RegisterType<IGameServer, GameServer>(new TransientLifetimeManager());
            _ = _container.RegisterType<IPlayerOrderService, PlayerOrderService>(new ExternallyControlledLifetimeManager());
            _ = _container.RegisterType<IGameController, GameController>(new TransientLifetimeManager());
            //_container.RegisterInstance<IScriptService>(new ScriptService());*/

            _ = _container.RegisterType<StatusWindow>(new ContainerControlledLifetimeManager());
            _ = _container.RegisterInstance(new CombatWindow());

            _ = _container.RegisterType<GalaxyScreenPresentationModel>(new ExternallyControlledLifetimeManager());
            _ = _container.RegisterType<ColonyScreenPresentationModel>(new ExternallyControlledLifetimeManager());
            _ = _container.RegisterType<DiplomacyScreenViewModel>(new ExternallyControlledLifetimeManager());
            _ = _container.RegisterType<ScienceScreenPresentationModel>(new ExternallyControlledLifetimeManager());
            _ = _container.RegisterType<AssetsScreenPresentationModel>(new ExternallyControlledLifetimeManager());
            _ = _container.RegisterType<EncyclopediaScreenPresentationModel>(new ExternallyControlledLifetimeManager());
            _ = _container.RegisterType<SystemAssaultScreenViewModel>(new ContainerControlledLifetimeManager());

            //_container.RegisterType<ISinglePlayerScreen, SinglePlayerScreen>(new ExternallyControlledLifetimeManager());

            _ = _container.RegisterType<IGalaxyScreenView, GalaxyScreenView>(new ExternallyControlledLifetimeManager());
            _ = _container.RegisterType<IColonyScreenView, ColonyScreenView>(new ExternallyControlledLifetimeManager());
            _ = _container.RegisterType<INewDiplomacyScreenView, NewDiplomacyScreen>(new ExternallyControlledLifetimeManager());
            _ = _container.RegisterType<IScienceScreenView, ResearchScreen>(new ExternallyControlledLifetimeManager());
            // _container.RegisterType<IIntelScreenView, IntelScreen>(new ExternallyControlledLifetimeManager());
            _ = _container.RegisterType<IAssetsScreenView, AssetsScreen>(new ExternallyControlledLifetimeManager());
            _ = _container.RegisterType<IEncyclopediaScreenView, EncyclopediaScreen>(new ExternallyControlledLifetimeManager());
            _ = _container.RegisterType<ILobbyScreenView, MultiplayerLobby>(new ContainerControlledLifetimeManager());
            _ = _container.RegisterType<ISystemAssaultScreenView, SystemAssaultScreen>(new ContainerControlledLifetimeManager());

            _ = _container.RegisterType<IGalaxyScreenPresenter, GalaxyScreenPresenter>(new ExternallyControlledLifetimeManager());
            _ = _container.RegisterType<IColonyScreenPresenter, ColonyScreenPresenter>(new ExternallyControlledLifetimeManager());
            _ = _container.RegisterType<IDiplomacyScreenPresenter, DiplomacyScreenPresenter>(new ExternallyControlledLifetimeManager());
            _ = _container.RegisterType<IScienceScreenPresenter, ScienceScreenPresenter>(new ExternallyControlledLifetimeManager());
            _ = _container.RegisterType<IAssetsScreenPresenter, AssetsScreenPresenter>(new ExternallyControlledLifetimeManager());
            //_container.RegisterType<IScienceScreenPresenter, EncyclodepiaScreenPresenter>(new ExternallyControlledLifetimeManager());
            _ = _container.RegisterType<IEncyclopediaScreenPresenter, EncyclopediaScreenPresenter>(new ExternallyControlledLifetimeManager());

            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.GameScreens, StandardGameScreens.MenuScreen, typeof(MenuScreen));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.GameScreens, StandardGameScreens.MultiplayerLobby, typeof(ILobbyScreenView));

            // first is first shown in Options
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.OptionsPages, typeof(AllOptionsPage));
            //_regionViewRegistry.RegisterViewWithRegion(ClientRegions.OptionsPages, typeof(SecondOptionsPage));
            //_regionViewRegistry.RegisterViewWithRegion(ClientRegions.OptionsPages, typeof(TracesOptionsPage));   // moved into own Dialog
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.TracesPages, typeof(TracesOptionsPage));
            //_regionViewRegistry.RegisterViewWithRegion(ClientRegions.FakeDialog, typeof(FakeDialog));
            //_regionViewRegistry.RegisterViewWithRegion(ClientRegions.OptionsPages, typeof(AudioOptionsPage));   // remove outcomment to be shown again
            //_regionViewRegistry.RegisterViewWithRegion(ClientRegions.OptionsPages, typeof(GraphicsOptionsPage));  // remove outcomment to be shown again
            //_regionViewRegistry.RegisterViewWithRegion(ClientRegions.OptionsPages, typeof(GeneralOptionsPage));
            //_regionViewRegistry.RegisterViewWithRegion(ClientRegions.OptionsPages, typeof(AllOptionsPage));

            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f08_Pages, typeof(F08_Tab_1));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f08_Pages, typeof(F08_Tab_2));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f08_Pages, typeof(F08_Tab_3));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f08_Pages, typeof(F08_Tab_4));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f08_Pages, typeof(F08_Tab_5));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f08_Pages, typeof(F08_Tab_6));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f08_Pages, typeof(F08_Tab_7));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f08_Pages, typeof(F08_Tab_8));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f08_Pages, typeof(F08_Tab_9));

            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f09_Pages, typeof(F09_Tab_1));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f09_Pages, typeof(F09_Tab_2));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f09_Pages, typeof(F09_Tab_3));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f09_Pages, typeof(F09_Tab_4));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f09_Pages, typeof(F09_Tab_5));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f09_Pages, typeof(F09_Tab_6));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f09_Pages, typeof(F09_Tab_7));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f09_Pages, typeof(F09_Tab_8));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f09_Pages, typeof(F09_Tab_9));

            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f10_Pages, typeof(F10_Tab_1));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f10_Pages, typeof(F10_Tab_2));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f10_Pages, typeof(F10_Tab_3));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f10_Pages, typeof(F10_Tab_4));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f10_Pages, typeof(F10_Tab_5));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f10_Pages, typeof(F10_Tab_6));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f10_Pages, typeof(F10_Tab_7));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f10_Pages, typeof(F10_Tab_8));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f10_Pages, typeof(F10_Tab_9));

            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f11_Pages, typeof(F11_Tab_1));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f11_Pages, typeof(F11_Tab_2));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f11_Pages, typeof(F11_Tab_3));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f11_Pages, typeof(F11_Tab_4));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f11_Pages, typeof(F11_Tab_5));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f11_Pages, typeof(F11_Tab_6));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f11_Pages, typeof(F11_Tab_7));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f11_Pages, typeof(F11_Tab_8));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f11_Pages, typeof(F11_Tab_9));


            // _regionViewRegistry.RegisterViewWithRegion(AssetsScreenRegions.SpyList, typeof(SpyListView)); // keep it simple for now
            //_regionViewRegistry.RegisterViewWithRegion(AssetsScreenRegions.EmpireOverview, typeof(EmpireInfoView));
            //_regionViewRegistry.RegisterViewWithRegion(AssetsScreenRegions.EmpireResources, typeof(EmpireResourcesView));
            //_regionViewRegistry.RegisterViewWithRegion(AssetsScreenRegions.GalaxyGrid, typeof(GalaxyGridView));
            //_regionViewRegistry.RegisterViewWithRegion(AssetsScreenRegions.TradeRouteList, typeof(TradeRouteListView));
            //_regionViewRegistry.RegisterViewWithRegion(AssetsScreenRegions.TaskForceList, typeof(TaskForceListView));
            //_regionViewRegistry.RegisterViewWithRegion(AssetsScreenRegions.AssignedShipList, typeof(AssignedShipListView));
            //_regionViewRegistry.RegisterViewWithRegion(AssetsScreenRegions.AvailableShipList, typeof(AvailableShipListView));
            //_regionViewRegistry.RegisterViewWithRegion(AssetsScreenRegions.ShipStats, typeof(ShipInfoPanel));

            _regionViewRegistry.RegisterViewWithRegion(CommonGameScreenRegions.PlanetsView, typeof(StarSystemPanel));
            // _regionViewRegistry.RegisterViewWithRegion(CommonGameScreenRegions.SpyListView, typeof(SpyListView));

            _regionViewRegistry.RegisterViewWithRegion(GalaxyScreenRegions.EmpireOverview, typeof(EmpireInfoView));
            _regionViewRegistry.RegisterViewWithRegion(GalaxyScreenRegions.EmpireResources, typeof(EmpireResourcesView));
            _regionViewRegistry.RegisterViewWithRegion(GalaxyScreenRegions.GalaxyGrid, typeof(GalaxyGridView));
            _regionViewRegistry.RegisterViewWithRegion(GalaxyScreenRegions.TradeRouteList, typeof(TradeRouteListView));
            _regionViewRegistry.RegisterViewWithRegion(GalaxyScreenRegions.TaskForceList, typeof(TaskForceListView));
            _regionViewRegistry.RegisterViewWithRegion(GalaxyScreenRegions.AssignedShipList, typeof(AssignedShipListView));
            _regionViewRegistry.RegisterViewWithRegion(GalaxyScreenRegions.AvailableShipList, typeof(AvailableShipListView));
            _regionViewRegistry.RegisterViewWithRegion(GalaxyScreenRegions.ShipStats, typeof(ShipInfoPanel));

            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.ColonyInfo, ColonyScreenRegions.ColonyInfo, typeof(ColonyInfoView));
            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.ProductionManagement, ColonyScreenRegions.ProductionManagement, typeof(SystemProductionPanel));
            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.PlanetaryBuildQueue, ColonyScreenRegions.PlanetaryBuildQueue, typeof(ColonyPlanetaryBuildQueueView));
            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.PlanetaryBuildList, ColonyScreenRegions.PlanetaryBuildList, typeof(ColonyPlanetaryBuildListView));
            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.ShipyardBuildList, ColonyScreenRegions.ShipyardBuildList, typeof(ColonyShipyardBuildListView));
            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.SelectedPlanetaryBuildProjectInfo, ColonyScreenRegions.SelectedPlanetaryBuildProjectInfo, typeof(ColonyBuildProjectInfoView));
            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.ShipyardBuildQueue, ColonyScreenRegions.ShipyardBuildQueue, typeof(ColonyShipyardBuildQueueView));
            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.ShipyardBuildList, ColonyScreenRegions.ShipyardBuildList, typeof(ColonyShipyardBuildListView));
            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.SelectedShipyardBuildProjectInfo, ColonyScreenRegions.SelectedShipyardBuildProjectInfo, typeof(ColonyBuildProjectInfoView));
            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.StructureList, ColonyScreenRegions.StructureList, typeof(ColonyStructureListView));
            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.HandlingList, ColonyScreenRegions.HandlingList, typeof(ColonyHandlingListView));
        }

        private void RegisterEventHandlers()
        {
            _ = ClientEvents.ClientConnectionFailed.Subscribe(OnClientConnectionFailed, ThreadOption.UIThread);
            _ = ClientEvents.ClientDisconnected.Subscribe(OnClientDisconnected, ThreadOption.UIThread);
            _ = ClientEvents.GameStarted.Subscribe(OnGameStarted, ThreadOption.UIThread);
            _ = ClientEvents.GameStarting.Subscribe(OnGameStarting, ThreadOption.UIThread);
            _ = ClientEvents.GameEnding.Subscribe(OnGameEnding, ThreadOption.UIThread);
            _ = ClientEvents.ClientConnected.Subscribe(OnClientConnected, ThreadOption.BackgroundThread);
            _ = ClientEvents.LocalPlayerJoined.Subscribe(OnLocalPlayerJoined, ThreadOption.UIThread);
            _ = ClientEvents.PlayerExited.Subscribe(OnPlayerExited, ThreadOption.UIThread);

            _ = Channel<GameSavedMessage>.Public
                .ObserveOn(Scheduler.ThreadPool)
                .Subscribe(_ => ShellIntegration.UpdateJumpList());
            Console.WriteLine("Step_0230: RegisterEventHandlers done...");
        }

        private void OnPlayerExited(ClientDataEventArgs<IPlayer> args)
        {
            IPlayer player = args.Value;

            if (!_appContext.IsGameInPlay)
            {
                return;
            }

            if (Equals(player, _appContext.LocalPlayer))
            {
                return;
            }

            IEnumerable<IPlayer> remainingPlayers = _appContext.RemotePlayers.Where(o => !Equals(o, player));
            if (!remainingPlayers.Any())
            {
                MessageDialogResult result = MessageDialog.Show(
                    _resourceManager.GetString("PLAYER_EXITED_MESSAGE_HEADER"),
                    _resourceManager.GetStringFormat("LAST_PLAYER_EXITED_MESSAGE_CONTENT", player.Name),
                    MessageDialogButtons.YesNo);
                if (result == MessageDialogResult.No)
                {
                    _ = EndGame(false);
                }
            }
            else
            {
                _ = MessageDialog.Show(
                    _resourceManager.GetString("PLAYER_EXITED_MESSAGE_HEADER"),
                    _resourceManager.GetStringFormat("PLAYER_EXITED_MESSAGE_CONTENT", player.Name),
                    MessageDialogButtons.Ok);
            }
        }

        private void OnLocalPlayerJoined(LocalPlayerJoinedEventArgs args)
        {
            if (!_appContext.IsSinglePlayerGame)
            {
                ClearStatusWindow();
            }
        }

        private void OnGameStarting(ClientEventArgs obj)
        {
            if (!_appContext.IsSinglePlayerGame)
            {
                ShowLoadingScreen();
            }
        }

        private void ShowLoadingScreen()
        {
            StatusWindow statusWindow = _container.Resolve<StatusWindow>();
            //statusWindow.Header = _resourceManager.GetString("LOADING_GAME_MESSAGE");
            statusWindow.Header = " ***     Loading Game . . .      ***"; // +Environment.NewLine;


            statusWindow.Content = Environment.NewLine
            + Environment.NewLine + "----------------------------------------------------------------------------------------------------------------------------------------------"
            + Environment.NewLine + "For more information on game play please read the manual or press F9-Key."
            + Environment.NewLine + "----------------------------------------------------------------------------------------------------------------------------------------------"
            + Environment.NewLine + "Star Trek and all related marks, logos and characters are solely owned by CBS Studios Inc."
            + Environment.NewLine + "This fan production is not endorsed by, sponsored by, nor affiliated with CBS, Paramount Pictures, or"
            + Environment.NewLine + "any other Star Trek franchise, and is a non-commercial fan-made game intended for recreational use."
            + Environment.NewLine + "No commercial exhibition or distribution is permitted. No alleged independent rights will be asserted"
            + Environment.NewLine + "against CBS or Paramount Pictures."
            + Environment.NewLine + "----------------------------------------------------------------------------------------------------------------------------------------------"
            + Environment.NewLine + "This work is licensed under the Creative Commons"
            + Environment.NewLine + "Attribution - NonCommercial - ShareAlike 4.0 International ( CC BY - NC - SA 4.0 )"
            + Environment.NewLine 
            + Environment.NewLine 
            + Environment.NewLine + ">>>  if you reach an empty screen press F1 or do a right click"
            ;

            

            //string techlvl = "3";
            //var options = localEmpireID;
            //string techlvl = startTechLvl;
            //string techlvl = _appContext.LobbyData.GameOptions.StartingTechLevel.ToString();
            //string empireID = _appContext.LocalPlayerEmpire.Civilization.Key.Substring(3, 0);

            string introTextCase;  // SinglePlayerGame working
            //string introTextCase = "empty_introTextCase";


            introTextCase = localEmpire + startTechLvl;  // startTechLvl = -1 shown
            if (startTechLvl == -1)
            {
                introTextCase = _resourceManager.GetString("GAME_START_INFO_LOADING_GAME");
            }
            //"...history from the saved game continues ... let's see what the future will bring...";

            try
            {
                if (_appContext.RemotePlayers != null)
                {
                    introTextCase = _resourceManager.GetString("GAME_START_INFO_MP_JOINER_LOADING_GAME");
                }
                //"...Competition to Supremacy of Galaxy begins... join and let your empire raise ...";
            }
            catch { }

            if (_appContext.IsGameHost == true)
            {
                introTextCase = _resourceManager.GetString("GAME_START_INFO_MP_HOSTER_LOADING_GAME");
            }
            //"...Competition to Supremacy of Galaxy begins... let your empire raise and lead others ...";



            GameLog.Client.GameInitDataDetails.DebugFormat("introTextCase = {0}", introTextCase);
            //string introTextCase = "FED1"; 
            string introText = Environment.NewLine;
            //+ "----------------------------------------------------------------------------------------------------------------------------------------------"
            //+ Environment.NewLine;
            try
            {
                introText += _resourceManager.GetString(introTextCase);
            }
            catch { introText = ""; }

            statusWindow.Content = introText + statusWindow.Content + Environment.NewLine;

            //var _red = statusWindow.

////Brush GlobalBlueBrush = Brush.;
//            switch (localEmpire)
//            {
//                default:
//                    statusWindow.Background = Path. GlobalBlueBrush;
//                    break;
//            }
            



            // Hints screen will not show for host of a multiplayer game so is excluded here, the host cannot progress to the loaded game.
            // to do line 425 to 435 add hint for people new to game

            //if (_appContext.IsSinglePlayerGame == false)   // see below, depending on Length out of en.txt or later on OPTION
            {
                string _hints = _resourceManager.GetString("LOADING_GAME_HINTS");

                if (_hints.Length > 0)   // later: make additional OPTION to show hints or not
                {
                    _ = MessageDialog.Show(statusWindow.Content = _resourceManager.GetString("LOADING_GAME_HINTS"),
                                MessageDialogButtons.Ok);
                    //"Remember:" + Environment.NewLine +
                    //    "- Right mouse click in the game to see the Panel Access Menu." + Environment.NewLine +
                    //    "- Before assaulting a system declare war in an earlier turn." + Environment.NewLine +
                    //    "- In Diplomacy select a race then click the Outbox to declare war. " + Environment.NewLine +
                    //    "- To conquer a system assault it with transport ships in your taskforce.",
                }
            }

            //statusWindow.Content = null;
            statusWindow.Show();

            IRegion gameScreensRegion = _container.Resolve<IRegionManager>().Regions[ClientRegions.GameScreens];
            gameScreensRegion.Deactivate(gameScreensRegion.GetView(StandardGameScreens.MenuScreen));
            gameScreensRegion.Deactivate(gameScreensRegion.GetView(StandardGameScreens.MultiplayerLobby));

        }

        private void OnGameEnding(ClientEventArgs obj) => UpdateCommands();

        private void OnClientConnected(ClientConnectedEventArgs obj) => UpdateCommands();

        private void UpdateCommands()
        {
            bool isConnected = _appContext.IsConnected;
            bool isGameEnding = _appContext.IsGameEnding;
            bool isGameInPlay = _appContext.IsGameInPlay;
            bool gameControllerExists = Interlocked.CompareExchange(ref _gameController, null, null) != null;

            _optionsCommand.IsActive = true;
            _tracesCommand.IsActive = true;
            _f06_Command.IsActive = true;  // F1 - F5 already used
            _f07_Command.IsActive = true;
            _f08_Command.IsActive = true;
            _f09_Command.IsActive = true;
            _f10_Command.IsActive = true;
            _f11_Command.IsActive = true;
            _f12_Command.IsActive = true;
            _ctrl_f01_Command.IsActive = true;
            //_ctrl_f02_Command.IsActive = true;
            //_ctrl_f03_Command.IsActive = true;
            //_ctrl_f04_Command.IsActive = true;
            //_ctrl_f05_Command.IsActive = true;
            _ctrl_f06_Command.IsActive = true;
            _ctrl_f07_Command.IsActive = true;
            //_ctrl_f08_Command.IsActive = true;
            //_ctrl_f09_Command.IsActive = true;
            //_ctrl_f10_Command.IsActive = true;
            //_ctrl_f11_Command.IsActive = true;
            //_ctrl_f12_Command.IsActive = true;
            _s0_Command.IsActive = true;
            _s1_Command.IsActive = true;
            _s2_Command.IsActive = true;
            _s3_Command.IsActive = true;
            _s4_Command.IsActive = true;
            _s5_Command.IsActive = true;
            _s6_Command.IsActive = true;
            //_fakeCommand.IsActive = true;
            _logTxtCommand.IsActive = true;
            _errorTxtCommand.IsActive = true;
            _showCreditsDialogCommand.IsActive = true;
            _showSettingsFileCommand.IsActive = true;
            _showPlayersHistoryFileCommand.IsActive = true;
            _showAllHistoryFileCommand.IsActive = true;
            _startSinglePlayerGameCommand.IsActive = !isConnected && !isGameEnding && !gameControllerExists;
            _joinMultiplayerGameCommand.IsActive = !isConnected && !isGameEnding && !gameControllerExists;
            _hostMultiplayerGameCommand.IsActive = !isConnected && !isGameEnding && !gameControllerExists;
            _loadGameCommand.IsActive = !isConnected && !isGameEnding && !gameControllerExists;
            _deleteManualSavedGameCommand.IsActive = true;
            _deleteAutoSavedGameCommand.IsActive = true;
            _continueGameCommand.IsActive = isGameInPlay;
            _endGameCommand.IsActive = isConnected && !isGameEnding;

            _Hotkey_Alt_D0.IsActive = true;
            _Hotkey_Alt_D1.IsActive = true;
            _Hotkey_Alt_D2.IsActive = true;
            _Hotkey_Alt_D3.IsActive = true;
            _Hotkey_Alt_D4.IsActive = true;
            _Hotkey_Alt_D5.IsActive = true;
            _Hotkey_Alt_D6.IsActive = true;
            _Hotkey_Alt_D7.IsActive = true;
            _Hotkey_Alt_D8.IsActive = true;
            _Hotkey_Alt_D9.IsActive = true;

            //_OnMA_Ferengi.IsActive = true;


            _Hotkey_Alt_F01.IsActive = true;
            _Hotkey_Alt_F02.IsActive = true;
            _Hotkey_Alt_F03.IsActive = true;
            _Hotkey_Alt_F04.IsActive = true;
            _Hotkey_Alt_F05.IsActive = true;
            _Hotkey_Alt_F06.IsActive = true;
            _Hotkey_Alt_F07.IsActive = true;
            _Hotkey_Alt_F08.IsActive = true;
            _Hotkey_Alt_F09.IsActive = true;
            _Hotkey_Alt_F10.IsActive = true;
            _Hotkey_Alt_F11.IsActive = true;

            _Hotkey_Alt_A.IsActive = true;
            _Hotkey_Alt_B.IsActive = true;
            _Hotkey_Alt_C.IsActive = true;
            _Hotkey_Alt_D.IsActive = true;
            _Hotkey_Alt_E.IsActive = true;
            _Hotkey_Alt_F.IsActive = true;
            _Hotkey_Alt_G.IsActive = true;
            _Hotkey_Alt_H.IsActive = true;
            _Hotkey_Alt_I.IsActive = true;
            _Hotkey_Alt_J.IsActive = true;
            _Hotkey_Alt_K.IsActive = true;
            _Hotkey_Alt_L.IsActive = true;
            _Hotkey_Alt_M.IsActive = true;
            _Hotkey_Alt_N.IsActive = true;
            _Hotkey_Alt_O.IsActive = true;
            _Hotkey_Alt_P.IsActive = true;
            _Hotkey_Alt_Q.IsActive = true;
            _Hotkey_Alt_R.IsActive = true;
            _Hotkey_Alt_S.IsActive = true;
            _Hotkey_Alt_T.IsActive = true;
            _Hotkey_Alt_U.IsActive = true;
            _Hotkey_Alt_V.IsActive = true;
            _Hotkey_Alt_W.IsActive = true;
            _Hotkey_Alt_X.IsActive = true;
            _Hotkey_Alt_Y.IsActive = true;
            _Hotkey_Alt_Z.IsActive = true;

        }

        private void OnGameStarted(ClientDataEventArgs<GameStartData> obj)
        {
            UpdateCommands();

            if (_appContext.IsGameInPlay)
            {
                if (_appContext.LocalPlayer.Empire.Key == "FEDERATION")
                {
                    LoadTheme("Federation");
                    localCivID = 0;
                }
                else if (_appContext.LocalPlayer.Empire.Key == "TERRANEMPIRE")
                {
                    LoadTheme("TerranEmpire");
                    localCivID = 1;
                }
                else if (_appContext.LocalPlayer.Empire.Key == "ROMULANS")
                {
                    LoadTheme("Romulans");
                    localCivID = 2;
                }
                else if (_appContext.LocalPlayer.Empire.Key == "KLINGONS")
                {
                    LoadTheme("Klingons");
                    localCivID = 3;
                }
                else if (_appContext.LocalPlayer.Empire.Key == "CARDASSIANS")
                {
                    LoadTheme("Cardassians");
                    localCivID = 4;
                }
                else if (_appContext.LocalPlayer.Empire.Key == "DOMINION")
                {
                    LoadTheme("Dominion");
                    localCivID = 5;
                    LocalCivID();
                }
                else if (_appContext.LocalPlayer.Empire.Key == "BORG")
                {
                    LoadTheme("Borg");
                    localCivID = 6;
                }

                else
                {
                    _ = MessageBox.Show("Empire is set to NOT-Playable - falling back to Default - Please restart, Select Single Player Menu and set Empire Playable to YES");
                    LoadDefaultTheme();
                }

            }
        }

        // hopefully info about played empire public available
        private int LocalCivID() => localCivID;

        public void LoadDefaultTheme() => _ = _app.LoadDefaultResources();

        public void LoadTheme(string theme)
        {
            //works: GameLog.Client.GameData.DebugFormat("ClientModule.cs: UI-Theme={0} (or default), EmpireID={1}", theme, _appContext.LocalPlayer.EmpireID);

            if (!_app.LoadThemeResources(theme))
            {
                _ = _app.LoadDefaultResources();
            }

            ThemeShipyard = theme;

            _ = _app.LoadThemeResourcesShipyard(ThemeShipyard);

            // load theme music
            _appContext.ThemeMusicLibrary.Load(Path.Combine(MusicThemeBasePath, theme, MusicPackFileName));
            _musicPlayer.SwitchMusic("DefaultMusic");
        }

        private void OnClientConnectionFailed(EventArgs args)
        {
            ClearStatusWindow();

            _ = MessageDialog.Show(
                _resourceManager.GetString("CLIENT_CONNECTION_FAILURE_HEADER"),
                _resourceManager.GetString("CLIENT_CONNECTION_FAILURE_MESSAGE"),
                MessageDialogButtons.Ok);

            IGameController gameController = Interlocked.Exchange(ref _gameController, null);
            if (gameController == null)
            {
                return;
            }

            try
            {
                gameController.Terminate();
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
            }

            UpdateCommands();
        }

        protected void ClearStatusWindow()
        {
            StatusWindow statusWindow = _container.Resolve<StatusWindow>();
            if ((statusWindow != null) && statusWindow.IsOpen)
            {
                statusWindow.Close();
            }
        }

        private void OnClientDisconnected(DataEventArgs<ClientDisconnectReason> args)
        {
            ClearStatusWindow();

            IGameController gameController = Interlocked.Exchange(ref _gameController, null);
            if (gameController == null)
            {
                return;
            }

            string disconnectMessage = null;

            switch (args.Value)
            {
                case ClientDisconnectReason.ConnectionBroken:
                    disconnectMessage = _resourceManager.GetString("CLIENT_DISCONNECT_MESSAGE_CONNECTION_BROKEN");
                    break;
                case ClientDisconnectReason.GameAlreadyStarted:
                    disconnectMessage = _resourceManager.GetString("CLIENT_DISCONNECT_MESSAGE_GAME_ALREADY_STARTED");
                    break;
                case ClientDisconnectReason.GameIsFull:
                    disconnectMessage = _resourceManager.GetString("CLIENT_DISCONNECT_MESSAGE_GAME_IS_FULL");
                    break;
                case ClientDisconnectReason.VersionMismatch:
                    disconnectMessage = _resourceManager.GetString("CLIENT_DISCONNECT_MESSAGE_VERSION_MISMATCH");
                    break;
                case ClientDisconnectReason.ConnectionClosed:
                    disconnectMessage = _resourceManager.GetString("CLIENT_DISCONNECT_MESSAGE_CONNECTION_CLOSED");
                    break;
                case ClientDisconnectReason.LoadGameFailure:
                    disconnectMessage = _resourceManager.GetString("CLIENT_DISCONNECT_MESSAGE_LOAD_GAME_FAILURE");
                    break;
                case ClientDisconnectReason.LocalServiceFailure:
                    disconnectMessage = _resourceManager.GetString("CLIENT_DISCONNECT_MESSAGE_LOCAL_SERVICE_FAILURE");
                    break;
                case ClientDisconnectReason.UnknownFailure:
                    disconnectMessage = _resourceManager.GetString("CLIENT_DISCONNECT_MESSAGE_UNKNOWN_FAILURE");
                    break;
            }

            if (disconnectMessage != null)
            {
                _ = MessageDialog.Show(
                    _resourceManager.GetString("CLIENT_DISCONNECT_HEADER"),
                    disconnectMessage,
                    MessageDialogButtons.Ok);
            }

            try
            {
                _navigationCommands.ActivateScreen.Execute(StandardGameScreens.MenuScreen);
            }
            finally
            {
                try
                {
                    gameController.Terminate();
                }
                catch (Exception e)
                {
                    GameLog.Client.General.Error(e);
                }
            }

            UpdateCommands();

            //AsyncHelper.Invoke((Action)GC.Collect);
        }
        #endregion

        private void RegisterCommandHandlers()
        {
            ClientCommands.OptionsCommand.RegisterCommand(_optionsCommand);
            ClientCommands.TracesCommand.RegisterCommand(_tracesCommand);
            ClientCommands.F06_Command.RegisterCommand(_f06_Command); // F1 - F5 already used
            ClientCommands.F07_Command.RegisterCommand(_f07_Command);
            ClientCommands.F08_Command.RegisterCommand(_f08_Command);
            ClientCommands.F09_Command.RegisterCommand(_f09_Command);
            ClientCommands.F10_Command.RegisterCommand(_f10_Command);
            ClientCommands.F11_Command.RegisterCommand(_f11_Command);
            ClientCommands.F12_Command.RegisterCommand(_f12_Command);

            ClientCommands.CTRL_F01_Command.RegisterCommand(_ctrl_f01_Command);
            //ClientCommands.CTRL_F02_Command.RegisterCommand(_ctrl_f02_Command);
            //ClientCommands.CTRL_F03_Command.RegisterCommand(_ctrl_f03_Command);
            //ClientCommands.CTRL_F04_Command.RegisterCommand(_ctrl_f04_Command);
            //ClientCommands.CTRL_F05_Command.RegisterCommand(_ctrl_f05_Command);
            ClientCommands.CTRL_F06_Command.RegisterCommand(_ctrl_f06_Command);
            ClientCommands.CTRL_F07_Command.RegisterCommand(_ctrl_f07_Command);
            //ClientCommands.CTRL_F08_Command.RegisterCommand(_ctrl_f08_Command);
            //ClientCommands.CTRL_F09_Command.RegisterCommand(_ctrl_f09_Command);
            //ClientCommands.CTRL_F10_Command.RegisterCommand(_ctrl_f10_Command);
            //ClientCommands.CTRL_F11_Command.RegisterCommand(_ctrl_f11_Command);
            //ClientCommands.CTRL_F12_Command.RegisterCommand(_ctrl_f12_Command);

            ClientCommands.S0_Command.RegisterCommand(_s0_Command);
            ClientCommands.S1_Command.RegisterCommand(_s1_Command);
            ClientCommands.S2_Command.RegisterCommand(_s2_Command);
            ClientCommands.S3_Command.RegisterCommand(_s3_Command);
            ClientCommands.S4_Command.RegisterCommand(_s4_Command);
            ClientCommands.S5_Command.RegisterCommand(_s5_Command);
            ClientCommands.S6_Command.RegisterCommand(_s6_Command);

            //ClientCommands.FakeCommand.RegisterCommand(_fakeCommand);
            ClientCommands.LogTxtCommand.RegisterCommand(_logTxtCommand);
            ClientCommands.ErrorTxtCommand.RegisterCommand(_errorTxtCommand);
            ClientCommands.StartSinglePlayerGame.RegisterCommand(_startSinglePlayerGameCommand);
            ClientCommands.ContinueGame.RegisterCommand(_continueGameCommand);
            ClientCommands.EndGame.RegisterCommand(_endGameCommand);
            ClientCommands.JoinMultiplayerGame.RegisterCommand(_joinMultiplayerGameCommand);
            ClientCommands.HostMultiplayerGame.RegisterCommand(_hostMultiplayerGameCommand);
            ClientCommands.LoadGame.RegisterCommand(_loadGameCommand);
            ClientCommands.SaveGameDeleteManualSaved.RegisterCommand(_deleteManualSavedGameCommand);
            ClientCommands.SaveGameDeleteAutoSaved.RegisterCommand(_deleteAutoSavedGameCommand);
            ClientCommands.ShowCreditsDialog.RegisterCommand(_showCreditsDialogCommand);
            ClientCommands.ShowSettingsFileCommand.RegisterCommand(_showSettingsFileCommand);
            ClientCommands.ShowPlayersHistoryFileCommand.RegisterCommand(_showPlayersHistoryFileCommand);
            ClientCommands.ShowAllHistoryFileCommand.RegisterCommand(_showAllHistoryFileCommand);
            ClientCommands.Exit.RegisterCommand(_exitCommand);

            //ClientCommands.OnMA_Ferengi.RegisterCommand(_OnMA_Ferengi);

            ClientCommands.Hotkey_Alt_D0.RegisterCommand(_Hotkey_Alt_D0);
            ClientCommands.Hotkey_Alt_D1.RegisterCommand(_Hotkey_Alt_D1);
            ClientCommands.Hotkey_Alt_D2.RegisterCommand(_Hotkey_Alt_D2);
            ClientCommands.Hotkey_Alt_D3.RegisterCommand(_Hotkey_Alt_D3);
            ClientCommands.Hotkey_Alt_D4.RegisterCommand(_Hotkey_Alt_D4);
            ClientCommands.Hotkey_Alt_D5.RegisterCommand(_Hotkey_Alt_D5);
            ClientCommands.Hotkey_Alt_D6.RegisterCommand(_Hotkey_Alt_D6);
            ClientCommands.Hotkey_Alt_D7.RegisterCommand(_Hotkey_Alt_D7);
            ClientCommands.Hotkey_Alt_D8.RegisterCommand(_Hotkey_Alt_D8);
            ClientCommands.Hotkey_Alt_D9.RegisterCommand(_Hotkey_Alt_D9);


            ClientCommands.Hotkey_Alt_F01.RegisterCommand(_Hotkey_Alt_F01);
            ClientCommands.Hotkey_Alt_F02.RegisterCommand(_Hotkey_Alt_F02);
            ClientCommands.Hotkey_Alt_F03.RegisterCommand(_Hotkey_Alt_F03);
            ClientCommands.Hotkey_Alt_F04.RegisterCommand(_Hotkey_Alt_F04);
            ClientCommands.Hotkey_Alt_F05.RegisterCommand(_Hotkey_Alt_F05);
            ClientCommands.Hotkey_Alt_F06.RegisterCommand(_Hotkey_Alt_F06);
            ClientCommands.Hotkey_Alt_F07.RegisterCommand(_Hotkey_Alt_F07);
            ClientCommands.Hotkey_Alt_F08.RegisterCommand(_Hotkey_Alt_F08);
            ClientCommands.Hotkey_Alt_F09.RegisterCommand(_Hotkey_Alt_F09);
            ClientCommands.Hotkey_Alt_F10.RegisterCommand(_Hotkey_Alt_F10);
            ClientCommands.Hotkey_Alt_F11.RegisterCommand(_Hotkey_Alt_F11);

            ClientCommands.Hotkey_Alt_A.RegisterCommand(_Hotkey_Alt_A);
            ClientCommands.Hotkey_Alt_B.RegisterCommand(_Hotkey_Alt_B);
            ClientCommands.Hotkey_Alt_C.RegisterCommand(_Hotkey_Alt_C);
            ClientCommands.Hotkey_Alt_D.RegisterCommand(_Hotkey_Alt_D);
            ClientCommands.Hotkey_Alt_E.RegisterCommand(_Hotkey_Alt_E);
            ClientCommands.Hotkey_Alt_F.RegisterCommand(_Hotkey_Alt_F);
            ClientCommands.Hotkey_Alt_G.RegisterCommand(_Hotkey_Alt_G);
            ClientCommands.Hotkey_Alt_H.RegisterCommand(_Hotkey_Alt_H);
            ClientCommands.Hotkey_Alt_I.RegisterCommand(_Hotkey_Alt_I);
            ClientCommands.Hotkey_Alt_J.RegisterCommand(_Hotkey_Alt_J);
            ClientCommands.Hotkey_Alt_K.RegisterCommand(_Hotkey_Alt_K);
            ClientCommands.Hotkey_Alt_L.RegisterCommand(_Hotkey_Alt_L);
            ClientCommands.Hotkey_Alt_M.RegisterCommand(_Hotkey_Alt_M);
            ClientCommands.Hotkey_Alt_N.RegisterCommand(_Hotkey_Alt_N);
            ClientCommands.Hotkey_Alt_O.RegisterCommand(_Hotkey_Alt_O);
            ClientCommands.Hotkey_Alt_P.RegisterCommand(_Hotkey_Alt_P);
            ClientCommands.Hotkey_Alt_Q.RegisterCommand(_Hotkey_Alt_Q);
            ClientCommands.Hotkey_Alt_R.RegisterCommand(_Hotkey_Alt_R);
            ClientCommands.Hotkey_Alt_S.RegisterCommand(_Hotkey_Alt_S);
            ClientCommands.Hotkey_Alt_T.RegisterCommand(_Hotkey_Alt_T);
            ClientCommands.Hotkey_Alt_U.RegisterCommand(_Hotkey_Alt_U);
            ClientCommands.Hotkey_Alt_V.RegisterCommand(_Hotkey_Alt_V);
            ClientCommands.Hotkey_Alt_W.RegisterCommand(_Hotkey_Alt_W);
            ClientCommands.Hotkey_Alt_X.RegisterCommand(_Hotkey_Alt_X);
            ClientCommands.Hotkey_Alt_Y.RegisterCommand(_Hotkey_Alt_Y);
            ClientCommands.Hotkey_Alt_Z.RegisterCommand(_Hotkey_Alt_Z);




        }
        private void ExecuteSP_DirectlyGameCommand(int _id)
        {
            if (_appContext.IsGameInPlay)
            {
                return;
            }

            SinglePlayerStartScreen startScreen = new SinglePlayerStartScreen(_soundPlayer);
            GameOptions options = startScreen.Options;

            switch (_id)
            {
                case 0: options.FederationPlayable = EmpirePlayable.Yes; break;
                case 1: options.TerranEmpirePlayable = EmpirePlayable.Yes; break;
                case 2: options.RomulanPlayable = EmpirePlayable.Yes; break;
                case 3: options.KlingonPlayable = EmpirePlayable.Yes; break;
                case 4: options.CardassianPlayable = EmpirePlayable.Yes; break;
                case 5: options.DominionPlayable = EmpirePlayable.Yes; break;
                case 6: options.BorgPlayable = EmpirePlayable.Yes; break;
                default:
                    break;
            }

            _text = "Step_1000: GameInitData.CreateSinglePlayerGame .... ";
            Console.WriteLine(_text);
            GameLog.Client.GameData.DebugFormat(_text);
            GameInitData initData = GameInitData.CreateSinglePlayerGame(startScreen.Options, _id);
            
            localEmpire = GetLocalEmpireShortage(_id, out string localempire);
            startTechLvl = GetStartTechLvl(startScreen.Options.StartingTechLevel.ToString());

            _text = "Step_1300: RunLocal.... ";
            Console.WriteLine(_text);
            GameLog.Client.GameData.DebugFormat(_text);

            RunGameController(gameController => gameController.RunLocal(initData), false);
        }

        private void ExecuteStartSinglePlayerGameCommand(object parameter)
        {
            if (Interlocked.CompareExchange(ref _gameController, null, null) != null)
            {
                return;
            }

            LoadDefaultTheme();

            SinglePlayerStartScreen startScreen = new SinglePlayerStartScreen(_soundPlayer);
            GameOptions options = startScreen.Options;

            bool? dialogResult = startScreen.ShowDialog();
            if (!dialogResult.HasValue || !dialogResult.Value)
            {
                return;
            }

            GameInitData initData = GameInitData.CreateSinglePlayerGame(options, startScreen.EmpireID);

            localEmpire = GetLocalEmpireShortage(startScreen.EmpireID, out string localempire);
            startTechLvl = GetStartTechLvl(startScreen.Options.StartingTechLevel.ToString());

            RunGameController(gameController => gameController.RunLocal(initData), false);

            // activate following for switching to using MP-Screen as well for SP
            //var initData = GameInitData.CreateMultiplayerGame(GameOptionsManager.LoadDefaults(), "LOCAL PLAYER");
            //RunGameController(gameController => gameController.RunLocal(initData), true);
        }

        private int GetStartTechLvl(string startTechLvlText)
        {
            switch (startTechLvlText)
            {
                case "Early": startTechLvl = 1; break;
                case "Developed": startTechLvl = 2; break;
                case "Sophisticated": startTechLvl = 3; break;
                case "Advanced": startTechLvl = 4; break;
                case "Supreme": startTechLvl = 5; break;
                default:
                    startTechLvl = 1;
                    break;
            }
            return startTechLvl;
        }

        private string GetLocalEmpireShortage(int empireID, out string localEmpire)
        {
            switch (empireID)
            {
                case 0: localEmpire = "FED"; break;
                case 1: localEmpire = "TER"; break;
                case 2: localEmpire = "ROM"; break;
                case 3: localEmpire = "KLI"; break;
                case 4: localEmpire = "CAR"; break;
                case 5: localEmpire = "DOM"; break;
                case 6: localEmpire = "BOR"; break;
                default:
                    localEmpire = "FED";
                    break;
            }
            return localEmpire;
        }

        private void RunGameController(Action<IGameController> runDelegate, bool remoteConnection)
        {
            if (Interlocked.CompareExchange(ref _gameController, null, null) != null)
            {
                return;
            }

            try
            {
                _text = "Step_1310: RunDelegate.... ";
                Console.WriteLine(_text);
                GameLog.Client.GameData.DebugFormat(_text);

                _gameController = ResolveGameController();

                if (remoteConnection)
                {
                    ShowConnectingScreen();
                }
                else
                {
                    _text = "Step_1320: ShowLoadingScreen.... ";
                    Console.WriteLine(_text);
                    GameLog.Client.GameData.DebugFormat(_text);

                    ShowLoadingScreen();
                }

                _text = "Step_1330: BeginInvoke.... ";
                Console.WriteLine(_text);
                GameLog.Client.GameData.DebugFormat(_text);

                _ = runDelegate.BeginInvoke(
                    _gameController,
                    delegate (IAsyncResult result)
                    {
                        try
                        {
                            runDelegate.EndInvoke(result);
                            //GameLog.Print("trying runDelegate.EndInvoke");
                        }
                        catch (SupremacyException e)
                        {
                            GameLog.Client.General.Error("runDelegate.EndInvoke failed", e);
                            _ = Interlocked.Exchange(ref _gameController, null);
                            _dispatcherService.InvokeAsync((Action)ClearStatusWindow);
                            _errorService.HandleError(e);
                            _dispatcherService.InvokeAsync((Action)ActivateMenuScreen);
                        }
                    },
                    null);
            }
            catch (SupremacyException e)
            {
                GameLog.Client.General.Error("ResolveGameController failed", e);
                ClearStatusWindow();
                _errorService.HandleError(e);
                _ = Interlocked.Exchange(ref _gameController, null);
                ActivateMenuScreen();
            }

            //ShowLoadingScreen();  // additional showing

        }

        private void ShowConnectingScreen()
        {
            StatusWindow statusWindow = _container.Resolve<StatusWindow>();
            statusWindow.Header = "Connecting";
            statusWindow.Content = null;
            statusWindow.Show();

        }

        protected void DeactivateMenuScreen()
        {
            IRegion region = _regionManager.Regions[ClientRegions.GameScreens];
            if (region == null)
            {
                return;
            }

            object menuScreen = region.GetView(StandardGameScreens.MenuScreen);
            if (menuScreen == null)
            {
                return;
            }

            region.Deactivate(menuScreen);
        }

        protected void ActivateMenuScreen()
        {
            IRegion region = _regionManager.Regions[ClientRegions.GameScreens];
            if (region == null)
            {
                return;
            }

            object menuScreen = region.GetView(StandardGameScreens.MenuScreen);
            if (menuScreen == null)
            {
                return;
            }

            region.Activate(menuScreen);
        }
        //private void ExecuteOnMA_UFP(object parameter) { Process.Start("https://memory-alpha.fandom.com/wiki/United_Federation_of_Planets"); }
        //private void ExecuteOnMA_TERRAN(object parameter) { Process.Start("https://memory-alpha.fandom.com/wiki/Star_Trek:_Enterprise"); }
        //private void ExecuteOnMA_ROM(object parameter) { Process.Start("https://memory-alpha.fandom.com/wiki/Star_Trek:_Enterprise"); }
        //private void ExecuteOnMA_KLING(object parameter) { Process.Start("https://memory-alpha.fandom.com/wiki/Star_Trek:_Enterprise"); }
        //private void ExecuteOnMA_CARD(object parameter) { Process.Start("https://memory-alpha.fandom.com/wiki/Star_Trek:_Enterprise"); }
        //private void ExecuteOnMA_DOM(object parameter) { Process.Start("https://memory-alpha.fandom.com/wiki/Star_Trek:_Enterprise"); }
        //private void ExecuteOnMA_BORG(object parameter) { Process.Start("https://memory-alpha.fandom.com/wiki/Star_Trek:_Enterprise"); }

        //private void ExecuteOnMA_Ferengi(object parameter) { Process.Start("https://memory-alpha.fandom.com/wiki/Star_Trek:_Enterprise"); }


            private IGameController ResolveGameController()
        {
            GCHelper.Collect();

            IGameController gameController = _container.Resolve<IGameController>() ?? throw new SupremacyException("A game controller could not be created.");

            if (Interlocked.CompareExchange(ref _gameController, gameController, null) != null)
            {
                return _gameController;
            }

            gameController.Terminated += OnGameControllerTerminated;
            return gameController;
        }

        private void OnGameControllerTerminated(object sender, EventArgs args)
        {
            if (!(sender is IGameController gameController))
            {
                return;
            }

            gameController.Terminated -= OnGameControllerTerminated;
            _ = Interlocked.CompareExchange(ref _gameController, null, gameController);
            _app.DoEvents();
            GC.Collect(2, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            UpdateCommands();
        }

        public string ThemeShipyard { get; set; }
    }
}