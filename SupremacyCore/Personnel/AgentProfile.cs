using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Markup;

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.IO.Serialization;
using Supremacy.Text;
using Supremacy.Types;
using Supremacy.Utility;

using System.Linq;

namespace Supremacy.Personnel
{
    public enum AgentGender
    {
        Male,
        Female
    }

    [Serializable]
    [DictionaryKeyProperty("Name")]
    public class AgentProfile : SupportInitializeBase, IOwnedDataSerializableAndRecreatable
    {
        private const string ImageUriFormat = "vfs:///Resources/Images/Agents/{0}";

        private string _name;
        private string _image;
        private int _minTechLevelBeforeAppearance;
        private int _maxTechLevelBeforeAppearance;
        private IIndexedCollection<AgentSkill> _naturalSkills;
        private LocalizedString _displayName;
        private AgentGender _gender;
        private string _raceId;

        [DefaultValue(null)]
        [TypeConverter(typeof(RaceConverter))]
        public Race Race
        {
            get
            {
                if (_raceId == null)
                    return null;
                return GameContext.Current.Races[_raceId];
            }
            set
            {
                VerifyInitializing();
                _raceId = (value != null) ? value.Key : null;
                OnPropertyChanged("Race");
            }
        }

        [DefaultValue(null)]
        public string Name
        {
            get { return _name; }
            set
            {
                VerifyInitializing();
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        [DefaultValue(AgentGender.Male)]
        public AgentGender Gender
        {
            get { return _gender; }
            set
            {
                VerifyInitializing();
                _gender = value;
                OnPropertyChanged("Gender");
            }
        }

        [DefaultValue(null)]
        public string Image
        {
            get { return string.Format(ImageUriFormat, _image); }
            set
            {
                VerifyInitializing();
                _image = value;
                OnPropertyChanged("Image");
            }
        }

        [DefaultValue(null)]
        public LocalizedString DisplayName
        {
            get { return _displayName; }
            set
            {
                VerifyInitializing();
                _displayName = value;
                OnPropertyChanged("DisplayName");
            }
        }

        [NotNull]
        [TypeConverter(typeof(AgentSkillsConverter))]
        public IIndexedCollection<AgentSkill> NaturalSkills
        {
            get
            {
                if (_naturalSkills == null)
                    _naturalSkills = ArrayWrapper<AgentSkill>.Empty;

                return _naturalSkills;
            }
            set
            {
                VerifyInitializing();

                if (value == null)
                    throw new ArgumentNullException("value");

                _naturalSkills = value;

                OnPropertyChanged("NaturalSkills");
            }
        }

        [DefaultValue(0)]
        public int MinTechLevelBeforeAppearance
        {
            get { return _minTechLevelBeforeAppearance; }
            set
            {
                VerifyInitializing();

                if (value < 0)
                    value = 0;
               
                _minTechLevelBeforeAppearance = value;

                OnPropertyChanged("MinTechLevelBeforeAppearance");
            }
        }

        [DefaultValue(0)]
        public int MaxTechLevelBeforeAppearance
        {
            get { return _maxTechLevelBeforeAppearance; }
            set
            {
                VerifyInitializing();

                if (value < 0)
                    value = 0;

                _maxTechLevelBeforeAppearance = value;

                OnPropertyChanged("MaxTechLevelBeforeAppearance");
            }
        }

        protected override void EndInitCore()
        {
            base.EndInitCore();

            if (_maxTechLevelBeforeAppearance < _minTechLevelBeforeAppearance)
            {
                GameLog.Client.GameData.WarnFormat(
                    "Agent \'{0}\' has a MaxTechLevelBeforeAppearance lower than the " +
                    "MinTechLevelBeforeAppearance.  Setting MaxTechLevelBeforeAppearance" +
                    " to '{1}' instead.",
                    Name,
                    _minTechLevelBeforeAppearance);

                _maxTechLevelBeforeAppearance = _minTechLevelBeforeAppearance;
            }

            var naturalSkillsPerAgent = PersonnelConstants.Instance.NaturalSkillsPerAgent;
            var assignedSkills = new List<AgentSkill>(naturalSkillsPerAgent);

            if (_naturalSkills != null)
            {
                assignedSkills.AddRange(_naturalSkills);
                assignedSkills.RemoveDuplicatesInPlace();
            }
            
            if (assignedSkills.Count > naturalSkillsPerAgent)
            {
                var excessSkills = assignedSkills.Range(
                    naturalSkillsPerAgent,
                    assignedSkills.Count - naturalSkillsPerAgent);

                GameLog.Client.GameData.WarnFormat(
                    "Agent '{0}' has too many natural skills assigned.  The following " +
                    "skills will be removed: {1}.",
                    _name,
                    string.Join(", ", excessSkills));

                while (assignedSkills.Count > naturalSkillsPerAgent)
                    assignedSkills.RemoveAt(naturalSkillsPerAgent);
            }
            else if (assignedSkills.Count < naturalSkillsPerAgent)
            {
                var randomizedSkills = EnumHelper.GetRandomizedValues<AgentSkill>(RandomProvider.Shared);
                var availableSkills = new List<AgentSkill>(randomizedSkills);

                availableSkills.RemoveRange(assignedSkills);

                var addedSkills = availableSkills.Range(
                    0,
                    naturalSkillsPerAgent - assignedSkills.Count);

                if (GameLog.Client.GameData.IsDebugEnabled)
                {
                    GameLog.Client.GameData.DebugFormat(
                        "Agent '{0}' doesn't have enough natural skills assigned.  " +
                        "The following skills will be added: {1}.",
                        _name,
                        string.Join(", ", addedSkills));
                }

                assignedSkills.AddRange(addedSkills);
            }

            _naturalSkills = new ArrayWrapper<AgentSkill>(assignedSkills.ToArray());
        }

        void IOwnedDataSerializable.DeserializeOwnedData(SerializationReader reader, object context)
        {
            
            _name = reader.ReadString();
            _displayName = reader.Read<LocalizedString>();
            _image = reader.ReadString();
            _gender = (AgentGender)reader.ReadByte();
            _raceId = reader.ReadString();
            _naturalSkills = new ArrayWrapper<AgentSkill>(reader.ReadArray<AgentSkill>());
            _minTechLevelBeforeAppearance = reader.ReadOptimizedInt32();
            _maxTechLevelBeforeAppearance = reader.ReadOptimizedInt32();

            if (_name == "Archer")
            GameLog.Client.GameData.DebugFormat("AgentProfile.cs: DeserializeOwnedData for {0}", _name);
        }

        void IOwnedDataSerializable.SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.Write(_name);
            writer.WriteObject(_displayName);
            writer.Write(_image);
            writer.Write((byte)_gender);
            writer.Write(_raceId);
            writer.WriteArray(_naturalSkills.ToArray());
            writer.WriteOptimized(_minTechLevelBeforeAppearance);
            writer.WriteOptimized(_maxTechLevelBeforeAppearance);
        }

        [NotNull]
        public Agent Spawn([NotNull] Civilization owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            return new Agent(this, owner);
        }
    }
}