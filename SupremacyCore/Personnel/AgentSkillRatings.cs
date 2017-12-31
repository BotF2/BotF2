using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Supremacy.Collections;
using Supremacy.IO.Serialization;
using Supremacy.Types;
using Supremacy.Utility;

namespace Supremacy.Personnel
{
    [Serializable]
    public class AgentSkillRatings : IKeyedCollection<AgentSkill, Meter>, IOwnedDataSerializable
    {
        private readonly Meter[] _meters;

        public AgentSkillRatings()
        {
            var experienceCategories = EnumHelper.GetValues<AgentSkill>();
            var meters = new Meter[experienceCategories.Length];

            foreach (var agentExperienceCategory in experienceCategories)
                meters[(int)agentExperienceCategory] = new Meter(0, 255);

            _meters = meters;
        }

        public IEnumerator<Meter> GetEnumerator()
        {
            return ((IEnumerable<Meter>)_meters).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public Meter this[AgentSkill key]
        {
            get { return _meters[(int)key]; }
        }

        public bool Contains(AgentSkill key)
        {
            return EnumHelper.IsDefined(key);
        }

        public IEqualityComparer<AgentSkill> KeyComparer
        {
            get { return EqualityComparer<AgentSkill>.Default; }
        }

        public IEnumerable<AgentSkill> Keys
        {
            get { return EnumHelper.GetValues<AgentSkill>(); }
        }

        public bool MeetsOrExceeds(AgentSkillRatingsSnapshot categories)
        {
            if (categories == null)
                throw new ArgumentNullException("categories");

            return EnumHelper
                .GetValues<AgentSkill>()
                .All(category => this[category].CurrentValue >= categories[category]);
        }

        public void Add(AgentSkillRatingsSnapshot ratings)
        {
            if (ratings == null)
                throw new ArgumentNullException("ratings");

            EnumHelper
                .GetValues<AgentSkill>()
                .ForEach(o => _meters[(int)o].AdjustCurrent(ratings[o]));
        }

        public void UpdateAndReset()
        {
            _meters.ForEach(o => o.UpdateAndReset());
        }

        public bool TryGetValue(AgentSkill key, out Meter value)
        {
            if (EnumHelper.IsDefined(key))
            {
                value = _meters[(int)key];
                return true;
            }

            value = null;
            return false;
        }

        public void DeserializeOwnedData(SerializationReader reader, object context)
        {
            var meters = reader.ReadArray<Meter>();
            meters.CopyTo(_meters, 0);
        }

        public void SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.WriteArray(_meters);
        }
    }
}