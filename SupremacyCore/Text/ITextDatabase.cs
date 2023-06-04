// FIle:ITextDatabase.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Linq;

using Supremacy.Annotations;
using Supremacy.Collections;

using System.Linq;

using Supremacy.Resources;
using Supremacy.Types;
using Supremacy.Utility;
using System.IO;

namespace Supremacy.Text
{
    public interface ITextDatabase : IRevertibleChangeTracking
    {
        #region Properties and Indexers
        bool IsReadOnly { get; }
        #endregion

        #region Public and Protected Methods
        ITextDatabaseTable<TLocalizedEntry> GetTable<TLocalizedEntry>() where TLocalizedEntry : ILocalizedTextDatabaseEntry;
        void Save();
        #endregion
    }

    public interface ITechObjectTextDatabaseEntry : ILocalizedTextDatabaseEntry
    {
        string Name { get; set; }
        string Description { get; set; }
        string Custom1 { get; set; }
        string Custom2 { get; set; }

    }

    public interface IRaceTextDatabaseEntry : ILocalizedTextDatabaseEntry
    {
        string SingularName { get; set; }
        string PluralName { get; set; }
        string Description { get; set; }
    }

    [Serializable]
    public sealed class ClientTextDatabase : ITextDatabase
    {
        private readonly ClientTextDatabaseTable<ITechObjectTextDatabaseEntry> _techObjectTextTable;
        private readonly ClientTextDatabaseTable<IRaceTextDatabaseEntry> _raceTextTable;

        private ClientTextDatabase()
        {
            _techObjectTextTable = new ClientTextDatabaseTable<ITechObjectTextDatabaseEntry>();
            _raceTextTable = new ClientTextDatabaseTable<IRaceTextDatabaseEntry>();
        }

