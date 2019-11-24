using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Resources;
using Supremacy.Types;
using Supremacy.Utility;
using Supremacy.VFS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Markup;
using System.Xaml;

namespace Supremacy.Text
{
    [ContentProperty("Groups")]
    public sealed class LocalizedTextDatabase : SupportInitializeBase
    {
        private readonly LocalizedTextGroupCollection _groups;

        public LocalizedTextDatabase()
        {
            _groups = new LocalizedTextGroupCollection();
        }

        public string GetString(object groupKey, object stringkey)
        {
            LocalizedTextGroup group;
            LocalizedString entry;

            if (_groups.TryGetValue(groupKey, out group) && group.Entries.TryGetValue(stringkey, out entry))
                return entry.LocalText;

            return null;
        }

        public LocalizedTextGroupCollection Groups
        {
            get { return _groups; }
        }

        public void Merge([NotNull] LocalizedTextDatabase database, bool overwrite = false)
        {
            if (database == null)
                throw new ArgumentNullException("database");

            foreach (var group in database.Groups)
                Merge(group, overwrite);
        }

        public void Merge([NotNull] LocalizedTextGroup group, bool overwrite = false)
        {
            if (group == null)
                throw new ArgumentNullException("group");

            LocalizedTextGroup existingGroup;

            if (_groups.TryGetValue(group.Key, out existingGroup))
            {
                foreach (var entry in group.Entries)
                {
                    LocalizedString existingEntry;

                    if (existingGroup.Entries.TryGetValue(entry.Name, out existingEntry))
                        existingEntry.Merge(entry, overwrite);
                    else
                        existingGroup.Entries.Add(entry);
                }
            }
            else
            {
                _groups.Add(group);
            }
        }

        public static IEnumerable<Uri> LocateTextFiles()
        {
            var vfsService = ResourceManager.VfsService;
            if (vfsService == null)
                return Enumerable.Empty<Uri>();

            return LocateTextFiles(vfsService);
        }

        private static IEnumerable<Uri> LocateTextFiles(IVfsService vfsService)
        {
            var files = new List<Uri>();
            
            var fileNames =
                (
                    from source in vfsService.Sources
                    from fileName in source.GetFiles(@"Resources\Data", true, "*.xaml")
                    select fileName
                ).Distinct();

            files.AddRange(fileNames.Select(ResourceManager.GetResourceUri));

            return files;
        }

        private static readonly Lazy<LocalizedTextDatabase> _instance = new Lazy<LocalizedTextDatabase>(Load, LazyThreadSafetyMode.PublicationOnly);

        public static LocalizedTextDatabase Instance
        {
            get { return _instance.Value; }
        }

        public static LocalizedTextDatabase Load()
        {
            var vfsService = ResourceManager.VfsService;

            var files = LocateTextFiles(vfsService);
            var database = new LocalizedTextDatabase();

            foreach (var file in files)
            {
                try
                {
                    IVirtualFileInfo fileInfo;

                    if (!vfsService.TryGetFileInfo(file, out fileInfo))
                        continue;

                    object content;

                    using (var stream = fileInfo.OpenRead())
                    {
                        content = XamlServices.Load(stream);
                    }

                    var databaseContent = content as LocalizedTextDatabase;
                    if (databaseContent != null)
                    {
                        database.Merge(databaseContent);
                        continue;
                    }

                    var textGroup = content as LocalizedTextGroup;
                    if (textGroup != null)
                    {
                        database.Merge(textGroup);
                        continue;
                    }

                    // all files *.xaml of the old folder \Text were available for LocalizedText

                    // in new folder \Data at the moment two files *.xaml are present (and not necessary to be Localized)
                    // DiplomacyDatabase.xaml + ScriptedEvents.xaml
                    if (file.LocalPath.Contains("ScriptedEvents.xaml") ||
                        file.LocalPath.Contains("DiplomacyDatabase.xaml"))
                    {
                        continue;
                    }

                    GameLog.Client.GameData.WarnFormat(
                        "Could not determine the content type of text file '{0}'.  " +
                        "The file will be ignored for LocalizedText tasks.",
                        file);
                        
                }
                catch (Exception e)
                {
                    GameLog.Client.GameData.Error(
                        string.Format(
                            "An error occurred while loading localized text file '{0}'.",
                            file),
                        e);
                }
            }

            return database;
        }
    }

