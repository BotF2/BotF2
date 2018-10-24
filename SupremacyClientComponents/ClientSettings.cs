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
                GameLog.Client.General.DebugFormat("SAVE     {0}: Content: {1}", filePath, File.ReadAllText(filePath));
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

                        GameLog.Client.General.DebugFormat("LOADCORE {0}: Content: {1}", filePath, File.ReadAllText(filePath));
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