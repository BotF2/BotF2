using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

using Supremacy.Collections;
using Supremacy.Utility;

namespace Supremacy.Personnel
{
    public sealed class AgentSkillsConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return (destinationType == typeof(string));
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var stringValue = value as string;
            
            if (string.IsNullOrWhiteSpace(stringValue))
                return ArrayWrapper<AgentSkill>.Empty;

            // ReSharper disable PossibleNullReferenceException
            var parts = stringValue
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(o => o.Trim())
                .Select(
                    o =>
                    {
                        AgentSkill skill;
                        return new
                               {
                                   Success = EnumHelper.TryParse(o, out skill),
                                   Skill = skill
                               };
                    })
                .Where(o => o.Success)
                .Select(o => o.Skill)
                .ToArray();
            // ReSharper restore PossibleNullReferenceException

            return new ArrayWrapper<AgentSkill>(parts);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var skills = value as IEnumerable<AgentSkill>;
            if (skills == null)
                return null;

            var stringValue = string.Join(", ", skills);
            
            if (string.IsNullOrWhiteSpace(stringValue))
                return null;

            return stringValue;
        }
    }
}