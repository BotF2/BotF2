using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Supremacy.Scripting.Utility
{
    public static partial class TypeManager
    {
        public static class PredefinedAttributes
        {
            private static Type _dynamic;
            private static Type _extension;
            private static Type _paramArray;
            private static Type _defaultMember;
            private static Type _decimalConstant;
            private static Type _fixedBuffer;
            private static Type _clsCompliant;
            private static Type _obsolete;
            private static Type _conditional;

            public static Type Dynamic
            {
                get
                {
                    if (_dynamic == null)
                    {
                        _dynamic = typeof(DynamicAttribute);
                    }

                    return _dynamic;
                }
            }

            public static Type Extension
            {
                get
                {
                    if (_extension == null)
                    {
                        _extension = typeof(ExtensionAttribute);
                    }

                    return _extension;
                }
            }

            public static Type ParamArray
            {
                get
                {
                    if (_paramArray == null)
                    {
                        _paramArray = typeof(ParamArrayAttribute);
                    }

                    return _paramArray;
                }
            }

            public static Type DefaultMember
            {
                get
                {
                    if (_defaultMember == null)
                    {
                        _defaultMember = typeof(DefaultMemberAttribute);
                    }

                    return _defaultMember;
                }
            }

            public static Type DecimalConstant
            {
                get
                {
                    if (_decimalConstant == null)
                    {
                        _decimalConstant = typeof(DecimalConstantAttribute);
                    }

                    return _decimalConstant;
                }
            }

            public static Type FixedBuffer
            {
                get
                {
                    if (_fixedBuffer == null)
                    {
                        _fixedBuffer = typeof(FixedBufferAttribute);
                    }

                    return _fixedBuffer;
                }
            }

            public static Type CLSCompliant
            {
                get
                {
                    if (_clsCompliant == null)
                    {
                        _clsCompliant = typeof(CLSCompliantAttribute);
                    }

                    return _clsCompliant;
                }
            }

            public static Type Obsolete
            {
                get
                {
                    if (_obsolete == null)
                    {
                        _obsolete = typeof(ObsoleteAttribute);
                    }

                    return _obsolete;
                }
            }

            public static Type Conditional
            {
                get
                {
                    if (_conditional == null)
                    {
                        _conditional = typeof(ConditionalAttribute);
                    }

                    return _conditional;
                }
            }
        }
    }
}