        public static ClientTextDatabase Load(string path)
        {
            XDocument doc = XDocument.Load(path);

            ClientTextDatabase database = new ClientTextDatabase();

            ClientTextDatabaseTable<ITechObjectTextDatabaseEntry> techObjectTable = database.TechObjectTextTable;
            string techObjectEntryType = typeof(ITechObjectTextDatabaseEntry).FullName;
            XElement techObjectTableElement = doc.Root.Elements("Tables")
                .Elements("Table")
                .FirstOrDefault(e => string.Equals((string)e.Attribute("EntryType"), techObjectEntryType));

            if (techObjectTableElement == null)
            {
                return database;
            }

            IEnumerable<XElement> techObjectEntries = techObjectTableElement
                .Elements("Entries")
                .Elements("Entry");

            // for Output file
            string pathOutputfile = "./lib/";  // instead of ./Resources/Data/
            string separator = ";";
            string line = "";
            //StreamWriter streamWriter;
            string file = "./lib/test-FromTextDatabase.txt";
            string strHeader = "";  // first line of output files


            try // avoid hang up if this file is opened by another program 
            {
                //if (_XML2CSVOutput == false)
                //    continue;

                file = pathOutputfile + "_TextDatabase_List(autoCreated).csv";
                Console.WriteLine("Step_9160: writing {0}", file); // _TextDatabase_List(autoCreated).csv

                if (file == null)
                {
                    goto WriterCloseFromTextDatabase;
                }

                //streamWriter = new StreamWriter(file);

                strHeader =    // Head line
                    "ATT_Key" 
                    + separator + "CE_Name" 
                    + separator + "CE_Description" 
                    + separator + "CE_Custom1" 
                    + separator + "CE_Custom2"
                    + separator + "Text"
                    ;

                //streamWriter.WriteLine(strHeader);
                // End of head line

                foreach (XElement entryElement in techObjectEntries)
                {
                    string key = (string)entryElement.Attribute("Key");

                    if (key == null)
                    {
                        continue;
                    }

                    ClientTextDatabaseEntry<ITechObjectTextDatabaseEntry> entry = new ClientTextDatabaseEntry<ITechObjectTextDatabaseEntry>(key);
                    IEnumerable<XElement> localizedEntries = entryElement.Elements("LocalizedEntries").Elements("LocalizedEntry");


                    foreach (XElement localizedEntryElement in localizedEntries)
                    {
                        //App.DoEvents();  // for avoid error after 60 seconds
                        TechObjectTextDatabaseEntry localizedEntry = new TechObjectTextDatabaseEntry(
                            (string)localizedEntryElement.Attribute("Language"),
                            (string)localizedEntryElement.Element("Name"),
                            (string)localizedEntryElement.Element("Description"),
                            (string)localizedEntryElement.Element("Custom1"),
                            (string)localizedEntryElement.Element("Custom2")
                            );

                        entry.AddInternal(localizedEntry);

                        //for output file
                        line =
                            entry.Key + separator +
                            (string)localizedEntryElement.Attribute("Language") + separator +
                            (string)localizedEntryElement.Element("Name") + separator +
                            (string)localizedEntryElement.Element("Custom1") + separator +
                            (string)localizedEntryElement.Element("Custom2") + separator +
                            (string)localizedEntryElement.Element("Description");   // Description at end because of some semicolon inside

                        //Console.WriteLine("{0}", line);
                        //streamWriter.WriteLine(line);

                    }
                    techObjectTable.AddInternal(entry);
                }

                //streamWriter.Close();
            WriterCloseFromTextDatabase:;
                // End of Autocreated files   


                ClientTextDatabaseTable<IRaceTextDatabaseEntry> raceTable = database.RaceTextTable;
                string raceEntryType = typeof(IRaceTextDatabaseEntry).FullName;
                XElement raceTableElement = doc.Root.Elements("Tables")
                    .Elements("Table")
                    .FirstOrDefault(e => string.Equals((string)e.Attribute("EntryType"), raceEntryType));

                if (raceTableElement == null)    // Races might be done in RaceDatabase.cs
                {
                    return database;
                }

                IEnumerable<XElement> raceEntries = raceTableElement
                    .Elements("Entries")
                    .Elements("Entry");

                foreach (XElement entryElement in raceEntries)
                {
                    string key = (string)entryElement.Attribute("Key");

                    if (key == null)
                    {
                        continue;
                    }

                    ClientTextDatabaseEntry<IRaceTextDatabaseEntry> entry = new ClientTextDatabaseEntry<IRaceTextDatabaseEntry>(key);
                    IEnumerable<XElement> localizedEntries = entryElement.Elements("LocalizedEntries").Elements("LocalizedEntry");

                    foreach (XElement localizedEntryElement in localizedEntries)
                    {
                        RaceTextDatabaseEntry localizedEntry = new RaceTextDatabaseEntry(
                            (string)localizedEntryElement.Attribute("Language"),
                            (string)localizedEntryElement.Element("SingularName"),
                            (string)localizedEntryElement.Element("PluralName"),
                            (string)localizedEntryElement.Element("Description"));

                        entry.AddInternal(localizedEntry);
                    }

                    raceTable.AddInternal(entry);
                }
            }
            catch (Exception e)
            {
                GameLog.Core.GameData.Error("Cannot write ... ./lib/TextDatabase__List(autoCreated).csv", e);
            }

            return database;
        }

        public void AcceptChanges()
        {
            throw new NotSupportedException();
        }

        public bool IsChanged => false;

        public void RejectChanges()
        {
            throw new NotSupportedException();
        }

        public bool IsReadOnly => true;

        ITextDatabaseTable<TEntry> ITextDatabase.GetTable<TEntry>()
        {
            if (typeof(TEntry) == typeof(ITechObjectTextDatabaseEntry))
            {
                return (ITextDatabaseTable<TEntry>)_techObjectTextTable;
            }

            if (typeof(TEntry) == typeof(IRaceTextDatabaseEntry))
            {
                return (ITextDatabaseTable<TEntry>)_raceTextTable;
            }

            return null;
        }

        private ClientTextDatabaseTable<ITechObjectTextDatabaseEntry> TechObjectTextTable => _techObjectTextTable;

        private ClientTextDatabaseTable<IRaceTextDatabaseEntry> RaceTextTable => _raceTextTable;

        public void Save()
        {
            throw new NotSupportedException();
        }

