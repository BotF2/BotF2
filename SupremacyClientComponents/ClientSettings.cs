// File:ClientSettings.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xaml;
using XamlReader = System.Windows.Markup.XamlReader;
using XamlWriter = System.Windows.Markup.XamlWriter;

using Supremacy.Collections;
using Supremacy.Resources;
using Supremacy.Utility;


namespace Supremacy.Client
{
    public class ClientSettings : DependencyObject, IAttachedPropertyStore, INotifyPropertyChanged
    {
        private const string ClientSettingsFileName = "SupremacyClient..Settings.xaml";

        bool _tracingClientSettings = false;
        public bool _XML2CSVOutput = false;


        private static ClientSettings _current;

        private readonly Dictionary<AttachableMemberIdentifier, object> _attachedValues;

        static ClientSettings()
        {
            EnableDialogAnimationsProperty.AddOwner(typeof(UIElement));
        }

        public ClientSettings()
        {
            try
            {
                // not here     _ = MessageBox.Show("New SupremacyClient..Settings.xaml generated", "INFO", MessageBoxButton.OK);
                _attachedValues = new Dictionary<AttachableMemberIdentifier, object>();
            }
            catch
            {
                GameLog.Client.General.ErrorFormat("Problem with SupremacyClient..Settings.xaml");
            }
        }

        public static ClientSettings Current
        {
            get
            {
                if (_current == null)
                    _current = LoadCore();
                return _current;
            }
        }

        public event EventHandler Saved;

        private void OnSaved()
        {
            Saved?.Invoke(null, EventArgs.Empty);

            var settingsDirectory = ResourceManager.GetResourcePath("");
            var filePath = Path.Combine(
                settingsDirectory,
                ClientSettingsFileName);

            if (_tracingClientSettings)
                GameLog.Client.General.InfoFormat("SAVE     {0}: Content: " + Environment.NewLine + Environment.NewLine + "{1}" + Environment.NewLine, filePath, File.ReadAllText(filePath));
        }

        public event EventHandler Loaded;

        private void OnLoaded()
        {
            Loaded?.Invoke(null, EventArgs.Empty);
        }

        public void Reload()
        {
            try
            {
                var savedOrDefaultSettings = LoadCore();
                var localValueEnumerator = savedOrDefaultSettings.GetLocalValueEnumerator();

                while (localValueEnumerator.MoveNext())
                {
                    var currentEntry = localValueEnumerator.Current;
                    GameLog.Client.General.DebugFormat("RELOAD: Property {0} = {1}",
                        currentEntry.Property, currentEntry.Value);
                    SetValue(
                        currentEntry.Property,
                        currentEntry.Value);
                }

                var removedMembers = _attachedValues.Keys
                    .Where(o => !savedOrDefaultSettings._attachedValues.ContainsKey(o))
                    .ToList();

                foreach (var attachableMemberIdentifier in removedMembers)
                {
                    AttachablePropertyServices.RemoveProperty(this, attachableMemberIdentifier);
                    GameLog.Client.General.DebugFormat("RELOAD: REMOVED entry: {0} = {1}",
                        attachableMemberIdentifier);
                }

                foreach (var key in _attachedValues.Keys)
                { 
                    AttachablePropertyServices.SetProperty(this, key, _attachedValues[key]);
                    GameLog.Client.General.DebugFormat("RELOAD: ADDED entry: {0} = {1}",
                        key, _attachedValues[key]);
                }

                OnLoaded();
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
            }
        }

        public void Save()
        {
            try
            {
                var settingsDirectory = ResourceManager.GetResourcePath("");

                var filePath = Path.Combine(
                    settingsDirectory,
                    ClientSettingsFileName);

                if (!Directory.Exists(settingsDirectory))
                    Directory.CreateDirectory(settingsDirectory);

                using (var fileWriter = File.Create(filePath))
                {
                    XamlWriter.Save(this, fileWriter);
                }

                OnSaved();
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
            }
        }

        private static ClientSettings LoadCore()
        {
            try
            {
                var settingsDirectory = ResourceManager.GetResourcePath("");

                var filePath = Path.Combine(
                    settingsDirectory,
                    ClientSettingsFileName);

                ClientSettings settings;

               
                if (File.Exists(filePath))
                {
                    using (var fileReader = File.OpenRead(filePath))
                    {
                        settings = new ClientSettings();
                        try
                        {
                            // filePath = SupremacyClient_Settings.xaml
                            string _text = "for problems: just try to deleted " + filePath + " manually from your hard disk !";
                            GameLog.Client.General.InfoFormat(_text);
                            Console.WriteLine(_text);
                            settings = XamlReader.Load(fileReader) as ClientSettings ??
                                       new ClientSettings();

                            GameLog.Client.General.InfoFormat("LOADCORE {0}: Content: " + Environment.NewLine + Environment.NewLine + "{1}" + Environment.NewLine, filePath, File.ReadAllText(filePath));
                        
                            if (settings == null)
                                settings = new ClientSettings();

                            settings.OnLoaded();
                        }
                        catch (Exception e)
                        {

                            //_ = System.Windows.MessageBox.Show("please stop the game and delete manually: " + Environment.NewLine + filePath, "PROBLEM",MessageBoxButton.OK);

                            string _text = "LOADCORE "+ filePath+": Problem reading the file >> will be deleted" + Environment.NewLine + e;
                            GameLog.Client.General.InfoFormat(_text);
                            Console.WriteLine(_text);
                            File.Delete(filePath);
                            //LoadCore();  // retry
                        }



                    }
                    return settings;
                }
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
            }

            return new ClientSettings();
        }

        #region EnableDialogAnimations (Attached Property)
        public static readonly DependencyProperty EnableDialogAnimationsProperty =
            DependencyProperty.RegisterAttached(
                "EnableDialogAnimations",
                typeof(bool),
                typeof(ClientSettings),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.Inherits));

        public static bool GetEnableDialogAnimations(DependencyObject d)
        {
            return (bool)d.GetValue(EnableDialogAnimationsProperty);
        }

        public static void SetEnableDialogAnimations(DependencyObject d, bool value)
        {
            d.SetValue(EnableDialogAnimationsProperty, value);
        }

        public bool EnableDialogAnimations
        {
            get { return GetEnableDialogAnimations(this); }
            set { SetEnableDialogAnimations(this, value); }
        }
        #endregion

        #region EnableFullScreenMode Property
        public static readonly DependencyProperty EnableFullScreenModeProperty = DependencyProperty.Register(
            "EnableFullScreenMode",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool EnableFullScreenMode
        {
            get { return (bool)GetValue(EnableFullScreenModeProperty); }
            set { SetValue(EnableFullScreenModeProperty, value); }
        }
        #endregion

        #region MasterVolume Property
        public static readonly DependencyProperty MasterVolumeProperty = DependencyProperty.Register(
            "MasterVolume",
            typeof(double),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                0.2,
                FrameworkPropertyMetadataOptions.None,
                (o, args) => ((ClientSettings)o).OnMasterVolumeChanged((double)args.OldValue, (double)args.NewValue)));

        public double MasterVolume
        {
            get { return (double)GetValue(MasterVolumeProperty); }
            set { SetValue(MasterVolumeProperty, value); }
        }

        public event EventHandler<PropertyChangedRoutedEventArgs<double>> MasterVolumeChanged;

