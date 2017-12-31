using System;
using System.Windows.Markup;

namespace Supremacy.Client.Markup
{
    [MarkupExtensionReturnType(typeof(bool))]
    public class TrueExtension : MarkupExtension
    {
        #region Overrides of MarkupExtension

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return true;
        }

        #endregion
    }
}