using System;

using Supremacy.IO.Serialization;

using Supremacy.Types;

namespace Supremacy.Personnel
{
    [Serializable]
    public class AgentSkillMeter : Meter
    {
        private AgentSkill _skill;

        public AgentSkillMeter(AgentSkill skill)
            : base(0, 100)
        {
            _skill = skill;
        }

        public AgentSkill Skill
        {
            get { return _skill; }
        }

        protected override Cloneable CreateInstance(ICloneContext context)
        {
            return new AgentSkillMeter(_skill);
        }

        public override void SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.WriteOptimized((int)_skill);
            base.SerializeOwnedData(writer, context);
        }

        public override void DeserializeOwnedData(SerializationReader reader, object context)
        {
            _skill = (AgentSkill)reader.ReadOptimizedInt32();
            base.DeserializeOwnedData(reader, context);
        }
    }
}