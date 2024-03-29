﻿// CivString.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Microsoft.Practices.ServiceLocation;
using Supremacy.Annotations;
using Supremacy.Client;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Supremacy.Resources
{
    [Serializable]
    [DebuggerDisplay("{{CivString Civilization={_civKey}, Key={_stringKey}}}")]
    public class CivString : INotifyPropertyChanged
    {
        #region Constants
        public const string DefaultCategory = null;
        public const string DiplomacyCategory = "Diplomacy";
        public const string ScriptedEventsCategory = "ScriptedEvents";
        #endregion

        #region Fields
        private static IClientContext s_clientContext;
        private readonly int _randomIndex;
        private readonly string _category;
        private readonly string _stringKey;
        private readonly Tone? _demeanor;
        private string _civKey;
        [NonSerialized]
        private bool _cachedValuePresent;
        [NonSerialized]
        private string _cachedValue;
        #endregion

        #region Constructors
        static CivString()
        {

        }

        public CivString(string category, string key) : this(null, category, key) { }

        public CivString([CanBeNull] Civilization civilization, [CanBeNull] Civilization civilization2, [NotNull] string category, [NotNull] string key) : this(civilization, civilization2, category, key, null) { }

        public CivString([CanBeNull] Civilization civilization, [NotNull] string category, [NotNull] string key) : this(civilization, null, category, key, null) { }

        public CivString([CanBeNull] Civilization civilization, [CanBeNull] Civilization civilization2, [NotNull] string category, [NotNull] string key, Tone? demeanor)
        {
            if (string.IsNullOrEmpty(category))
            {
                throw new ArgumentException("category must be a non-null, non-empty string.", "category");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("key must be a non-null, non-empty string.", "key");
            }

            if (civilization != null)
            {
                _civKey = civilization.Key;
            }

            if (civilization2 != null)
            {
                _civKey = civilization.Key;
            }

            _category = category;
            _stringKey = key;
            _randomIndex = RandomProvider.Shared.Next(255);
            _demeanor = demeanor;
        }
        #endregion

        #region Properties

        protected IClientContext ClientContext
        {
            get
            {
                if (s_clientContext == null)
                {
                    s_clientContext = ServiceLocator.Current.GetInstance<IClientContext>();
                }

                return s_clientContext;
            }
        }


        public Civilization Civilization
        {
            get => GameContext.Current.Civilizations[_civKey];
            set
            {
                _civKey = value?.Key;
                OnPropertyChanged("Civilization");
                OnPropertyChanged("Value");
            }
        }

        public Tone? Tone => _demeanor;

        public string Category => _category;

        public string Key => _stringKey;

        public string Value
        {
            get
            {
                if (!_cachedValuePresent)
                {
                    LookupValue();
                }

                return _cachedValue ?? _stringKey;
            }
        }

        public bool HasValue
        {
            get
            {
                if (!_cachedValuePresent)
                {
                    LookupValue();
                }

                return _cachedValue != null;
            }
        }
        #endregion

        #region Methods
        protected void LookupValue()
        {
            Civilization civ = Civilization;
            if ((civ == null) && (ClientContext != null) && (ClientContext.LocalPlayer != null))
            {
                civ = ClientContext.LocalPlayer.Empire;
            }

            _cachedValue = CivStringDatabase.GetString(civ, Category, _stringKey, _demeanor, _randomIndex);
            _cachedValuePresent = true;
        }

        public override string ToString()
        {
            return Value;
        }
        #endregion

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    public static class CivStringDatabase
    {
        #region Fields
        private static readonly Func<string, string, string, Tone?, IEnumerable<string>>[] s_searchFunctions;
        private static readonly XDocument s_databaseXml;
        #endregion

        #region Constructors
        static CivStringDatabase()
        {
            try
            {
                XNamespace ns = XNamespace.Get("Supremacy:CivStringDatabase.xsd");

                s_databaseXml = XDocument.Load(
                    ResourceManager.GetResourcePath("vfs:///Resources/Data/CivStringDatabase.xml"),
                    LoadOptions.SetLineInfo);

                Debug.Assert(s_databaseXml.Root != null);

                s_databaseXml.Root.Name = ns + s_databaseXml.Root.Name.LocalName;
                s_databaseXml.Root.SetAttributeValue(XNamespace.Xmlns + "cs", ns);

                s_searchFunctions = new Func<string, string, string, Tone?, IEnumerable<string>>[]
                                    {
                                        // Civilization-Specific Entries, Player's Language, Specified Tone
                                        (civKey, category, key, demeanor) => s_databaseXml.Root.Elements(ns + "CivStrings")
                                            .Where(o => string.Equals(civKey, (string)o.Attribute("Civilization"), StringComparison.OrdinalIgnoreCase))
                                            .Elements(ns + "Category")
                                            .Where(o => string.Equals(category, (string)o.Attribute("Name"), StringComparison.OrdinalIgnoreCase))
                                            .Elements(ns + "CivString")
                                            .Where(o => string.Equals(key, (string)o.Attribute("Key"), StringComparison.OrdinalIgnoreCase) &&
                                                        string.Equals(demeanor.HasValue ? demeanor.ToString() : null, (string)o.Attribute("Tone"), StringComparison.OrdinalIgnoreCase))
                                            .Elements(ns + "Value")
                                            .Where(o => IsValidLanguageForUser((string)o.Attribute("Language")))
                                            .Select(o => o.Value),

                                        // Civilization-Specific Entries, Player's Language, Any Tone
                                        (civKey, category, key, demeanor) => s_databaseXml.Root.Elements(ns + "CivStrings")
                                             .Where(o => string.Equals(civKey, (string)o.Attribute("Civilization"), StringComparison.OrdinalIgnoreCase))
                                             .Elements(ns + "Category")
                                             .Where(o => string.Equals(category, (string)o.Attribute("Name"), StringComparison.OrdinalIgnoreCase))
                                             .Elements(ns + "CivString")
                                             .Where(o => string.Equals(key, (string)o.Attribute("Key"), StringComparison.OrdinalIgnoreCase))
                                             .Elements(ns + "Value")
                                             .Where(o => IsValidLanguageForUser((string)o.Attribute("Language")))
                                             .Select(o => o.Value),

                                        // Default Entries, Player's Language, Specified Tone
                                        (civKey, category, key, demeanor) => s_databaseXml.Root.Elements(ns + "DefaultStrings")
                                             .Elements(ns + "Category")
                                             .Where(o => string.Equals(category, (string)o.Attribute("Name"), StringComparison.OrdinalIgnoreCase))
                                             .Elements(ns + "CivString")
                                             .Where(o => string.Equals(key, (string)o.Attribute("Key"), StringComparison.OrdinalIgnoreCase) &&
                                                         string.Equals(demeanor.HasValue ? demeanor.ToString() : null, (string)o.Attribute("Tone"), StringComparison.OrdinalIgnoreCase))
                                             .Elements(ns + "Value")
                                             .Where(o => IsValidLanguageForUser((string)o.Attribute("Language")))
                                             .Select(o => o.Value),

                                        // Default Entries, Player's Language, Any Tone
                                        (civKey, category, key, demeanor) => s_databaseXml.Root.Elements(ns + "DefaultStrings")
                                             .Elements(ns + "Category")
                                             .Where(o => string.Equals(category, (string)o.Attribute("Name"), StringComparison.OrdinalIgnoreCase))
                                             .Elements(ns + "CivString")
                                             .Where(o => string.Equals(key, (string)o.Attribute("Key"), StringComparison.OrdinalIgnoreCase))
                                             .Elements(ns + "Value")
                                             .Where(o => IsValidLanguageForUser((string)o.Attribute("Language")))
                                             .Select(o => o.Value),

                                        // Civilization-Specific Entries, Neutral Language, Specified Tone
                                        (civKey, category, key, demeanor) => s_databaseXml.Root.Elements(ns + "CivStrings")
                                             .Where(o => string.Equals(civKey, (string)o.Attribute("Civilization"), StringComparison.OrdinalIgnoreCase))
                                             .Elements(ns + "Category")
                                             .Where(o => string.Equals(category, (string)o.Attribute("Name"), StringComparison.OrdinalIgnoreCase))
                                             .Elements(ns + "CivString")
                                             .Where(o => string.Equals(key, (string)o.Attribute("Key"), StringComparison.OrdinalIgnoreCase) &&
                                                         string.Equals(demeanor.HasValue ? demeanor.ToString() : null, (string)o.Attribute("Tone"), StringComparison.OrdinalIgnoreCase))
                                             .Elements(ns + "Value")
                                             .Where(o => IsNeutralLanguage((string)o.Attribute("Language")))
                                             .Select(o => o.Value),

                                        // Civilization-Specific Entries, Neutral Language, Any Tone
                                        (civKey, category, key, demeanor) => s_databaseXml.Root.Elements(ns + "CivStrings")
                                             .Where(o => string.Equals(civKey, (string)o.Attribute("Civilization"), StringComparison.OrdinalIgnoreCase))
                                             .Elements(ns + "Category")
                                             .Where(o => string.Equals(category, (string)o.Attribute("Name"), StringComparison.OrdinalIgnoreCase))
                                             .Elements(ns + "CivString")
                                             .Where(o => string.Equals(key, (string)o.Attribute("Key"), StringComparison.OrdinalIgnoreCase))
                                             .Elements(ns + "Value")
                                             .Where(o => IsNeutralLanguage((string)o.Attribute("Language")))
                                             .Select(o => o.Value),

                                        // Default Entries, Neutral Language, Specified Tone
                                        (civKey, category, key, demeanor) => s_databaseXml.Root.Elements(ns + "DefaultStrings")
                                             .Elements(ns + "Category")
                                             .Where(o => string.Equals(category, (string)o.Attribute("Name"), StringComparison.OrdinalIgnoreCase))
                                             .Elements(ns + "CivString")
                                             .Where(o => string.Equals(key, (string)o.Attribute("Key"), StringComparison.OrdinalIgnoreCase) &&
                                                         string.Equals(demeanor.HasValue ? demeanor.ToString() : null, (string)o.Attribute("Tone"), StringComparison.OrdinalIgnoreCase))
                                             .Elements(ns + "Value")
                                             .Where(o => IsNeutralLanguage((string)o.Attribute("Language")))
                                             .Select(o => o.Value),

                                        // Default Entries, Neutral Language, Any Tone
                                        (civKey, category, key, demeanor) => s_databaseXml.Root.Elements(ns + "DefaultStrings")
                                             .Elements(ns + "Category")
                                             .Where(o => string.Equals(category, (string)o.Attribute("Name"), StringComparison.OrdinalIgnoreCase))
                                             .Elements(ns + "CivString")
                                             .Where(o => string.Equals(key, (string)o.Attribute("Key"), StringComparison.OrdinalIgnoreCase))
                                             .Elements(ns + "Value")
                                             .Where(o => IsNeutralLanguage((string)o.Attribute("Language")))
                                             .Select(o => o.Value),
                                    };
            }
            catch (Exception e)
            {
                GameLog.Core.CivsAndRaces.DebugFormat("civString {0} {1}", e.Message, e.StackTrace);
                s_databaseXml = null;
            }
        }
        #endregion

        #region Methods
        private static bool IsNeutralLanguage(string language)
        {
            // An empty string matches InvariantCulture.  We recognize null in the same way.
            if (string.IsNullOrEmpty(language))
            {
                return true;
            }

            CultureInfo specifiedCulture;

            try
            {
                specifiedCulture = CultureInfo.GetCultureInfo(language);
            }
            catch (Exception e)
            {
                GameLog.Core.GameData.Error(e);
                return false;
            }

            if (Equals(specifiedCulture, ResourceManager.NeutralCulture))
            {
                return true;
            }

            CultureInfo specifiedNeutralCulture = specifiedCulture;
            while (!specifiedNeutralCulture.IsNeutralCulture)
            {
                specifiedNeutralCulture = specifiedNeutralCulture.Parent;
            }

            if (Equals(specifiedNeutralCulture, ResourceManager.NeutralCulture))
            {
                return true;
            }

            return Equals(specifiedNeutralCulture, CultureInfo.InvariantCulture);
        }

        private static bool IsValidLanguageForUser(string language)
        {
            if (language == null)
            {
                return false;
            }

            CultureInfo specifiedCulture;

            try
            {
                specifiedCulture = CultureInfo.GetCultureInfo(language);
            }
            catch (Exception e)
            {
                GameLog.Core.GameData.Error(e);
                return false;
            }

            CultureInfo currentCulture = ResourceManager.CurrentCulture;
            if (Equals(currentCulture, specifiedCulture))
            {
                return true;
            }

            CultureInfo currentNeutralCulture = currentCulture;
            CultureInfo specifiedNeutralCulture = specifiedCulture;

            while (!currentNeutralCulture.IsNeutralCulture &&
                   (currentNeutralCulture.Parent != currentNeutralCulture))
            {
                currentNeutralCulture = currentNeutralCulture.Parent;
            }

            while (!specifiedNeutralCulture.IsNeutralCulture &&
                   (specifiedNeutralCulture.Parent != specifiedNeutralCulture))
            {
                specifiedNeutralCulture = specifiedNeutralCulture.Parent;
            }

            return Equals(currentNeutralCulture, specifiedNeutralCulture);
        }

        public static string GetString(Civilization civ, string category, string key, int randomIndex)
        {
            return GetString(civ?.Key, category, key, null, randomIndex);
        }

        public static string GetString(Civilization civ, string category, string key, Tone? demeanor, int randomIndex)
        {
            return GetString(civ?.Key, category, key, randomIndex);
        }

        public static string GetString(string civKey, string category, string key, int randomIndex)
        {
            return GetString(civKey, category, key, null, randomIndex);
        }

        public static string GetString(string civKey, string category, string key, Tone? demeanor, int randomIndex)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            IEnumerable<string> result = s_searchFunctions
                .Select(searchFunction => searchFunction(civKey, category, key, demeanor))
.FirstOrDefault(results => results.Any());

            if (result != null)
            {
                return result.ElementAt(randomIndex % result.Count());
            }

            return key;
        }
        #endregion
    }
}