    public sealed class LocalizedTextGroupCollection : KeyedCollectionBase<object, LocalizedTextGroup>
    {
        public LocalizedTextGroupCollection()
            : base(o => o.Key) { }
    }

    public static class LocalizedTextGroups
    {
        private static readonly object _galaxyScreen = new StandardLocalizedTextGroupKey("GalaxyScreen");
        private static readonly object _colonyScreen = new StandardLocalizedTextGroupKey("ColonyScreen");
        private static readonly object _invasionScreen = new StandardLocalizedTextGroupKey("InvasionScreen");
        private static readonly object _assetsScreen = new StandardLocalizedTextGroupKey("AssetsScreen");
        private static readonly object _diplomacyScreen = new StandardLocalizedTextGroupKey("DiplomacyScreen");
        private static readonly object _diplomacyText = new StandardLocalizedTextGroupKey("DiplomacyText");

        public static object GalaxyScreen
        {
            get { return _galaxyScreen; }
        }

        public static object ColonyScreen
        {
            get { return _colonyScreen; }
        }

        public static object InvasionScreen
        {
            get { return _invasionScreen; }
        }

        public static object AssetsScreen
        {
            get { return _assetsScreen; }
        }

        public static object DiplomacyScreen
        {
            get { return _diplomacyScreen; }
        }

        public static object DiplomacyText
        {
            get { return _diplomacyText; }
        }

        [TypeConverter(typeof(LocalizedTextGroupKeyConverter))]
        public sealed class StandardLocalizedTextGroupKey
        {
            private readonly string _name;

            internal StandardLocalizedTextGroupKey([NotNull] string name)
            {
                if (name == null)
                    throw new ArgumentNullException("name");

                _name = name;
            }

            public string Name
            {
                get { return _name; }
            }

            public override string ToString()
            {
                return _name;
            }
        }

        internal class LocalizedTextGroupKeyConverter : TypeConverter
        {
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (destinationType == typeof(MarkupExtension))
                    return true;
                return base.CanConvertTo(context, destinationType);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                var standardKey = value as StandardLocalizedTextGroupKey;
                if (standardKey != null &&
                    destinationType == typeof(MarkupExtension))
                {
                    var serializerContext = context as IValueSerializerContext;
                    if (serializerContext != null)
                    {
                        var typeSerializer = serializerContext.GetValueSerializerFor(typeof(Type));
                        if (typeSerializer != null)
                        {
                            return new StaticExtension(
                                typeSerializer.ConvertToString(typeof(LocalizedTextGroups), serializerContext) +
                                "." +
                                standardKey.Name);
                        }
                    }
                    return new StaticExtension
                           {
                               MemberType = typeof(LocalizedTextGroups),
                               Member = standardKey.Name
                           };
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }

    [Serializable]
    [ContentProperty("Entries")]
    public sealed class LocalizedTextGroup : SupportInitializeBase
    {
        private object _key;
        private readonly LocalizedStringCollection _entries;

        public LocalizedTextGroup()
        {
            _entries = new LocalizedStringCollection();
        }

        public object Key
        {
            get { return _key; }
            set
            {
                VerifyInitializing();
                _key = value;
                _entries.GroupKey = _key;
            }
        }

        public LocalizedStringCollection Entries
        {
            get { return _entries; }
        }

        public LocalizedString DefaultEntry
        {
            get
            {
                if (_entries.Count == 0)
                    return null;

                return _entries[0];
            }
        }

        public string DefaultLocalText
        {
            get
            {
                var defaultEntry = DefaultEntry;
                if (defaultEntry == null)
                    return null;

                return defaultEntry.LocalText;
            }
        }

        public string GetString(object entryKey)
        {
            LocalizedString entry;
            if (_entries.TryGetValue(entryKey, out entry))
                return entry.LocalText;
            return null;
        }
    }

