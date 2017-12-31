// CivString.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Linq;

using Microsoft.Practices.ServiceLocation;

using Supremacy.Annotations;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Client;
using Supremacy.Diplomacy;

using System.Linq;
using Supremacy.Utility;

namespace Supremacy.Resources
{
    [Serializable]
    [DebuggerDisplay("{{CivString Civilization={_civKey}, Key={_stringKey}}}")]
    public class CivString : INotifyPropertyChanged
    {
        #region Constants
        public const string DefaultCategory = null;
        public const string DiplomacyCategory = "Diplomacy";
        public const string PersonnelCategory = "Personnel";
        public const string ScriptedEventsCategory = "ScriptedEvents";
        #endregion

        #region Fields
        private static readonly Random s_random;
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
            s_random = new Random();
        }

        public CivString(string category, string key) : this(null, category, key) { }

        public CivString([CanBeNull] Civilization civilization, [NotNull] string category, [NotNull] string key) : this(civilization, category, key, null) { }

        public CivString([CanBeNull] Civilization civilization, [NotNull] string category, [NotNull] string key, Tone? demeanor)
        {
            if (String.IsNullOrEmpty(category))
                throw new ArgumentException("category must be a non-null, non-empty string.", "category");
            if (String.IsNullOrEmpty(key))
                throw new ArgumentException("key must be a non-null, non-empty string.", "key");
            if (civilization != null)
                _civKey = civilization.Key;
            _category = category;
            _stringKey = key;
            _randomIndex = s_random.Next(255);
            _demeanor = demeanor;
        }
        #endregion

        #region Properties
        // ReSharper disable MemberCanBeMadeStatic.Global
        protected IClientContext ClientContext
        {
            get
            {
                if (s_clientContext == null)
                    s_clientContext = ServiceLocator.Current.GetInstance<IClientContext>();
                return s_clientContext;
            }
        }
        // ReSharper restore MemberCanBeMadeStatic.Global

        public Civilization Civilization
        {
            get { return GameContext.Current.Civilizations[_civKey]; }
            set
            {
                _civKey = (value != null) ? value.Key : null;
                OnPropertyChanged("Civilization");
                OnPropertyChanged("Value");
            }
        }

        public Tone? Tone
        {
            get { return _demeanor; }
        }

        public string Category
        {
            get { return _category; }
        }

        public string Key
        {
            get { return _stringKey; }
        }

        public string Value
        {
            get
            {
                if (!_cachedValuePresent)
                    LookupValue();
                return _cachedValue ?? _stringKey;
            }
        }

        public bool HasValue
        {
            get
            {
                if (!_cachedValuePresent)
                    LookupValue();
                return (_cachedValue != null);
            }
        }
        #endregion

        #region Methods
        protected void LookupValue()
        {
            var civ = Civilization;
            if ((civ == null) && (ClientContext != null) && (ClientContext.LocalPlayer != null))
                civ = ClientContext.LocalPlayer.Empire;
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
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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
                var ns = XNamespace.Get("Supremacy:CivStringDatabase.xsd");

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
            catch
            {
                s_databaseXml = null;
            }
        }
        #endregion

        #region Methods
        private static bool IsNeutralLanguage(string language)
        {
            // An empty string matches InvariantCulture.  We recognize null in the same way.
            if (string.IsNullOrEmpty(language))
                return true;

            CultureInfo specifiedCulture;

            try
            {
                specifiedCulture = CultureInfo.GetCultureInfo(language);
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
                return false;
            }

            if (Equals(specifiedCulture, ResourceManager.NeutralCulture))
                return true;

            var specifiedNeutralCulture = specifiedCulture;
            while (!specifiedNeutralCulture.IsNeutralCulture)
                specifiedNeutralCulture = specifiedNeutralCulture.Parent;

            if (Equals(specifiedNeutralCulture, ResourceManager.NeutralCulture))
                return true;

            return Equals(specifiedNeutralCulture, CultureInfo.InvariantCulture);
        }

        private static bool IsValidLanguageForUser(string language)
        {
            if (language == null)
                return false;

            CultureInfo specifiedCulture;

            try
            {
                specifiedCulture = CultureInfo.GetCultureInfo(language);
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
                return false;
            }
            
            var currentCulture = ResourceManager.CurrentCulture;
            if (Equals(currentCulture, specifiedCulture))
                return true;

            var currentNeutralCulture = currentCulture;
            var specifiedNeutralCulture = specifiedCulture;
            
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
            return GetString((civ != null) ? civ.Key : null, category, key, null, randomIndex);
        }

        public static string GetString(Civilization civ, string category, string key, Tone? demeanor, int randomIndex)
        {
            return GetString((civ != null) ? civ.Key : null, category, key, randomIndex);
        }

        public static string GetString(string civKey, string category, string key, int randomIndex)
        {
            return GetString(civKey, category, key, null, randomIndex);
        }

        public static string GetString(string civKey, string category, string key, Tone? demeanor, int randomIndex)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            var result = s_searchFunctions
                .Select(searchFunction => searchFunction(civKey, category, key, demeanor))
                .Where(results => results.Any())
                .FirstOrDefault();

            if (result != null)
                return result.ElementAt(randomIndex % result.Count());
            
            return key;
        }
        #endregion
    }
}