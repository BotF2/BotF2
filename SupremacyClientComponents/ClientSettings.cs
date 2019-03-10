// ClientSettings.cs
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
using System.Windows;
using System.Xaml;
using XamlReader = System.Windows.Markup.XamlReader;
using XamlWriter = System.Windows.Markup.XamlWriter;

using Supremacy.Collections;

using System.Linq;
using Supremacy.Utility;
using Supremacy.Resources;

namespace Supremacy.Client
{
    public class ClientSettings : DependencyObject, IAttachedPropertyStore, INotifyPropertyChanged
    {
        private const string ClientSettingsFileName = "ClientSettings.xaml";

        bool _tracingClientSettings = false;

        private static ClientSettings _current;

        private readonly Dictionary<AttachableMemberIdentifier, object> _attachedValues;

        static ClientSettings()
        {
            EnableDialogAnimationsProperty.AddOwner(typeof(UIElement));
        }

        public ClientSettings()
        {
            _attachedValues = new Dictionary<AttachableMemberIdentifier, object>();
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
            var handler = Saved;
            if (handler != null)
                handler(null, EventArgs.Empty);

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
            var handler = Loaded;
            if (handler != null)
                handler(null, EventArgs.Empty);
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
                        settings = XamlReader.Load(fileReader) as ClientSettings ??
                                   new ClientSettings();

                        GameLog.Client.General.InfoFormat("LOADCORE {0}: Content: " + Environment.NewLine + Environment.NewLine + "{1}" + Environment.NewLine, filePath, File.ReadAllText(filePath));
                    }

                    settings.OnLoaded();

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
                0.5,
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
            var handler = MasterVolumeChanged;
            if (handler != null)
                handler(this, new PropertyChangedRoutedEventArgs<double>(oldValue, newValue));
        }
        #endregion

        #region MusicVolume Property
        public static readonly DependencyProperty MusicVolumeProperty = DependencyProperty.Register(
            "MusicVolume",
            typeof(double),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                0.2,
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
            var handler = MusicVolumeChanged;
            if (handler != null)
                handler(this, new PropertyChangedRoutedEventArgs<double>(oldValue, newValue));
        }
        #endregion

        #region FXVolume Property
        public static readonly DependencyProperty FXVolumeProperty = DependencyProperty.Register(
            "FXVolume",
            typeof(double),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                0.5,
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
            var handler = FXVolumeChanged;
            if (handler != null)
                handler(this, new PropertyChangedRoutedEventArgs<double>(oldValue, newValue));
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
            var handler = EnableAntiAliasingChanged;
            if (handler != null)
                handler(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));
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

