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
        public LocalizedTextDatabase()
        {
            Groups = new LocalizedTextGroupCollection();
        }

        public string GetString(object groupKey, object stringkey)
        {

            if (Groups.TryGetValue(groupKey, out LocalizedTextGroup group) && group.Entries.TryGetValue(stringkey, out LocalizedString entry))
            {
                return entry.LocalText;
            }

            return null;
        }

        public LocalizedTextGroupCollection Groups { get; }

        public void Merge([NotNull] LocalizedTextDatabase database, bool overwrite = false)
        {
            if (database == null)
            {
                throw new ArgumentNullException("database");
            }

            foreach (LocalizedTextGroup group in database.Groups)
            {
                Merge(group, overwrite);
            }
        }

        public void Merge([NotNull] LocalizedTextGroup group, bool overwrite = false)
        {
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }


            if (Groups.TryGetValue(group.Key, out LocalizedTextGroup existingGroup))
            {
                foreach (LocalizedString entry in group.Entries)
                {

                    if (existingGroup.Entries.TryGetValue(entry.Name, out LocalizedString existingEntry))
                    {
                        existingEntry.Merge(entry, overwrite);
                    }
                    else
                    {
                        existingGroup.Entries.Add(entry);
                    }
                }
            }
            else
            {
                Groups.Add(group);
            }
        }

        public static IEnumerable<Uri> LocateTextFiles()
        {
            IVfsService vfsService = ResourceManager.VfsService;
            if (vfsService == null)
            {
                return Enumerable.Empty<Uri>();
            }

            return LocateTextFiles(vfsService);
        }

        private static IEnumerable<Uri> LocateTextFiles(IVfsService vfsService)
        {
            List<Uri> files = new List<Uri>();

            IEnumerable<string> fileNames =
                (
                    from source in vfsService.Sources
                    from fileName in source.GetFiles(@"Resources\Data", true, "*.xaml")
                    select fileName
                ).Distinct();

            files.AddRange(fileNames.Select(ResourceManager.GetResourceUri));

            return files;
        }

        private static readonly Lazy<LocalizedTextDatabase> _instance = new Lazy<LocalizedTextDatabase>(Load, LazyThreadSafetyMode.PublicationOnly);

        public static LocalizedTextDatabase Instance => _instance.Value;

        public static LocalizedTextDatabase Load()
        {
            IVfsService vfsService = ResourceManager.VfsService;

            IEnumerable<Uri> files = LocateTextFiles(vfsService);
            LocalizedTextDatabase database = new LocalizedTextDatabase();

            foreach (Uri file in files)
            {
                try
                {

                    if (!vfsService.TryGetFileInfo(file, out IVirtualFileInfo fileInfo))
                    {
                        continue;
                    }

                    object content;

                    using (System.IO.Stream stream = fileInfo.OpenRead())
                    {
                        content = XamlServices.Load(stream);
                    }

                    if (content is LocalizedTextDatabase databaseContent)
                    {
                        database.Merge(databaseContent);
                        continue;
                    }

                    if (content is LocalizedTextGroup textGroup)
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
        private static readonly object _diplomacyText = new StandardLocalizedTextGroupKey("DiplomacyText");

        public static object GalaxyScreen { get; } = new StandardLocalizedTextGroupKey("GalaxyScreen");

        public static object ColonyScreen { get; } = new StandardLocalizedTextGroupKey("ColonyScreen");

        public static object InvasionScreen { get; } = new StandardLocalizedTextGroupKey("InvasionScreen");

        public static object AssetsScreen { get; } = new StandardLocalizedTextGroupKey("AssetsScreen");

        public static object DiplomacyScreen { get; } = new StandardLocalizedTextGroupKey("DiplomacyScreen");

        public static object DiplomacyText => _diplomacyText;

        [TypeConverter(typeof(LocalizedTextGroupKeyConverter))]
        public sealed class StandardLocalizedTextGroupKey
        {
            internal StandardLocalizedTextGroupKey([NotNull] string name)
            {
                Name = name ?? throw new ArgumentNullException("name");
            }

            public string Name { get; }

            public override string ToString()
            {
                return Name;
            }
        }

        internal class LocalizedTextGroupKeyConverter : TypeConverter
        {
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (destinationType == typeof(MarkupExtension))
                {
                    return true;
                }

                return base.CanConvertTo(context, destinationType);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (value is StandardLocalizedTextGroupKey standardKey &&
                    destinationType == typeof(MarkupExtension))
                {
                    if (context is IValueSerializerContext serializerContext)
                    {
                        ValueSerializer typeSerializer = serializerContext.GetValueSerializerFor(typeof(Type));
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
            get => _key;
            set
            {
                VerifyInitializing();
                _key = value;
                _entries.GroupKey = _key;
            }
        }

        public LocalizedStringCollection Entries => _entries;

        public LocalizedString DefaultEntry
        {
            get
            {
                if (_entries.Count == 0)
                {
                    return null;
                }

                return _entries[0];
            }
        }

        public string DefaultLocalText
        {
            get
            {
                LocalizedString defaultEntry = DefaultEntry;
                if (defaultEntry == null)
                {
                    return null;
                }

                return defaultEntry.LocalText;
            }
        }

        public string GetString(object entryKey)
        {
            if (_entries.TryGetValue(entryKey, out LocalizedString entry))
            {
                return entry.LocalText;
            }

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
                if (TryGetValue(key, out LocalizedString value))
                {
                    return value;
                }

                return null;
            }
        }

        internal object GroupKey
        {

            get => _groupKey;
            set
            {
                VerifyInitializing();
                _groupKey = value;
            }

        }

        protected override void OnKeyCollision(object key, LocalizedString item)
        {
            string _text;
            if (key == null)
            {
                _text = "WARN_0123: Localized text group ' "+ FormatGroupKey() + " ' has more than one default entry defined.";
                Console.WriteLine(_text);
                GameLog.Client.GameData.WarnFormat(_text);
            }
            else
            {
                //GameLog.Client.GameData.WarnFormat(
                //    "WARN_0123:Localized text group '{0}' already contains entry '{1}'.",
                //    FormatGroupKey(),
                //    key);
                _text = "WARN_0124: Localized text group ' "+ FormatGroupKey() + " ' already contains entry ' " + key + " '";
                Console.WriteLine(_text);
                GameLog.Client.GameData.WarnFormat(_text);
            }
        }

        private string FormatGroupKey()
        {
            object groupKey = GroupKey;
            if (groupKey == null)
            {
                return string.Empty;
            }

            Type typeKey = groupKey as Type;
            if (typeKey != null)
            {
                return string.Format("{{{0}}}", typeKey.Name);
            }

            if (groupKey is NameTypeTextGroupKey nameTypeKey)
            {
                return string.Format("{{{0}, {1}}}", nameTypeKey.Type.Name, nameTypeKey.Name);
            }

            return string.Format("{0}", groupKey);
        }

        private bool _isInitialized;

        private void VerifyInitializing()
        {
            if (!_isInitialized)
            {
                return;
            }

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
                {
                    return;
                }

                if (_groupKey == null)
                {
                    throw new InvalidOperationException("Localized text groups must have a key defined.");
                }

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
                {
                    return _isInitialized;
                }
            }
        }

        [field: NonSerialized]
        public event EventHandler Initialized;

        private void OnInitialized()
        {
            Initialized?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }

    public sealed class NameTypeTextGroupKey
    {
        private readonly string _name;

        public NameTypeTextGroupKey([NotNull] Type type, [NotNull] string name)
        {
            Type = type ?? throw new ArgumentNullException("type");
            _name = name ?? throw new ArgumentNullException("name");
        }

        public Type Type { get; }

        public string Name => _name;
    }

    public sealed class ContextualTextEntryKey : IEquatable<ContextualTextEntryKey>
    {
        private readonly object _context;

        public ContextualTextEntryKey([NotNull] object context, [NotNull] object baseKey)
        {
            BaseKey = baseKey ?? throw new ArgumentNullException("baseKey");
            _context = context ?? throw new ArgumentNullException("context");
        }

        public object BaseKey { get; }

        public object Context => _context;

        public bool Equals(ContextualTextEntryKey other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(other.BaseKey, BaseKey) &&
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
                return (BaseKey.GetHashCode() * 397) ^ _context.GetHashCode();
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