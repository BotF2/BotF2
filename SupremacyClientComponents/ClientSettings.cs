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
        readonly bool _tracingClientSettings = false;
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
                {
                    _current = LoadCore();
                }

                return _current;
            }
        }

        public event EventHandler Saved;

        private void OnSaved()
        {
            Saved?.Invoke(null, EventArgs.Empty);

            string settingsDirectory = ResourceManager.GetResourcePath("");
            string filePath = Path.Combine(
                settingsDirectory,
                ClientSettingsFileName);

            if (_tracingClientSettings)
            {
                GameLog.Client.General.InfoFormat("SAVE     {0}: Content: (press ALT + X for Overview)" + Environment.NewLine + Environment.NewLine + "{1}" + Environment.NewLine, filePath, File.ReadAllText(filePath));
            }
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
                ClientSettings savedOrDefaultSettings = LoadCore();
                LocalValueEnumerator localValueEnumerator = savedOrDefaultSettings.GetLocalValueEnumerator();

                while (localValueEnumerator.MoveNext())
                {
                    LocalValueEntry currentEntry = localValueEnumerator.Current;
                    GameLog.Client.GeneralDetails.DebugFormat("RELOAD: Property {0} = {1}",
                        currentEntry.Property, currentEntry.Value);
                    SetValue(
                        currentEntry.Property,
                        currentEntry.Value);
                }

                List<AttachableMemberIdentifier> removedMembers = _attachedValues.Keys
                    .Where(o => !savedOrDefaultSettings._attachedValues.ContainsKey(o))
                    .ToList();

                foreach (AttachableMemberIdentifier attachableMemberIdentifier in removedMembers)
                {
                    AttachablePropertyServices.RemoveProperty(this, attachableMemberIdentifier);
                    GameLog.Client.GeneralDetails.DebugFormat("RELOAD: REMOVED entry: {0} = {1}",
                        attachableMemberIdentifier);
                }

                foreach (AttachableMemberIdentifier key in _attachedValues.Keys)
                {
                    AttachablePropertyServices.SetProperty(this, key, _attachedValues[key]);
                    GameLog.Client.GeneralDetails.DebugFormat("RELOAD: ADDED entry: {0} = {1}",
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
                string settingsDirectory = ResourceManager.GetResourcePath("");

                string filePath = Path.Combine(
                    settingsDirectory,
                    ClientSettingsFileName);

                if (!Directory.Exists(settingsDirectory))
                {
                    Directory.CreateDirectory(settingsDirectory);
                }

                using (FileStream fileWriter = File.Create(filePath))
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
                string settingsDirectory = ResourceManager.GetResourcePath("");

                string filePath = Path.Combine(
                    settingsDirectory,
                    ClientSettingsFileName);

                ClientSettings settings;


                if (File.Exists(filePath))
                {
                    using (FileStream fileReader = File.OpenRead(filePath))
                    {
                        settings = new ClientSettings();
                        try
                        {
                            // filePath = SupremacyClient..Settings.xaml
                            string _text = "for problems: just try to deleted " + filePath + " manually from your hard disk !";
                            GameLog.Client.General.InfoFormat(_text);
                            Console.WriteLine(_text);
                            settings = XamlReader.Load(fileReader) as ClientSettings ?? new ClientSettings();


                            GameLog.Client.General.InfoFormat("LOADCORE {0}: Content: (press ALT + X for Overview)" + Environment.NewLine + Environment.NewLine + "{1}" + Environment.NewLine, filePath, File.ReadAllText(filePath));

                            if (settings == null)
                            {
                                settings = new ClientSettings();
                            }

                            settings.OnLoaded();
                        }
                        catch (Exception e)
                        {

                            //_ = System.Windows.MessageBox.Show("please stop the game and delete manually: " + Environment.NewLine + filePath, "PROBLEM",MessageBoxButton.OK);

                            string _text = "LOADCORE " + filePath + ": Problem reading the file >> will be deleted" + Environment.NewLine + e;
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
            get => GetEnableDialogAnimations(this);
            set => SetEnableDialogAnimations(this, value);
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
            get => (bool)GetValue(EnableFullScreenModeProperty);
            set => SetValue(EnableFullScreenModeProperty, value);
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
            get => (double)GetValue(MasterVolumeProperty);
            set => SetValue(MasterVolumeProperty, value);
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
            get => (double)GetValue(MusicVolumeProperty);
            set => SetValue(MusicVolumeProperty, value);
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
            get => (double)GetValue(FXVolumeProperty);
            set => SetValue(FXVolumeProperty, value);
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
            get => (bool)GetValue(EnableAntiAliasingProperty);
            set => SetValue(EnableAntiAliasingProperty, value);
        }

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> EnableAntiAliasingChanged;

        private void OnEnableAntiAliasingChanged(bool oldValue, bool newValue)
        => EnableAntiAliasingChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));
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
            get => (bool)GetValue(EnableHighQualityScalingProperty);
            set => SetValue(EnableHighQualityScalingProperty, value);
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
            get => (bool)GetValue(EnableStarMapAnimationsProperty);
            set => SetValue(EnableStarMapAnimationsProperty, value);
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
            get => (bool)GetValue(EnableAnimationProperty);
            set => SetValue(EnableAnimationProperty, value);
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
        //(o, args) => ((ClientSettings)o).OnEnableCombatScreenChanged((bool)args.OldValue, (bool)args.NewValue)));

        //public event EventHandler<PropertyChangedRoutedEventArgs<bool>> EnableCombatScreenChanged;
        //private void OnEnableCombatScreenChanged(bool oldValue, bool newValue)
        //=> EnableCombatScreenChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));
        public bool EnableCombatScreen
        {
            get => (bool)GetValue(EnableCombatScreenProperty);
            set => SetValue(EnableCombatScreenProperty, value);
        }
        #endregion EnableCombatScreen Property

        #region EnableSummaryScreen Property
        public static readonly DependencyProperty EnableSummaryScreenProperty = DependencyProperty.Register(
            "EnableSummaryScreen",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));
        //(o, args) => ((ClientSettings)o).OnEnableSummaryScreenChanged((bool)args.OldValue, (bool)args.NewValue)));

        //public event EventHandler<PropertyChangedRoutedEventArgs<bool>> EnableSummaryScreenChanged;
        //private void OnEnableSummaryScreenChanged(bool oldValue, bool newValue)
        //=> EnableSummaryScreenChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool EnableSummaryScreen
        {
            get => (bool)GetValue(EnableSummaryScreenProperty);
            set => SetValue(EnableSummaryScreenProperty, value);
        }
        #endregion EnableSummaryScreen Property

        #region EnableSitRepDetailsScreen Property
        public static readonly DependencyProperty EnableSitRepDetailsScreenProperty = DependencyProperty.Register(
            "EnableSitRepDetailsScreen",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));
        //(o, args) => ((ClientSettings)o).OnEnableSitRepDetailsScreenChanged((bool)args.OldValue, (bool)args.NewValue)));

        //public event EventHandler<PropertyChangedRoutedEventArgs<bool>> EnableSitRepDetailsScreenChanged;

        //private void OnEnableSitRepDetailsScreenChanged(bool oldValue, bool newValue)
        //=> EnableSitRepDetailsScreenChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool EnableSitRepDetailsScreen
        {
            get => (bool)GetValue(EnableSitRepDetailsScreenProperty);
            set => SetValue(EnableSitRepDetailsScreenProperty, value);
        }
        #endregion EnableSitRepDetailsScreen Property

        #region TracesAI Property
        public static readonly DependencyProperty TracesAIProperty = DependencyProperty.Register(
            "AI",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));
        //(o, args) => ((ClientSettings)o).OnTracesAIChanged((bool)args.OldValue, (bool)args.NewValue)));


        public bool TracesAI
        {
            get => (bool)GetValue(TracesAIProperty);
            set
            {
                SetValue(TracesAIProperty, value);
                //GameLog.Client.General.InfoFormat("AI = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("AI");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("AI");
                }
            }
        }

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesAIChanged;

        private void OnTracesAIChanged(bool oldValue, bool newValue)
        => TracesAIChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        #endregion TracesAI Property

        #region TracesAIDetails Property
        public static readonly DependencyProperty TracesAIDetailsProperty = DependencyProperty.Register(
            "AIDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));
        //(o, args) => ((ClientSettings)o).OnTracesAIDetailsChanged((bool)args.OldValue, (bool)args.NewValue)));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesAIDetailsChanged;

        private void OnTracesAIDetailsChanged(bool oldValue, bool newValue)
        => TracesAIDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesAIDetails
        {
            get => (bool)GetValue(TracesAIDetailsProperty);
            set
            {
                SetValue(TracesAIDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesAIDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("AIDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("AIDetails");
                }
            }
        }
        #endregion TracesAIDetails Property

        #region TracesAudio Property

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesAudioChanged;

        private void OnTracesAudioChanged(bool oldValue, bool newValue)
        => TracesAudioChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public static readonly DependencyProperty TracesAudioProperty = DependencyProperty.Register(
            "Audio",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));
        //(o, args) => ((ClientSettings)o).OnTracesAudioChanged((bool)args.OldValue, (bool)args.NewValue)));

        public bool TracesAudio
        {
            get => (bool)GetValue(TracesAudioProperty);
            set
            {
                SetValue(TracesAudioProperty, value);
                //GameLog.Client.General.InfoFormat("TracesAudio = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("Audio");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("Audio");
                }
            }
        }

        #endregion TracesAudio Property

        #region TracesAudioDetails Property
        public static readonly DependencyProperty TracesAudioDetailsProperty = DependencyProperty.Register(
            "AudioDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesAudioDetailsChanged;

        private void OnTracesAudioDetailsChanged(bool oldValue, bool newValue)
        => TracesAudioDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesAudioDetails
        {
            get => (bool)GetValue(TracesAudioDetailsProperty);
            set
            {
                SetValue(TracesAudioDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesAudioDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("AudioDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("AudioDetails");
                }
            }
        }
        #endregion TracesAudioDetails Property

        #region TracesCivsAndRaces Property
        public static readonly DependencyProperty TracesCivsAndRacesProperty = DependencyProperty.Register(
            "CivsAndRaces",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesCivsAndRacesChanged;

        private void OnTracesCivsAndRacesChanged(bool oldValue, bool newValue)
        => TracesCivsAndRacesChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesCivsAndRaces
        {
            get => (bool)GetValue(TracesCivsAndRacesProperty);
            set
            {
                SetValue(TracesCivsAndRacesProperty, value);
                //GameLog.Client.General.InfoFormat("TracesCredits= {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("CivsAndRaces");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("CivsAndRaces");
                }
            }
        }
        #endregion TracesCivsAndRaces

        #region TracesCivsAndRacesDetails Property
        public static readonly DependencyProperty TracesCivsAndRacesDetailsProperty = DependencyProperty.Register(
            "CivsAndRacesDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesCivsAndRacesDetailsChanged;

        private void OnTracesCivsAndRacesDetailsChanged(bool oldValue, bool newValue)
        => TracesCivsAndRacesDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesCivsAndRacesDetails
        {
            get => (bool)GetValue(TracesCivsAndRacesDetailsProperty);
            set
            {
                SetValue(TracesCivsAndRacesDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesCivsAndRacesDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("CivsAndRacesDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("CivsAndRacesDetails");
                }
            }
        }
        #endregion TracesCivsAndRacesDetails Property

        #region TracesColonies Property
        public static readonly DependencyProperty TracesColoniesProperty = DependencyProperty.Register(
            "Colonies",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesColoniesChanged;

        private void OnTracesColoniesChanged(bool oldValue, bool newValue)
        => TracesColoniesChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesColonies
        {
            get => (bool)GetValue(TracesColoniesProperty);
            set
            {
                SetValue(TracesColoniesProperty, value);
                //GameLog.Client.General.InfoFormat("TracesColonies = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("Colonies");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("Colonies");
                }
            }
        }
        #endregion TracesColonies Property

        #region TracesColoniesDetails Property
        public static readonly DependencyProperty TracesColoniesDetailsProperty = DependencyProperty.Register(
            "ColoniesDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesColoniesDetailsChanged;

        private void OnTracesColoniesDetailsChanged(bool oldValue, bool newValue)
        => TracesColoniesDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesColoniesDetails
        {
            get => (bool)GetValue(TracesColoniesDetailsProperty);
            set
            {
                SetValue(TracesColoniesDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesColoniesDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("ColoniesDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("ColoniesDetails");
                }
            }
        }
        #endregion TracesColoniesDetails Property

        #region TracesCombat Property
        public static readonly DependencyProperty TracesCombatProperty = DependencyProperty.Register(
            "Combat",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesCombatChanged;

        private void OnTracesCombatChanged(bool oldValue, bool newValue)
        => TracesCombatChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesCombat
        {
            get => (bool)GetValue(TracesCombatProperty);
            set
            {
                SetValue(TracesCombatProperty, value);
                //GameLog.Client.General.InfoFormat("TracesCombat = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("Combat");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("Combat");
                }
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

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesCombatDetailsChanged;

        private void OnTracesCombatDetailsChanged(bool oldValue, bool newValue)
        => TracesCombatDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesCombatDetails
        {
            get => (bool)GetValue(TracesCombatDetailsProperty);
            set
            {
                SetValue(TracesCombatDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesCombatDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("CombatDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("CombatDetails");
                }
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

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesCreditsChanged;

        private void OnTracesCreditsChanged(bool oldValue, bool newValue)
        => TracesCreditsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesCredits
        {
            get => (bool)GetValue(TracesCreditsProperty);
            set
            {
                SetValue(TracesCreditsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesCredits= {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("Credits");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("Credits");
                }
            }
        }
        #endregion TracesCredits

        #region TracesCreditsDetails Property
        public static readonly DependencyProperty TracesCreditsDetailsProperty = DependencyProperty.Register(
            "CreditsDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesCreditsDetailsChanged;

        private void OnTracesCreditsDetailsChanged(bool oldValue, bool newValue)
        => TracesCreditsDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesCreditsDetails
        {
            get => (bool)GetValue(TracesCreditsDetailsProperty);
            set
            {
                SetValue(TracesCreditsDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesCreditsDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("CreditsDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("CreditsDetails");
                }
            }
        }
        #endregion TracesCreditsDetails Property

        #region TracesDeuterium Property

        public static readonly DependencyProperty TracesDeuteriumProperty = DependencyProperty.Register(
            "Deuterium",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesDeuteriumChanged;

        private void OnTracesDeuteriumChanged(bool oldValue, bool newValue)
        => TracesDeuteriumChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesDeuterium
        {
            get => (bool)GetValue(TracesDeuteriumProperty);
            set
            {
                SetValue(TracesDeuteriumProperty, value);
                //GameLog.Client.General.InfoFormat("TracesCredits= {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("Deuterium");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("Deuterium");
                }
            }
        }
        #endregion TracesDeuterium

        #region TracesDeuteriumDetails Property
        public static readonly DependencyProperty TracesDeuteriumDetailsProperty = DependencyProperty.Register(
            "DeuteriumDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesDeuteriumDetailsChanged;

        private void OnTracesDeuteriumDetailsChanged(bool oldValue, bool newValue)
        => TracesDeuteriumDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesDeuteriumDetails
        {
            get => (bool)GetValue(TracesDeuteriumDetailsProperty);
            set
            {
                SetValue(TracesDeuteriumDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesDeuteriumDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("DeuteriumDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("DeuteriumDetails");
                }
            }
        }
        #endregion TracesDeuteriumDetails Property

        #region TracesDilithium Property
        public static readonly DependencyProperty TracesDilithiumProperty = DependencyProperty.Register(
            "Dilithium",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesDilithiumChanged;

        private void OnTracesDilithiumChanged(bool oldValue, bool newValue)
        => TracesDilithiumChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesDilithium
        {
            get => (bool)GetValue(TracesDilithiumProperty);
            set
            {
                SetValue(TracesDilithiumProperty, value);
                //GameLog.Client.General.InfoFormat("TracesCredits= {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("Dilithium");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("Dilithium");
                }
            }
        }
        #endregion TracesDilithium

        #region TracesDilithiumDetails Property
        public static readonly DependencyProperty TracesDilithiumDetailsProperty = DependencyProperty.Register(
            "DilithiumDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesDilithiumDetailsChanged;

        private void OnTracesDilithiumDetailsChanged(bool oldValue, bool newValue)
        => TracesDilithiumDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesDilithiumDetails
        {
            get => (bool)GetValue(TracesDilithiumDetailsProperty);
            set
            {
                SetValue(TracesDilithiumDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesDilithiumDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("DilithiumDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("DilithiumDetails");
                }
            }
        }
        #endregion TracesDilithiumDetails Property

        #region TracesDuranium Property
        public static readonly DependencyProperty TracesDuraniumProperty = DependencyProperty.Register(
            "Duranium",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesDuraniumChanged;

        private void OnTracesDuraniumChanged(bool oldValue, bool newValue)
        => TracesDuraniumChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesDuranium
        {
            get => (bool)GetValue(TracesDuraniumProperty);
            set
            {
                SetValue(TracesDuraniumProperty, value);
                //GameLog.Client.General.InfoFormat("TracesCredits= {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("Duranium");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("Duranium");
                }
            }
        }
        #endregion TracesDuranium

        #region TracesDuraniumDetails Property
        public static readonly DependencyProperty TracesDuraniumDetailsProperty = DependencyProperty.Register(
            "DuraniumDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesDuraniumDetailsChanged;

        private void OnTracesDuraniumDetailsChanged(bool oldValue, bool newValue)
        => TracesDuraniumDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesDuraniumDetails
        {
            get => (bool)GetValue(TracesDuraniumDetailsProperty);
            set
            {
                SetValue(TracesDuraniumDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesDuraniumDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("DuraniumDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("DuraniumDetails");
                }
            }
        }
        #endregion TracesDuraniumDetails Property

        #region TracesDiplomacy Property
        public static readonly DependencyProperty TracesDiplomacyProperty = DependencyProperty.Register(
            "Diplomacy",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesDiplomacyChanged;

        private void OnTracesDiplomacyChanged(bool oldValue, bool newValue)
        => TracesDiplomacyChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesDiplomacy
        {
            get => (bool)GetValue(TracesDiplomacyProperty);
            set
            {
                SetValue(TracesDiplomacyProperty, value);
                //GameLog.Client.General.InfoFormat("TracesDiplomacy = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("Diplomacy");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("Diplomacy");
                }
            }
        }
        #endregion TracesDiplomacy Property

        #region TracesDiplomacyDetails Property
        public static readonly DependencyProperty TracesDiplomacyDetailsProperty = DependencyProperty.Register(
            "DiplomacyDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesDiplomacyDetailsChanged;

        private void OnTracesDiplomacyDetailsChanged(bool oldValue, bool newValue)
        => TracesDiplomacyDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesDiplomacyDetails
        {
            get => (bool)GetValue(TracesDiplomacyDetailsProperty);
            set
            {
                SetValue(TracesDiplomacyDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesDiplomacyDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("DiplomacyDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("DiplomacyDetails");
                }
            }
        }
        #endregion TracesDiplomacyDetails Property

        #region TracesEnergy Property
        public static readonly DependencyProperty TracesEnergyProperty = DependencyProperty.Register(
            "Energy",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesEnergyChanged;

        private void OnTracesEnergyChanged(bool oldValue, bool newValue)
        => TracesEnergyChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesEnergy
        {
            get => (bool)GetValue(TracesEnergyProperty);
            set
            {
                SetValue(TracesEnergyProperty, value);
                //GameLog.Client.General.InfoFormat("Trace for Energy = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("Energy");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("Energy");
                }
            }
        }
        #endregion TracesEnergy Property

        #region TracesEnergyDetails Property
        public static readonly DependencyProperty TracesEnergyDetailsProperty = DependencyProperty.Register(
            "EnergyDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesEnergyDetailsChanged;

        private void OnTracesEnergyDetailsChanged(bool oldValue, bool newValue)
        => TracesEnergyDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesEnergyDetails
        {
            get => (bool)GetValue(TracesEnergyDetailsProperty);
            set
            {
                SetValue(TracesEnergyDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesEnergyDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("EnergyDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("EnergyDetails");
                }
            }
        }
        #endregion TracesEnergyDetails Property

        #region TracesEvents Property
        public static readonly DependencyProperty TracesEventsProperty = DependencyProperty.Register(
            "Events",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesEventsChanged;

        private void OnTracesEventsChanged(bool oldValue, bool newValue)
        => TracesEventsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesEvents
        {
            get => (bool)GetValue(TracesEventsProperty);
            set
            {
                SetValue(TracesEventsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesEvents = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("Events");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("Events");
                }
            }
        }
        #endregion TracesEvents Property

        #region TracesEventsDetails Property
        public static readonly DependencyProperty TracesEventsDetailsProperty = DependencyProperty.Register(
            "EventsDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesEventsDetailsChanged;

        private void OnTracesEventsDetailsChanged(bool oldValue, bool newValue)
        => TracesEventsDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesEventsDetails
        {
            get => (bool)GetValue(TracesEventsDetailsProperty);
            set
            {
                SetValue(TracesEventsDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesEventsDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("EventsDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("EventsDetails");
                }
            }
        }
        #endregion TracesEventsDetails Property

        #region TracesGalaxyGenerator Property   
        // even used after retire and start a new game
        public static readonly DependencyProperty TracesGalaxyGeneratorProperty = DependencyProperty.Register(
            "GalaxyGenerator",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesGalaxyGeneratorChanged;

        private void OnTracesGalaxyGeneratorChanged(bool oldValue, bool newValue)
        => TracesGalaxyGeneratorChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesGalaxyGenerator
        {
            get => (bool)GetValue(TracesGalaxyGeneratorProperty);
            set
            {
                SetValue(TracesGalaxyGeneratorProperty, value);
                //GameLog.Client.General.InfoFormat("TracesGalaxyGenerator = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("GalaxyGenerator");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("GalaxyGenerator");
                }
            }
        }
        #endregion TracesGalaxyGenerator Property 

        #region TracesGalaxyGeneratorDetails Property
        public static readonly DependencyProperty TracesGalaxyGeneratorDetailsProperty = DependencyProperty.Register(
            "GalaxyGeneratorDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesGalaxyGeneratorDetailsChanged;

        private void OnTracesGalaxyGeneratorDetailsChanged(bool oldValue, bool newValue)
        => TracesGalaxyGeneratorDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesGalaxyGeneratorDetails
        {
            get => (bool)GetValue(TracesGalaxyGeneratorDetailsProperty);
            set
            {
                SetValue(TracesGalaxyGeneratorDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesGalaxyGeneratorDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("GalaxyGeneratorDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("GalaxyGeneratorDetails");
                }
            }
        }
        #endregion TracesGalaxyGeneratorDetails Property

        #region TracesGameData Property
        public static readonly DependencyProperty TracesGameDataProperty = DependencyProperty.Register(
            "GameData",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesGameDataChanged;

        private void OnTracesGameDataChanged(bool oldValue, bool newValue)
        => TracesGameDataChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesGameData
        {
            get => (bool)GetValue(TracesGameDataProperty);
            set
            {
                SetValue(TracesGameDataProperty, value);
                //GameLog.Client.General.InfoFormat("TracesGameData = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("GameData");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("GameData");
                }
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

        #region TracesGameDataDetails Property
        public static readonly DependencyProperty TracesGameDataDetailsProperty = DependencyProperty.Register(
            "GameDataDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesGameDataDetailsChanged;

        private void OnTracesGameDataDetailsChanged(bool oldValue, bool newValue)
        => TracesGameDataDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesGameDataDetails
        {
            get => (bool)GetValue(TracesGameDataDetailsProperty);
            set
            {
                SetValue(TracesGameDataDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesGameDataDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("GameDataDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("GameDataDetails");
                }
            }
        }
        #endregion TracesGameDataDetails Property

        #region TracesGameInitData Property
        public static readonly DependencyProperty TracesGameInitDataProperty = DependencyProperty.Register(
            "GameInitData",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesGameInitDataChanged;

        private void OnTracesGameInitDataChanged(bool oldValue, bool newValue)
        => TracesGameInitDataChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesGameInitData
        {
            get => (bool)GetValue(TracesGameInitDataProperty);
            set
            {
                SetValue(TracesGameInitDataProperty, value);
                //GameLog.Client.General.InfoFormat("TracesCredits= {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("GameInitData");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("GameInitData");
                }
            }
        }
        #endregion TracesGameInitData

        #region TracesGameInitDataDetails Property
        public static readonly DependencyProperty TracesGameInitDataDetailsProperty = DependencyProperty.Register(
            "GameInitDataDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesGameInitDataDetailsChanged;

        private void OnTracesGameInitDataDetailsChanged(bool oldValue, bool newValue)
        => TracesGameInitDataDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesGameInitDataDetails
        {
            get => (bool)GetValue(TracesGameInitDataDetailsProperty);
            set
            {
                SetValue(TracesGameInitDataDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesGameInitDataDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("GameInitDataDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("GameInitDataDetails");
                }
            }
        }
        #endregion TracesGameInitDataDetails Property


        // Traces General at the end !!! must be this !!!

        #region TracesInfoText Property
        public static readonly DependencyProperty TracesInfoTextProperty = DependencyProperty.Register(
            "InfoText",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesInfoTextChanged;

        private void OnTracesInfoTextChanged(bool oldValue, bool newValue)
        => TracesInfoTextChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesInfoText
        {
            get => (bool)GetValue(TracesInfoTextProperty);
            set
            {
                SetValue(TracesInfoTextProperty, value);
                //GameLog.Client.General.InfoFormat("TracesInfoText = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("InfoText");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("InfoText");
                }
            }
        }
        #endregion TracesInfoText Property

        #region TracesInfoTextDetails Property
        public static readonly DependencyProperty TracesInfoTextDetailsProperty = DependencyProperty.Register(
            "InfoTextDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesInfoTextDetailsChanged;

        private void OnTracesInfoTextDetailsChanged(bool oldValue, bool newValue)
        => TracesInfoTextDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesInfoTextDetails
        {
            get => (bool)GetValue(TracesInfoTextDetailsProperty);
            set
            {
                SetValue(TracesInfoTextDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesInfoTextDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("InfoTextDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("InfoTextDetails");
                }
            }
        }
        #endregion TracesInfoTextDetails Property

        #region TracesIntel Property
        public static readonly DependencyProperty TracesIntelProperty = DependencyProperty.Register(
            "Intel",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesIntelChanged;

        private void OnTracesIntelChanged(bool oldValue, bool newValue)
        => TracesIntelChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesIntel
        {
            get => (bool)GetValue(TracesIntelProperty);
            set
            {
                SetValue(TracesIntelProperty, value);
                //GameLog.Client.General.InfoFormat("TracesIntel = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("Intel");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("Intel");
                }
            }
        }
        #endregion TracesIntel Property

        #region TracesIntelDetails Property
        public static readonly DependencyProperty TracesIntelDetailsProperty = DependencyProperty.Register(
            "IntelDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesIntelDetailsChanged;

        private void OnTracesIntelDetailsChanged(bool oldValue, bool newValue)
        => TracesIntelDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesIntelDetails
        {
            get => (bool)GetValue(TracesIntelDetailsProperty);
            set
            {
                SetValue(TracesIntelDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesIntelDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("IntelDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("IntelDetails");
                }
            }
        }
        #endregion TracesIntelDetails Property


        #region TracesMapData Property
        public static readonly DependencyProperty TracesMapDataProperty = DependencyProperty.Register(
            "MapData",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesMapDataChanged;

        private void OnTracesMapDataChanged(bool oldValue, bool newValue)
        => TracesMapDataChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesMapData
        {
            get => (bool)GetValue(TracesMapDataProperty);
            set
            {
                SetValue(TracesMapDataProperty, value);
                //GameLog.Client.General.InfoFormat("TracesMapData = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("MapData");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("MapData");
                }
            }
        }
        #endregion TracesMapData Property

        #region TracesMapDataDetails Property
        public static readonly DependencyProperty TracesMapDataDetailsProperty = DependencyProperty.Register(
            "MapDataDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesMapDataDetailsChanged;

        private void OnTracesMapDataDetailsChanged(bool oldValue, bool newValue)
        => TracesMapDataDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesMapDataDetails
        {
            get => (bool)GetValue(TracesMapDataDetailsProperty);
            set
            {
                SetValue(TracesMapDataDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesMapDataDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("MapDataDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("MapDataDetails");
                }
            }
        }
        #endregion TracesMapDataDetails Property


        #region TracesMultiPlay Property
        public static readonly DependencyProperty TracesMultiPlayProperty = DependencyProperty.Register(
            "MultiPlay",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesMultiPlayChanged;

        private void OnTracesMultiPlayChanged(bool oldValue, bool newValue)
        => TracesMultiPlayChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesMultiPlay
        {
            get => (bool)GetValue(TracesMultiPlayProperty);
            set
            {
                SetValue(TracesMultiPlayProperty, value);
                //GameLog.Client.General.InfoFormat("TracesMultiPlay = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("MultiPlay");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("MultiPlay");
                }
            }
        }
        #endregion TracesMultiPlay Property

        #region TracesMultiPlayDetails Property
        public static readonly DependencyProperty TracesMultiPlayDetailsProperty = DependencyProperty.Register(
            "MultiPlayDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesMultiPlayDetailsChanged;

        private void OnTracesMultiPlayDetailsChanged(bool oldValue, bool newValue)
        => TracesMultiPlayDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesMultiPlayDetails
        {
            get => (bool)GetValue(TracesMultiPlayDetailsProperty);
            set
            {
                SetValue(TracesMultiPlayDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesMultiPlayDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("MultiPlayDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("MultiPlayDetails");
                }
            }
        }
        #endregion TracesMultiPlayDetails Property


        #region TracesProduction Property
        public static readonly DependencyProperty TracesProductionProperty = DependencyProperty.Register(
            "Production",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesProductionChanged;

        private void OnTracesProductionChanged(bool oldValue, bool newValue)
        => TracesProductionChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesProduction
        {
            get => (bool)GetValue(TracesProductionProperty);
            set
            {
                SetValue(TracesProductionProperty, value);
                //GameLog.Client.General.InfoFormat("TracesProduction = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("Production");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("Production");
                }
            }
        }
        #endregion TracesProduction Property

        #region TracesProductionDetails Property
        public static readonly DependencyProperty TracesProductionDetailsProperty = DependencyProperty.Register(
            "ProductionDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesProductionDetailsChanged;

        private void OnTracesProductionDetailsChanged(bool oldValue, bool newValue)
        => TracesProductionDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesProductionDetails
        {
            get => (bool)GetValue(TracesProductionDetailsProperty);
            set
            {
                SetValue(TracesProductionDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesProductionDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("ProductionDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("ProductionDetails");
                }
            }
        }
        #endregion TracesProductionDetails Property



        #region TracesSitReps Property
        public static readonly DependencyProperty TracesSitRepsProperty = DependencyProperty.Register(
            "SitReps",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesSitRepsChanged;

        private void OnTracesSitRepsChanged(bool oldValue, bool newValue)
        => TracesSitRepsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesSitReps
        {
            get => (bool)GetValue(TracesSitRepsProperty);
            set
            {
                SetValue(TracesSitRepsProperty, value);
                //GameLog.Client.General.InfoFormat("SitReps = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("SitReps");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("SitReps");
                }
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

        #region TracesSitRepsDetails Property
        public static readonly DependencyProperty TracesSitRepsDetailsProperty = DependencyProperty.Register(
            "SitRepsDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesSitRepsDetailsChanged;

        private void OnTracesSitRepsDetailsChanged(bool oldValue, bool newValue)
        => TracesSitRepsDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesSitRepsDetails
        {
            get => (bool)GetValue(TracesSitRepsDetailsProperty);
            set
            {
                SetValue(TracesSitRepsDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesSitRepsDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("SitRepsDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("SitRepsDetails");
                }
            }
        }
        #endregion TracesSitRepsDetails Property


        #region TracesReportErrorsToEmail Property
        public static readonly DependencyProperty ReportErrorsToEmailProperty = DependencyProperty.Register(
            "ReportErrorsToEmail",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        //public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesReportErrorsToEmailChanged;

        //private void OnTracesReportErrorsToEmailChanged(bool oldValue, bool newValue)
        //=> TracesReportErrorsToEmailChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesReportErrorsToEmail
        {
            get => (bool)GetValue(ReportErrorsToEmailProperty);
            set => SetValue(ReportErrorsToEmailProperty, value);
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

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesResearchChanged;

        private void OnTracesResearchChanged(bool oldValue, bool newValue)
        => TracesResearchChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesResearch
        {
            get => (bool)GetValue(TracesResearchProperty);
            set
            {
                SetValue(TracesResearchProperty, value);
                //GameLog.Client.General.InfoFormat("TracesResearch = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("Research");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("Research");
                }
            }
        }
        #endregion TracesResearch Property

        #region TracesResearchDetails Property
        public static readonly DependencyProperty TracesResearchDetailsProperty = DependencyProperty.Register(
            "ResearchDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesResearchDetailsChanged;

        private void OnTracesResearchDetailsChanged(bool oldValue, bool newValue)
        => TracesResearchDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesResearchDetails
        {
            get => (bool)GetValue(TracesResearchDetailsProperty);
            set
            {
                SetValue(TracesResearchDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesResearchDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("ResearchDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("ResearchDetails");
                }
            }
        }
        #endregion TracesResearchDetails Property

        #region TracesSaveLoad Property
        public static readonly DependencyProperty TracesSaveLoadProperty = DependencyProperty.Register(
            "SaveLoad",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesSaveLoadChanged;

        private void OnTracesSaveLoadChanged(bool oldValue, bool newValue)
        => TracesSaveLoadChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesSaveLoad
        {
            get => (bool)GetValue(TracesSaveLoadProperty);
            set
            {
                SetValue(TracesSaveLoadProperty, value);
                //GameLog.Client.General.InfoFormat("TracesSaveLoad = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("SaveLoad");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("SaveLoad");
                }
            }
        }
        #endregion TracesSaveLoad Property

        #region TracesSaveLoadDetails Property
        public static readonly DependencyProperty TracesSaveLoadDetailsProperty = DependencyProperty.Register(
            "SaveLoadDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesSaveLoadDetailsChanged;

        private void OnTracesSaveLoadDetailsChanged(bool oldValue, bool newValue)
        => TracesSaveLoadDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesSaveLoadDetails
        {
            get => (bool)GetValue(TracesSaveLoadDetailsProperty);
            set
            {
                SetValue(TracesSaveLoadDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesSaveLoadDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("SaveLoadDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("SaveLoadDetails");
                }
            }
        }
        #endregion TracesSaveLoadDetails Property

        #region TracesShips Property
        public static readonly DependencyProperty TracesShipsProperty = DependencyProperty.Register(
            "Ships",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesShipsChanged;

        private void OnTracesShipsChanged(bool oldValue, bool newValue)
        => TracesShipsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesShips
        {
            get => (bool)GetValue(TracesShipsProperty);
            set
            {
                SetValue(TracesShipsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesShips = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("Ships");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("Ships");
                }
            }
        }
        #endregion TracesShips

        #region TracesShipsDetails Property
        public static readonly DependencyProperty TracesShipsDetailsProperty = DependencyProperty.Register(
            "ShipsDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesShipsDetailsChanged;

        private void OnTracesShipsDetailsChanged(bool oldValue, bool newValue)
        => TracesShipsDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesShipsDetails
        {
            get => (bool)GetValue(TracesShipsDetailsProperty);
            set
            {
                SetValue(TracesShipsDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesShipsDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("ShipsDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("ShipsDetails");
                }
            }
        }
        #endregion TracesShipsDetails Property

        #region TracesShipProduction Property
        public static readonly DependencyProperty TracesShipProductionProperty = DependencyProperty.Register(
            "ShipProduction",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesShipProductionChanged;

        private void OnTracesShipProductionChanged(bool oldValue, bool newValue)
        => TracesShipProductionChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesShipProduction
        {
            get => (bool)GetValue(TracesShipProductionProperty);
            set
            {
                SetValue(TracesShipProductionProperty, value);
                //GameLog.Client.General.InfoFormat("TracesShipProduction = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("ShipProduction");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("ShipProduction");
                }
            }
        }
        #endregion TracesShipProduction Property

        #region TracesShipProductionDetails Property
        public static readonly DependencyProperty TracesShipProductionDetailsProperty = DependencyProperty.Register(
            "ShipProductionDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesShipProductionDetailsChanged;

        private void OnTracesShipProductionDetailsChanged(bool oldValue, bool newValue)
        => TracesShipProductionDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesShipProductionDetails
        {
            get => (bool)GetValue(TracesShipProductionDetailsProperty);
            set
            {
                SetValue(TracesShipProductionDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesShipProductionDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("ShipProductionDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("ShipProductionDetails");
                }
            }
        }
        #endregion TracesShipProductionDetails Property

        #region TracesStations Property
        public static readonly DependencyProperty TracesStationsProperty = DependencyProperty.Register(
            "Stations",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesStationsChanged;

        private void OnTracesStationsChanged(bool oldValue, bool newValue)
        => TracesStationsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesStations
        {
            get => (bool)GetValue(TracesStationsProperty);
            set
            {
                SetValue(TracesStationsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesStations = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("Stations");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("Stations");
                }
            }
        }
        #endregion TracesStations Property

        #region TracesStationsDetails Property
        public static readonly DependencyProperty TracesStationsDetailsProperty = DependencyProperty.Register(
            "StationsDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesStationsDetailsChanged;

        private void OnTracesStationsDetailsChanged(bool oldValue, bool newValue)
        => TracesStationsDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesStationsDetails
        {
            get => (bool)GetValue(TracesStationsDetailsProperty);
            set
            {
                SetValue(TracesStationsDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesStationsDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("StationsDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("StationsDetails");
                }
            }
        }
        #endregion TracesStationsDetails Property

        #region TracesStructures Property
        public static readonly DependencyProperty TracesStructuresProperty = DependencyProperty.Register(
            "Structures",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesStructuresChanged;

        private void OnTracesStructuresChanged(bool oldValue, bool newValue)
        => TracesStructuresChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesStructures
        {
            get => (bool)GetValue(TracesStructuresProperty);
            set
            {
                SetValue(TracesStructuresProperty, value);
                //GameLog.Client.General.InfoFormat("TracesStructures = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("Structures");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("Structures");
                }
            }
        }
        #endregion TracesStructures Property

        #region TracesStructuresDetails Property
        public static readonly DependencyProperty TracesStructuresDetailsProperty = DependencyProperty.Register(
            "StructuresDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesStructuresDetailsChanged;

        private void OnTracesStructuresDetailsChanged(bool oldValue, bool newValue)
        => TracesStructuresDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesStructuresDetails
        {
            get => (bool)GetValue(TracesStructuresDetailsProperty);
            set
            {
                SetValue(TracesStructuresDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesStructuresDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("StructuresDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("StructuresDetails");
                }
            }
        }
        #endregion TracesStructuresDetails Property

        #region TracesSystemAssault Property
        public static readonly DependencyProperty TracesSystemAssaultProperty = DependencyProperty.Register(
            "SystemAssault",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesSystemAssaultChanged;

        private void OnTracesSystemAssaultChanged(bool oldValue, bool newValue)
        => TracesSystemAssaultChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesSystemAssault
        {
            get => (bool)GetValue(TracesSystemAssaultProperty);
            set
            {
                SetValue(TracesSystemAssaultProperty, value);
                //GameLog.Client.General.InfoFormat("TracesSystemAssault = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("SystemAssault");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("SystemAssault");
                }
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

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesSystemAssaultDetailsChanged;

        private void OnTracesSystemAssaultDetailsChanged(bool oldValue, bool newValue)
        => TracesSystemAssaultDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesSystemAssaultDetails
        {
            get => (bool)GetValue(TracesSystemAssaultDetailsProperty);
            set
            {
                SetValue(TracesSystemAssaultDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesSystemAssaultDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("SystemAssaultDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("SystemAssaultDetails");
                }
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

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesTestChanged;

        private void OnTracesTestChanged(bool oldValue, bool newValue)
        => TracesTestChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesTest
        {
            get => (bool)GetValue(TracesTestProperty);
            set
            {
                SetValue(TracesTestProperty, value);
                //GameLog.Client.General.InfoFormat("TracesTest = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("Test");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("Test");
                }
            }
        }
        #endregion TracesTest Property  

        #region TracesTestDetails Property
        public static readonly DependencyProperty TracesTestDetailsProperty = DependencyProperty.Register(
            "TestDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesTestDetailsChanged;

        private void OnTracesTestDetailsChanged(bool oldValue, bool newValue)
        => TracesTestDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesTestDetails
        {
            get => (bool)GetValue(TracesTestDetailsProperty);
            set
            {
                SetValue(TracesTestDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesTestDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("TestDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("TestDetails");
                }
            }
        }
        #endregion TracesTestDetails Property

        #region TracesTradeRoutes Property
        public static readonly DependencyProperty TracesTradeRoutesProperty = DependencyProperty.Register(
            "TradeRoutes",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesTradeRoutesChanged;

        private void OnTracesTradeRoutesChanged(bool oldValue, bool newValue)
        => TracesTradeRoutesChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesTradeRoutes
        {
            get => (bool)GetValue(TracesTradeRoutesProperty);
            set
            {
                SetValue(TracesTradeRoutesProperty, value);
                //GameLog.Client.General.InfoFormat("TracesTradeRoutes = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("TradeRoutes");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("TradeRoutes");
                }
            }
        }
        #endregion TracesTradeRoutes Property

        #region TracesTradeRoutesDetails Property
        public static readonly DependencyProperty TracesTradeRoutesDetailsProperty = DependencyProperty.Register(
            "TradeRoutesDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesTradeRoutesDetailsChanged;

        private void OnTracesTradeRoutesDetailsChanged(bool oldValue, bool newValue)
        => TracesTradeRoutesDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesTradeRoutesDetails
        {
            get => (bool)GetValue(TracesTradeRoutesDetailsProperty);
            set
            {
                SetValue(TracesTradeRoutesDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesTradeRoutesDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("TradeRoutesDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("TradeRoutesDetails");
                }
            }
        }
        #endregion TracesTradeRoutesDetails Property

        #region TracesUI Property
        public static readonly DependencyProperty TracesUIProperty = DependencyProperty.Register(
            "UI",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesUIChanged;

        private void OnTracesUIChanged(bool oldValue, bool newValue)
        => TracesUIChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesUI
        {
            get => (bool)GetValue(TracesUIProperty);
            set
            {
                SetValue(TracesUIProperty, value);
                //GameLog.Client.General.InfoFormat("TracesUI = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("UI");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("UI");
                }
            }
        }
        #endregion TracesUI Property

        #region TracesUIDetails Property
        public static readonly DependencyProperty TracesUIDetailsProperty = DependencyProperty.Register(
            "UIDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesUIDetailsChanged;

        private void OnTracesUIDetailsChanged(bool oldValue, bool newValue)
        => TracesUIDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesUIDetails
        {
            get => (bool)GetValue(TracesUIDetailsProperty);
            set
            {
                SetValue(TracesUIDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesUIDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("UIDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("UIDetails");
                }
            }
        }
        #endregion TracesUIDetails Property

        #region TracesXMLCheck Property  
        public static readonly DependencyProperty TracesXMLCheckProperty = DependencyProperty.Register(
            "XMLCheck",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesXMLCheckChanged;

        private void OnTracesXMLCheckChanged(bool oldValue, bool newValue)
        => TracesXMLCheckChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesXMLCheck
        {
            get => (bool)GetValue(TracesXMLCheckProperty);
            set
            {
                SetValue(TracesXMLCheckProperty, value);
                //GameLog.Client.General.InfoFormat("TracesXMLCheck = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("XMLCheck");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("XMLCheck");
                }
            }
        }
        #endregion TracesXMLCheck

        #region TracesXMLCheckDetails Property
        public static readonly DependencyProperty TracesXMLCheckDetailsProperty = DependencyProperty.Register(
            "XMLCheckDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesXMLCheckDetailsChanged;

        private void OnTracesXMLCheckDetailsChanged(bool oldValue, bool newValue)
        => TracesXMLCheckDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesXMLCheckDetails
        {
            get => (bool)GetValue(TracesXMLCheckDetailsProperty);
            set
            {
                SetValue(TracesXMLCheckDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesXMLCheckDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("XMLCheckDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("XMLCheckDetails");
                }
            }
        }
        #endregion TracesXMLCheckDetails Property

        #region TracesXML2CSVOutput Property  
        public static readonly DependencyProperty TracesXML2CSVOutputProperty = DependencyProperty.Register(
            "XML2CSVOutput",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesXML2CSVOutputChanged;

        private void OnTracesXML2CSVOutputChanged(bool oldValue, bool newValue)
        => TracesXML2CSVOutputChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesXML2CSVOutput
        {
            get => (bool)GetValue(TracesXML2CSVOutputProperty);
            set
            {
                SetValue(TracesXML2CSVOutputProperty, value);
                //GameLog.Client.General.InfoFormat("TracesXML2CSVOutput = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("XML2CSVOutput");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("XML2CSVOutput");
                }
            }
        }
        #endregion TracesXML2CSVOutput

        #region TracesXML2CSVOutputDetails Property
        public static readonly DependencyProperty TracesXML2CSVOutputDetailsProperty = DependencyProperty.Register(
            "XML2CSVOutputDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesXML2CSVOutputDetailsChanged;

        private void OnTracesXML2CSVOutputDetailsChanged(bool oldValue, bool newValue)
        => TracesXML2CSVOutputDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesXML2CSVOutputDetails
        {
            get => (bool)GetValue(TracesXML2CSVOutputDetailsProperty);
            set
            {
                SetValue(TracesXML2CSVOutputDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesXML2CSVOutputDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("XML2CSVOutputDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("XML2CSVOutputDetails");
                }
            }
        }
        #endregion TracesXML2CSVOutputDetails Property

        #region TracesGeneral Property
        public static readonly DependencyProperty TracesGeneralProperty = DependencyProperty.Register(
            "General",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesGeneralChanged;

        private void OnTracesGeneralChanged(bool oldValue, bool newValue)
        => TracesGeneralChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesGeneral
        {
            get => (bool)GetValue(TracesGeneralProperty);
            set
            {
                SetValue(TracesGeneralProperty, value);
                //GameLog.Client.General.InfoFormat("TracesGeneral = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("General");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("General");
                }
            }
        }
        #endregion TracesGeneral Property

        #region TracesGeneralDetails Property
        public static readonly DependencyProperty TracesGeneralDetailsProperty = DependencyProperty.Register(
            "GeneralDetails",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesGeneralDetailsChanged;

        private void OnTracesGeneralDetailsChanged(bool oldValue, bool newValue)
        => TracesGeneralDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool TracesGeneralDetails
        {
            get => (bool)GetValue(TracesGeneralDetailsProperty);
            set
            {
                SetValue(TracesGeneralDetailsProperty, value);
                //GameLog.Client.General.InfoFormat("TracesGeneralDetails = {0}", value);
                if (value)
                {
                    GameLog.SetRepositoryToDebug("GeneralDetails");
                }
                else
                {
                    GameLog.SetRepositoryToErrorOnly("GeneralDetails");
                }
            }
        }
        #endregion TracesGeneralDetails Property



        #region ReportErrors Property
        public static readonly DependencyProperty ReportErrorsProperty = DependencyProperty.Register(
            "ReportErrors",
            typeof(bool),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        //public event EventHandler<PropertyChangedRoutedEventArgs<bool>> TracesReportErrorsChanged;

        //private void OnTracesReportErrorsChanged(bool oldValue, bool newValue)
        //=> TracesReportErrorsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool ReportErrors
        {
            get => (bool)GetValue(ReportErrorsProperty);
            set => SetValue(ReportErrorsProperty, value);
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
            get => (double)GetValue(ClientWindowWidthProperty);
            set => SetValue(ClientWindowWidthProperty, value);
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
            get => (double)GetValue(ClientWindowHeightProperty);
            set => SetValue(ClientWindowHeightProperty, value);
        }
        #endregion ClientWindowHeight Property

        #region WidthSpecial1 Property
        public static readonly DependencyProperty WidthSpecial1Property = DependencyProperty.Register(
            "WidthSpecial1",
            typeof(double),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                576.0,
                FrameworkPropertyMetadataOptions.None));

        public double WidthSpecial1
        {
            get => (double)GetValue(WidthSpecial1Property);
            set => SetValue(WidthSpecial1Property, value);
        }
        #endregion WidthSpecial1 Property

        #region HeightSpecial1 Property
        public static readonly DependencyProperty HeightSpecial1Property = DependencyProperty.Register(
            "HeightSpecial1",
            typeof(double),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                480.0,
                FrameworkPropertyMetadataOptions.None));

        public double HeightSpecial1
        {
            get => (double)GetValue(HeightSpecial1Property);
            set => SetValue(HeightSpecial1Property, value);
        }
        #endregion HeightSpecial1 Property

        #region WidthSpecial2 Property
        public static readonly DependencyProperty WidthSpecial2Property = DependencyProperty.Register(
            "WidthSpecial2",
            typeof(double),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                1152.0,
                FrameworkPropertyMetadataOptions.None));

        public double WidthSpecial2
        {
            get => (double)GetValue(WidthSpecial2Property);
            set => SetValue(WidthSpecial2Property, value);
        }
        #endregion WidthSpecial2 Property

        #region HeightSpecial2 Property
        public static readonly DependencyProperty HeightSpecial2Property = DependencyProperty.Register(
            "HeightSpecial2",
            typeof(double),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                960.0,
                FrameworkPropertyMetadataOptions.None));

        public double HeightSpecial2
        {
            get => (double)GetValue(HeightSpecial2Property);
            set => SetValue(HeightSpecial2Property, value);
        }
        #endregion HeightSpecial2 Property

        #region WidthSpecial3 Property
        public static readonly DependencyProperty WidthSpecial3Property = DependencyProperty.Register(
            "WidthSpecial3",
            typeof(double),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                270.0,
                FrameworkPropertyMetadataOptions.None));

        public double WidthSpecial3
        {
            get => (double)GetValue(WidthSpecial3Property);
            set => SetValue(WidthSpecial3Property, value);
        }
        #endregion WidthSpecial3 Property

        #region HeightSpecial3 Property
        public static readonly DependencyProperty HeightSpecial3Property = DependencyProperty.Register(
            "HeightSpecial3",
            typeof(double),
            typeof(ClientSettings),
            new FrameworkPropertyMetadata(
                225.0,
                FrameworkPropertyMetadataOptions.None));

        public double HeightSpecial3
        {
            get => (double)GetValue(HeightSpecial3Property);
            set => SetValue(HeightSpecial3Property, value);
        }
        #endregion HeightSpecial3 Property

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
            get => (bool)GetValue(EnableScreenTransitionsProperty);
            set => SetValue(EnableScreenTransitionsProperty, value);
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
            get => (int)GetValue(DesiredAnimationFrameRateProperty);
            set => SetValue(DesiredAnimationFrameRateProperty, value);
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
            {
                return false;
            }

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

        int IAttachedPropertyStore.PropertyCount => _attachedValues.Count;

        #endregion

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        //public event EventHandler<PropertyChangedRoutedEventArgs<bool>> Traces_SetAll_without_DetailsChanged;

        //private void OnTraces_SetAll_without_DetailsChanged(bool oldValue, bool newValue)
        //=> Traces_SetAll_without_DetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));


        public bool Traces_SetAll_without_Details
        {
            get => (bool)GetValue(Traces_SetAll_without_DetailsProperty);
            set
            {
                SetValue(Traces_SetAll_without_DetailsProperty, value);

                GameLog.Client.General.InfoFormat("#### Log.Txt: 'Set All w/o Details' for Traces (press ingame CTRL + P, for overview > ALT + X)");  // in Log.Txt only DEBUG = yes get a line

                if (value)
                {
                    // "General" shows the Log.txt-lines for all the others
                    SetValue(TracesGeneralProperty, value); OnTracesGeneralChanged(false, true); GameLog.SetRepositoryToDebug("General");
                    SetValue(TracesGeneralDetailsProperty, false); OnTracesGeneralDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GeneralDetails");
                    // Audio changes shall be done directly = OnTracesAudioChanged

                    SetValue(TracesAIProperty, value); OnTracesAIChanged(false, true); GameLog.SetRepositoryToDebug("AI");
                    SetValue(TracesAudioProperty, value); OnTracesAudioChanged(false, true); GameLog.SetRepositoryToDebug("Audio");
                    SetValue(TracesCivsAndRacesProperty, value); OnTracesCivsAndRacesChanged(false, true); GameLog.SetRepositoryToDebug("CivsAndRaces");
                    SetValue(TracesColoniesProperty, value); OnTracesColoniesChanged(false, true); GameLog.SetRepositoryToDebug("Colonies");
                    SetValue(TracesCombatProperty, value); OnTracesCombatChanged(false, true); GameLog.SetRepositoryToDebug("Combat");
                    SetValue(TracesCreditsProperty, value); OnTracesCreditsChanged(false, true); GameLog.SetRepositoryToDebug("Credits");
                    SetValue(TracesDeuteriumProperty, value); OnTracesDeuteriumChanged(false, true); GameLog.SetRepositoryToDebug("Deuterium");
                    SetValue(TracesDilithiumProperty, value); OnTracesDilithiumChanged(false, true); GameLog.SetRepositoryToDebug("Dilithium");
                    SetValue(TracesDiplomacyProperty, value); OnTracesDiplomacyChanged(false, true); GameLog.SetRepositoryToDebug("Diplomacy");
                    SetValue(TracesDuraniumProperty, value); OnTracesDuraniumDetailsChanged(false, true); GameLog.SetRepositoryToDebug("Duranium");
                    SetValue(TracesEnergyProperty, value); OnTracesEnergyChanged(false, true); GameLog.SetRepositoryToDebug("Energy");
                    SetValue(TracesEventsProperty, value); OnTracesEventsChanged(false, true); GameLog.SetRepositoryToDebug("Events");
                    SetValue(TracesGalaxyGeneratorProperty, value); OnTracesGalaxyGeneratorChanged(false, true); GameLog.SetRepositoryToDebug("GalaxyGenerator");
                    SetValue(TracesGameDataProperty, value); OnTracesGameDataChanged(false, true); GameLog.SetRepositoryToDebug("GameData");
                    SetValue(TracesGameInitDataProperty, value); OnTracesGameInitDataChanged(false, true); GameLog.SetRepositoryToDebug("GameInitData");
                    SetValue(TracesGeneralProperty, value); OnTracesGeneralChanged(false, true); GameLog.SetRepositoryToDebug("General");
                    SetValue(TracesInfoTextProperty, value); OnTracesInfoTextChanged(false, true); GameLog.SetRepositoryToDebug("InfoText");
                    SetValue(TracesIntelProperty, value); OnTracesIntelChanged(false, true); GameLog.SetRepositoryToDebug("Intel");
                    SetValue(TracesMapDataProperty, value); OnTracesMapDataChanged(false, true); GameLog.SetRepositoryToDebug("MapData");
                    SetValue(TracesMultiPlayProperty, value); OnTracesMultiPlayChanged(false, true); GameLog.SetRepositoryToDebug("MultiPlay");
                    SetValue(TracesProductionProperty, value); OnTracesProductionChanged(false, true); GameLog.SetRepositoryToDebug("Production");
                    //SetValue(TracesReportErrorsProperty, value); OnTracesReportErrorsChanged(false, true); GameLog.SetRepositoryToDebug("ReportErrors");
                    SetValue(TracesResearchProperty, value); OnTracesResearchChanged(false, true); GameLog.SetRepositoryToDebug("Research");
                    SetValue(TracesSitRepsProperty, value); OnTracesSitRepsChanged(false, true); GameLog.SetRepositoryToDebug("SitReps");
                    SetValue(TracesSaveLoadProperty, value); OnTracesSaveLoadChanged(false, true); GameLog.SetRepositoryToDebug("SaveLoad");
                    SetValue(TracesShipsProperty, value); OnTracesShipsChanged(false, true); GameLog.SetRepositoryToDebug("Ships");
                    SetValue(TracesShipProductionProperty, value); OnTracesShipProductionChanged(false, true); GameLog.SetRepositoryToDebug("ShipProduction");
                    SetValue(TracesStationsProperty, value); OnTracesStationsChanged(false, true); GameLog.SetRepositoryToDebug("Stations");
                    SetValue(TracesStructuresProperty, value); OnTracesStructuresChanged(false, true); GameLog.SetRepositoryToDebug("Structures");
                    SetValue(TracesSystemAssaultProperty, value); OnTracesSystemAssaultChanged(false, true); GameLog.SetRepositoryToDebug("SystemAssault");
                    // no  SetValue(TracesSystemAssaultDetailsProperty, value); OnTracesSystemAssaultDetailsChanged(false, true); GameLog.SetRepositoryToDebug("SystemAssaultDetails");
                    SetValue(TracesTestProperty, value); OnTracesTestChanged(false, true); GameLog.SetRepositoryToDebug("Test");
                    SetValue(TracesTradeRoutesProperty, value); OnTracesTradeRoutesChanged(false, true); GameLog.SetRepositoryToDebug("TradeRoutes");
                    SetValue(TracesUIProperty, value); OnTracesUIChanged(false, true); GameLog.SetRepositoryToDebug("UI");
                    SetValue(TracesXMLCheckProperty, value); OnTracesXMLCheckChanged(false, true); GameLog.SetRepositoryToDebug("XMLCheck");
                    SetValue(TracesXML2CSVOutputProperty, value); OnTracesXML2CSVOutputChanged(false, true); GameLog.SetRepositoryToDebug("XML2CSVOutput");

                    // Details
                    value = false;
                    SetValue(TracesAIDetailsProperty, value); OnTracesAIDetailsChanged(false, true); GameLog.SetRepositoryToDebug("AIDetails");
                    SetValue(TracesAudioDetailsProperty, value); OnTracesAudioDetailsChanged(false, true); GameLog.SetRepositoryToDebug("AudioDetails");
                    SetValue(TracesCivsAndRacesDetailsProperty, value); OnTracesCivsAndRacesDetailsChanged(false, true); GameLog.SetRepositoryToDebug("CivsAndRacesDetails");
                    SetValue(TracesColoniesDetailsProperty, value); OnTracesColoniesDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ColoniesDetails");
                    SetValue(TracesCombatDetailsProperty, value); OnTracesCombatDetailsChanged(false, true); GameLog.SetRepositoryToDebug("CombatDetails");
                    SetValue(TracesCreditsDetailsProperty, value); OnTracesCreditsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("CreditsDetails");
                    SetValue(TracesDeuteriumDetailsProperty, value); OnTracesDeuteriumDetailsChanged(false, true); GameLog.SetRepositoryToDebug("DeuteriumDetails");
                    SetValue(TracesDilithiumDetailsProperty, value); OnTracesDilithiumDetailsChanged(false, true); GameLog.SetRepositoryToDebug("DilithiumDetails");
                    SetValue(TracesDuraniumDetailsProperty, value); OnTracesDuraniumDetailsChanged(false, true); GameLog.SetRepositoryToDebug("DuraniumDetails");
                    SetValue(TracesDiplomacyDetailsProperty, value); OnTracesDiplomacyDetailsChanged(false, true); GameLog.SetRepositoryToDebug("DiplomacyDetails");
                    SetValue(TracesEnergyDetailsProperty, value); OnTracesEnergyDetailsChanged(false, true); GameLog.SetRepositoryToDebug("EnergyDetails");
                    SetValue(TracesEventsDetailsProperty, value); OnTracesEventsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("EventsDetails");
                    SetValue(TracesGalaxyGeneratorDetailsProperty, value); OnTracesGalaxyGeneratorDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GalaxyGeneratorDetails");
                    SetValue(TracesGameDataDetailsProperty, value); OnTracesGameDataDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GameDataDetails");
                    SetValue(TracesGameInitDataDetailsProperty, value); OnTracesGameInitDataDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GameInitDataDetails");
                    // done at first
                    //SetValue(TracesGeneralDetailsProperty, value); OnTracesGeneralDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GeneralDetails");
                    SetValue(TracesInfoTextDetailsProperty, value); OnTracesInfoTextDetailsChanged(false, true); GameLog.SetRepositoryToDebug("InfoTextDetails");
                    SetValue(TracesIntelDetailsProperty, value); OnTracesIntelDetailsChanged(false, true); GameLog.SetRepositoryToDebug("IntelDetails");
                    SetValue(TracesMapDataDetailsProperty, value); OnTracesMapDataDetailsChanged(false, true); GameLog.SetRepositoryToDebug("MapDataDetails");
                    SetValue(TracesMultiPlayDetailsProperty, value); OnTracesMultiPlayDetailsChanged(false, true); GameLog.SetRepositoryToDebug("MultiPlayDetails");
                    SetValue(TracesProductionDetailsProperty, value); OnTracesProductionDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ProductionDetails");
                    //////SetValue(TracesReportErrorsDetailsProperty, value); OnTracesReportErrorsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ReportErrorsDetails");
                    SetValue(TracesResearchDetailsProperty, value); OnTracesResearchDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ResearchDetails");
                    SetValue(TracesSitRepsDetailsProperty, value); OnTracesSitRepsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("SitRepsDetails");
                    SetValue(TracesSaveLoadDetailsProperty, value); OnTracesSaveLoadDetailsChanged(false, true); GameLog.SetRepositoryToDebug("SaveLoadDetails");
                    SetValue(TracesShipsDetailsProperty, value); OnTracesShipsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ShipsDetails");
                    SetValue(TracesShipProductionDetailsProperty, value); OnTracesShipProductionDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ShipProductionDetails");
                    SetValue(TracesStationsDetailsProperty, value); OnTracesStationsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("StationsDetails");
                    SetValue(TracesStructuresDetailsProperty, value); OnTracesStructuresDetailsChanged(false, true); GameLog.SetRepositoryToDebug("StructuresDetails");
                    SetValue(TracesSystemAssaultDetailsProperty, value); OnTracesSystemAssaultDetailsChanged(false, true); GameLog.SetRepositoryToDebug("SystemAssaultDetails");
                    SetValue(TracesTestDetailsProperty, value); OnTracesTestDetailsChanged(false, true); GameLog.SetRepositoryToDebug("TestDetails");
                    SetValue(TracesTradeRoutesDetailsProperty, value); OnTracesTradeRoutesDetailsChanged(false, true); GameLog.SetRepositoryToDebug("TradeRoutesDetails");
                    SetValue(TracesUIDetailsProperty, value); OnTracesUIDetailsChanged(false, true); GameLog.SetRepositoryToDebug("UIDetails");
                    SetValue(TracesXMLCheckDetailsProperty, value); OnTracesXMLCheckDetailsChanged(false, true); GameLog.SetRepositoryToDebug("XMLCheckDetails");
                    SetValue(TracesXML2CSVOutputDetailsProperty, value); OnTracesXML2CSVOutputDetailsChanged(false, true); GameLog.SetRepositoryToDebug("XML2CSVOutputDetails");

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

        //public event EventHandler<PropertyChangedRoutedEventArgs<bool>> Traces_SetAll_and_DetailsChanged;

        //private void OnTraces_SetAll_and_DetailsChanged(bool oldValue, bool newValue)
        //=> Traces_SetAll_and_DetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool Traces_SetAll_and_Details
        {
            get => (bool)GetValue(Traces_SetAll_and_DetailsProperty);
            set
            {
                SetValue(Traces_SetAll_and_DetailsProperty, value);

                GameLog.Client.General.InfoFormat("    #### Log.Txt: 'SetAll_and_Details'  for Traces (press ingame CTRL + P, for overview > ALT + X)");  // in Log.Txt only DEBUG = yes get a line

                if (value)
                {
                    // "General" shows the Log.txt-lines for all the others
                    SetValue(TracesGeneralProperty, value); OnTracesGeneralChanged(false, true); GameLog.SetRepositoryToDebug("General");
                    SetValue(TracesGeneralDetailsProperty, value); OnTracesGeneralDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GeneralDetails");
                    // Audio changes shall be done directly = OnTracesAudioChanged

                    SetValue(TracesAIProperty, value); OnTracesAIChanged(false, true); GameLog.SetRepositoryToDebug("AI");
                    SetValue(TracesAudioProperty, value); OnTracesAudioChanged(false, true); GameLog.SetRepositoryToDebug("Audio");
                    SetValue(TracesCivsAndRacesProperty, value); OnTracesCivsAndRacesChanged(false, true); GameLog.SetRepositoryToDebug("CivsAndRaces");
                    SetValue(TracesColoniesProperty, value); OnTracesColoniesChanged(false, true); GameLog.SetRepositoryToDebug("Colonies");
                    SetValue(TracesCombatProperty, value); OnTracesCombatChanged(false, true); GameLog.SetRepositoryToDebug("Combat");
                    SetValue(TracesCombatDetailsProperty, value); OnTracesCombatDetailsChanged(false, true); GameLog.SetRepositoryToDebug("CombatDetails");
                    SetValue(TracesCreditsProperty, value); OnTracesCreditsChanged(false, true); GameLog.SetRepositoryToDebug("Credits");
                    SetValue(TracesDeuteriumProperty, value); OnTracesDeuteriumChanged(false, true); GameLog.SetRepositoryToDebug("Deuterium");
                    SetValue(TracesDilithiumProperty, value); OnTracesDilithiumChanged(false, true); GameLog.SetRepositoryToDebug("Dilithium");
                    SetValue(TracesDiplomacyProperty, value); OnTracesDiplomacyChanged(false, true); GameLog.SetRepositoryToDebug("Diplomacy");
                    SetValue(TracesDuraniumProperty, value); OnTracesDuraniumDetailsChanged(false, true); GameLog.SetRepositoryToDebug("Duranium");
                    SetValue(TracesEnergyProperty, value); OnTracesEnergyChanged(false, true); GameLog.SetRepositoryToDebug("Energy");
                    SetValue(TracesEventsProperty, value); OnTracesEventsChanged(false, true); GameLog.SetRepositoryToDebug("Events");
                    SetValue(TracesGalaxyGeneratorProperty, value); OnTracesGalaxyGeneratorChanged(false, true); GameLog.SetRepositoryToDebug("GalaxyGenerator");
                    SetValue(TracesGameDataProperty, value); OnTracesGameDataChanged(false, true); GameLog.SetRepositoryToDebug("GameData");
                    SetValue(TracesGameInitDataProperty, value); OnTracesGameInitDataChanged(false, true); GameLog.SetRepositoryToDebug("GameInitData");
                    SetValue(TracesGeneralProperty, value); OnTracesGeneralChanged(false, true); GameLog.SetRepositoryToDebug("General");
                    SetValue(TracesInfoTextProperty, value); OnTracesInfoTextChanged(false, true); GameLog.SetRepositoryToDebug("InfoText");
                    SetValue(TracesIntelProperty, value); OnTracesIntelChanged(false, true); GameLog.SetRepositoryToDebug("Intel");
                    SetValue(TracesMapDataProperty, value); OnTracesMapDataChanged(false, true); GameLog.SetRepositoryToDebug("MapData");
                    SetValue(TracesMultiPlayProperty, value); OnTracesMultiPlayChanged(false, true); GameLog.SetRepositoryToDebug("MultiPlay");
                    SetValue(TracesProductionProperty, value); OnTracesProductionChanged(false, true); GameLog.SetRepositoryToDebug("Production");
                    //SetValue(TracesReportErrorsProperty, value); OnTracesReportErrorsChanged(false, true); GameLog.SetRepositoryToDebug("ReportErrors");
                    SetValue(TracesResearchProperty, value); OnTracesResearchChanged(false, true); GameLog.SetRepositoryToDebug("Research");
                    SetValue(TracesSitRepsProperty, value); OnTracesSitRepsChanged(false, true); GameLog.SetRepositoryToDebug("SitReps");
                    SetValue(TracesSaveLoadProperty, value); OnTracesSaveLoadChanged(false, true); GameLog.SetRepositoryToDebug("SaveLoad");
                    SetValue(TracesShipsProperty, value); OnTracesShipsChanged(false, true); GameLog.SetRepositoryToDebug("Ships");
                    SetValue(TracesShipProductionProperty, value); OnTracesShipProductionChanged(false, true); GameLog.SetRepositoryToDebug("ShipProduction");
                    SetValue(TracesStationsProperty, value); OnTracesStationsChanged(false, true); GameLog.SetRepositoryToDebug("Stations");
                    SetValue(TracesStructuresProperty, value); OnTracesStructuresChanged(false, true); GameLog.SetRepositoryToDebug("Structures");
                    SetValue(TracesSystemAssaultProperty, value); OnTracesSystemAssaultChanged(false, true); GameLog.SetRepositoryToDebug("SystemAssault");
                    SetValue(TracesSystemAssaultDetailsProperty, value); OnTracesSystemAssaultDetailsChanged(false, true); GameLog.SetRepositoryToDebug("SystemAssaultDetails");
                    SetValue(TracesTestProperty, value); OnTracesTestChanged(false, true); GameLog.SetRepositoryToDebug("Test");
                    SetValue(TracesTradeRoutesProperty, value); OnTracesTradeRoutesChanged(false, true); GameLog.SetRepositoryToDebug("TradeRoutes");
                    SetValue(TracesUIProperty, value); OnTracesUIChanged(false, true); GameLog.SetRepositoryToDebug("UI");
                    SetValue(TracesXMLCheckProperty, value); OnTracesXMLCheckChanged(false, true); GameLog.SetRepositoryToDebug("XMLCheck");
                    SetValue(TracesXML2CSVOutputProperty, value); OnTracesXML2CSVOutputChanged(false, true); GameLog.SetRepositoryToDebug("XML2CSVOutput");

                    // Details
                    SetValue(TracesAIDetailsProperty, value); OnTracesAIDetailsChanged(false, true); GameLog.SetRepositoryToDebug("AIDetails");
                    SetValue(TracesAudioDetailsProperty, value); OnTracesAudioDetailsChanged(false, true); GameLog.SetRepositoryToDebug("AudioDetails");
                    SetValue(TracesCivsAndRacesDetailsProperty, value); OnTracesCivsAndRacesDetailsChanged(false, true); GameLog.SetRepositoryToDebug("CivsAndRacesDetails");
                    SetValue(TracesColoniesDetailsProperty, value); OnTracesColoniesDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ColoniesDetails");
                    SetValue(TracesCombatDetailsProperty, value); OnTracesCombatDetailsChanged(false, true); GameLog.SetRepositoryToDebug("CombatDetails");
                    SetValue(TracesCreditsDetailsProperty, value); OnTracesCreditsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("CreditsDetails");
                    SetValue(TracesDeuteriumDetailsProperty, value); OnTracesDeuteriumDetailsChanged(false, true); GameLog.SetRepositoryToDebug("DeuteriumDetails");
                    SetValue(TracesDilithiumDetailsProperty, value); OnTracesDilithiumDetailsChanged(false, true); GameLog.SetRepositoryToDebug("DilithiumDetails");
                    SetValue(TracesDuraniumDetailsProperty, value); OnTracesDuraniumDetailsChanged(false, true); GameLog.SetRepositoryToDebug("DuraniumDetails");
                    SetValue(TracesDiplomacyDetailsProperty, value); OnTracesDiplomacyDetailsChanged(false, true); GameLog.SetRepositoryToDebug("DiplomacyDetails");
                    SetValue(TracesEnergyDetailsProperty, value); OnTracesEnergyDetailsChanged(false, true); GameLog.SetRepositoryToDebug("EnergyDetails");
                    SetValue(TracesEventsDetailsProperty, value); OnTracesEventsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("EventsDetails");
                    SetValue(TracesGalaxyGeneratorDetailsProperty, value); OnTracesGalaxyGeneratorDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GalaxyGeneratorDetails");
                    SetValue(TracesGameDataDetailsProperty, value); OnTracesGameDataDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GameDataDetails");
                    SetValue(TracesGameInitDataDetailsProperty, value); OnTracesGameInitDataDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GameInitDataDetails");
                    // done at first
                    //SetValue(TracesGeneralDetailsProperty, value); OnTracesGeneralDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GeneralDetails");
                    SetValue(TracesInfoTextDetailsProperty, value); OnTracesInfoTextDetailsChanged(false, true); GameLog.SetRepositoryToDebug("InfoTextDetails");
                    SetValue(TracesIntelDetailsProperty, value); OnTracesIntelDetailsChanged(false, true); GameLog.SetRepositoryToDebug("IntelDetails");
                    SetValue(TracesMapDataDetailsProperty, value); OnTracesMapDataDetailsChanged(false, true); GameLog.SetRepositoryToDebug("MapDataDetails");
                    SetValue(TracesMultiPlayDetailsProperty, value); OnTracesMultiPlayDetailsChanged(false, true); GameLog.SetRepositoryToDebug("MultiPlayDetails");
                    SetValue(TracesProductionDetailsProperty, value); OnTracesProductionDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ProductionDetails");
                    //////SetValue(TracesReportErrorsDetailsProperty, value); OnTracesReportErrorsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ReportErrorsDetails");
                    SetValue(TracesResearchDetailsProperty, value); OnTracesResearchDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ResearchDetails");
                    SetValue(TracesSitRepsDetailsProperty, value); OnTracesSitRepsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("SitRepsDetails");
                    SetValue(TracesSaveLoadDetailsProperty, value); OnTracesSaveLoadDetailsChanged(false, true); GameLog.SetRepositoryToDebug("SaveLoadDetails");
                    SetValue(TracesShipsDetailsProperty, value); OnTracesShipsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ShipsDetails");
                    SetValue(TracesShipProductionDetailsProperty, value); OnTracesShipProductionDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ShipProductionDetails");
                    SetValue(TracesStationsDetailsProperty, value); OnTracesStationsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("StationsDetails");
                    SetValue(TracesStructuresDetailsProperty, value); OnTracesStructuresDetailsChanged(false, true); GameLog.SetRepositoryToDebug("StructuresDetails");
                    SetValue(TracesSystemAssaultDetailsProperty, value); OnTracesSystemAssaultDetailsChanged(false, true); GameLog.SetRepositoryToDebug("SystemAssaultDetails");
                    SetValue(TracesTestDetailsProperty, value); OnTracesTestDetailsChanged(false, true); GameLog.SetRepositoryToDebug("TestDetails");
                    SetValue(TracesTradeRoutesDetailsProperty, value); OnTracesTradeRoutesDetailsChanged(false, true); GameLog.SetRepositoryToDebug("TradeRoutesDetails");
                    SetValue(TracesUIDetailsProperty, value); OnTracesUIDetailsChanged(false, true); GameLog.SetRepositoryToDebug("UIDetails");
                    SetValue(TracesXMLCheckDetailsProperty, value); OnTracesXMLCheckDetailsChanged(false, true); GameLog.SetRepositoryToDebug("XMLCheckDetails");
                    SetValue(TracesXML2CSVOutputDetailsProperty, value); OnTracesXML2CSVOutputDetailsChanged(false, true); GameLog.SetRepositoryToDebug("XML2CSVOutputDetails");

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

        //public event EventHandler<PropertyChangedRoutedEventArgs<bool>> Traces_ClearAllChanged;

        //private void OnTraces_ClearAllChanged(bool oldValue, bool newValue)
        //=> Traces_ClearAllChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));
        public bool Traces_ClearAll
        {
            get => (bool)GetValue(Traces_ClearAllProperty);
            set
            {
                SetValue(Traces_ClearAllProperty, value);

                GameLog.Client.General.InfoFormat("              #### Log.Txt: 'ClearAll'            for Traces (press ingame CTRL + P, for overview > ALT + X)");  // in Log.Txt only DEBUG = yes get a line

                if (value)
                {
                    //value = false;
                    // "General" shows the Log.txt-lines for all the others
                    //SetValue(TracesGeneralProperty, true); 
                    GameLog.SetRepositoryToDebug("General");

                    // Audio changes shall be done directly = OnTracesAudioChanged
                    SetValue(TracesAIProperty, false); OnTracesAIChanged(false, true); GameLog.SetRepositoryToErrorOnly("AI");
                    SetValue(TracesAudioProperty, false); OnTracesAudioChanged(false, true); GameLog.SetRepositoryToErrorOnly("Audio");
                    SetValue(TracesCivsAndRacesProperty, false); OnTracesCivsAndRacesChanged(false, true); GameLog.SetRepositoryToErrorOnly("CivsAndRaces");
                    SetValue(TracesColoniesProperty, false); OnTracesColoniesChanged(false, true); GameLog.SetRepositoryToErrorOnly("Colonies");
                    SetValue(TracesCombatProperty, false); OnTracesCombatChanged(false, true); GameLog.SetRepositoryToErrorOnly("Combat");
                    SetValue(TracesCombatDetailsProperty, false); OnTracesCombatDetailsChanged(false, true); GameLog.SetRepositoryToErrorOnly("CombatDetails");
                    SetValue(TracesCreditsProperty, false); OnTracesCreditsChanged(false, true); GameLog.SetRepositoryToErrorOnly("Credits");
                    SetValue(TracesDeuteriumProperty, false); OnTracesDeuteriumChanged(false, true); GameLog.SetRepositoryToErrorOnly("Deuterium");
                    SetValue(TracesDilithiumProperty, false); OnTracesDilithiumChanged(false, true); GameLog.SetRepositoryToErrorOnly("Dilithium");
                    SetValue(TracesDiplomacyProperty, false); OnTracesDiplomacyChanged(false, true); GameLog.SetRepositoryToErrorOnly("Diplomacy");
                    SetValue(TracesDuraniumProperty, false); OnTracesDuraniumDetailsChanged(false, true); GameLog.SetRepositoryToDebug("Duranium");
                    SetValue(TracesEnergyProperty, false); OnTracesEnergyChanged(false, true); GameLog.SetRepositoryToErrorOnly("Energy");
                    SetValue(TracesEventsProperty, false); OnTracesEventsChanged(false, true); GameLog.SetRepositoryToErrorOnly("Events");
                    SetValue(TracesGalaxyGeneratorProperty, false); OnTracesGalaxyGeneratorChanged(false, true); GameLog.SetRepositoryToErrorOnly("GalaxyGenerator");
                    SetValue(TracesGameDataProperty, false); OnTracesGameDataChanged(false, true); GameLog.SetRepositoryToErrorOnly("GameData");
                    SetValue(TracesGameInitDataProperty, false); OnTracesGameInitDataChanged(false, true); GameLog.SetRepositoryToErrorOnly("GameInitData");
                    // "General" shows the Log.txt-lines for all the others => do this at the end
                    //SetValue(TracesGeneralProperty, false); OnTracesGeneralChanged(false, true); GameLog.SetRepositoryToErrorOnly("General");
                    SetValue(TracesInfoTextProperty, false); OnTracesInfoTextChanged(false, true); GameLog.SetRepositoryToDebug("InfoText");
                    SetValue(TracesIntelProperty, false); OnTracesIntelChanged(false, true); GameLog.SetRepositoryToErrorOnly("Intel");
                    SetValue(TracesMapDataProperty, false); OnTracesMapDataChanged(false, true); GameLog.SetRepositoryToErrorOnly("MapData");
                    SetValue(TracesMultiPlayProperty, false); OnTracesMultiPlayChanged(false, true); GameLog.SetRepositoryToErrorOnly("MultiPlay");
                    SetValue(TracesProductionProperty, false); OnTracesProductionChanged(false, true); GameLog.SetRepositoryToErrorOnly("Production");
                    //SetValue(TracesReportErrorsProperty, false); OnTracesReportErrorsChanged(false, true); GameLog.SetRepositoryToErrorOnly("ReportErrors");
                    SetValue(TracesResearchProperty, false); OnTracesResearchChanged(false, true); GameLog.SetRepositoryToErrorOnly("Research");
                    SetValue(TracesSitRepsProperty, false); OnTracesSitRepsChanged(false, true); GameLog.SetRepositoryToErrorOnly("SitReps");
                    SetValue(TracesSaveLoadProperty, false); OnTracesSaveLoadChanged(false, true); GameLog.SetRepositoryToErrorOnly("SaveLoad");
                    SetValue(TracesShipsProperty, false); OnTracesShipsChanged(false, true); GameLog.SetRepositoryToErrorOnly("Ships");
                    SetValue(TracesShipProductionProperty, false); OnTracesShipProductionChanged(false, true); GameLog.SetRepositoryToErrorOnly("ShipProduction");
                    SetValue(TracesStationsProperty, false); OnTracesStationsChanged(false, true); GameLog.SetRepositoryToErrorOnly("Stations");
                    SetValue(TracesStructuresProperty, false); OnTracesStructuresChanged(false, true); GameLog.SetRepositoryToErrorOnly("Structures");
                    SetValue(TracesSystemAssaultProperty, false); OnTracesSystemAssaultChanged(false, true); GameLog.SetRepositoryToErrorOnly("SystemAssault");
                    SetValue(TracesSystemAssaultDetailsProperty, false); OnTracesSystemAssaultDetailsChanged(false, true); GameLog.SetRepositoryToErrorOnly("SystemAssaultDetails");
                    SetValue(TracesTestProperty, false); OnTracesTestChanged(false, true); GameLog.SetRepositoryToErrorOnly("Test");
                    SetValue(TracesTradeRoutesProperty, false); OnTracesTradeRoutesChanged(false, true); GameLog.SetRepositoryToErrorOnly("TradeRoutes");
                    SetValue(TracesUIProperty, false); OnTracesUIChanged(false, true); GameLog.SetRepositoryToErrorOnly("UI");
                    SetValue(TracesXMLCheckProperty, false); OnTracesXMLCheckChanged(false, true); GameLog.SetRepositoryToErrorOnly("XMLCheck");
                    SetValue(TracesXML2CSVOutputProperty, false); OnTracesXML2CSVOutputChanged(false, true); GameLog.SetRepositoryToErrorOnly("XML2CSVOutput");

                    // Details
                    //value = false;
                    //SetValue(TracesAIDetailsProperty, false); OnTracesAIDetailsChanged(false, true); GameLog.SetRepositoryToDebug("AIDetails");
                    //SetValue(TracesAudioDetailsProperty, false); OnTracesAudioDetailsChanged(false, true); GameLog.SetRepositoryToDebug("AudioDetails");
                    //SetValue(TracesCivsAndRacesDetailsProperty, false); OnTracesCivsAndRacesDetailsChanged(false, true); GameLog.SetRepositoryToDebug("CivsAndRacesDetails");
                    //SetValue(TracesColoniesDetailsProperty, false); OnTracesColoniesDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ColoniesDetails");
                    //SetValue(TracesCombatDetailsProperty, false); OnTracesCombatDetailsChanged(false, true); GameLog.SetRepositoryToDebug("CombatDetails");
                    //SetValue(TracesCreditsDetailsProperty, false); OnTracesCreditsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("CreditsDetails");
                    //SetValue(TracesDeuteriumDetailsProperty, false); OnTracesDeuteriumDetailsChanged(false, true); GameLog.SetRepositoryToDebug("DeuteriumDetails");
                    //SetValue(TracesDilithiumDetailsProperty, false); OnTracesDilithiumDetailsChanged(false, true); GameLog.SetRepositoryToDebug("DilithiumDetails");
                    //SetValue(TracesDuraniumDetailsProperty, false); OnTracesDuraniumDetailsChanged(false, true); GameLog.SetRepositoryToDebug("DuraniumDetails");
                    //SetValue(TracesDiplomacyDetailsProperty, false); OnTracesDiplomacyDetailsChanged(false, true); GameLog.SetRepositoryToDebug("DiplomacyDetails");
                    //SetValue(TracesEnergyDetailsProperty, false); OnTracesEnergyDetailsChanged(false, true); GameLog.SetRepositoryToDebug("EnergyDetails");
                    //SetValue(TracesEventsDetailsProperty, false); OnTracesEventsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("EventsDetails");
                    //SetValue(TracesGalaxyGeneratorDetailsProperty, false); OnTracesGalaxyGeneratorDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GalaxyGeneratorDetails");
                    //SetValue(TracesGameDataDetailsProperty, false); OnTracesGameDataDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GameDataDetails");
                    //SetValue(TracesGameInitDataDetailsProperty, false); OnTracesGameInitDataDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GameInitDataDetails");

                    //// done at first
                    ////SetValue(TracesGeneralDetailsProperty, false); OnTracesGeneralDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GeneralDetails");

                    //SetValue(TracesInfoTextDetailsProperty, false); OnTracesInfoTextDetailsChanged(false, true); GameLog.SetRepositoryToDebug("InfoTextDetails");
                    //SetValue(TracesIntelDetailsProperty, false); OnTracesIntelDetailsChanged(false, true); GameLog.SetRepositoryToDebug("IntelDetails");
                    //SetValue(TracesMapDataDetailsProperty, false); OnTracesMapDataDetailsChanged(false, true); GameLog.SetRepositoryToDebug("MapDataDetails");
                    //SetValue(TracesMultiPlayDetailsProperty, false); OnTracesMultiPlayDetailsChanged(false, true); GameLog.SetRepositoryToDebug("MultiPlayDetails");
                    //SetValue(TracesProductionDetailsProperty, false); OnTracesProductionDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ProductionDetails");
                    ////////SetValue(TracesReportErrorsDetailsProperty, false); OnTracesReportErrorsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ReportErrorsDetails");
                    //SetValue(TracesResearchDetailsProperty, false); OnTracesResearchDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ResearchDetails");
                    //SetValue(TracesSitRepsDetailsProperty, false); OnTracesSitRepsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("SitRepsDetails");
                    //SetValue(TracesSaveLoadDetailsProperty, false); OnTracesSaveLoadDetailsChanged(false, true); GameLog.SetRepositoryToDebug("SaveLoadDetails");
                    //SetValue(TracesShipsDetailsProperty, false); OnTracesShipsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ShipsDetails");
                    //SetValue(TracesShipProductionDetailsProperty, false); OnTracesShipProductionDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ShipProductionDetails");
                    //SetValue(TracesStationsDetailsProperty, false); OnTracesStationsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("StationsDetails");
                    //SetValue(TracesStructuresDetailsProperty, false); OnTracesStructuresDetailsChanged(false, true); GameLog.SetRepositoryToDebug("StructuresDetails");
                    //SetValue(TracesSystemAssaultDetailsProperty, false); OnTracesSystemAssaultDetailsChanged(false, true); GameLog.SetRepositoryToDebug("SystemAssaultDetails");
                    //SetValue(TracesTestDetailsProperty, false); OnTracesTestDetailsChanged(false, true); GameLog.SetRepositoryToDebug("TestDetails");
                    //SetValue(TracesTradeRoutesDetailsProperty, false); OnTracesTradeRoutesDetailsChanged(false, true); GameLog.SetRepositoryToDebug("TradeRoutesDetails");
                    //SetValue(TracesUIDetailsProperty, false); OnTracesUIDetailsChanged(false, true); GameLog.SetRepositoryToDebug("UIDetails");
                    //SetValue(TracesXMLCheckDetailsProperty, false); OnTracesXMLCheckDetailsChanged(false, true); GameLog.SetRepositoryToDebug("XMLCheckDetails");
                    //SetValue(TracesXML2CSVOutputDetailsProperty, false); OnTracesXML2CSVOutputDetailsChanged(false, true); GameLog.SetRepositoryToDebug("XML2CSVOutputDetails");

                    // "General" shows the Log.txt-lines for all the others => do this at the end
                    GameLog.Client.GeneralDetails.DebugFormat("At last turning of GENERAL");
                    //SetValue(TracesGeneralDetailsProperty, value); OnTracesGeneralDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GeneralDetails");
                    SetValue(TracesGeneralProperty, true); OnTracesGeneralChanged(false, true); GameLog.SetRepositoryToErrorOnly("General");


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

        //public event EventHandler<PropertyChangedRoutedEventArgs<bool>> Traces_ClearAllDetailsChanged;

        //private void OnTraces_ClearAllDetailsChanged(bool oldValue, bool newValue)
        //=> Traces_ClearAllDetailsChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));
        public bool Traces_ClearAllDetails
        {
            get => (bool)GetValue(Traces_ClearAllDetailsProperty);
            set
            {
                SetValue(Traces_ClearAllDetailsProperty, value);

                GameLog.Client.General.InfoFormat("       #### Log.Txt: 'ClearAllDetails'     for Traces (press ingame CTRL + P, for overview > ALT + X)");  // in Log.Txt only DEBUG = yes get a line

                if (value)
                {
                    value = false;
                    // "General" shows the Log.txt-lines for all the others
                    //SetValue(TracesGeneralProperty, true); 
                    GameLog.SetRepositoryToDebug("General");

                    // Audio changes shall be done directly = OnTracesAudioChanged
                    //SetValue(TracesAIProperty, value); OnTracesAIChanged(false, true); GameLog.SetRepositoryToErrorOnly("AI");
                    //SetValue(TracesAudioProperty, value); OnTracesAudioChanged(false, true); GameLog.SetRepositoryToErrorOnly("Audio");
                    //SetValue(TracesCivsAndRacesProperty, value); OnTracesCivsAndRacesChanged(false, true); GameLog.SetRepositoryToErrorOnly("CivsAndRaces");
                    //SetValue(TracesColoniesProperty, value); OnTracesColoniesChanged(false, true); GameLog.SetRepositoryToErrorOnly("Colonies");
                    //SetValue(TracesCombatProperty, value); OnTracesCombatChanged(false, true); GameLog.SetRepositoryToErrorOnly("Combat");
                    //SetValue(TracesCreditsProperty, value); OnTracesCreditsChanged(false, true); GameLog.SetRepositoryToErrorOnly("Credits");
                    //SetValue(TracesDeuteriumProperty, value); OnTracesDeuteriumChanged(false, true); GameLog.SetRepositoryToErrorOnly("Deuterium");
                    //SetValue(TracesDilithiumProperty, value); OnTracesDilithiumChanged(false, true); GameLog.SetRepositoryToErrorOnly("Dilithium");
                    //SetValue(TracesDuraniumProperty, value); OnTracesDuraniumChanged(false, true); GameLog.SetRepositoryToDebug("Duranium");
                    //SetValue(TracesDiplomacyProperty, value); OnTracesDiplomacyChanged(false, true); GameLog.SetRepositoryToErrorOnly("Diplomacy");
                    //SetValue(TracesEnergyProperty, value); OnTracesEnergyChanged(false, true); GameLog.SetRepositoryToErrorOnly("Energy");
                    //SetValue(TracesEventsProperty, value); OnTracesEventsChanged(false, true); GameLog.SetRepositoryToErrorOnly("Events");
                    //SetValue(TracesGalaxyGeneratorProperty, value); OnTracesGalaxyGeneratorChanged(false, true); GameLog.SetRepositoryToErrorOnly("GalaxyGenerator");
                    //SetValue(TracesGameDataProperty, value); OnTracesGameDataChanged(false, true); GameLog.SetRepositoryToErrorOnly("GameData");
                    //SetValue(TracesGameInitDataProperty, value); OnTracesGameInitDataChanged(false, true); GameLog.SetRepositoryToErrorOnly("GameInitData");
                    //// "General" shows the Log.txt-lines for all the others => do this at the end
                    ////SetValue(TracesGeneralProperty, value); OnTracesGeneralChanged(false, true); GameLog.SetRepositoryToErrorOnly("General");
                    /////SetValue(TracesInfoTextProperty, value); OnTracesInfoTextChanged(false, true); GameLog.SetRepositoryToDebug("InfoText");
                    //SetValue(TracesIntelProperty, value); OnTracesIntelChanged(false, true); GameLog.SetRepositoryToErrorOnly("Intel");
                    //SetValue(TracesMapDataProperty, value); OnTracesMapDataChanged(false, true); GameLog.SetRepositoryToErrorOnly("MapData");
                    //SetValue(TracesMultiPlayProperty, value); OnTracesMultiPlayChanged(false, true); GameLog.SetRepositoryToErrorOnly("MultiPlay");
                    //SetValue(TracesProductionProperty, value); OnTracesProductionChanged(false, true); GameLog.SetRepositoryToErrorOnly("Production");
                    ////SetValue(TracesReportErrorsProperty, value); OnTracesReportErrorsChanged(false, true); GameLog.SetRepositoryToErrorOnly("ReportErrors");
                    //SetValue(TracesResearchProperty, value); OnTracesResearchChanged(false, true); GameLog.SetRepositoryToErrorOnly("Research");
                    //SetValue(TracesSitRepsProperty, value); OnTracesSitRepsChanged(false, true); GameLog.SetRepositoryToErrorOnly("SitReps");
                    //SetValue(TracesSaveLoadProperty, value); OnTracesSaveLoadChanged(false, true); GameLog.SetRepositoryToErrorOnly("SaveLoad");
                    //SetValue(TracesShipsProperty, value); OnTracesShipsChanged(false, true); GameLog.SetRepositoryToErrorOnly("Ships");
                    //SetValue(TracesShipProductionProperty, value); OnTracesShipProductionChanged(false, true); GameLog.SetRepositoryToErrorOnly("ShipProduction");
                    //SetValue(TracesStationsProperty, value); OnTracesStationsChanged(false, true); GameLog.SetRepositoryToErrorOnly("Stations");
                    //SetValue(TracesStructuresProperty, value); OnTracesStructuresChanged(false, true); GameLog.SetRepositoryToErrorOnly("Structures");
                    //SetValue(TracesSystemAssaultProperty, value); OnTracesSystemAssaultChanged(false, true); GameLog.SetRepositoryToErrorOnly("SystemAssault");
                    //SetValue(TracesTestProperty, value); OnTracesTestChanged(false, true); GameLog.SetRepositoryToErrorOnly("Test");
                    //SetValue(TracesTradeRoutesProperty, value); OnTracesTradeRoutesChanged(false, true); GameLog.SetRepositoryToErrorOnly("TradeRoutes");
                    //SetValue(TracesUIProperty, value); OnTracesUIChanged(false, true); GameLog.SetRepositoryToErrorOnly("UI");
                    //SetValue(TracesXMLCheckProperty, value); OnTracesXMLCheckChanged(false, true); GameLog.SetRepositoryToErrorOnly("XMLCheck");
                    //SetValue(TracesXML2CSVOutputProperty, value); OnTracesXML2CSVOutputChanged(false, true); GameLog.SetRepositoryToErrorOnly("XML2CSVOutput");

                    // Details
                    SetValue(TracesAIDetailsProperty, value); OnTracesAIDetailsChanged(false, true); GameLog.SetRepositoryToDebug("AIDetails");
                    SetValue(TracesAudioDetailsProperty, value); OnTracesAudioDetailsChanged(false, true); GameLog.SetRepositoryToDebug("AudioDetails");
                    SetValue(TracesCivsAndRacesDetailsProperty, value); OnTracesCivsAndRacesDetailsChanged(false, true); GameLog.SetRepositoryToDebug("CivsAndRacesDetails");
                    SetValue(TracesColoniesDetailsProperty, value); OnTracesColoniesDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ColoniesDetails");
                    SetValue(TracesCombatDetailsProperty, value); OnTracesCombatDetailsChanged(false, true); GameLog.SetRepositoryToDebug("CombatDetails");
                    SetValue(TracesCreditsDetailsProperty, value); OnTracesCreditsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("CreditsDetails");
                    SetValue(TracesDeuteriumDetailsProperty, value); OnTracesDeuteriumDetailsChanged(false, true); GameLog.SetRepositoryToDebug("DeuteriumDetails");
                    SetValue(TracesDilithiumDetailsProperty, value); OnTracesDilithiumDetailsChanged(false, true); GameLog.SetRepositoryToDebug("DilithiumDetails");
                    SetValue(TracesDuraniumDetailsProperty, value); OnTracesDuraniumDetailsChanged(false, true); GameLog.SetRepositoryToDebug("DuraniumDetails");
                    SetValue(TracesDiplomacyDetailsProperty, value); OnTracesDiplomacyDetailsChanged(false, true); GameLog.SetRepositoryToDebug("DiplomacyDetails");
                    SetValue(TracesEnergyDetailsProperty, value); OnTracesEnergyDetailsChanged(false, true); GameLog.SetRepositoryToDebug("EnergyDetails");
                    SetValue(TracesEventsDetailsProperty, value); OnTracesEventsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("EventsDetails");
                    SetValue(TracesGalaxyGeneratorDetailsProperty, value); OnTracesGalaxyGeneratorDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GalaxyGeneratorDetails");
                    SetValue(TracesGameDataDetailsProperty, value); OnTracesGameDataDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GameDataDetails");
                    SetValue(TracesGameInitDataDetailsProperty, value); OnTracesGameInitDataDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GameInitDataDetails");
                    // done at first
                    //SetValue(TracesGeneralDetailsProperty, value); OnTracesGeneralDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GeneralDetails");
                    SetValue(TracesInfoTextDetailsProperty, value); OnTracesInfoTextDetailsChanged(false, true); GameLog.SetRepositoryToDebug("InfoTextDetails");
                    SetValue(TracesIntelDetailsProperty, value); OnTracesIntelDetailsChanged(false, true); GameLog.SetRepositoryToDebug("IntelDetails");
                    SetValue(TracesMapDataDetailsProperty, value); OnTracesMapDataDetailsChanged(false, true); GameLog.SetRepositoryToDebug("MapDataDetails");
                    SetValue(TracesMultiPlayDetailsProperty, value); OnTracesMultiPlayDetailsChanged(false, true); GameLog.SetRepositoryToDebug("MultiPlayDetails");
                    SetValue(TracesProductionDetailsProperty, value); OnTracesProductionDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ProductionDetails");
                    //////SetValue(TracesReportErrorsDetailsProperty, value); OnTracesReportErrorsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ReportErrorsDetails");
                    SetValue(TracesResearchDetailsProperty, value); OnTracesResearchDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ResearchDetails");
                    SetValue(TracesSitRepsDetailsProperty, value); OnTracesSitRepsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("SitRepsDetails");
                    SetValue(TracesSaveLoadDetailsProperty, value); OnTracesSaveLoadDetailsChanged(false, true); GameLog.SetRepositoryToDebug("SaveLoadDetails");
                    SetValue(TracesShipsDetailsProperty, value); OnTracesShipsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ShipsDetails");
                    SetValue(TracesShipProductionDetailsProperty, value); OnTracesShipProductionDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ShipProductionDetails");
                    SetValue(TracesStationsDetailsProperty, value); OnTracesStationsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("StationsDetails");
                    SetValue(TracesStructuresDetailsProperty, value); OnTracesStructuresDetailsChanged(false, true); GameLog.SetRepositoryToDebug("StructuresDetails");
                    SetValue(TracesSystemAssaultDetailsProperty, value); OnTracesSystemAssaultDetailsChanged(false, true); GameLog.SetRepositoryToDebug("SystemAssaultDetails");
                    SetValue(TracesTestDetailsProperty, value); OnTracesTestDetailsChanged(false, true); GameLog.SetRepositoryToDebug("TestDetails");
                    SetValue(TracesTradeRoutesDetailsProperty, value); OnTracesTradeRoutesDetailsChanged(false, true); GameLog.SetRepositoryToDebug("TradeRoutesDetails");
                    SetValue(TracesUIDetailsProperty, value); OnTracesUIDetailsChanged(false, true); GameLog.SetRepositoryToDebug("UIDetails");
                    SetValue(TracesXMLCheckDetailsProperty, value); OnTracesXMLCheckDetailsChanged(false, true); GameLog.SetRepositoryToDebug("XMLCheckDetails");
                    SetValue(TracesXML2CSVOutputDetailsProperty, value); OnTracesXML2CSVOutputDetailsChanged(false, true); GameLog.SetRepositoryToDebug("XML2CSVOutputDetails");

                    // "General" shows the Log.txt-lines for all the others => do this at the end
                    GameLog.Client.GeneralDetails.DebugFormat("At last turning of GENERAL");
                    SetValue(TracesGeneralProperty, true); OnTracesGeneralChanged(false, true); GameLog.SetRepositoryToErrorOnly("General");
                    SetValue(TracesGeneralDetailsProperty, value); OnTracesGeneralDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GeneralDetails");

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

        //public event EventHandler<PropertyChangedRoutedEventArgs<bool>> Traces_SetSomeChanged;

        //private void OnTraces_SetSomeChanged(bool oldValue, bool newValue)
        //=> Traces_SetSomeChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));

        public bool Traces_SetSome  // some making most sense
        {
            get => (bool)GetValue(Traces_SetSomeProperty);
            set
            {
                SetValue(Traces_SetSomeProperty, value);

                GameLog.Client.General.InfoFormat("               #### Log.Txt: 'Some'                for Traces (press ingame CTRL + P, for overview > ALT + X)");  // in Log.Txt only DEBUG = yes get a line

                if (value)
                {
                    SetValue(TracesGeneralProperty, value); OnTracesGeneralChanged(false, true); GameLog.SetRepositoryToDebug("General");
                    SetValue(TracesGeneralDetailsProperty, false); OnTracesGeneralDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GeneralDetails");

                    // Audio changes shall be done directly = OnTracesAudioChanged

                    SetValue(TracesAIProperty, false); OnTracesAIChanged(false, true); GameLog.SetRepositoryToDebug("AI");
                    //SetValue(TracesAudioProperty, false); OnTracesAudioChanged(false, true); GameLog.SetRepositoryToDebug("Audio");
                    SetValue(TracesCivsAndRacesProperty, false); OnTracesCivsAndRacesChanged(false, true); GameLog.SetRepositoryToDebug("CivsAndRaces");
                    SetValue(TracesColoniesProperty, false); OnTracesColoniesChanged(false, true); GameLog.SetRepositoryToDebug("Colonies");
                    SetValue(TracesCombatProperty, false); OnTracesCombatChanged(false, true); GameLog.SetRepositoryToDebug("Combat");
                    SetValue(TracesCreditsProperty, false); OnTracesCreditsChanged(false, true); GameLog.SetRepositoryToDebug("Credits");
                    //SetValue(TracesDeuteriumProperty, false); OnTracesDeuteriumChanged(false, true); GameLog.SetRepositoryToDebug("Deuterium");
                    //SetValue(TracesDilithiumProperty, false); OnTracesDilithiumChanged(false, true); GameLog.SetRepositoryToDebug("Dilithium");
                    //SetValue(TracesDuraniumProperty, false); OnTracesDuraniumDetailsChanged(false, true); GameLog.SetRepositoryToDebug("Duranium");
                    SetValue(TracesDiplomacyProperty, false); OnTracesDiplomacyChanged(false, true); GameLog.SetRepositoryToDebug("Diplomacy");
                    SetValue(TracesEnergyProperty, false); OnTracesEnergyChanged(false, true); GameLog.SetRepositoryToDebug("Energy");
                    SetValue(TracesEventsProperty, false); OnTracesEventsChanged(false, true); GameLog.SetRepositoryToDebug("Events");
                    //SetValue(TracesGalaxyGeneratorProperty, false); OnTracesGalaxyGeneratorChanged(false, true); GameLog.SetRepositoryToDebug("GalaxyGenerator");
                    SetValue(TracesGameDataProperty, false); OnTracesGameDataChanged(false, true); GameLog.SetRepositoryToDebug("GameData");
                    //SetValue(TracesGameInitDataProperty, false); OnTracesGameInitDataChanged(false, true); GameLog.SetRepositoryToDebug("GameInitData");
                    // done at first
                    //SetValue(TracesGeneralProperty, value); OnTracesGeneralChanged(false, true); GameLog.SetRepositoryToDebug("General");
                    //SetValue(TracesInfoTextProperty, false); OnTracesInfoTextChanged(false, true); GameLog.SetRepositoryToDebug("InfoText");
                    SetValue(TracesIntelProperty, false); OnTracesIntelChanged(false, true); GameLog.SetRepositoryToDebug("Intel");
                    //SetValue(TracesMapDataProperty, false); OnTracesMapDataChanged(false, true); GameLog.SetRepositoryToDebug("MapData");
                    //SetValue(TracesMultiPlayProperty, false); OnTracesMultiPlayChanged(false, true); GameLog.SetRepositoryToDebug("MultiPlay");
                    SetValue(TracesProductionProperty, false); OnTracesProductionChanged(false, true); GameLog.SetRepositoryToDebug("Production");
                    ////////SetValue(TracesReportErrorsProperty, value); OnTracesReportErrorsChanged(false, true); GameLog.SetRepositoryToDebug("ReportErrors");
                    SetValue(TracesResearchProperty, false); OnTracesResearchChanged(false, true); GameLog.SetRepositoryToDebug("Research");
                    SetValue(TracesSitRepsProperty, value); OnTracesSitRepsChanged(false, true); GameLog.SetRepositoryToDebug("SitReps");
                    //SetValue(TracesSaveLoadProperty, false); OnTracesSaveLoadChanged(false, true); GameLog.SetRepositoryToDebug("SaveLoad");
                    SetValue(TracesShipsProperty, false); OnTracesShipsChanged(false, true); GameLog.SetRepositoryToDebug("Ships");
                    SetValue(TracesShipProductionProperty, false); OnTracesShipProductionChanged(false, true); GameLog.SetRepositoryToDebug("ShipProduction");
                    SetValue(TracesStationsProperty, false); OnTracesStationsChanged(false, true); GameLog.SetRepositoryToDebug("Stations");
                    //SetValue(TracesStructuresProperty, false); OnTracesStructuresChanged(false, true); GameLog.SetRepositoryToDebug("Structures");
                    SetValue(TracesSystemAssaultProperty, false); OnTracesSystemAssaultChanged(false, true); GameLog.SetRepositoryToDebug("SystemAssault");
                    SetValue(TracesTestProperty, false); OnTracesTestChanged(false, true); GameLog.SetRepositoryToDebug("Test");
                    //SetValue(TracesTradeRoutesProperty, value); OnTracesTradeRoutesChanged(false, true); GameLog.SetRepositoryToDebug("TradeRoutes");
                    //SetValue(TracesUIProperty, false); OnTracesUIChanged(false, true); GameLog.SetRepositoryToDebug("UI");
                    //SetValue(TracesXMLCheckProperty, false); OnTracesXMLCheckChanged(false, true); GameLog.SetRepositoryToDebug("XMLCheck");
                    //SetValue(TracesXML2CSVOutputProperty, false); OnTracesXML2CSVOutputChanged(false, true); GameLog.SetRepositoryToDebug("XML2CSVOutput");

                    // Details
                    //SetValue(TracesAIDetailsProperty, false); OnTracesAIDetailsChanged(false, true); GameLog.SetRepositoryToDebug("AIDetails");
                    //SetValue(TracesAudioDetailsProperty, false); OnTracesAudioDetailsChanged(false, true); GameLog.SetRepositoryToDebug("AudioDetails");
                    //SetValue(TracesCivsAndRacesDetailsProperty, false); OnTracesCivsAndRacesDetailsChanged(false, true); GameLog.SetRepositoryToDebug("CivsAndRacesDetails");
                    //SetValue(TracesColoniesDetailsProperty, false); OnTracesColoniesDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ColoniesDetails");
                    //SetValue(TracesCombatDetailsProperty, false); OnTracesCombatDetailsChanged(false, true); GameLog.SetRepositoryToDebug("CombatDetails");
                    //SetValue(TracesCreditsDetailsProperty, false); OnTracesCreditsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("CreditsDetails");
                    //SetValue(TracesDeuteriumDetailsProperty, false); OnTracesDeuteriumDetailsChanged(false, true); GameLog.SetRepositoryToDebug("DeuteriumDetails");
                    //SetValue(TracesDilithiumDetailsProperty, false); OnTracesDilithiumDetailsChanged(false, true); GameLog.SetRepositoryToDebug("DilithiumDetails");
                    //SetValue(TracesDuraniumDetailsProperty, false); OnTracesDuraniumDetailsChanged(false, true); GameLog.SetRepositoryToDebug("DuraniumDetails");
                    //SetValue(TracesDiplomacyDetailsProperty, false); OnTracesDiplomacyDetailsChanged(false, true); GameLog.SetRepositoryToDebug("DiplomacyDetails");
                    //SetValue(TracesEnergyDetailsProperty, false); OnTracesEnergyDetailsChanged(false, true); GameLog.SetRepositoryToDebug("EnergyDetails");
                    //SetValue(TracesEventsDetailsProperty, false); OnTracesEventsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("EventsDetails");
                    //SetValue(TracesGalaxyGeneratorDetailsProperty, false); OnTracesGalaxyGeneratorDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GalaxyGeneratorDetails");
                    //SetValue(TracesGameDataDetailsProperty, false); OnTracesGameDataDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GameDataDetails");
                    //SetValue(TracesGameInitDataDetailsProperty, false); OnTracesGameInitDataDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GameInitDataDetails");
                    //// done at first
                    ////SetValue(TracesGeneralDetailsProperty, value); OnTracesGeneralDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GeneralDetails");
                    //SetValue(TracesInfoTextDetailsProperty, false); OnTracesInfoTextDetailsChanged(false, true); GameLog.SetRepositoryToDebug("InfoTextDetails");
                    //SetValue(TracesIntelDetailsProperty, false); OnTracesIntelDetailsChanged(false, true); GameLog.SetRepositoryToDebug("IntelDetails");
                    //SetValue(TracesMapDataDetailsProperty, false); OnTracesMapDataDetailsChanged(false, true); GameLog.SetRepositoryToDebug("MapDataDetails");
                    //SetValue(TracesMultiPlayDetailsProperty, false); OnTracesMultiPlayDetailsChanged(false, true); GameLog.SetRepositoryToDebug("MultiPlayDetails");
                    //SetValue(TracesProductionDetailsProperty, false); OnTracesProductionDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ProductionDetails");
                    ////////SetValue(TracesReportErrorsDetailsProperty, false); OnTracesReportErrorsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ReportErrorsDetails");
                    //SetValue(TracesResearchDetailsProperty, false); OnTracesResearchDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ResearchDetails");
                    //SetValue(TracesSitRepsDetailsProperty, false); OnTracesSitRepsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("SitRepsDetails");
                    //SetValue(TracesSaveLoadDetailsProperty, false); OnTracesSaveLoadDetailsChanged(false, true); GameLog.SetRepositoryToDebug("SaveLoadDetails");
                    //SetValue(TracesShipsDetailsProperty, false); OnTracesShipsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ShipsDetails");
                    //SetValue(TracesShipProductionDetailsProperty, false); OnTracesShipProductionDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ShipProductionDetails");
                    //SetValue(TracesStationsDetailsProperty, false); OnTracesStationsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("StationsDetails");
                    //SetValue(TracesStructuresDetailsProperty, false); OnTracesStructuresDetailsChanged(false, true); GameLog.SetRepositoryToDebug("StructuresDetails");
                    //SetValue(TracesSystemAssaultDetailsProperty, false); OnTracesSystemAssaultDetailsChanged(false, true); GameLog.SetRepositoryToDebug("SystemAssaultDetails");
                    //SetValue(TracesTestDetailsProperty, false); OnTracesTestDetailsChanged(false, true); GameLog.SetRepositoryToDebug("TestDetails");
                    //SetValue(TracesTradeRoutesDetailsProperty, false); OnTracesTradeRoutesDetailsChanged(false, true); GameLog.SetRepositoryToDebug("TradeRoutesDetails");
                    //SetValue(TracesUIDetailsProperty, false); OnTracesUIDetailsChanged(false, true); GameLog.SetRepositoryToDebug("UIDetails");
                    //SetValue(TracesXMLCheckDetailsProperty, false); OnTracesXMLCheckDetailsChanged(false, true); GameLog.SetRepositoryToDebug("XMLCheckDetails");
                    //SetValue(TracesXML2CSVOutputDetailsProperty, false); OnTracesXML2CSVOutputDetailsChanged(false, true); GameLog.SetRepositoryToDebug("XML2CSVOutputDetails");

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

        //public event EventHandler<PropertyChangedRoutedEventArgs<bool>> Traces_SetSelection2Changed;

        //private void OnTraces_SetSelection2Changed(bool oldValue, bool newValue) => Traces_SetSelection2Changed?.Invoke(this, new PropertyChangedRoutedEventArgs<bool>(oldValue, newValue));


        public bool Traces_SetSelection2
        {
            get => (bool)GetValue(Traces_SetSelection2Property);
            set
            {
                SetValue(Traces_SetSelection2Property, value);


                GameLog.Client.General.InfoFormat("         #### Log.Txt: 'Selection 2'         for Traces (press ingame CTRL + P, for overview > ALT + X)");  // in Log.Txt only DEBUG = yes get a line

                if (value)
                {
                    SetValue(TracesGeneralProperty, value); OnTracesGeneralChanged(false, true); GameLog.SetRepositoryToDebug("General");
                    SetValue(TracesGeneralDetailsProperty, false); OnTracesGeneralDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GeneralDetails");

                    // Audio changes shall be done directly = OnTracesAudioChanged

                    SetValue(TracesAIProperty, false); OnTracesAIChanged(false, true); GameLog.SetRepositoryToDebug("AI");
                    SetValue(TracesAudioProperty, false); OnTracesAudioChanged(false, true); GameLog.SetRepositoryToDebug("Audio");
                    SetValue(TracesCivsAndRacesProperty, false); OnTracesCivsAndRacesChanged(false, true); GameLog.SetRepositoryToDebug("CivsAndRaces");
                    SetValue(TracesColoniesProperty, false); OnTracesColoniesChanged(false, true); GameLog.SetRepositoryToDebug("Colonies");
                    SetValue(TracesCombatProperty, value); OnTracesCombatChanged(false, true); GameLog.SetRepositoryToDebug("Combat");
                    SetValue(TracesCreditsProperty, false); OnTracesCreditsChanged(false, true); GameLog.SetRepositoryToDebug("Credits");
                    SetValue(TracesDeuteriumProperty, false); OnTracesDeuteriumChanged(false, true); GameLog.SetRepositoryToDebug("Deuterium");
                    SetValue(TracesDilithiumProperty, false); OnTracesDilithiumChanged(false, true); GameLog.SetRepositoryToDebug("Dilithium");
                    SetValue(TracesDuraniumProperty, false); OnTracesDuraniumChanged(false, true); GameLog.SetRepositoryToDebug("Duranium");
                    SetValue(TracesDiplomacyProperty, false); OnTracesDiplomacyChanged(false, true); GameLog.SetRepositoryToDebug("Diplomacy");
                    SetValue(TracesEnergyProperty, false); OnTracesEnergyChanged(false, true); GameLog.SetRepositoryToDebug("Energy");
                    SetValue(TracesEventsProperty, false); OnTracesEventsChanged(false, true); GameLog.SetRepositoryToDebug("Events");
                    SetValue(TracesGalaxyGeneratorProperty, false); OnTracesGalaxyGeneratorChanged(false, true); GameLog.SetRepositoryToDebug("GalaxyGenerator");
                    SetValue(TracesGameDataProperty, false); OnTracesGameDataChanged(false, true); GameLog.SetRepositoryToDebug("GameData");
                    SetValue(TracesGameInitDataProperty, false); OnTracesGameInitDataChanged(false, true); GameLog.SetRepositoryToDebug("GameInitData");

                    // done at first
                    //SetValue(TracesGeneralProperty, value); OnTracesGeneralChanged(false, true); GameLog.SetRepositoryToDebug("General");

                    SetValue(TracesInfoTextProperty, false); OnTracesInfoTextChanged(false, true); GameLog.SetRepositoryToDebug("InfoText");
                    SetValue(TracesIntelProperty, false); OnTracesIntelChanged(false, true); GameLog.SetRepositoryToDebug("Intel");
                    SetValue(TracesMapDataProperty, false); OnTracesMapDataChanged(false, true); GameLog.SetRepositoryToDebug("MapData");
                    SetValue(TracesMultiPlayProperty, false); OnTracesMultiPlayChanged(false, true); GameLog.SetRepositoryToDebug("MultiPlay");
                    SetValue(TracesProductionProperty, false); OnTracesProductionChanged(false, true); GameLog.SetRepositoryToDebug("Production");
                    //////SetValue(TracesReportErrorsProperty, false); OnTracesReportErrorsChanged(false, true); GameLog.SetRepositoryToDebug("ReportErrors");
                    SetValue(TracesResearchProperty, false); OnTracesResearchChanged(false, true); GameLog.SetRepositoryToDebug("Research");
                    SetValue(TracesSitRepsProperty, value); OnTracesSitRepsChanged(false, true); GameLog.SetRepositoryToDebug("SitReps");
                    SetValue(TracesSaveLoadProperty, false); OnTracesSaveLoadChanged(false, true); GameLog.SetRepositoryToDebug("SaveLoad");
                    SetValue(TracesShipsProperty, false); OnTracesShipsChanged(false, true); GameLog.SetRepositoryToDebug("Ships");
                    SetValue(TracesShipProductionProperty, false); OnTracesShipProductionChanged(false, true); GameLog.SetRepositoryToDebug("ShipProduction");
                    SetValue(TracesStationsProperty, false); OnTracesStationsChanged(false, true); GameLog.SetRepositoryToDebug("Stations");
                    SetValue(TracesStructuresProperty, false); OnTracesStructuresChanged(false, true); GameLog.SetRepositoryToDebug("Structures");
                    SetValue(TracesSystemAssaultProperty, false); OnTracesSystemAssaultChanged(false, true); GameLog.SetRepositoryToDebug("SystemAssault");
                    SetValue(TracesTestProperty, false); OnTracesTestChanged(false, true); GameLog.SetRepositoryToDebug("Test");
                    SetValue(TracesTradeRoutesProperty, false); OnTracesTradeRoutesChanged(false, true); GameLog.SetRepositoryToDebug("TradeRoutes");
                    SetValue(TracesUIProperty, false); OnTracesUIChanged(false, true); GameLog.SetRepositoryToDebug("UI");
                    SetValue(TracesXMLCheckProperty, false); OnTracesXMLCheckChanged(false, true); GameLog.SetRepositoryToDebug("XMLCheck");
                    SetValue(TracesXML2CSVOutputProperty, false); OnTracesXML2CSVOutputChanged(false, true); GameLog.SetRepositoryToDebug("XML2CSVOutput");

                    // Details
                    SetValue(TracesAIDetailsProperty, false); OnTracesAIDetailsChanged(false, true); GameLog.SetRepositoryToDebug("AIDetails");
                    SetValue(TracesAudioDetailsProperty, false); OnTracesAudioDetailsChanged(false, true); GameLog.SetRepositoryToDebug("AudioDetails");
                    SetValue(TracesCivsAndRacesDetailsProperty, false); OnTracesCivsAndRacesDetailsChanged(false, true); GameLog.SetRepositoryToDebug("CivsAndRacesDetails");
                    SetValue(TracesColoniesDetailsProperty, false); OnTracesColoniesDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ColoniesDetails");
                    SetValue(TracesCombatDetailsProperty, false); OnTracesCombatDetailsChanged(false, true); GameLog.SetRepositoryToDebug("CombatDetails");
                    SetValue(TracesCreditsDetailsProperty, false); OnTracesCreditsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("CreditsDetails");
                    SetValue(TracesDeuteriumDetailsProperty, false); OnTracesDeuteriumDetailsChanged(false, true); GameLog.SetRepositoryToDebug("DeuteriumDetails");
                    SetValue(TracesDilithiumDetailsProperty, false); OnTracesDilithiumDetailsChanged(false, true); GameLog.SetRepositoryToDebug("DilithiumDetails");
                    SetValue(TracesDuraniumDetailsProperty, false); OnTracesDuraniumDetailsChanged(false, true); GameLog.SetRepositoryToDebug("DuraniumDetails");
                    SetValue(TracesDiplomacyDetailsProperty, false); OnTracesDiplomacyDetailsChanged(false, true); GameLog.SetRepositoryToDebug("DiplomacyDetails");
                    SetValue(TracesEnergyDetailsProperty, false); OnTracesEnergyDetailsChanged(false, true); GameLog.SetRepositoryToDebug("EnergyDetails");
                    SetValue(TracesEventsDetailsProperty, false); OnTracesEventsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("EventsDetails");
                    SetValue(TracesGalaxyGeneratorDetailsProperty, false); OnTracesGalaxyGeneratorDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GalaxyGeneratorDetails");
                    SetValue(TracesGameDataDetailsProperty, false); OnTracesGameDataDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GameDataDetails");
                    SetValue(TracesGameInitDataDetailsProperty, false); OnTracesGameInitDataDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GameInitDataDetails");

                    // done at first
                    //SetValue(TracesGeneralDetailsProperty, value); OnTracesGeneralDetailsChanged(false, true); GameLog.SetRepositoryToDebug("GeneralDetails");

                    SetValue(TracesInfoTextDetailsProperty, false); OnTracesInfoTextDetailsChanged(false, true); GameLog.SetRepositoryToDebug("InfoTextDetails");
                    SetValue(TracesIntelDetailsProperty, false); OnTracesIntelDetailsChanged(false, true); GameLog.SetRepositoryToDebug("IntelDetails");
                    SetValue(TracesMapDataDetailsProperty, false); OnTracesMapDataDetailsChanged(false, true); GameLog.SetRepositoryToDebug("MapDataDetails");
                    SetValue(TracesMultiPlayDetailsProperty, false); OnTracesMultiPlayDetailsChanged(false, true); GameLog.SetRepositoryToDebug("MultiPlayDetails");
                    SetValue(TracesProductionDetailsProperty, false); OnTracesProductionDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ProductionDetails");
                    //////SetValue(TracesReportErrorsDetailsProperty, false); OnTracesReportErrorsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ReportErrorsDetails");
                    SetValue(TracesResearchDetailsProperty, false); OnTracesResearchDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ResearchDetails");
                    SetValue(TracesSitRepsDetailsProperty, false); OnTracesSitRepsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("SitRepsDetails");
                    SetValue(TracesSaveLoadDetailsProperty, false); OnTracesSaveLoadDetailsChanged(false, true); GameLog.SetRepositoryToDebug("SaveLoadDetails");
                    SetValue(TracesShipsDetailsProperty, false); OnTracesShipsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ShipsDetails");
                    SetValue(TracesShipProductionDetailsProperty, false); OnTracesShipProductionDetailsChanged(false, true); GameLog.SetRepositoryToDebug("ShipProductionDetails");
                    SetValue(TracesStationsDetailsProperty, false); OnTracesStationsDetailsChanged(false, true); GameLog.SetRepositoryToDebug("StationsDetails");
                    SetValue(TracesStructuresDetailsProperty, false); OnTracesStructuresDetailsChanged(false, true); GameLog.SetRepositoryToDebug("StructuresDetails");
                    SetValue(TracesSystemAssaultDetailsProperty, false); OnTracesSystemAssaultDetailsChanged(false, true); GameLog.SetRepositoryToDebug("SystemAssaultDetails");
                    SetValue(TracesTestDetailsProperty, false); OnTracesTestDetailsChanged(false, true); GameLog.SetRepositoryToDebug("TestDetails");
                    SetValue(TracesTradeRoutesDetailsProperty, false); OnTracesTradeRoutesDetailsChanged(false, true); GameLog.SetRepositoryToDebug("TradeRoutesDetails");
                    SetValue(TracesUIDetailsProperty, false); OnTracesUIDetailsChanged(false, true); GameLog.SetRepositoryToDebug("UIDetails");
                    SetValue(TracesXMLCheckDetailsProperty, false); OnTracesXMLCheckDetailsChanged(false, true); GameLog.SetRepositoryToDebug("XMLCheckDetails");
                    SetValue(TracesXML2CSVOutputDetailsProperty, false); OnTracesXML2CSVOutputDetailsChanged(false, true); GameLog.SetRepositoryToDebug("XML2CSVOutputDetails");

                    //SendKeys.SendWait("{ENTER}");  // doesn't work - close OptionsDialog ...(and reload)
                    //Thread.Sleep(1000);
                    //SendKeys.SendWait("^o"); // OptionsDialog
                    //Thread.Sleep(1000);
                }
            }
        }
        #endregion Traces_SetSelection2
    }
}