    [Serializable]
    public sealed class LocalizedStringCollection : KeyedCollectionBase<object, LocalizedString>,
                                                    ISupportInitializeNotification
    {
        private object _groupKey;

        public LocalizedStringCollection()
            : base(o => o.Name) { }

        public override LocalizedString this[object key]
        {
            get
            {
                LocalizedString value;
                if (TryGetValue(key, out value))
                    return value;
                return null;
            }
        }

        internal object GroupKey
        {
            // ReSharper disable MemberCanBePrivate.Local
            get { return _groupKey; }
            set
            {
                VerifyInitializing();
                _groupKey = value;
            }
            // ReSharper restore MemberCanBePrivate.Local
        }

        protected override void OnKeyCollision(object key, LocalizedString item)
        {
            if (key == null)
            {
                GameLog.Client.GameData.WarnFormat(
                    "Localized text group '{0}' has more than one default entry defined." +
                    FormatGroupKey());
            }
            else
            {
                GameLog.Client.GameData.WarnFormat(
                    "Localized text group '{0}' already contains entry '{1}'.",
                    FormatGroupKey(),
                    key);
            }
        }

        private string FormatGroupKey()
        {
            var groupKey = GroupKey;
            if (groupKey == null)
                return string.Empty;

            var typeKey = groupKey as Type;
            if (typeKey != null)
                return string.Format("{{{0}}}", typeKey.Name);

            var nameTypeKey = groupKey as NameTypeTextGroupKey;
            if (nameTypeKey != null)
                return string.Format("{{{0}, {1}}}", nameTypeKey.Type.Name, nameTypeKey.Name);

            return string.Format("{0}", groupKey);
        }

        private bool _isInitialized;

        private void VerifyInitializing()
        {
            if (!_isInitialized)
                return;

            throw new InvalidOperationException(SR.InvalidOperationException_AlreadyInitialized);
        }

        #region Implementation of ISupportInitialize
        public void BeginInit()
        {
            lock (SyncRoot)
            {
                _isInitialized = false;
            }
        }

        public void EndInit()
        {
            lock (SyncRoot)
            {
                if (_isInitialized)
                    return;

                if (_groupKey == null)
                    throw new InvalidOperationException("Localized text groups must have a key defined.");

                _isInitialized = true;

                OnInitialized();
            }
        }
        #endregion

        #region Implementation of ISupportInitializeNotification
        public bool IsInitialized
        {
            get
            {
                lock (SyncRoot)
                    return _isInitialized;
            }
        }

        [field: NonSerialized]
        public event EventHandler Initialized;

        private void OnInitialized()
        {
            EventHandler handler = Initialized;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        #endregion
    }
    
    public sealed class NameTypeTextGroupKey
    {
        private readonly Type _type;
        private readonly string _name;

        public NameTypeTextGroupKey([NotNull] Type type, [NotNull] string name)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (name == null)
                throw new ArgumentNullException("name");

            _type = type;
            _name = name;
        }

        public Type Type
        {
            get { return _type; }
        }

        public string Name
        {
            get { return _name; }
        }
    }

    public sealed class ContextualTextEntryKey : IEquatable<ContextualTextEntryKey>
    {
        private readonly object _baseKey;
        private readonly object _context;

        public ContextualTextEntryKey([NotNull] object context, [NotNull] object baseKey)
        {
            if (baseKey == null)
                throw new ArgumentNullException("baseKey");
            if (context == null)
                throw new ArgumentNullException("context");

            _baseKey = baseKey;
            _context = context;
        }

        public object BaseKey
        {
            get { return _baseKey; }
        }

        public object Context
        {
            get { return _context; }
        }

        public bool Equals(ContextualTextEntryKey other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Equals(other._baseKey, _baseKey) &&
                   Equals(other._context, _context);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ContextualTextEntryKey);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_baseKey.GetHashCode() * 397) ^ _context.GetHashCode();
            }
        }

        public static bool operator ==(ContextualTextEntryKey left, ContextualTextEntryKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ContextualTextEntryKey left, ContextualTextEntryKey right)
        {
            return !Equals(left, right);
        }
    }
}