using System;
using System.Windows.Markup;

using Supremacy.Text;

namespace Supremacy.Markup
{
    [MarkupExtensionReturnType(typeof(ContextualTextEntryKey))]
    public sealed class ContextualTextEntryExtension : MarkupExtension
    {
        public object BaseKey { get; set; }
        public object Context { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var baseKey = BaseKey;
            var context = Context;

            if (baseKey == null || context == null)
                throw new InvalidOperationException("BaseKey and Context must both be set on a ContextualEnumTextGroupExtension.");

            return new ContextualTextEntryKey(context, baseKey);
        }
    }
}