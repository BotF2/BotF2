using System;
using System.Windows.Markup;

using Supremacy.Annotations;

namespace Supremacy.Client.Markup
{
    [MarkupExtensionReturnType(typeof(Array))]
    public sealed class EnumValueCollectionExtension : MarkupExtension
    {
        private readonly Type _enumType;

        public EnumValueCollectionExtension([NotNull] Type enumType)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException("enumType");
            }

            if (!enumType.IsEnum)
            {
                throw new ArgumentException(string.Format("'{0}' is not an enum type.", enumType.FullName), "enumType");
            }

            _enumType = enumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Enum.GetValues(_enumType);
        }
    }
}