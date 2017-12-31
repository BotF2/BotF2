using System;
using System.Windows.Markup;

namespace Supremacy.Client.Markup
{
    public static class KnownBoxes
    {
        public static readonly object True = true;
        public static readonly object False = false;
    }

    [MarkupExtensionReturnType(typeof(bool))]
    public class FalseExtension : MarkupExtension
    {
        #region Overrides of MarkupExtension

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return false;
        }

        #endregion
    }
}