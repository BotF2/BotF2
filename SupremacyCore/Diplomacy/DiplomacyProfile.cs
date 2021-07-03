using System;
using System.ComponentModel;
using System.Windows.Markup;
using System.Xaml;

using Supremacy.Collections;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Resources;
using Supremacy.Types;
using Supremacy.VFS;

namespace Supremacy.Diplomacy
{
    [Serializable]
    [DictionaryKeyProperty("Civilization")]
    public class DiplomacyProfile : SupportInitializeBase
    {
        private string _civilizationKey;
        private readonly RelationshipMemoryWeightCollection _memoryWeights;

        public DiplomacyProfile()
        {
            _memoryWeights = new RelationshipMemoryWeightCollection();
        }

        [DefaultValue(null)]
        [TypeConverter(typeof(RaceConverter))]
        public Civilization Civilization
        {
            get
            {
                if (_civilizationKey == null)
                {
                    return null;
                }

                return GameContext.Current.Civilizations[_civilizationKey];
            }
            set
            {
                VerifyInitializing();
                _civilizationKey = (value != null) ? value.Key : null;
                OnPropertyChanged("Civilization");
            }
        }

        public RelationshipMemoryWeightCollection MemoryWeights => _memoryWeights;

        protected override void BeginInitCore()
        {
            _memoryWeights.BeginInit();
        }

        protected override void EndInitCore()
        {
            _memoryWeights.EndInit();
        }
    }

    [Serializable]
    [DictionaryKeyProperty("MemoryType")]
    public sealed class RelationshipMemoryWeight : SupportInitializeBase
    {
        private MemoryType _memoryType;
        private int _weight;
        private int _maxConcurrentMemories = 5;

        [DefaultValue(MemoryType.None)]
        public MemoryType MemoryType
        {
            get => _memoryType;
            set
            {
                VerifyInitializing();
                _memoryType = value;
                OnPropertyChanged("MemoryType");
            }
        }

        [DefaultValue(0)]
        public int Weight
        {
            get => _weight;
            set
            {
                VerifyInitializing();
                _weight = value;
                OnPropertyChanged("Weight");
            }
        }

        [DefaultValue(5)]
        public int MaxConcurrentMemories
        {
            get => _maxConcurrentMemories;
            set
            {
                VerifyInitializing();
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Value must be non-negative.");
                }

                _maxConcurrentMemories = value;
                OnPropertyChanged("MaxConcurrentMemories");
            }
        }
    }

    [Serializable]
    public sealed class RelationshipMemoryWeightCollection : InitOnlyKeyedCollection<MemoryType, RelationshipMemoryWeight>
    {
        public RelationshipMemoryWeightCollection()
            : base(w => w.MemoryType) { }
    }

    [Serializable]
    public sealed class DiplomacyProfileCollection : InitOnlyKeyedCollection<Civilization, DiplomacyProfile>
    {
        public DiplomacyProfileCollection()
            : base(o => o.Civilization) { }
    }

    [Serializable]
    public sealed class DiplomacyDatabase : SupportInitializeBase
    {
        private DiplomacyProfile _defaultProfile;
        private readonly DiplomacyProfileCollection _civilizationProfiles;

        public DiplomacyDatabase()
        {
            _civilizationProfiles = new DiplomacyProfileCollection();
        }

        public DiplomacyProfile DefaultProfile
        {
            get => _defaultProfile;
            set
            {
                VerifyInitializing();
                _defaultProfile = value;
                OnPropertyChanged("DefaultProfile");
            }
        }

        public DiplomacyProfileCollection CivilizationProfiles => _civilizationProfiles;

        public static DiplomacyDatabase Load()
        {
            GameContext gameContext = GameContext.Current;
            if (gameContext == null)
            {
                gameContext = GameContext.Create(GameOptionsManager.LoadDefaults(), false);
            }

            GameContext.PushThreadContext(gameContext);

            try
            {

                if (!ResourceManager.VfsService.TryGetFileInfo(new Uri("vfs:///Resources/Data/DiplomacyDatabase.xaml"), out IVirtualFileInfo fileInfo))
                {
                    return null;
                }

                if (!fileInfo.Exists)
                {
                    return null;
                }

                using (System.IO.Stream stream = fileInfo.OpenRead())
                {
                    return (DiplomacyDatabase)XamlServices.Load(stream);
                }
            }
            finally
            {
                _ = GameContext.PopThreadContext();
            }
        }
    }
}