        #region Nested Type: TechObjectTextDatabaseTable
        [Serializable]
        private class ClientTextDatabaseTable<TLocalizedEntry> : ITextDatabaseTable<TLocalizedEntry> where TLocalizedEntry : ILocalizedTextDatabaseEntry
        {
            private readonly KeyedCollectionBase<string, ITextDatabaseEntry<TLocalizedEntry>> _entries;

            public ClientTextDatabaseTable()
            {
                _entries = new KeyedCollectionBase<string, ITextDatabaseEntry<TLocalizedEntry>>(entry => entry.Key);
            }

            public IEnumerator<ITextDatabaseEntry<TLocalizedEntry>> GetEnumerator()
            {
                return _entries.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            internal void AddInternal(ITextDatabaseEntry<TLocalizedEntry> item)
            {
                _entries.Add(item);
            }

            public void Add(ITextDatabaseEntry<TLocalizedEntry> item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(ITextDatabaseEntry<TLocalizedEntry> item)
            {
                return _entries.Contains(item);
            }

            public void CopyTo(ITextDatabaseEntry<TLocalizedEntry>[] array, int arrayIndex)
            {
                _entries.CopyTo(array, arrayIndex);
            }

            public bool Remove(ITextDatabaseEntry<TLocalizedEntry> item)
            {
                throw new NotSupportedException();
            }

            public int Count => _entries.Count;

            public bool IsReadOnly => true;

            public void AcceptChanges()
            {
                throw new NotSupportedException();
            }

            public bool IsChanged => false;

            public void RejectChanges()
            {
                throw new NotSupportedException();
            }

            public ITextDatabaseEntry<TLocalizedEntry> this[string key]
            {
                get { return _entries[key]; }
            }

            public ITextDatabaseEntry<TLocalizedEntry> CreateEntry(string key)
            {
                throw new NotSupportedException();
            }

            public bool Contains(string key)
            {
                return _entries.Contains(key);
            }

            public bool Remove(string key)
            {
                throw new NotSupportedException();
            }

            public bool TryGetEntry(string key, out ITextDatabaseEntry<TLocalizedEntry> entry)
            {
                return _entries.TryGetValue(key, out entry);
            }

            public event NotifyCollectionChangedEventHandler CollectionChanged
            {
                add { }
                remove { }
            }

            public void BeginEdit() { throw new NotSupportedException(); }
            public void EndEdit() { throw new NotSupportedException(); }
            public void CancelEdit() { throw new NotSupportedException(); }
        }
        #endregion

        #region Nested Type: ClientTextDatabaseEntry
        [Serializable]
        private class ClientTextDatabaseEntry<TLocalizedEntry> : ITextDatabaseEntry<TLocalizedEntry>
            where TLocalizedEntry : class, ILocalizedTextDatabaseEntry
        {
            private readonly string _key;
            private readonly KeyedCollectionBase<string, TLocalizedEntry> _localizedEntries;

            public ClientTextDatabaseEntry([NotNull] string key)
            {
                _key = key ?? throw new ArgumentNullException("key");
                _localizedEntries = new KeyedCollectionBase<string, TLocalizedEntry>(entry => entry.Language);
            }

            public IEnumerator<TLocalizedEntry> GetEnumerator()
            {
                return _localizedEntries.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(TLocalizedEntry item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(TLocalizedEntry item)
            {
                return _localizedEntries.Contains(item);
            }

            public void CopyTo(TLocalizedEntry[] array, int arrayIndex)
            {
                _localizedEntries.CopyTo(array, arrayIndex);
            }

            public bool Remove(TLocalizedEntry item)
            {
                throw new NotSupportedException();
            }

            public int Count => _localizedEntries.Count;

            public bool IsReadOnly => true;

            public void AcceptChanges()
            {
                throw new NotSupportedException();
            }

            public bool IsChanged => false;

            public void RejectChanges()
            {
                throw new NotSupportedException();
            }

            public string Key => _key;

            public IEnumerable<TLocalizedEntry> LocalizedEntries => _localizedEntries;

            public TLocalizedEntry CreateLocalizedEntry(string language)
            {
                throw new NotSupportedException();
            }

            public void AddInternal(TLocalizedEntry entry)
            {
                _localizedEntries.Add(entry);
            }

            public TLocalizedEntry GetLocalizedEntry(string language)
            {
                CultureInfo culture = ResourceManager.CurrentCulture;


                if (_localizedEntries.TryGetValue(culture.Name, out TLocalizedEntry localizedEntry))
                {
                    return localizedEntry;
                }

                while (!culture.IsNeutralCulture &&
                       culture.Parent != CultureInfo.InvariantCulture)
                {
                    culture = culture.Parent;

                    if (_localizedEntries.TryGetValue(culture.Name, out localizedEntry))
                    {
                        return localizedEntry;
                    }
                }

                culture = ResourceManager.NeutralCulture;

                if (_localizedEntries.TryGetValue(culture.Name, out localizedEntry))
                {
                    return localizedEntry;
                }

                while (!culture.IsNeutralCulture &&
                       culture.Parent != CultureInfo.InvariantCulture)
                {
                    culture = culture.Parent;

                    if (_localizedEntries.TryGetValue(culture.Name, out localizedEntry))
                    {
                        return localizedEntry;
                    }
                }

                return null;
            }

            public event NotifyCollectionChangedEventHandler CollectionChanged
            {
                add { }
                remove { }
            }

            public void BeginEdit() { throw new NotSupportedException(); }
            public void EndEdit() { throw new NotSupportedException(); }
            public void CancelEdit() { throw new NotSupportedException(); }

            public event EventHandler Changed
            {
                add { }
                remove { }
            }
        }
        #endregion

        #region Nested Type: TechObjectTextDatabaseEntry
        [Serializable]
        private class TechObjectTextDatabaseEntry : ITechObjectTextDatabaseEntry
        {
            private readonly string _language;

            public TechObjectTextDatabaseEntry(
                [NotNull] string language,
                string name,
                string description,
                string custom1,
                string custom2)
            {
                _language = language ?? throw new ArgumentNullException("language");

                if (name != null)
                {
                    name = name.Trim();
                }

                if (description != null)
                {
                    description = description.Trim();
                }

                if (custom1 != null)
                {
                    custom1 = custom1.Trim();
                }

                if (custom2 != null)
                {
                    custom2 = custom2.Trim();
                }

                Name = name;
                Description = description;
                Custom1 = custom1;
                Custom2 = custom2;
            }

            public void AcceptChanges()
            {
                throw new NotSupportedException();
            }

            public bool IsChanged => false;

            public void RejectChanges() { }

            public string Language => _language;

            public string LanguageName => CultureInfo.GetCultureInfo(Language).EnglishName;

            public Dictionary<string, object> GetSavedValues()
            {
                return new Dictionary<string, object>
                       {
                           { "Name", Name },
                           //{ "ClassLevel", ClassLevel },
                           { "Description", Description },
                           { "Custom1", Custom1 },
                           { "Custom2", Custom2 }
                       };
            }

            public string Name { get; set; }
            //public string ClassLevel { get; set; }
            public string Description { get; set; }
            public string Custom1 { get; set; }
            public string Custom2 { get; set; }

            public void BeginEdit() { }
            public void EndEdit() { }
            public void CancelEdit() { }

            public event EventHandler Changed
            {
                add { }
                remove { }
            }
        }
        #endregion

        #region Nested Type: RaceTextDatabaseEntry
        [Serializable]
        private class RaceTextDatabaseEntry : IRaceTextDatabaseEntry
        {
            private readonly string _language;

            public RaceTextDatabaseEntry(
                [NotNull] string language,
                string singularName,
                string pluralName,
                //string classLevel,
                string description)
            {
                _language = language ?? throw new ArgumentNullException("language");

                if (singularName != null)
                {
                    singularName = singularName.Trim();
                }

                if (pluralName != null)
                {
                    pluralName = pluralName.Trim();
                }
                //if (classLevel != null)
                //    classLevel = classLevel.Trim();
                if (description != null)
                {
                    description = description.Trim();
                }

                SingularName = singularName;
                PluralName = pluralName;
                //ClassLevel = classLevel;
                Description = description;
            }

            public void AcceptChanges()
            {
                throw new NotSupportedException();
            }

            public bool IsChanged => false;

            public void RejectChanges() { }

            public string Language => _language;

            public string LanguageName => CultureInfo.GetCultureInfo(Language).EnglishName;

            public Dictionary<string, object> GetSavedValues()
            {
                return new Dictionary<string, object>
                       {
                           { "SingularName", SingularName },
                           { "PluralName", PluralName },
                           //{ "ClassLevel", ClassLevel },
                           { "SingularName", Description }
                       };
            }

            public string SingularName { get; set; }
            public string PluralName { get; set; }
            //public string ClassLevel { get; set; }
            public string Description { get; set; }

            public void BeginEdit() { }
            public void EndEdit() { }
            public void CancelEdit() { }

            public event EventHandler Changed
            {
                add { }
                remove { }
            }
        }
        #endregion
    }

    public interface ITextDatabaseTable<TLocalizedEntry> : ITextDatabaseEntryCollection<TLocalizedEntry>
        where TLocalizedEntry : ILocalizedTextDatabaseEntry
    {
        #region Properties and Indexers
        ITextDatabaseEntry<TLocalizedEntry> this[string key] { get; }
        #endregion

        #region Public and Protected Methods
        ITextDatabaseEntry<TLocalizedEntry> CreateEntry(string key);
        bool Contains(string key);
        bool Remove(string key);
        bool TryGetEntry(string key, out ITextDatabaseEntry<TLocalizedEntry> entry);
        #endregion
    }

    public interface ITextDatabaseEntryCollection<TLocalizedEntry> : ICollection<ITextDatabaseEntry<TLocalizedEntry>>, INotifyCollectionChanged, IRevertibleChangeTracking, IEditableObject
        where TLocalizedEntry : ILocalizedTextDatabaseEntry
    { }

    public interface ITextDatabaseEntry<TLocalizedEntry> : ICollection<TLocalizedEntry>, IEditableObject, IRevertibleChangeTracking, INotifyCollectionChanged, INotifyChanged where TLocalizedEntry : ILocalizedTextDatabaseEntry
    {
        #region Properties and Indexers
        string Key { get; }
        IEnumerable<TLocalizedEntry> LocalizedEntries { get; }
        #endregion

        #region Public and Protected Methods
        TLocalizedEntry CreateLocalizedEntry(string language);
        TLocalizedEntry GetLocalizedEntry(string language);
        #endregion
    }

    public static class TextDatabaseEntryExtensions
    {
        #region Public and Protected Methods
        public static TLocalizedEntry GetOrCreateLocalizedEntry<TLocalizedEntry>(
            [NotNull] this ITextDatabaseEntry<TLocalizedEntry> self,
            [NotNull] string language)
            where TLocalizedEntry : class, ILocalizedTextDatabaseEntry
        {
            if (self == null)
            {
                throw new ArgumentNullException("self");
            }

            TLocalizedEntry entry = self.GetLocalizedEntry(language);
            if ((entry == null) || !string.Equals(entry.Language, language, StringComparison.OrdinalIgnoreCase))
            {
                entry = self.CreateLocalizedEntry(language);
            }

            return entry;
        }

        public static TLocalizedEntry GetDisplayEntry<TLocalizedEntry>(
            [NotNull] this ITextDatabaseEntry<TLocalizedEntry> self)
            where TLocalizedEntry : class, ILocalizedTextDatabaseEntry
        {
            if (self == null)
            {
                throw new ArgumentNullException("self");
            }

            return self.GetLocalizedEntry(ResourceManager.CurrentLocale);
        }
        #endregion
    }

    public interface ILocalizedStringCollection
        : ICollection<ILocalizedTextDatabaseEntry>, INotifyCollectionChanged, IEditableObject, IRevertibleChangeTracking
    { }

    public interface ILocalizedTextDatabaseEntry : IRevertibleChangeTracking, IEditableObject, INotifyChanged
    {
        #region Properties and Indexers
        string Language { get; }
        string LanguageName { get; }
        #endregion

        Dictionary<string, object> GetSavedValues();
    }
}