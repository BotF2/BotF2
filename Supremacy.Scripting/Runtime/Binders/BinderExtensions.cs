using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;

namespace Supremacy.Scripting.Runtime.Binders
{
    internal static class ScriptExtensions
    {
        internal static ConversionResultKind? ToConversionResultKind(this NarrowingLevel narrowingLevel)
        {
            switch (narrowingLevel)
            {
                case NarrowingLevel.None:
                    return null;

                case NarrowingLevel.One:
                    return ConversionResultKind.ImplicitCast;

                case NarrowingLevel.Two:
                    return ConversionResultKind.ImplicitTry;

                case NarrowingLevel.Three:
                    return ConversionResultKind.ExplicitCast;

                case NarrowingLevel.All:
                default:
                    return ConversionResultKind.ExplicitTry;
            }
        }
    }
}