        private void OnMasterVolumeChanged(double oldValue, double newValue)
        {
            MasterVolumeChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<double>(oldValue, newValue));
        }
        #endregion

        #region MusicVolume Property
        public static readonly DependencyProperty MusicVolumeProperty = DependencyProperty.Register(
            "MusicVolume",
            typeof(double),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                0.1,
                FrameworkPropertyMetadataOptions.None,
                (o, args) => ((ClientSettings)o).OnMusicVolumeChanged((double)args.OldValue, (double)args.NewValue)));

        public double MusicVolume
        {
            get { return (double)GetValue(MusicVolumeProperty); }
            set { SetValue(MusicVolumeProperty, value); }
        }

        public event EventHandler<PropertyChangedRoutedEventArgs<double>> MusicVolumeChanged;

        private void OnMusicVolumeChanged(double oldValue, double newValue)
        {
            MusicVolumeChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<double>(oldValue, newValue));
        }
        #endregion

        #region FXVolume Property
        public static readonly DependencyProperty FXVolumeProperty = DependencyProperty.Register(
            "FXVolume",
            typeof(double),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                0.2,
                FrameworkPropertyMetadataOptions.None,
                (o, args) => ((ClientSettings)o).OnFXVolumeChanged((double)args.OldValue, (double)args.NewValue)));

        public double FXVolume
        {
            get { return (double)GetValue(FXVolumeProperty); }
            set { SetValue(FXVolumeProperty, value); }
        }

        public event EventHandler<PropertyChangedRoutedEventArgs<double>> FXVolumeChanged;

        private void OnFXVolumeChanged(double oldValue, double newValue)
        {
            FXVolumeChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<double>(oldValue, newValue));
        }
        #endregion

        #region EnableAntiAliasing Property
        public static readonly DependencyProperty EnableAntiAliasingProperty = DependencyProperty.Register(
            "EnableAntiAliasing",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None,
                (o, args) => ((ClientSettings)o).OnEnableAntiAliasingChanged((bool)args.OldValue, (bool)args.NewValue)));

        public bool EnableAntiAliasing
        {
            get { return (bool)GetValue(EnableAntiAliasingProperty); }
            set { SetValue(EnableAntiAliasingProperty, value); }
        }

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> EnableAntiAliasingChanged;

        private void OnEnableAntiAliasingChanged(bool oldValue, bool newValue)
        {
            EnableAntiAliasingChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));
        }
        #endregion

        #region EnableHighQualityScaling Property
        public static readonly DependencyProperty EnableHighQualityScalingProperty = DependencyProperty.Register(
            "EnableHighQualityScaling",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public bool EnableHighQualityScaling
        {
            get { return (bool)GetValue(EnableHighQualityScalingProperty); }
            set { SetValue(EnableHighQualityScalingProperty, value); }
        }
        #endregion

        #region EnableStarMapAnimations Property
        public static readonly DependencyProperty EnableStarMapAnimationsProperty = DependencyProperty.Register(
            "EnableStarMapAnimations",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public bool EnableStarMapAnimations
        {
            get { return (bool)GetValue(EnableStarMapAnimationsProperty); }
            set { SetValue(EnableStarMapAnimationsProperty, value); }
        }
        #endregion

        #region EnableAnimation Property
        public static readonly DependencyProperty EnableAnimationProperty = DependencyProperty.Register(
            "EnableAnimation",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public bool EnableAnimation
        {
            get { return (bool)GetValue(EnableAnimationProperty); }
            set { SetValue(EnableAnimationProperty, value); }
        }
        #endregion

        #region EnableCombatScreen Property
        public static readonly DependencyProperty EnableCombatScreenProperty = DependencyProperty.Register(
            "EnableCombatScreen",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public bool EnableCombatScreen
        {
            get { return (bool)GetValue(EnableCombatScreenProperty); }
            set { SetValue(EnableCombatScreenProperty, value); }
        }
        #endregion

        #region Traces_SetAll_without_DetailsProperty
        public static readonly DependencyProperty Traces_SetAll_without_DetailsProperty = DependencyProperty.Register(
            "SetAll_without_Details",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));


        public bool Traces_SetAll_without_Details
        {
            get { return (bool)GetValue(Traces_SetAll_without_DetailsProperty); }
            set
            {
                SetValue(Traces_SetAll_without_DetailsProperty, value);

                GameLog.Client.General.InfoFormat("#### Log.Txt: Traces (** SET ALL **) to DEBUG (press ingame CTRL + Z)");  // in Log.Txt only DEBUG = yes get a line

                if (value == true)
                {
                    // "General" shows the Log.txt-lines for all the others
                    SetValue(TracesGeneralProperty, value); /*OnTracesGeneralChanged(false, true);*/ GameLog.SetRepositoryToDebug("General");
                    // Audio changes shall be done directly = OnTracesAudioChanged

                    SetValue(TracesAIProperty, value); /*OnTracesAIChanged(false, true);*/ GameLog.SetRepositoryToDebug("AI");
                    SetValue(TracesAudioProperty, value); OnTracesAudioChanged(false, true); GameLog.SetRepositoryToDebug("Audio");
                    SetValue(TracesCivsAndRacesProperty, value); /*OnTracesCivsAndRacesChanged(false, true);*/ GameLog.SetRepositoryToDebug("CivsAndRaces");
                    SetValue(TracesColoniesProperty, value); /*OnTracesColoniesChanged(false, true);*/ GameLog.SetRepositoryToDebug("Colonies");
                    SetValue(TracesCombatProperty, value); /*OnTracesCombatChanged(false, true);*/ GameLog.SetRepositoryToDebug("Combat");
                    // no SetValue(TracesCombatDetailsProperty, value); /*OnTracesCombatDetailsChanged(false, true);*/ GameLog.SetRepositoryToDebug("CombatDetails");
                    SetValue(TracesCreditsProperty, value); /*OnTracesCreditsChanged(false, true);*/ GameLog.SetRepositoryToDebug("Credits");
                    SetValue(TracesDeuteriumProperty, value); /*OnTracesDeuteriumChanged(false, true);*/ GameLog.SetRepositoryToDebug("Deuterium");
                    SetValue(TracesDilithiumProperty, value); /*OnTracesDilithiumChanged(false, true);*/ GameLog.SetRepositoryToDebug("Dilithium");
                    SetValue(TracesDiplomacyProperty, value); /*OnTracesDiplomacyChanged(false, true);*/ GameLog.SetRepositoryToDebug("Diplomacy");
                    SetValue(TracesEnergyProperty, value); /*OnTracesEnergyChanged(false, true);*/ GameLog.SetRepositoryToDebug("Energy");
                    SetValue(TracesEventsProperty, value); /*OnTracesEventsChanged(false, true);*/ GameLog.SetRepositoryToDebug("Events");
                    SetValue(TracesGalaxyGeneratorProperty, value); /*OnTracesGalaxyGeneratorChanged(false, true);*/ GameLog.SetRepositoryToDebug("GalaxyGenerator");
                    SetValue(TracesGameDataProperty, value); /*OnTracesGameDataChanged(false, true);*/ GameLog.SetRepositoryToDebug("GameData");
                    //SetValue(TracesGameInitDataProperty, value); /*OnTracesGameInitDataChanged(false, true);*/ GameLog.SetRepositoryToDebug("GameInitData");
                    SetValue(TracesGeneralProperty, value); /*OnTracesGeneralChanged(false, true);*/ GameLog.SetRepositoryToDebug("General");
                    SetValue(TracesIntelProperty, value); /*OnTracesIntelChanged(false, true);*/ GameLog.SetRepositoryToDebug("Intel");
                    SetValue(TracesMapDataProperty, value); /*OnTracesMapDataChanged(false, true);*/ GameLog.SetRepositoryToDebug("MapData");
                    SetValue(TracesMultiPlayProperty, value); /*OnTracesMultiPlayChanged(false, true);*/ GameLog.SetRepositoryToDebug("MultiPlay");
                    SetValue(TracesProductionProperty, value); /*OnTracesProductionChanged(false, true);*/ GameLog.SetRepositoryToDebug("Production");
                    //SetValue(TracesReportErrorsProperty, value); /*OnTracesReportErrorsChanged(false, true);*/ GameLog.SetRepositoryToDebug("ReportErrors");
                    SetValue(TracesResearchProperty, value); /*OnTracesResearchChanged(false, true);*/ GameLog.SetRepositoryToDebug("Research");
                    SetValue(TracesSitRepsProperty, value); /*OnTracesSitRepsChanged(false, true);*/ GameLog.SetRepositoryToDebug("SitReps");
                    SetValue(TracesSaveLoadProperty, value); /*OnTracesSaveLoadChanged(false, true);*/ GameLog.SetRepositoryToDebug("SaveLoad");
                    SetValue(TracesShipsProperty, value); /*OnTracesShipsChanged(false, true);*/ GameLog.SetRepositoryToDebug("Ships");
                    SetValue(TracesShipProductionProperty, value); /*OnTracesShipProductionChanged(false, true);*/ GameLog.SetRepositoryToDebug("ShipProduction");
                    SetValue(TracesStationsProperty, value); /*OnTracesStationsChanged(false, true);*/ GameLog.SetRepositoryToDebug("Stations");
                    SetValue(TracesStructuresProperty, value); /*OnTracesStructuresChanged(false, true);*/ GameLog.SetRepositoryToDebug("Structures");
                    SetValue(TracesSystemAssaultProperty, value); /*OnTracesSystemAssaultChanged(false, true);*/ GameLog.SetRepositoryToDebug("SystemAssault");
                    // no  SetValue(TracesSystemAssaultDetailsProperty, value); /*OnTracesSystemAssaultDetailsChanged(false, true);*/ GameLog.SetRepositoryToDebug("SystemAssaultDetails");
                    SetValue(TracesTestProperty, value); /*OnTracesTestChanged(false, true);*/ GameLog.SetRepositoryToDebug("Test");
                    SetValue(TracesTradeRoutesProperty, value); /*OnTracesTradeRoutesChanged(false, true);*/ GameLog.SetRepositoryToDebug("TradeRoutes");
                    SetValue(TracesUIProperty, value); /*OnTracesUIChanged(false, true);*/ GameLog.SetRepositoryToDebug("UI");
                    SetValue(TracesXMLCheckProperty, value); /*OnTracesXMLCheckChanged(false, true);*/ GameLog.SetRepositoryToDebug("XMLCheck");
                    SetValue(TracesXML2CSVOutputProperty, value); /*OnTracesXML2CSVOutputChanged(false, true);*/ GameLog.SetRepositoryToDebug("XML2CSVOutput");

                    //Reload();

                    //SendKeys.SendWait("{ENTER}");  // doesn't work - close OptionsDialog ...(and reload)
                    //Thread.Sleep(1000);
                    //SendKeys.SendWait("^o"); // OptionsDialog
                    //Thread.Sleep(1000);
                }
            }
        }
        #endregion Traces_SetAll_without_Details

        #region Traces_SetAll_and_Details Property
        public static readonly DependencyProperty Traces_SetAll_and_DetailsProperty = DependencyProperty.Register(
            "SetAll_and_Details",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));


        public bool Traces_SetAll_and_Details
        {
            get { return (bool)GetValue(Traces_SetAll_and_DetailsProperty); }
            set
            {
                SetValue(Traces_SetAll_and_DetailsProperty, value);

                GameLog.Client.General.InfoFormat("#### Log.Txt: Traces (** Set All and DETAILS OME **) to DEBUG (press ingame CTRL + Z)");  // in Log.Txt only DEBUG = yes get a line

                if (value == true)
                {
                    // "General" shows the Log.txt-lines for all the others
                    SetValue(TracesGeneralProperty, value); /*OnTracesGeneralChanged(false, true);*/ GameLog.SetRepositoryToDebug("General");
                    // Audio changes shall be done directly = OnTracesAudioChanged

                    SetValue(TracesAIProperty, value); /*OnTracesAIChanged(false, true);*/ GameLog.SetRepositoryToDebug("AI");
                    //SetValue(TracesAudioProperty, value); OnTracesAudioChanged(false, true); GameLog.SetRepositoryToDebug("Audio");
                    SetValue(TracesCivsAndRacesProperty, value); /*OnTracesCivsAndRacesChanged(false, true);*/ GameLog.SetRepositoryToDebug("CivsAndRaces");
                    SetValue(TracesColoniesProperty, value); /*OnTracesColoniesChanged(false, true);*/ GameLog.SetRepositoryToDebug("Colonies");
                    SetValue(TracesCombatProperty, value); /*OnTracesCombatChanged(false, true);*/ GameLog.SetRepositoryToDebug("Combat");
                    SetValue(TracesCombatDetailsProperty, value); /*OnTracesCombatDetailsChanged(false, true);*/ GameLog.SetRepositoryToDebug("CombatDetails");
                    SetValue(TracesCreditsProperty, value); /*OnTracesCreditsChanged(false, true);*/ GameLog.SetRepositoryToDebug("Credits");
                    SetValue(TracesDeuteriumProperty, value); /*OnTracesDeuteriumChanged(false, true);*/ GameLog.SetRepositoryToDebug("Deuterium");
                    SetValue(TracesDilithiumProperty, value); /*OnTracesDilithiumChanged(false, true);*/ GameLog.SetRepositoryToDebug("Dilithium");
                    SetValue(TracesDiplomacyProperty, value); /*OnTracesDiplomacyChanged(false, true);*/ GameLog.SetRepositoryToDebug("Diplomacy");
                    SetValue(TracesEnergyProperty, value); /*OnTracesEnergyChanged(false, true);*/ GameLog.SetRepositoryToDebug("Energy");
                    SetValue(TracesEventsProperty, value); /*OnTracesEventsChanged(false, true);*/ GameLog.SetRepositoryToDebug("Events");
                    SetValue(TracesGalaxyGeneratorProperty, value); /*OnTracesGalaxyGeneratorChanged(false, true);*/ GameLog.SetRepositoryToDebug("GalaxyGenerator");
                    SetValue(TracesGameDataProperty, value); /*OnTracesGameDataChanged(false, true);*/ GameLog.SetRepositoryToDebug("GameData");
                    //SetValue(TracesGameInitDataProperty, value); /*OnTracesGameInitDataChanged(false, true);*/ GameLog.SetRepositoryToDebug("GameInitData");
                    SetValue(TracesGeneralProperty, value); /*OnTracesGeneralChanged(false, true);*/ GameLog.SetRepositoryToDebug("General");
                    SetValue(TracesIntelProperty, value); /*OnTracesIntelChanged(false, true);*/ GameLog.SetRepositoryToDebug("Intel");
                    SetValue(TracesMapDataProperty, value); /*OnTracesMapDataChanged(false, true);*/ GameLog.SetRepositoryToDebug("MapData");
                    SetValue(TracesMultiPlayProperty, value); /*OnTracesMultiPlayChanged(false, true);*/ GameLog.SetRepositoryToDebug("MultiPlay");
                    SetValue(TracesProductionProperty, value); /*OnTracesProductionChanged(false, true);*/ GameLog.SetRepositoryToDebug("Production");
                    //SetValue(TracesReportErrorsProperty, value); /*OnTracesReportErrorsChanged(false, true);*/ GameLog.SetRepositoryToDebug("ReportErrors");
                    SetValue(TracesResearchProperty, value); /*OnTracesResearchChanged(false, true);*/ GameLog.SetRepositoryToDebug("Research");
                    SetValue(TracesSitRepsProperty, value); /*OnTracesSitRepsChanged(false, true);*/ GameLog.SetRepositoryToDebug("SitReps");
                    SetValue(TracesSaveLoadProperty, value); /*OnTracesSaveLoadChanged(false, true);*/ GameLog.SetRepositoryToDebug("SaveLoad");
                    SetValue(TracesShipsProperty, value); /*OnTracesShipsChanged(false, true);*/ GameLog.SetRepositoryToDebug("Ships");
                    SetValue(TracesShipProductionProperty, value); /*OnTracesShipProductionChanged(false, true);*/ GameLog.SetRepositoryToDebug("ShipProduction");
                    SetValue(TracesStationsProperty, value); /*OnTracesStationsChanged(false, true);*/ GameLog.SetRepositoryToDebug("Stations");
                    SetValue(TracesStructuresProperty, value); /*OnTracesStructuresChanged(false, true);*/ GameLog.SetRepositoryToDebug("Structures");
                    SetValue(TracesSystemAssaultProperty, value); /*OnTracesSystemAssaultChanged(false, true);*/ GameLog.SetRepositoryToDebug("SystemAssault");
                    SetValue(TracesSystemAssaultDetailsProperty, value); /*OnTracesSystemAssaultDetailsChanged(false, true);*/ GameLog.SetRepositoryToDebug("SystemAssaultDetails");
                    SetValue(TracesTestProperty, value); /*OnTracesTestChanged(false, true);*/ GameLog.SetRepositoryToDebug("Test");
                    SetValue(TracesTradeRoutesProperty, value); /*OnTracesTradeRoutesChanged(false, true);*/ GameLog.SetRepositoryToDebug("TradeRoutes");
                    SetValue(TracesUIProperty, value); /*OnTracesUIChanged(false, true);*/ GameLog.SetRepositoryToDebug("UI");
                    SetValue(TracesXMLCheckProperty, value); /*OnTracesXMLCheckChanged(false, true);*/ GameLog.SetRepositoryToDebug("XMLCheck");
                    SetValue(TracesXML2CSVOutputProperty, value); /*OnTracesXML2CSVOutputChanged(false, true);*/ GameLog.SetRepositoryToDebug("XML2CSVOutput");

                    //Reload();

                    //SendKeys.SendWait("{ENTER}");  // doesn't work - close OptionsDialog ...(and reload)
                    //Thread.Sleep(1000);
                    //SendKeys.SendWait("^o"); // OptionsDialog
                    //Thread.Sleep(1000);
                }
            }
        }
        #endregion Traces_SetAll_and_Details

        #region Traces_ClearAll Property
        public static readonly DependencyProperty Traces_ClearAllProperty = DependencyProperty.Register(
            "ClearAll",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));
        public bool Traces_ClearAll
        {
            get { return (bool)GetValue(Traces_ClearAllProperty); }
            set
            {
                SetValue(Traces_ClearAllProperty, value);


                //if (value == true)
                //{
                //    SetValue(Traces_SetAllProperty, false);
                //    //SetValue(Traces_SetSomeProperty, false);
                //}

                GameLog.Client.General.InfoFormat("#### Log.Txt: Traces mostly set to ERROR only (press ingame CTRL + Z)");  // in Log.Txt only DEBUG = yes get a line

                if (value == true)
                {
                    value = false;
                    // "General" shows the Log.txt-lines for all the others
                    //SetValue(TracesGeneralProperty, true); 
                    GameLog.SetRepositoryToDebug("General");

                    // Audio changes shall be done directly = OnTracesAudioChanged
                    SetValue(TracesAIProperty, value); /*OnTracesAIChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("AI");
                    SetValue(TracesAudioProperty, value); OnTracesAudioChanged(false, true); GameLog.SetRepositoryToErrorOnly("Audio");
                    SetValue(TracesCivsAndRacesProperty, value); /*OnTracesCivsAndRacesChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("CivsAndRaces");
                    SetValue(TracesColoniesProperty, value); /*OnTracesColoniesChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Colonies");
                    SetValue(TracesCombatProperty, value); /*OnTracesCombatChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Combat");
                    SetValue(TracesCombatDetailsProperty, value); /*OnTracesCombatDetailsChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("CombatDetails");
                    SetValue(TracesCreditsProperty, value); /*OnTracesCreditsChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Credits");
                    SetValue(TracesDeuteriumProperty, value); /*OnTracesDeuteriumChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Deuterium");
                    SetValue(TracesDilithiumProperty, value); /*OnTracesDilithiumChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Dilithium");
                    SetValue(TracesDiplomacyProperty, value); /*OnTracesDiplomacyChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Diplomacy");
                    SetValue(TracesEnergyProperty, value); /*OnTracesEnergyChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Energy");
                    SetValue(TracesEventsProperty, value); /*OnTracesEventsChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Events");
                    SetValue(TracesGalaxyGeneratorProperty, value); /*OnTracesGalaxyGeneratorChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("GalaxyGenerator");
                    SetValue(TracesGameDataProperty, value); /*OnTracesGameDataChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("GameData");
                    SetValue(TracesGameInitDataProperty, value); /*OnTracesGameInitDataChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("GameInitData");
                    // "General" shows the Log.txt-lines for all the others => do this at the end
                    //SetValue(TracesGeneralProperty, value); /*OnTracesGeneralChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("General");
                    SetValue(TracesIntelProperty, value); /*OnTracesIntelChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Intel");
                    SetValue(TracesMapDataProperty, value); /*OnTracesMapDataChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("MapData");
                    SetValue(TracesMultiPlayProperty, value); /*OnTracesMultiPlayChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("MultiPlay");
                    SetValue(TracesProductionProperty, value); /*OnTracesProductionChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Production");
                    //SetValue(TracesReportErrorsProperty, value); /*OnTracesReportErrorsChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("ReportErrors");
                    SetValue(TracesResearchProperty, value); /*OnTracesResearchChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Research");
                    SetValue(TracesSitRepsProperty, value); /*OnTracesSitRepsChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("SitReps");
                    SetValue(TracesSaveLoadProperty, value); /*OnTracesSaveLoadChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("SaveLoad");
                    SetValue(TracesShipsProperty, value); /*OnTracesShipsChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Ships");
                    SetValue(TracesShipProductionProperty, value); /*OnTracesShipProductionChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("ShipProduction");
                    SetValue(TracesStationsProperty, value); /*OnTracesStationsChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Stations");
                    SetValue(TracesStructuresProperty, value); /*OnTracesStructuresChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Structures");
                    SetValue(TracesSystemAssaultProperty, value); /*OnTracesSystemAssaultChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("SystemAssault");
                    SetValue(TracesSystemAssaultDetailsProperty, value); /*OnTracesSystemAssaultDetailsChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("SystemAssaultDetails");
                    SetValue(TracesTestProperty, value); /*OnTracesTestChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Test");
                    SetValue(TracesTradeRoutesProperty, value); /*OnTracesTradeRoutesChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("TradeRoutes");
                    SetValue(TracesUIProperty, value); /*OnTracesUIChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("UI");
                    SetValue(TracesXMLCheckProperty, value); /*OnTracesXMLCheckChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("XMLCheck");
                    SetValue(TracesXML2CSVOutputProperty, value); OnTracesXML2CSVOutputChanged(false, true); GameLog.SetRepositoryToErrorOnly("XML2CSVOutput");

                    // "General" shows the Log.txt-lines for all the others => do this at the end
                    GameLog.Client.General.DebugFormat("At last turning of GENERAL");
                    SetValue(TracesGeneralProperty, value); /*OnTracesGeneralChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("General");

                    //SendKeys.SendWait("{ENTER}");  // doesn't work - close OptionsDialog ...(and reload)
                    //Thread.Sleep(1000);
                    //SendKeys.SendWait("^o"); // OptionsDialog
                    //Thread.Sleep(1000);

                    //Reload();
                }
            }
        }
        #endregion Traces_ClearAll

        #region Traces_ClearAllDetails Property
        public static readonly DependencyProperty Traces_ClearAllDetailsProperty = DependencyProperty.Register(
            "ClearAllDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));
        public bool Traces_ClearAllDetails
        {
            get { return (bool)GetValue(Traces_ClearAllDetailsProperty); }
            set
            {
                SetValue(Traces_ClearAllDetailsProperty, value);


                //if (value == true)
                //{
                //    SetValue(Traces_SetAllProperty, false);
                //    //SetValue(Traces_SetSomeProperty, false);
                //}

                GameLog.Client.General.InfoFormat("#### Log.Txt: Traces mostly set to ERROR only (press ingame CTRL + Z)");  // in Log.Txt only DEBUG = yes get a line

                if (value == true)
                {
                    value = false;
                    // "General" shows the Log.txt-lines for all the others
                    //SetValue(TracesGeneralProperty, true); 
                    GameLog.SetRepositoryToDebug("General");

                    // Audio changes shall be done directly = OnTracesAudioChanged
                    //SetValue(TracesAIProperty, value); /*OnTracesAIChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("AI");
                    //SetValue(TracesAudioProperty, value); OnTracesAudioChanged(false, true); GameLog.SetRepositoryToErrorOnly("Audio");
                    //SetValue(TracesCivsAndRacesProperty, value); /*OnTracesCivsAndRacesChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("CivsAndRaces");
                    //SetValue(TracesColoniesProperty, value); /*OnTracesColoniesChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Colonies");
                    //SetValue(TracesCombatProperty, value); /*OnTracesCombatChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Combat");
                    SetValue(TracesCombatDetailsProperty, value); /*OnTracesCombatDetailsChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("CombatDetails");
                    //SetValue(TracesCreditsProperty, value); /*OnTracesCreditsChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Credits");
                    //SetValue(TracesDeuteriumProperty, value); /*OnTracesDeuteriumChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Deuterium");
                    //SetValue(TracesDilithiumProperty, value); /*OnTracesDilithiumChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Dilithium");
                    //SetValue(TracesDiplomacyProperty, value); /*OnTracesDiplomacyChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Diplomacy");
                    //SetValue(TracesEnergyProperty, value); /*OnTracesEnergyChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Energy");
                    //SetValue(TracesEventsProperty, value); /*OnTracesEventsChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Events");
                    //SetValue(TracesGalaxyGeneratorProperty, value); /*OnTracesGalaxyGeneratorChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("GalaxyGenerator");
                    //SetValue(TracesGameDataProperty, value); /*OnTracesGameDataChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("GameData");
                    //SetValue(TracesGameInitDataProperty, value); /*OnTracesGameInitDataChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("GameInitData");
                    //// "General" shows the Log.txt-lines for all the others => do this at the end
                    ////SetValue(TracesGeneralProperty, value); /*OnTracesGeneralChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("General");
                    //SetValue(TracesIntelProperty, value); /*OnTracesIntelChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Intel");
                    //SetValue(TracesMapDataProperty, value); /*OnTracesMapDataChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("MapData");
                    //SetValue(TracesMultiPlayProperty, value); /*OnTracesMultiPlayChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("MultiPlay");
                    //SetValue(TracesProductionProperty, value); /*OnTracesProductionChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Production");
                    ////SetValue(TracesReportErrorsProperty, value); /*OnTracesReportErrorsChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("ReportErrors");
                    //SetValue(TracesResearchProperty, value); /*OnTracesResearchChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Research");
                    //SetValue(TracesSitRepsProperty, value); /*OnTracesSitRepsChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("SitReps");
                    //SetValue(TracesSaveLoadProperty, value); /*OnTracesSaveLoadChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("SaveLoad");
                    //SetValue(TracesShipsProperty, value); /*OnTracesShipsChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Ships");
                    //SetValue(TracesShipProductionProperty, value); /*OnTracesShipProductionChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("ShipProduction");
                    //SetValue(TracesStationsProperty, value); /*OnTracesStationsChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Stations");
                    //SetValue(TracesStructuresProperty, value); /*OnTracesStructuresChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Structures");
                    //SetValue(TracesSystemAssaultProperty, value); /*OnTracesSystemAssaultChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("SystemAssault");
                    SetValue(TracesSystemAssaultDetailsProperty, value); /*OnTracesSystemAssaultDetailsChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("SystemAssaultDetails");
                    //SetValue(TracesTestProperty, value); /*OnTracesTestChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("Test");
                    //SetValue(TracesTradeRoutesProperty, value); /*OnTracesTradeRoutesChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("TradeRoutes");
                    //SetValue(TracesUIProperty, value); /*OnTracesUIChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("UI");
                    //SetValue(TracesXMLCheckProperty, value); /*OnTracesXMLCheckChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("XMLCheck");
                    //SetValue(TracesXML2CSVOutputProperty, value); /*OnTracesXML2CSVOutputChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("XML2CSVOutput");

                    // "General" shows the Log.txt-lines for all the others => do this at the end
                    GameLog.Client.General.DebugFormat("At last turning of GENERAL");
                    SetValue(TracesGeneralProperty, value); /*OnTracesGeneralChanged(false, true);*/ GameLog.SetRepositoryToErrorOnly("General");

                    //SendKeys.SendWait("{ENTER}");  // doesn't work - close OptionsDialog ...(and reload)
                    //Thread.Sleep(1000);
                    //SendKeys.SendWait("^o"); // OptionsDialog
                    //Thread.Sleep(1000);

                    //Reload();
                }
            }
        }
        #endregion Traces_ClearAllDetails


        #region Traces_SetSome Property
        public static readonly DependencyProperty Traces_SetSomeProperty = DependencyProperty.Register(
            "SetSome",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));


        public bool Traces_SetSome  // some making most sense
        {
            get { return (bool)GetValue(Traces_SetSomeProperty); }
            set
            {
                SetValue(Traces_SetSomeProperty, value);

                GameLog.Client.General.InfoFormat("#### Log.Txt: Traces (** SOME **) set to DEBUG (press ingame CTRL + Z)");  // in Log.Txt only DEBUG = yes get a line

                if (value == true)
                {
                    SetValue(TracesGeneralProperty, value); /*OnTracesGeneralChanged(false, true);*/ GameLog.SetRepositoryToDebug("General");

                    // Audio changes shall be done directly = OnTracesAudioChanged

                    SetValue(TracesAIProperty, value); /*OnTracesAIChanged(false, true);*/ GameLog.SetRepositoryToDebug("AI");
                    SetValue(TracesAudioProperty, false); OnTracesAudioChanged(false, true); GameLog.SetRepositoryToDebug("Audio");
                    SetValue(TracesCivsAndRacesProperty, value); /*OnTracesCivsAndRacesChanged(false, true);*/ GameLog.SetRepositoryToDebug("CivsAndRaces");
                    SetValue(TracesColoniesProperty, false); /*OnTracesColoniesChanged(false, true);*/ GameLog.SetRepositoryToDebug("Colonies");
                    SetValue(TracesCombatProperty, value); /*OnTracesCombatChanged(false, true);*/ GameLog.SetRepositoryToDebug("Combat");
                    SetValue(TracesCombatDetailsProperty, false); /*OnTracesCombatDetailsChanged(false, true);*/ GameLog.SetRepositoryToDebug("CombatDetails");
                    SetValue(TracesCreditsProperty, false); /*OnTracesCreditsChanged(false, true);*/ GameLog.SetRepositoryToDebug("Credits");
                    SetValue(TracesDeuteriumProperty, false); /*OnTracesDeuteriumChanged(false, true);*/ GameLog.SetRepositoryToDebug("Deuterium");
                    SetValue(TracesDilithiumProperty, false); /*OnTracesDilithiumChanged(false, true);*/ GameLog.SetRepositoryToDebug("Dilithium");
                    SetValue(TracesDiplomacyProperty, value); /*OnTracesDiplomacyChanged(false, true);*/ GameLog.SetRepositoryToDebug("Diplomacy");
                    SetValue(TracesEnergyProperty, false); /*OnTracesEnergyChanged(false, true);*/ GameLog.SetRepositoryToDebug("Energy");
                    SetValue(TracesEventsProperty, value); /*OnTracesEventsChanged(false, true);*/ GameLog.SetRepositoryToDebug("Events");
                    SetValue(TracesGalaxyGeneratorProperty, false); /*OnTracesGalaxyGeneratorChanged(false, true);*/ GameLog.SetRepositoryToDebug("GalaxyGenerator");
                    SetValue(TracesGameDataProperty, false); /*OnTracesGameDataChanged(false, true);*/ GameLog.SetRepositoryToDebug("GameData");
                    SetValue(TracesGameInitDataProperty, false); /*OnTracesGameInitDataChanged(false, true);*/ GameLog.SetRepositoryToDebug("GameInitData");

                    // done at first
                    //SetValue(TracesGeneralProperty, value); /*OnTracesGeneralChanged(false, true);*/ GameLog.SetRepositoryToDebug("General");

                    SetValue(TracesIntelProperty, false); /*OnTracesIntelChanged(false, true);*/ GameLog.SetRepositoryToDebug("Intel");
                    SetValue(TracesMapDataProperty, false); /*OnTracesMapDataChanged(false, true);*/ GameLog.SetRepositoryToDebug("MapData");
                    SetValue(TracesMultiPlayProperty, false); /*OnTracesMultiPlayChanged(false, true);*/ GameLog.SetRepositoryToDebug("MultiPlay");
                    SetValue(TracesProductionProperty, value); /*OnTracesProductionChanged(false, true);*/ GameLog.SetRepositoryToDebug("Production");
                    ////////SetValue(TracesReportErrorsProperty, value); /*OnTracesReportErrorsChanged(false, true);*/ GameLog.SetRepositoryToDebug("ReportErrors");
                    SetValue(TracesResearchProperty, value); /*OnTracesResearchChanged(false, true);*/ GameLog.SetRepositoryToDebug("Research");
                    SetValue(TracesSitRepsProperty, value); /*OnTracesSitRepsChanged(false, true);*/ GameLog.SetRepositoryToDebug("SitReps");
                    SetValue(TracesSaveLoadProperty, false); /*OnTracesSaveLoadChanged(false, true);*/ GameLog.SetRepositoryToDebug("SaveLoad");
                    SetValue(TracesShipsProperty, false); /*OnTracesShipsChanged(false, true);*/ GameLog.SetRepositoryToDebug("Ships");
                    SetValue(TracesShipProductionProperty, false); /*OnTracesShipProductionChanged(false, true);*/ GameLog.SetRepositoryToDebug("ShipProduction");
                    SetValue(TracesStationsProperty, false); /*OnTracesStationsChanged(false, true);*/ GameLog.SetRepositoryToDebug("Stations");
                    SetValue(TracesStructuresProperty, false); /*OnTracesStructuresChanged(false, true);*/ GameLog.SetRepositoryToDebug("Structures");
                    SetValue(TracesSystemAssaultProperty, value); /*OnTracesSystemAssaultChanged(false, true);*/ GameLog.SetRepositoryToDebug("SystemAssault");
                    SetValue(TracesSystemAssaultDetailsProperty, false); /*OnTracesSystemAssaultDetailsChanged(false, true);*/ GameLog.SetRepositoryToDebug("SystemAssaultDetails");
                    SetValue(TracesTestProperty, false); /*OnTracesTestChanged(false, true);*/ GameLog.SetRepositoryToDebug("Test");
                    SetValue(TracesTradeRoutesProperty, value); /*OnTracesTradeRoutesChanged(false, true);*/ GameLog.SetRepositoryToDebug("TradeRoutes");
                    SetValue(TracesUIProperty, false); /*OnTracesUIChanged(false, true);*/ GameLog.SetRepositoryToDebug("UI");
                    SetValue(TracesXMLCheckProperty, false); /*OnTracesXMLCheckChanged(false, true);*/ GameLog.SetRepositoryToDebug("XMLCheck");
                    SetValue(TracesXML2CSVOutputProperty, false); /*OnTracesXML2CSVOutputChanged(false, true);*/ GameLog.SetRepositoryToDebug("XML2CSVOutput");

                    //SendKeys.SendWait("{ENTER}");  // doesn't work - close OptionsDialog ...(and reload)
                    //Thread.Sleep(1000);
                    //SendKeys.SendWait("^o"); // OptionsDialog
                    //Thread.Sleep(1000);
                }
            }
        }
        #endregion Traces_SetSome

        #region Traces_SetSelection2 Property
        public static readonly DependencyProperty Traces_SetSelection2Property = DependencyProperty.Register(
            "SetSelection2",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));


        public bool Traces_SetSelection2
        {
            get { return (bool)GetValue(Traces_SetSelection2Property); }
            set
            {
                SetValue(Traces_SetSelection2Property, value);


                GameLog.Client.General.InfoFormat("#### Log.Txt: Traces (** Selection 2 **) set to DEBUG (press ingame CTRL + Z)");  // in Log.Txt only DEBUG = yes get a line

                if (value == true)
                {
                    SetValue(TracesGeneralProperty, value); /*OnTracesGeneralChanged(false, true);*/ GameLog.SetRepositoryToDebug("General");

                    // Audio changes shall be done directly = OnTracesAudioChanged

                    SetValue(TracesAIProperty, false); /*OnTracesAIChanged(false, true);*/ GameLog.SetRepositoryToDebug("AI");
                    SetValue(TracesAudioProperty, false); OnTracesAudioChanged(false, true); GameLog.SetRepositoryToDebug("Audio");
                    SetValue(TracesCivsAndRacesProperty, false); /*OnTracesCivsAndRacesChanged(false, true);*/ GameLog.SetRepositoryToDebug("CivsAndRaces");
                    SetValue(TracesColoniesProperty, false); /*OnTracesColoniesChanged(false, true);*/ GameLog.SetRepositoryToDebug("Colonies");
                    SetValue(TracesCombatProperty, value); /*OnTracesCombatChanged(false, true);*/ GameLog.SetRepositoryToDebug("Combat");
                    SetValue(TracesCombatDetailsProperty, value); /*OnTracesCombatDetailsChanged(false, true);*/ GameLog.SetRepositoryToDebug("CombatDetails");
                    SetValue(TracesCreditsProperty, false); /*OnTracesCreditsChanged(false, true);*/ GameLog.SetRepositoryToDebug("Credits");
                    SetValue(TracesDeuteriumProperty, false); /*OnTracesDeuteriumChanged(false, true);*/ GameLog.SetRepositoryToDebug("Deuterium");
                    SetValue(TracesDilithiumProperty, false); /*OnTracesDilithiumChanged(false, true);*/ GameLog.SetRepositoryToDebug("Dilithium");
                    SetValue(TracesDiplomacyProperty, false); /*OnTracesDiplomacyChanged(false, true);*/ GameLog.SetRepositoryToDebug("Diplomacy");
                    SetValue(TracesEnergyProperty, false); /*OnTracesEnergyChanged(false, true);*/ GameLog.SetRepositoryToDebug("Energy");
                    SetValue(TracesEventsProperty, false); /*OnTracesEventsChanged(false, true);*/ GameLog.SetRepositoryToDebug("Events");
                    SetValue(TracesGalaxyGeneratorProperty, false); /*OnTracesGalaxyGeneratorChanged(false, true);*/ GameLog.SetRepositoryToDebug("GalaxyGenerator");
                    SetValue(TracesGameDataProperty, false); /*OnTracesGameDataChanged(false, true);*/ GameLog.SetRepositoryToDebug("GameData");
                    SetValue(TracesGameInitDataProperty, false); /*OnTracesGameInitDataChanged(false, true);*/ GameLog.SetRepositoryToDebug("GameInitData");

                    // done at first
                    //SetValue(TracesGeneralProperty, value); /*OnTracesGeneralChanged(false, true);*/ GameLog.SetRepositoryToDebug("General");

                    SetValue(TracesIntelProperty, false); /*OnTracesIntelChanged(false, true);*/ GameLog.SetRepositoryToDebug("Intel");
                    SetValue(TracesMapDataProperty, false); /*OnTracesMapDataChanged(false, true);*/ GameLog.SetRepositoryToDebug("MapData");
                    SetValue(TracesMultiPlayProperty, false); /*OnTracesMultiPlayChanged(false, true);*/ GameLog.SetRepositoryToDebug("MultiPlay");
                    SetValue(TracesProductionProperty, false); /*OnTracesProductionChanged(false, true);*/ GameLog.SetRepositoryToDebug("Production");
                    //////SetValue(TracesReportErrorsProperty, false); /*OnTracesReportErrorsChanged(false, true);*/ GameLog.SetRepositoryToDebug("ReportErrors");
                    SetValue(TracesResearchProperty, false); /*OnTracesResearchChanged(false, true);*/ GameLog.SetRepositoryToDebug("Research");
                    SetValue(TracesSitRepsProperty, value); /*OnTracesSitRepsChanged(false, true);*/ GameLog.SetRepositoryToDebug("SitReps");
                    SetValue(TracesSaveLoadProperty, false); /*OnTracesSaveLoadChanged(false, true);*/ GameLog.SetRepositoryToDebug("SaveLoad");
                    SetValue(TracesShipsProperty, false); /*OnTracesShipsChanged(false, true);*/ GameLog.SetRepositoryToDebug("Ships");
                    SetValue(TracesShipProductionProperty, false); /*OnTracesShipProductionChanged(false, true);*/ GameLog.SetRepositoryToDebug("ShipProduction");
                    SetValue(TracesStationsProperty, false); /*OnTracesStationsChanged(false, true);*/ GameLog.SetRepositoryToDebug("Stations");
                    SetValue(TracesStructuresProperty, false); /*OnTracesStructuresChanged(false, true);*/ GameLog.SetRepositoryToDebug("Structures");
                    SetValue(TracesSystemAssaultProperty, false); /*OnTracesSystemAssaultChanged(false, true);*/ GameLog.SetRepositoryToDebug("SystemAssault");
                    SetValue(TracesSystemAssaultDetailsProperty, false); /*OnTracesSystemAssaultDetailsChanged(false, true);*/ GameLog.SetRepositoryToDebug("SystemAssaultDetails");
                    SetValue(TracesTestProperty, false); /*OnTracesTestChanged(false, true);*/ GameLog.SetRepositoryToDebug("Test");
                    SetValue(TracesTradeRoutesProperty, false); /*OnTracesTradeRoutesChanged(false, true);*/ GameLog.SetRepositoryToDebug("TradeRoutes");
                    SetValue(TracesUIProperty, false); /*OnTracesUIChanged(false, true);*/ GameLog.SetRepositoryToDebug("UI");
                    SetValue(TracesXMLCheckProperty, false); /*OnTracesXMLCheckChanged(false, true);*/ GameLog.SetRepositoryToDebug("XMLCheck");
                    SetValue(TracesXML2CSVOutputProperty, false); /*OnTracesXML2CSVOutputChanged(false, true);*/ GameLog.SetRepositoryToDebug("XML2CSVOutput");

                    //SendKeys.SendWait("{ENTER}");  // doesn't work - close OptionsDialog ...(and reload)
                    //Thread.Sleep(1000);
                    //SendKeys.SendWait("^o"); // OptionsDialog
                    //Thread.Sleep(1000);
                }
            }
        }
        #endregion Traces_SetSelection2


        #region TracesAI Property
        public static readonly DependencyProperty TracesAIProperty = DependencyProperty.Register(
            "AI",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                //(o, args) => ((ClientSettings)o).OnTracesAIChanged((bool)args.OldValue, (bool)args.NewValue)));
                FrameworkPropertyMetadataOptions.None));

        public bool TracesAI
        {
            get { return (bool)GetValue(TracesAIProperty); }
            set
            {
                SetValue(TracesAIProperty, value);
                //GameLog.Client.General.InfoFormat("AI = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("AI");
                else
                    GameLog.SetRepositoryToErrorOnly("AI");
            }
        }

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesAIChanged;

        private void OnTracesAIChanged(bool oldValue, bool newValue) => TracesAIChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        #endregion

        #region TracesAudio Property

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesAudioChanged;

        private void OnTracesAudioChanged(bool oldValue, bool newValue) => 
            TracesAudioChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public static readonly DependencyProperty TracesAudioProperty = DependencyProperty.Register(
            "Audio",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesAudio
        {
            get { return (bool)GetValue(TracesAudioProperty); }
            set
            {
                SetValue(TracesAudioProperty, value);
                //GameLog.Client.General.InfoFormat("TracesAudio = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("Audio");
                else
                    GameLog.SetRepositoryToErrorOnly("Audio");
            }
        }
        //public bool EnableCombatScreen
        //{
        //    get { return (bool)GetValue(EnableCombatScreenProperty); }
        //    set { SetValue(EnableCombatScreenProperty, value); }
        //}
        #endregion

        #region TracesCivsAndRaces Property
        public static readonly DependencyProperty TracesCivsAndRacesProperty = DependencyProperty.Register(
            "CivsAndRaces",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesCivsAndRaces
        {
            get { return (bool)GetValue(TracesCivsAndRacesProperty); }
            set
            {
                SetValue(TracesCivsAndRacesProperty, value);
                //GameLog.Client.General.InfoFormat("TracesCredits= {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("CivsAndRaces");
                else
                    GameLog.SetRepositoryToErrorOnly("CivsAndRaces");
            }
        }
        #endregion TracesCivsAndRaces

        #region TracesColonies Property
        public static readonly DependencyProperty TracesColoniesProperty = DependencyProperty.Register(
            "Colonies",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesColonies
        {
            get { return (bool)GetValue(TracesColoniesProperty); }
            set
            {
                SetValue(TracesColoniesProperty, value);
                //GameLog.Client.General.InfoFormat("TracesColonies = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("Colonies");
                else
                    GameLog.SetRepositoryToErrorOnly("Colonies");
            }
        }
        #endregion TracesColonies Property

        #region TracesCombat Property
        public static readonly DependencyProperty TracesCombatProperty = DependencyProperty.Register(
            "Combat",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesCombat
        {
            get { return (bool)GetValue(TracesCombatProperty); }
            set
            {
                SetValue(TracesCombatProperty, value);
                //GameLog.Client.General.InfoFormat("TracesCombat = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("Combat");
                else
                    GameLog.SetRepositoryToErrorOnly("Combat");
            }
        }
        #endregion TracesCombat Property

        #region TracesCombatDetails Property
        public static readonly DependencyProperty TracesCombatDetailsProperty = DependencyProperty.Register(
            "CombatDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesCombatDetails
        {
            get { return (bool)GetValue(TracesCombatDetailsProperty); }
            set
            {
                SetValue(TracesCombatDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesCombatDetails = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("CombatDetails");
                else
                    GameLog.SetRepositoryToErrorOnly("CombatDetails");
            }
        }
        #endregion TracesCombatDetails Property

        #region TracesCredits Property
        public static readonly DependencyProperty TracesCreditsProperty = DependencyProperty.Register(
            "Credits",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesCredits
        {
            get { return (bool)GetValue(TracesCreditsProperty); }
            set
            {
                SetValue(TracesCreditsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesCredits= {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("Credits");
                else
                    GameLog.SetRepositoryToErrorOnly("Credits");
            }
        }
        #endregion TracesCredits
            
        #region TracesDeuterium Property

        public static readonly DependencyProperty TracesDeuteriumProperty = DependencyProperty.Register(
            "Deuterium",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesDeuterium
        {
            get { return (bool)GetValue(TracesDeuteriumProperty); }
            set
            {
                SetValue(TracesDeuteriumProperty, value);
                //GameLog.Client.General.InfoFormat("TracesCredits= {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("Deuterium");
                else
                    GameLog.SetRepositoryToErrorOnly("Deuterium");
            }
        }
        #endregion TracesDeuterium

        #region TracesDilithium Property
        public static readonly DependencyProperty TracesDilithiumProperty = DependencyProperty.Register(
            "Dilithium",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesDilithium
        {
            get { return (bool)GetValue(TracesDilithiumProperty); }
            set
            {
                SetValue(TracesDilithiumProperty, value);
                //GameLog.Client.General.InfoFormat("TracesCredits= {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("Dilithium");
                else
                    GameLog.SetRepositoryToErrorOnly("Dilithium");
            }
        }
        #endregion TracesDilithium

        #region TracesDiplomacy Property
        public static readonly DependencyProperty TracesDiplomacyProperty = DependencyProperty.Register(
            "Diplomacy",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesDiplomacy
        {
            get { return (bool)GetValue(TracesDiplomacyProperty); }
            set
            {
                SetValue(TracesDiplomacyProperty, value);
                //GameLog.Client.General.InfoFormat("TracesDiplomacy = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("Diplomacy");
                else
                    GameLog.SetRepositoryToErrorOnly("Diplomacy");
            }
        }
        #endregion TracesDiplomacy Property

        #region TracesEnergy Property
        public static readonly DependencyProperty TracesEnergyProperty = DependencyProperty.Register(
            "Energy",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesEnergy
        {
            get { return (bool)GetValue(TracesEnergyProperty); }
            set
            {
                SetValue(TracesEnergyProperty, value);
                //GameLog.Client.General.InfoFormat("Trace for Energy = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("Energy");
                else
                    GameLog.SetRepositoryToErrorOnly("Energy");
            }
        }
        #endregion TracesEnergy Property

        #region TracesEvents Property
        public static readonly DependencyProperty TracesEventsProperty = DependencyProperty.Register(
            "Events",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesEvents
        {
            get { return (bool)GetValue(TracesEventsProperty); }
            set
            {
                SetValue(TracesEventsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesEvents = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("Events");
                else
                    GameLog.SetRepositoryToErrorOnly("Events");
            }
        }
        #endregion TracesEvents Property

        #region TracesGalaxyGenerator Property   
        // even used after retire and start a new game
        public static readonly DependencyProperty TracesGalaxyGeneratorProperty = DependencyProperty.Register(
            "GalaxyGenerator",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesGalaxyGenerator
        {
            get { return (bool)GetValue(TracesGalaxyGeneratorProperty); }
            set
            {
                SetValue(TracesGalaxyGeneratorProperty, value);
                //GameLog.Client.General.InfoFormat("TracesGalaxyGenerator = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("GalaxyGenerator");
                else
                    GameLog.SetRepositoryToErrorOnly("GalaxyGenerator");
            }
        }
        #endregion TracesGalaxyGenerator Property 

        #region TracesGameData Property
        public static readonly DependencyProperty TracesGameDataProperty = DependencyProperty.Register(
            "GameData",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesGameData
        {
            get { return (bool)GetValue(TracesGameDataProperty); }
            set
            {
                SetValue(TracesGameDataProperty, value);
                //GameLog.Client.General.InfoFormat("TracesGameData = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("GameData");
                else
                    GameLog.SetRepositoryToErrorOnly("GameData");
            }
        }

        //public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesGameDataChanged;

        //private void OnTracesGameDataChanged(bool)
        //{
        //    var handler = OnTracesGameDataChanged;
        //    if (handler != null)
        //        GameLog.Client.General.DebugFormat("TracesGameData changed");
        //        //handler(this, new)
        //}

        //public bool TracesGameData
        //{
        //    get { return (bool)GetValue(TracesGameDataProperty); }
        //    set { SetValue(TracesGameDataProperty, value); }
        //}
        #endregion TracesGameData Property

        #region TracesGameInitData Property
        public static readonly DependencyProperty TracesGameInitDataProperty = DependencyProperty.Register(
            "GameInitData",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesGameInitData
        {
            get { return (bool)GetValue(TracesGameInitDataProperty); }
            set
            {
                SetValue(TracesGameInitDataProperty, value);
                //GameLog.Client.General.InfoFormat("TracesCredits= {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("GameInitData");
                else
                    GameLog.SetRepositoryToErrorOnly("GameInitData");
            }
        }
        #endregion TracesGameInitData

        // Traces General at the end !!! must be this !!!

        #region TracesIntel Property
        public static readonly DependencyProperty TracesIntelProperty = DependencyProperty.Register(
            "Intel",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesIntel
        {
            get { return (bool)GetValue(TracesIntelProperty); }
            set
            {
                SetValue(TracesIntelProperty, value);
                //GameLog.Client.General.InfoFormat("TracesIntel = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("Intel");
                else
                    GameLog.SetRepositoryToErrorOnly("Intel");
            }
        }
        #endregion TracesIntel Property

        #region TracesMapData Property
        public static readonly DependencyProperty TracesMapDataProperty = DependencyProperty.Register(
            "MapData",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesMapData
        {
            get { return (bool)GetValue(TracesMapDataProperty); }
            set
            {
                SetValue(TracesMapDataProperty, value);
                //GameLog.Client.General.InfoFormat("TracesMapData = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("MapData");
                else
                    GameLog.SetRepositoryToErrorOnly("MapData");
            }
        }
        #endregion TracesMapData Property

        #region TracesMultiPlay Property
        public static readonly DependencyProperty TracesMultiPlayProperty = DependencyProperty.Register(
            "MultiPlay",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesMultiPlay
        {
            get { return (bool)GetValue(TracesMultiPlayProperty); }
            set
            {
                SetValue(TracesMultiPlayProperty, value);
                //GameLog.Client.General.InfoFormat("TracesMultiPlay = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("MultiPlay");
                else
                    GameLog.SetRepositoryToErrorOnly("MultiPlay");
            }
        }
        #endregion TracesMultiPlay Property

        #region TracesProduction Property
        public static readonly DependencyProperty TracesProductionProperty = DependencyProperty.Register(
            "Production",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesProduction
        {
            get { return (bool)GetValue(TracesProductionProperty); }
            set
            {
                SetValue(TracesProductionProperty, value);
                //GameLog.Client.General.InfoFormat("TracesProduction = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("Production");
                else
                    GameLog.SetRepositoryToErrorOnly("Production");
            }
        }
        #endregion TracesProduction Property

        #region TracesSitReps Property
        public static readonly DependencyProperty TracesSitRepsProperty = DependencyProperty.Register(
            "SitReps",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesSitReps
        {
            get { return (bool)GetValue(TracesSitRepsProperty); }
            set
            {
                SetValue(TracesSitRepsProperty, value);
                //GameLog.Client.General.InfoFormat("SitReps = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("SitReps");
                else
                    GameLog.SetRepositoryToErrorOnly("SitReps");
            }
        }

        //public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesSitRepsChanged;

        //private void OnTracesSitRepsChanged(bool oldValue, bool newValue)
        //{
        //    var handler = TracesSitRepsChanged;
        //    if (handler != null)
        //        handler(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));
        //}

        #endregion TracesSitReps Property

        #region TracesReportErrorsToEmail Property
        public static readonly DependencyProperty ReportErrorsToEmailProperty = DependencyProperty.Register(
            "ReportErrorsToEmail",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesReportErrorsToEmail
        {
            get { return (bool)GetValue(ReportErrorsToEmailProperty); }
            set { SetValue(ReportErrorsToEmailProperty, value); }
        }
        #endregion TracesReportErrorsToEmail

        #region TracesResearch Property
        public static readonly DependencyProperty TracesResearchProperty = DependencyProperty.Register(
            "Research",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesResearch
        {
            get { return (bool)GetValue(TracesResearchProperty); }
            set
            {
                SetValue(TracesResearchProperty, value);
                //GameLog.Client.General.InfoFormat("TracesResearch = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("Research");
                else
                    GameLog.SetRepositoryToErrorOnly("Research");
            }
        }
        #endregion TracesResearch Property



        #region TracesSaveLoad Property
        public static readonly DependencyProperty TracesSaveLoadProperty = DependencyProperty.Register(
            "SaveLoad",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesSaveLoad
        {
            get { return (bool)GetValue(TracesSaveLoadProperty); }
            set
            {
                SetValue(TracesSaveLoadProperty, value);
                //GameLog.Client.General.InfoFormat("TracesSaveLoad = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("SaveLoad");
                else
                    GameLog.SetRepositoryToErrorOnly("SaveLoad");
            }
        }
        #endregion TracesSaveLoad Property

        #region TracesShips Property
        public static readonly DependencyProperty TracesShipsProperty = DependencyProperty.Register(
            "Ships",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesShips
        {
            get { return (bool)GetValue(TracesShipsProperty); }
            set
            {
                SetValue(TracesShipsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesShips = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("Ships");
                else
                    GameLog.SetRepositoryToErrorOnly("Ships");
            }
        }
        #endregion TracesShips

        #region TracesShipProduction Property
        public static readonly DependencyProperty TracesShipProductionProperty = DependencyProperty.Register(
            "ShipProduction",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesShipProduction
        {
            get { return (bool)GetValue(TracesShipProductionProperty); }
            set
            {
                SetValue(TracesShipProductionProperty, value);
                //GameLog.Client.General.InfoFormat("TracesShipProduction = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("ShipProduction");
                else
                    GameLog.SetRepositoryToErrorOnly("ShipProduction");
            }
        }
        #endregion TracesShipProduction Property

        #region TracesStations Property
        public static readonly DependencyProperty TracesStationsProperty = DependencyProperty.Register(
            "Stations",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesStations
        {
            get { return (bool)GetValue(TracesStationsProperty); }
            set
            {
                SetValue(TracesStationsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesStations = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("Stations");
                else
                    GameLog.SetRepositoryToErrorOnly("Stations");
            }
        }
        #endregion TracesStations Property

        #region TracesStructures Property
        public static readonly DependencyProperty TracesStructuresProperty = DependencyProperty.Register(
            "Structures",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesStructures
        {
            get { return (bool)GetValue(TracesStructuresProperty); }
            set
            {
                SetValue(TracesStructuresProperty, value);
                //GameLog.Client.General.InfoFormat("TracesStructures = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("Structures");
                else
                    GameLog.SetRepositoryToErrorOnly("Structures");
            }
        }
        #endregion TracesStructures Property

        #region TracesSystemAssault Property
        public static readonly DependencyProperty TracesSystemAssaultProperty = DependencyProperty.Register(
            "SystemAssault",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesSystemAssault
        {
            get { return (bool)GetValue(TracesSystemAssaultProperty); }
            set
            {
                SetValue(TracesSystemAssaultProperty, value);
                //GameLog.Client.General.InfoFormat("TracesSystemAssault = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("SystemAssault");
                else
                    GameLog.SetRepositoryToErrorOnly("SystemAssault");
            }
        }
        #endregion TracesSystemAssault Property

        #region TracesSystemAssaultDetails Property
        public static readonly DependencyProperty TracesSystemAssaultDetailsProperty = DependencyProperty.Register(
            "SystemAssaultDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesSystemAssaultDetails
        {
            get { return (bool)GetValue(TracesSystemAssaultDetailsProperty); }
            set
            {
                SetValue(TracesSystemAssaultDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesSystemAssaultDetails = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("SystemAssaultDetails");
                else
                    GameLog.SetRepositoryToErrorOnly("SystemAssaultDetails");
            }
        }
        #endregion TracesSystemAssaultDetails Property

        // for Test Porpuse
        #region TracesTest Property  
        public static readonly DependencyProperty TracesTestProperty = DependencyProperty.Register(
            "Test",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesTest
        {
            get { return (bool)GetValue(TracesTestProperty); }
            set
            {
                SetValue(TracesTestProperty, value);
                //GameLog.Client.General.InfoFormat("TracesTest = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("Test");
                else
                    GameLog.SetRepositoryToErrorOnly("Test");
            }
        }
        #endregion TracesTest Property  

        #region TracesTradeRoutes Property
        public static readonly DependencyProperty TracesTradeRoutesProperty = DependencyProperty.Register(
            "TradeRoutes",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesTradeRoutes
        {
            get { return (bool)GetValue(TracesTradeRoutesProperty); }
            set
            {
                SetValue(TracesTradeRoutesProperty, value);
                //GameLog.Client.General.InfoFormat("TracesTradeRoutes = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("TradeRoutes");
                else
                    GameLog.SetRepositoryToErrorOnly("TradeRoutes");
            }
        }
        #endregion TracesTradeRoutes Property

        #region TracesUI Property
        public static readonly DependencyProperty TracesUIProperty = DependencyProperty.Register(
            "UI",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesUI
        {
            get { return (bool)GetValue(TracesUIProperty); }
            set
            {
                SetValue(TracesUIProperty, value);
                //GameLog.Client.General.InfoFormat("TracesUI = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("UI");
                else
                    GameLog.SetRepositoryToErrorOnly("UI");
            }
        }
        #endregion TracesUI Property

        #region TracesXMLCheck Property  
        public static readonly DependencyProperty TracesXMLCheckProperty = DependencyProperty.Register(
            "XMLCheck",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesXMLCheck
        {
            get { return (bool)GetValue(TracesXMLCheckProperty); }
            set
            {
                SetValue(TracesXMLCheckProperty, value);
                //GameLog.Client.General.InfoFormat("TracesXMLCheck = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("XMLCheck");
                else
                    GameLog.SetRepositoryToErrorOnly("XMLCheck");
            }
        }
        #endregion TracesXMLCheck

        #region TracesXML2CSVOutput Property  
        public static readonly DependencyProperty TracesXML2CSVOutputProperty = DependencyProperty.Register(
            "XML2CSVOutput",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesXML2CSVOutputChanged;

        private void OnTracesXML2CSVOutputChanged(bool oldValue, bool newValue) => TracesXML2CSVOutputChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesXML2CSVOutput
        {
            get { return (bool)GetValue(TracesXML2CSVOutputProperty); }
            set
            {
                SetValue(TracesXML2CSVOutputProperty, value);
                //GameLog.Client.General.InfoFormat("TracesXML2CSVOutput = {0}", value);
                if (value == true)
                { 
                    GameLog.SetRepositoryToDebug("XML2CSVOutput");
                }
                else
                    GameLog.SetRepositoryToErrorOnly("XML2CSVOutput");
            }
        }
        #endregion TracesXML2CSVOutput


        #region TracesGeneral Property
        public static readonly DependencyProperty TracesGeneralProperty = DependencyProperty.Register(
            "General",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesGeneral
        {
            get { return (bool)GetValue(TracesGeneralProperty); }
            set
            {
                SetValue(TracesGeneralProperty, value);
                //GameLog.Client.General.InfoFormat("TracesGeneral = {0}", value);
                if (value == true)
                    GameLog.SetRepositoryToDebug("General");
                else
                    GameLog.SetRepositoryToErrorOnly("General");
            }
        }
        #endregion TracesGeneral Property



        #region ReportErrors Property
        public static readonly DependencyProperty ReportErrorsProperty = DependencyProperty.Register(
            "ReportErrors",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool ReportErrors
        {
            get { return (bool)GetValue(ReportErrorsProperty); }
            set { SetValue(ReportErrorsProperty, value); }
        }
        #endregion ReportErrors Property

        #region ClientWindowWidth Property
        public static readonly DependencyProperty ClientWindowWidthProperty = DependencyProperty.Register(
            "ClientWindowWidth",
            typeof(double),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                1280.0,
                FrameworkPropertyMetadataOptions.None));

        public double ClientWindowWidth
        {
            get { return (double)GetValue(ClientWindowWidthProperty); }
            set { SetValue(ClientWindowWidthProperty, value); }
        }
        #endregion ClientWindowWidth Property

        #region ClientWindowHeight Property
        public static readonly DependencyProperty ClientWindowHeightProperty = DependencyProperty.Register(
            "ClientWindowHeight",
            typeof(double),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                768.0,
                FrameworkPropertyMetadataOptions.None));

        public double ClientWindowHeight
        {
            get { return (double)GetValue(ClientWindowHeightProperty); }
            set { SetValue(ClientWindowHeightProperty, value); }
        }
        #endregion ClientWindowHeight Property

        #region EnableScreenTransitions Property
        public static readonly DependencyProperty EnableScreenTransitionsProperty = DependencyProperty.Register(
            "EnableScreenTransitions",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool EnableScreenTransitions
        {
            get { return (bool)GetValue(EnableScreenTransitionsProperty); }
            set { SetValue(EnableScreenTransitionsProperty, value); }
        }
        #endregion EnableScreenTransitions Property

        #region DesiredAnimationFrameRate Property
        public static readonly DependencyProperty DesiredAnimationFrameRateProperty = DependencyProperty.Register(
            "DesiredAnimationFrameRate",
            typeof(int),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                30,
                FrameworkPropertyMetadataOptions.None));

        public int DesiredAnimationFrameRate
        {
            get { return (int)GetValue(DesiredAnimationFrameRateProperty); }
            set { SetValue(DesiredAnimationFrameRateProperty, value); }
        }
        #endregion DesiredAnimationFrameRate Property

        #region Implementation of IAttachedPropertyStore

        void IAttachedPropertyStore.CopyPropertiesTo(KeyValuePair<AttachableMemberIdentifier, object>[] array, int index)
        {
            _attachedValues.CopyTo(array, index);
        }

        bool IAttachedPropertyStore.RemoveProperty(AttachableMemberIdentifier attachableMemberIdentifier)
        {
            if (!_attachedValues.Remove(attachableMemberIdentifier))
                return false;

            OnPropertyChanged(attachableMemberIdentifier.MemberName);
            return true;
        }

        void IAttachedPropertyStore.SetProperty(AttachableMemberIdentifier attachableMemberIdentifier, object value)
        {
            _attachedValues[attachableMemberIdentifier] = value;
            OnPropertyChanged(attachableMemberIdentifier.MemberName);
        }

        bool IAttachedPropertyStore.TryGetProperty(AttachableMemberIdentifier attachableMemberIdentifier, out object value)
        {
            return _attachedValues.TryGetValue(attachableMemberIdentifier, out value);
        }

        int IAttachedPropertyStore.PropertyCount
        {
            get { return _attachedValues.Count; }
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}