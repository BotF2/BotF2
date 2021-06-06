using System;
using System.Windows.Markup;

using Supremacy.Text;

namespace Supremacy.Markup
{
    [MarkupExtensionReturnType(typeof(string))]
    public sealed class EnumStringExtension : MarkupExtension
    {
        private readonly object _enumValue;

        public EnumStringExtension(object enumValue)
        {
            if (!(enumValue is Enum))
                throw new ArgumentException("Value must be a valid Enum type.");
            _enumValue = enumValue;
        }

        #region Overrides of MarkupExtension
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            LocalizedTextDatabase textDatabase = LocalizedTextDatabase.Instance;

            LocalizedTextGroup group;

            if (!textDatabase.Groups.TryGetValue(_enumValue.GetType(), out group))
                return string.Format("{{! Unknown Text Group: {0} !}}", group);

            LocalizedString value;

            string entryName = _enumValue.ToString();

            if (!group.Entries.TryGetValue(entryName, out value))
                return entryName;

            return value.LocalText;
        }
        #endregion
    }
}