        #region TracesClearAll Property
        public static readonly DependencyProperty TracesClearAllProperty = DependencyProperty.Register(
            "ClearAll",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public bool TracesClearAll
        {
            get { return (bool)GetValue(TracesClearAllProperty); }
            set
            {
                SetValue(TracesClearAllProperty, value);


                if (value == true)
                {
                    SetValue(TracesAIProperty, value);
                    OnTracesAIChanged(false, true);
                    GameLog.SetRepositoryToDebug("AI");


                    SetValue(TracesAudioProperty, value);
                    GameLog.SetRepositoryToDebug("Audio");
                }
                else
                {

                    SetValue(TracesAIProperty, false);
                    OnTracesAIChanged(true, false);
                    GameLog.SetRepositoryToErrorOnly("AI");

                    SetValue(TracesAudioProperty, value);
                    GameLog.SetRepositoryToErrorOnly("Audio");
                }


            }
        }
        #endregion

        #region TracesAI Property
        public DependencyProperty TracesAIProperty = DependencyProperty.Register(
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

        private void OnTracesAIChanged(bool oldValue, bool newValue)
        {
            var handler = TracesAIChanged;
            if (handler != null)
                handler(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));
        }

        #endregion

        #region TracesAudio Property
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
        //public bool EnableCombatScreen
        //{
        //    get { return (bool)GetValue(EnableCombatScreenProperty); }
        //    set { SetValue(EnableCombatScreenProperty, value); }
        //}
        #endregion

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
        //public bool EnableCombatScreen
        //{
        //    get { return (bool)GetValue(EnableCombatScreenProperty); }
        //    set { SetValue(EnableCombatScreenProperty, value); }
        //}
        #endregion

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
        //public bool EnableCombatScreen
        //{
        //    get { return (bool)GetValue(EnableCombatScreenProperty); }
        //    set { SetValue(EnableCombatScreenProperty, value); }
        //}
        #endregion

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
        //public bool EnableCombatScreen
        //{
        //    get { return (bool)GetValue(EnableCombatScreenProperty); }
        //    set { SetValue(EnableCombatScreenProperty, value); }
        //}
        #endregion

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
        //public bool EnableCombatScreen
        //{
        //    get { return (bool)GetValue(EnableCombatScreenProperty); }
        //    set { SetValue(EnableCombatScreenProperty, value); }
        //}
        #endregion

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
        //public bool EnableCombatScreen
        //{
        //    get { return (bool)GetValue(EnableCombatScreenProperty); }
        //    set { SetValue(EnableCombatScreenProperty, value); }
        //}
        #endregion

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
        //public bool EnableCombatScreen
        //{
        //    get { return (bool)GetValue(EnableCombatScreenProperty); }
        //    set { SetValue(EnableCombatScreenProperty, value); }
        //}
        #endregion

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
        //public bool EnableCombatScreen
        //{
        //    get { return (bool)GetValue(EnableCombatScreenProperty); }
        //    set { SetValue(EnableCombatScreenProperty, value); }
        //}
        #endregion

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
        #endregion

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
        //public bool EnableCombatScreen
        //{
        //    get { return (bool)GetValue(EnableCombatScreenProperty); }
        //    set { SetValue(EnableCombatScreenProperty, value); }
        //}
        #endregion

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
        //public bool EnableCombatScreen
        //{
        //    get { return (bool)GetValue(EnableCombatScreenProperty); }
        //    set { SetValue(EnableCombatScreenProperty, value); }
        //}
        #endregion

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
        //public bool EnableCombatScreen
        //{
        //    get { return (bool)GetValue(EnableCombatScreenProperty); }
        //    set { SetValue(EnableCombatScreenProperty, value); }
        //}
        #endregion

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
        //public bool EnableCombatScreen
        //{
        //    get { return (bool)GetValue(EnableCombatScreenProperty); }
        //    set { SetValue(EnableCombatScreenProperty, value); }
        //}
        #endregion

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
        //public bool EnableCombatScreen
        //{
        //    get { return (bool)GetValue(EnableCombatScreenProperty); }
        //    set { SetValue(EnableCombatScreenProperty, value); }
        //}
        #endregion

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
        //public bool EnableCombatScreen
        //{
        //    get { return (bool)GetValue(EnableCombatScreenProperty); }
        //    set { SetValue(EnableCombatScreenProperty, value); }
        //}
        #endregion

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
        //public bool EnableCombatScreen
        //{
        //    get { return (bool)GetValue(EnableCombatScreenProperty); }
        //    set { SetValue(EnableCombatScreenProperty, value); }
        //}
        #endregion

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
        //public bool EnableCombatScreen
        //{
        //    get { return (bool)GetValue(EnableCombatScreenProperty); }
        //    set { SetValue(EnableCombatScreenProperty, value); }
        //}
        #endregion

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
        //public bool EnableCombatScreen
        //{
        //    get { return (bool)GetValue(EnableCombatScreenProperty); }
        //    set { SetValue(EnableCombatScreenProperty, value); }
        //}
        #endregion

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
        //public bool EnableCombatScreen
        //{
        //    get { return (bool)GetValue(EnableCombatScreenProperty); }
        //    set { SetValue(EnableCombatScreenProperty, value); }
        //}
        #endregion

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
        //public bool EnableCombatScreen
        //{
        //    get { return (bool)GetValue(EnableCombatScreenProperty); }
        //    set { SetValue(EnableCombatScreenProperty, value); }
        //}
        #endregion

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
        #endregion

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
        #endregion

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
        #endregion

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
        #endregion

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
        #endregion

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
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}