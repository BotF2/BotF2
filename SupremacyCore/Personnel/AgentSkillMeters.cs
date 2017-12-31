using System;
using System.Collections;
using System.Collections.Generic;

using System.Collections.Specialized;

using System.Linq;

using Supremacy.Annotations;

using Supremacy.Collections;
using Supremacy.IO.Serialization;
using Supremacy.Utility;

namespace Supremacy.Personnel
{
    [Serializable]
    public class AgentSkillMeters : IIndexedKeyedCollection<AgentSkill, AgentSkillMeter>, IOwnedDataSerializable
    {
        private AgentSkillMeter _primaryMeter;
        private AgentSkillMeter _secondaryMeter;
        private AgentSkillMeter _tertiaryMeter;

        public AgentSkillMeters(AgentSkill primarySkill, AgentSkill secondarySkill, AgentSkill tertiarySkill)
        {
            _primaryMeter = new AgentSkillMeter(primarySkill);
            _secondaryMeter = new AgentSkillMeter(secondarySkill);
            _tertiaryMeter = new AgentSkillMeter(tertiarySkill);
        }

        public AgentSkillMeters([NotNull] AgentSkillMeter primarySkillMeter, [NotNull] AgentSkillMeter secondarySkillMeter, [NotNull] AgentSkillMeter tertiarySkillMeter)
        {
            if (primarySkillMeter == null)
                throw new ArgumentNullException("primarySkillMeter");
            if (secondarySkillMeter == null)
                throw new ArgumentNullException("secondarySkillMeter");
            if (tertiarySkillMeter == null)
                throw new ArgumentNullException("tertiarySkillMeter");

            _primaryMeter = primarySkillMeter;
            _secondaryMeter = secondarySkillMeter;
            _tertiaryMeter = tertiarySkillMeter;
        }

        public IEnumerator<AgentSkillMeter> GetEnumerator()
        {
            yield return _primaryMeter;
            yield return _secondaryMeter;
            yield return _tertiaryMeter;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public AgentSkillMeter this[AgentSkill key]
        {
            get
            {
                if (key == _primaryMeter.Skill)
                    return _primaryMeter;
                if (key == _secondaryMeter.Skill)
                    return _secondaryMeter;                
                if (key == _tertiaryMeter.Skill)
                    return _tertiaryMeter;
                return null;
            }
        }

        public bool Contains(AgentSkill key)
        {
            return key == _primaryMeter.Skill ||
                   key == _secondaryMeter.Skill ||
                   key == _tertiaryMeter.Skill;
        }

        public IEqualityComparer<AgentSkill> KeyComparer
        {
            get { return EqualityComparer<AgentSkill>.Default; }
        }

        public IEnumerable<AgentSkill> Keys
        {
            get { return new[] { _primaryMeter.Skill, _secondaryMeter.Skill, _tertiaryMeter.Skill }; }
        }

        public bool MeetsOrExceeds(AgentSkillRatingsSnapshot ratings)
        {
            if (ratings == null)
                throw new ArgumentNullException("ratings");

            return EnumHelper
                .GetValues<AgentSkill>()
                .All(
                    skill =>
                    {
                        AgentSkillMeter meter;

                        var requiredLevel = ratings[skill];

                        return requiredLevel == 0 ||
                               TryGetValue(skill, out meter) && meter.CurrentValue >= requiredLevel;
                    });
        }

        public void Add(AgentSkillRatingsSnapshot ratings)
        {
            if (ratings == null)
                throw new ArgumentNullException("ratings");

            EnumHelper
                .GetValues<AgentSkill>()
                .ForEach(
                    skill =>
                    {
                        AgentSkillMeter meter;

                        if (TryGetValue(skill, out meter))
                            meter.AdjustCurrent(ratings[skill]);
                    });
        }

        public void UpdateAndReset()
        {
            _primaryMeter.UpdateAndReset();
            _secondaryMeter.UpdateAndReset();
            _tertiaryMeter.UpdateAndReset();
        }

        public bool TryGetValue(AgentSkill key, out AgentSkillMeter value)
        {
            if (key == _primaryMeter.Skill)
            {
                value = _primaryMeter;
                return true;
            }
            
            if (key == _secondaryMeter.Skill)
            {
                value = _secondaryMeter;
                return true;
            }
            
            if (key == _tertiaryMeter.Skill)
            {
                value = _tertiaryMeter;
                return true;
            }

            value = null;
            return false;
        }

        public void DeserializeOwnedData(SerializationReader reader, object context)
        {
            if (_primaryMeter == null)
                _primaryMeter = new AgentSkillMeter(default(AgentSkill));
            if (_secondaryMeter == null)
                _secondaryMeter = new AgentSkillMeter(default(AgentSkill));
            if (_tertiaryMeter == null)
                _tertiaryMeter = new AgentSkillMeter(default(AgentSkill));

            _primaryMeter.DeserializeOwnedData(reader, context);
            _secondaryMeter.DeserializeOwnedData(reader, context);
            _tertiaryMeter.DeserializeOwnedData(reader, context);
        }

        public void SerializeOwnedData(SerializationWriter writer, object context)
        {
            _primaryMeter.SerializeOwnedData(writer, context);
            _secondaryMeter.SerializeOwnedData(writer, context);
            _tertiaryMeter.SerializeOwnedData(writer, context);
        }

        public int Count
        {
            get { return 3; }
        }

        public AgentSkillMeter this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _primaryMeter;
                    case 1:
                        return _secondaryMeter;
                    case 2:
                        return _tertiaryMeter;
                    default:
                        throw new ArgumentOutOfRangeException("index");
                }
            }
        }

        bool IIndexedCollection<AgentSkillMeter>.Contains(AgentSkillMeter value)
        {
            return value == _primaryMeter ||
                   value == _secondaryMeter ||
                   value == _tertiaryMeter;
        }

        int IIndexedCollection<AgentSkillMeter>.IndexOf(AgentSkillMeter value)
        {
            if (value == _primaryMeter)
                return 0;
            if (value == _secondaryMeter)
                return 1;
            if (value == _tertiaryMeter)
                return 2;
            return -1;
        }

        event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
        {
            add {}
            remove {}
        }
    }
}