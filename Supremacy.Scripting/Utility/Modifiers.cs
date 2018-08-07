using System;
using System.Reflection;

using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Utility
{
    public class Modifiers
    {
        //
        // The ordering of the following 4 constants
        // has been carefully done.
        //
        public const int Protected = 0x0001;
        public const int Public = 0x0002;
        public const int Private = 0x0004;
        public const int Internal = 0x0008;
        public const int New = 0x0010;
        public const int Abstract = 0x0020;
        public const int Sealed = 0x0040;
        public const int Static = 0x0080;
        public const int Readonly = 0x0100;
        public const int Virtual = 0x0200;
        public const int Override = 0x0400;
        public const int Extern = 0x0800;
        public const int Volatile = 0x1000;
        public const int Unsafe = 0x2000;
        private const int Top = 0x4000;

        public const int PropertyCustom = 0x4000; // Custom property modifier

        //
        // Compiler specific flags
        //
        public const int Partial = 0x20000;
        public const int DefaultAccessModifer = 0x40000;
        public const int MethodExtension = 0x80000;
        public const int CompilerGenerated = 0x100000;
        public const int BackingField = 0x200000;
        public const int DebuggerHidden = 0x400000;

        public const int Accessibility =
            Public | Protected | Internal | Private;

        public const int AllowedExplicitImplFlags =
            Unsafe | Extern;

        public static string Name(int i)
        {
            var s = "";

            switch (i)
            {
                case New:
                    s = "new";
                    break;
                case Public:
                    s = "public";
                    break;
                case Protected:
                    s = "protected";
                    break;
                case Internal:
                    s = "internal";
                    break;
                case Private:
                    s = "private";
                    break;
                case Abstract:
                    s = "abstract";
                    break;
                case Sealed:
                    s = "sealed";
                    break;
                case Static:
                    s = "static";
                    break;
                case Readonly:
                    s = "readonly";
                    break;
                case Virtual:
                    s = "virtual";
                    break;
                case Override:
                    s = "override";
                    break;
                case Extern:
                    s = "extern";
                    break;
                case Volatile:
                    s = "volatile";
                    break;
                case Unsafe:
                    s = "unsafe";
                    break;
            }

            return s;
        }

        public static string GetDescription(MethodAttributes ma)
        {
            ma &= MethodAttributes.MemberAccessMask;

            if (ma == MethodAttributes.Assembly)
                return "internal";

            if (ma == MethodAttributes.Family)
                return "protected";

            if (ma == MethodAttributes.Public)
                return "public";

            if (ma == MethodAttributes.FamORAssem)
                return "protected internal";

            if (ma == MethodAttributes.Private)
                return "private";

            throw new ArgumentOutOfRangeException("ma");
        }

        public static TypeAttributes TypeAttr(int modFlags, bool isToplevel)
        {
            TypeAttributes t = 0;

            if (isToplevel)
            {
                if ((modFlags & Public) != 0)
                    t = TypeAttributes.Public;
                else if ((modFlags & Private) != 0)
                    t = TypeAttributes.NotPublic;
            }
            else
            {
                if ((modFlags & Public) != 0)
                    t = TypeAttributes.NestedPublic;
                else if ((modFlags & Private) != 0)
                    t = TypeAttributes.NestedPrivate;
                else if ((modFlags & (Protected | Internal)) == (Protected | Internal))
                    t = TypeAttributes.NestedFamORAssem;
                else if ((modFlags & Protected) != 0)
                    t = TypeAttributes.NestedFamily;
                else if ((modFlags & Internal) != 0)
                    t = TypeAttributes.NestedAssembly;
            }

            if ((modFlags & Sealed) != 0)
                t |= TypeAttributes.Sealed;
            if ((modFlags & Abstract) != 0)
                t |= TypeAttributes.Abstract;

            return t;
        }

        public static FieldAttributes FieldAttr(int modFlags)
        {
            FieldAttributes fa = 0;

            if ((modFlags & Public) != 0)
                fa |= FieldAttributes.Public;
            if ((modFlags & Private) != 0)
                fa |= FieldAttributes.Private;
            if ((modFlags & Protected) != 0)
            {
                if ((modFlags & Internal) != 0)
                    fa |= FieldAttributes.FamORAssem;
                else
                    fa |= FieldAttributes.Family;
            }
            else
            {
                if ((modFlags & Internal) != 0)
                    fa |= FieldAttributes.Assembly;
            }

            if ((modFlags & Static) != 0)
                fa |= FieldAttributes.Static;
            if ((modFlags & Readonly) != 0)
                fa |= FieldAttributes.InitOnly;

            return fa;
        }

        public static MethodAttributes MethodAttr(int modFlags)
        {
            var ma = MethodAttributes.HideBySig;

            if ((modFlags & Public) != 0)
                ma |= MethodAttributes.Public;
            else if ((modFlags & Private) != 0)
                ma |= MethodAttributes.Private;
            else if ((modFlags & Protected) != 0)
            {
                if ((modFlags & Internal) != 0)
                    ma |= MethodAttributes.FamORAssem;
                else
                    ma |= MethodAttributes.Family;
            }
            else
            {
                if ((modFlags & Internal) != 0)
                    ma |= MethodAttributes.Assembly;
            }

            if ((modFlags & Static) != 0)
                ma |= MethodAttributes.Static;
            if ((modFlags & Abstract) != 0)
            {
                ma |= MethodAttributes.Abstract | MethodAttributes.Virtual;
            }
            if ((modFlags & Sealed) != 0)
                ma |= MethodAttributes.Final;

            if ((modFlags & Virtual) != 0)
                ma |= MethodAttributes.Virtual;

            if ((modFlags & Override) != 0)
                ma |= MethodAttributes.Virtual;
            else
            {
                if ((ma & MethodAttributes.Virtual) != 0)
                    ma |= MethodAttributes.NewSlot;
            }

            return ma;
        }

        // <summary>
        //   Checks the object @mod modifiers to be in @allowed.
        //   Returns the new mask.  Side effect: reports any
        //   incorrect attributes. 
        // </summary>
        public static int Check(int allowed, int mod, int defAccess, SourceSpan span, ParseContext parseContext)
        {
            var invalidFlags = (~allowed) & (mod & (Top - 1));
            int i;

            if (invalidFlags == 0)
            {
                var a = mod;

                //
                // If no accessibility bits provided
                // then provide the defaults.
                //
                if ((mod & Accessibility) == 0)
                {
                    mod |= defAccess;
                    if (defAccess != 0)
                        mod |= DefaultAccessModifer;
                    return mod;
                }

                //
                // Make sure that no conflicting accessibility
                // bits have been set.  Protected+Internal is
                // allowed, that is why they are placed on bits
                // 1 and 4 (so the shift 3 basically merges them)
                //
                a &= 15;
                a |= (a >> 3);
                a = ((a & 2) >> 1) + (a & 5);
                a = ((a & 4) >> 2) + (a & 3);

                if (a > 1)
                {
                    parseContext.ReportError(
                        107,
                        "More than one protection modifier specified",
                        Severity.Error,
                        span);
                }

                return mod;
            }

            for (i = 1; i <= Top; i <<= 1)
            {
                if ((i & invalidFlags) == 0)
                    continue;

                parseContext.ReportError(
                    106,
                    "The modifier '" + Name(i) + "' is not valid for this item.",
                    Severity.Error,
                    span);
            }

            return allowed & mod;
        }
    }
}