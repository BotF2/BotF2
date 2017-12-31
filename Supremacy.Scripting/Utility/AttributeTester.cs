using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

using Microsoft.Scripting;

using System.Linq;

using Supremacy.Scripting.Ast;
using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Utility
{
    /// <summary>
    /// Helper class for attribute verification routine.
    /// </summary>
    internal static class AttributeTester
    {
        private static PtrHashtable _analyzedTypes;
        private static PtrHashtable _analyzedTypesObsolete;
        private static PtrHashtable _analyzedMemberObsolete;
        private static PtrHashtable _analyzedMethodExcluded;
        private static PtrHashtable _fixedBufferCache;

        static readonly object True = new object();
        static readonly object False = new object();

        static AttributeTester()
        {
            Reset();
        }

        public static void Reset()
        {
            _analyzedTypes = new PtrHashtable();
            _analyzedTypesObsolete = new PtrHashtable();
            _analyzedMemberObsolete = new PtrHashtable();
            _analyzedMethodExcluded = new PtrHashtable();
            _fixedBufferCache = new PtrHashtable();
        }

        public enum Result
        {
            Ok,
            RefOutArrayError,
            ArrayArrayError
        }

        /// <summary>
        /// Returns true if parameters of two compared methods are CLS-Compliant.
        /// It tests differing only in ref or out, or in array rank.
        /// </summary>
        public static Result AreOverloadedMethodParamsClsCompliant(ParametersCollection pa, ParametersCollection pb)
        {
            var typesA = pa.Types;
            var typesB = pb.Types;

            if (typesA == null || typesB == null)
                return Result.Ok;

            if (typesA.Length != typesB.Length)
                return Result.Ok;

            var result = Result.Ok;

            for (var i = 0; i < typesB.Length; ++i)
            {
                var aType = typesA[i];
                var bType = typesB[i];

                if (aType.IsArray && bType.IsArray)
                {
                    var elementTypeA = aType.GetElementType();
                    var elementTypeB = bType.GetElementType();

                    if (aType.GetArrayRank() != bType.GetArrayRank() && elementTypeA == elementTypeB)
                    {
                        result = Result.RefOutArrayError;
                        continue;
                    }

                    if (elementTypeA.IsArray || elementTypeB.IsArray)
                    {
                        result = Result.ArrayArrayError;
                        continue;
                    }
                }

                if (aType != bType)
                    return Result.Ok;

                const Parameter.Modifier outRefMod = (Parameter.Modifier.OutMask | Parameter.Modifier.RefMask);

                if ((pa.FixedParameters[i].ModifierFlags & outRefMod) != (pb.FixedParameters[i].ModifierFlags & outRefMod))
                    result = Result.RefOutArrayError;
            }
            return result;
        }

        /// <summary>
        /// This method tests the CLS compliance of external types. It doesn't test type visibility.
        /// </summary>
        public static bool IsClsCompliant(Type type)
        {
            if (type == null)
                return true;

            var typeCompliance = _analyzedTypes[type];
            if (typeCompliance != null)
                return typeCompliance == True;

            if (type.IsPointer)
            {
                _analyzedTypes.Add(type, False);
                return false;
            }

            bool result;
            if (type.IsArray)
            {
                result = IsClsCompliant(type);
            }
            else if (TypeManager.IsNullableType(type))
            {
                result = IsClsCompliant(type.GetGenericArguments()[0]);
            }
            else
            {
                result = AnalyzeTypeCompliance(type);
            }
            _analyzedTypes.Add(type, result ? True : False);
            return result;
        }

        /// <summary>
        /// Returns IFixedBuffer implementation if field is fixed buffer else null.
        /// </summary>
        public static IFixedBuffer GetFixedBuffer(FieldInfo fi)
        {
            // Fixed buffer helper type is generated as value type
            if (TypeManager.IsReferenceType(fi.FieldType))
                return null;

            if (TypeManager.GetConstant(fi) != null)
                return null;

            var o = _fixedBufferCache[fi];
            if (o == null)
            {
                var fixedBufferAttribute = TypeManager.PredefinedAttributes.FixedBuffer;

                if (!fi.IsDefined(fixedBufferAttribute, false))
                {
                    _fixedBufferCache.Add(fi, False);
                    return null;
                }

                IFixedBuffer iff = new FixedFieldExternal(fi);
                _fixedBufferCache.Add(fi, iff);
                return iff;
            }

            if (o == False)
                return null;

            return (IFixedBuffer)o;
        }

        static bool GetClsCompliantAttributeValue(ICustomAttributeProvider attributeProvider, Assembly assembly)
        {
            var clsCompliantAttribute = attributeProvider
                .GetCustomAttributes(TypeManager.PredefinedAttributes.CLSCompliant, false)
                .Cast<CLSCompliantAttribute>()
                .FirstOrDefault();

            if (clsCompliantAttribute == null)
                return GetClsCompliantAttributeValue(assembly, null);

            return clsCompliantAttribute.IsCompliant;
        }

        static bool AnalyzeTypeCompliance(Type type)
        {
            type = TypeManager.DropGenericTypeArguments(type);
        
            if (TypeManager.IsGenericParameter(type))
                return true;

            return GetClsCompliantAttributeValue(type, type.Assembly);
        }

        /// <summary>
        /// Returns instance of ObsoleteAttribute when type is obsolete
        /// </summary>
        public static ObsoleteAttribute GetObsoleteAttribute(Type type)
        {
            var typeObsolete = _analyzedTypesObsolete[type];
            if (typeObsolete == False)
                return null;

            if (typeObsolete != null)
                return (ObsoleteAttribute)typeObsolete;

            ObsoleteAttribute result = null;
            if (TypeManager.HasElementType(type))
            {
                result = GetObsoleteAttribute(type.GetElementType());
            }
            else if (TypeManager.IsGenericParameter(type))
            {
                throw new NotSupportedException("The 'Obsolete' attribute cannot be applied to a generic type parameter.s");
            }
            else if (TypeManager.IsGenericType(type) && !TypeManager.IsGenericTypeDefinition(type))
            {
                return GetObsoleteAttribute(TypeManager.DropGenericTypeArguments(type));
            }
            else
            {
                var obsoleteAttribute = type
                    .GetCustomAttributes(TypeManager.PredefinedAttributes.Obsolete, false)
                    .Cast<ObsoleteAttribute>()
                    .FirstOrDefault();

                if (obsoleteAttribute != null)
                    result = obsoleteAttribute;
            }

            // Cannot use .Add because of corlib bootstrap
            _analyzedTypesObsolete[type] = result ?? False;
            return result;
        }

        /// <summary>
        /// Returns instance of ObsoleteAttribute when method is obsolete
        /// </summary>
        public static ObsoleteAttribute GetMethodObsoleteAttribute(MethodBase mb)
        {
            // compiler generated methods are not registered by AddMethod
            if (mb.DeclaringType is TypeBuilder)
                return null;

            var memberInfo = (MemberInfo)TypeManager.GetPropertyFromAccessor(mb);

            if (memberInfo != null)
                return GetMemberObsoleteAttribute(memberInfo);

            memberInfo = TypeManager.GetEventFromAccessor(mb);

            if (memberInfo != null)
                return GetMemberObsoleteAttribute(memberInfo);

            return GetMemberObsoleteAttribute(mb);
        }

        /// <summary>
        /// Returns instance of ObsoleteAttribute when member is obsolete
        /// </summary>
        public static ObsoleteAttribute GetMemberObsoleteAttribute(MemberInfo mi)
        {
            var typeObsolete = _analyzedMemberObsolete[mi];
            if (typeObsolete == False)
                return null;

            if (typeObsolete != null)
                return (ObsoleteAttribute)typeObsolete;

            if ((mi.DeclaringType is TypeBuilder) || TypeManager.IsGenericType(mi.DeclaringType))
                return null;

            var obsoleteAttribute = Attribute.GetCustomAttribute(
                mi, 
                TypeManager.PredefinedAttributes.Obsolete,
                false) as ObsoleteAttribute;

            _analyzedMemberObsolete.Add(mi, obsoleteAttribute ?? False);

            return obsoleteAttribute;
        }

        /// <summary>
        /// Common method for Obsolete error/warning reporting.
        /// </summary>
        public static void ReportObsoleteMessage(ParseContext parseContext, ObsoleteAttribute oa, string member, SourceSpan loc)
        {
            if (oa.IsError)
            {
                parseContext.ReportError(
                    619,
                    string.Format("'{0}' is obsolete: '{1}'.", member, oa.Message),
                    loc);

                return;
            }

            if (string.IsNullOrEmpty(oa.Message))
            {
                parseContext.ReportError(
                    612,
                    string.Format("'{0}' is obsolete.", member),
                    Severity.Warning,
                    loc);

                return;
            }

            parseContext.ReportError(
                612,
                string.Format("'{0}' is obsolete: '{1}'.", member, oa.Message),
                Severity.Warning,
                loc);
        }

        public static bool IsConditionalMethodExcluded(ParseContext parseContext, MethodBase mb, SourceSpan loc)
        {
            var excluded = _analyzedMethodExcluded[mb];
            if (excluded != null)
                return excluded == True ? true : false;

            var conditionalAttributes = mb
                .GetCustomAttributes(TypeManager.PredefinedAttributes.Conditional, true)
                .Cast<ConditionalAttribute>();

            if (!conditionalAttributes.Any())
            {
                _analyzedMethodExcluded.Add(mb, False);
                return false;
            }

            if (conditionalAttributes.Any(a => parseContext.LanguageContext.IsConditionalDefined(a.ConditionString)))
            {
                _analyzedMethodExcluded.Add(mb, False);
                return false;
            }

            _analyzedMethodExcluded.Add(mb, True);
            return true;
        }

        /// <summary>
        /// Analyzes class whether it has attribute which has ConditionalAttribute
        /// and its condition is not defined.
        /// </summary>
        public static bool IsAttributeExcluded(ParseContext parseContext, Type type, SourceSpan loc)
        {
            if (!type.IsClass)
                return false;

            var attributes = type
                .GetCustomAttributes(TypeManager.PredefinedAttributes.Conditional, false)
                .Cast<ConditionalAttribute>();

            if (attributes.Any(ca => parseContext.LanguageContext.IsConditionalDefined(ca.ConditionString)))
                return false;
            
            return attributes.Any();
        }
    }

    internal interface IFixedBuffer
    {
        FieldInfo Element { get; }
        Type ElementType { get; }
    }

    internal class FixedFieldExternal : IFixedBuffer
    {
        public const string FixedElementName = "FixedElementField";

        private readonly FieldInfo _elementField;

        public FixedFieldExternal(FieldInfo fi)
        {
            _elementField = fi.FieldType.GetField(FixedElementName);
        }

        #region IFixedField Members

        public FieldInfo Element
        {
            get
            {
                return _elementField;
            }
        }

        public Type ElementType
        {
            get
            {
                return _elementField.FieldType;
            }
        }

        #endregion
    }
}