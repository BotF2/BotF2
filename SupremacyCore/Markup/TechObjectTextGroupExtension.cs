using System;
using System.Windows.Markup;

using Supremacy.Tech;

namespace Supremacy.Markup
{
    [MarkupExtensionReturnType(typeof(TechObjectTextGroupKey))]
    public class TechObjectTextGroupExtension : MarkupExtension
    {
        public string DesignKey { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            string designKey = DesignKey;
            if (designKey == null)
            {
                throw new InvalidOperationException("DesignKey must be set.");
            }

            return new TechObjectTextGroupKey(designKey);
        }
    }
}