using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    public interface IParameterData
    {
        Expression DefaultValue { get; }
        bool HasExtensionMethodModifier { get; }
        bool HasDefaultValue { get; }
        bool IsByRef { get; }
        string Name { get; }
        Parameter.Modifier ModifierFlags { get; }
    }

    public class Parameter : Ast, IParameterData
    {
        #region Modifier Enumeration
        [Flags]
        public enum Modifier : byte
        {
            None = 0,
            Ref = RefMask | IsByRef,
            Out = OutMask | IsByRef,
            Params = 4,
            // This is a flag which says that it's either REF or OUT.
            IsByRef = 8,
            RefMask = 32,
            OutMask = 64,
            This = 128
        }
        #endregion

        private readonly Modifier _modifierFlags;

        public Parameter(string name, Scope scope, SourceSpan span, Modifier modifierFlags = Modifier.None)
        {
            Name = name;
            Scope = scope;
            Span = span;

            _modifierFlags = modifierFlags;
        }

        public FullNamedExpression TypeName { get; set; }

        public Type ParameterType
        {
            get => ParameterTypeInternal ?? TypeManager.CoreTypes.Object;
            set => ParameterTypeInternal = value;
        }

        internal Type ParameterTypeInternal { get; set; }

        public int Index { get; private set; }

        public virtual bool IsParamsArray => false;

        public Scope Scope { get; set; }

        #region IParameterData Members
        public Expression DefaultValue { get; set; }

        public bool HasExtensionMethodModifier => (_modifierFlags & Modifier.This) == Modifier.This;

        public bool HasDefaultValue { get; set; }
        public string Name { get; set; }

        public bool IsByRef => ParameterType.IsByRef;

        public Modifier ModifierFlags => _modifierFlags & ~Modifier.This;
        #endregion

        public static string GetModifierSignature(Modifier mod)
        {
            switch (mod)
            {
                case Modifier.Out:
                    return "out";
                case Modifier.Params:
                    return "params";
                case Modifier.Ref:
                    return "ref";
                case Modifier.This:
                    return "this";
                default:
                    return string.Empty;
            }
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            if (!(target is Parameter targetParameter))
            {
                return;
            }

            targetParameter.Scope = cloneContext.LookupBlock(Scope);
        }

        public void ResolveVariable(int idx)
        {
            Index = idx;
        }

        public ParameterExpression Transform(ScriptGenerator generator)
        {
            return generator.Scope.GetOrMakeLocal(Name, ParameterType ?? TypeManager.CoreTypes.Object);
        }

        public Parameter Clone()
        {
            return (Parameter)MemberwiseClone();
        }

        public virtual Type Resolve(ParseContext rc)
        {
            if (ParameterType != null)
            {
                return ParameterType;
            }

            TypeExpression texpr = TypeName.ResolveAsTypeTerminal(rc, false);
            if (texpr == null)
            {
                return null;
            }

            ParameterType = texpr.Type;

            // Ignore all checks for dummy members
            if (DefaultValue != null)
            {
                DefaultValue = DefaultValue.Resolve(rc);
                if (DefaultValue != null)
                {
                    if (!(DefaultValue is ConstantExpression value))
                    {
                        if (DefaultValue != null)
                        {
                            bool isValid;
                            if (DefaultValue is DefaultValueExpression)
                            {
                                isValid = true;
                            }
                            else if (DefaultValue is NewExpression && ((NewExpression)DefaultValue).IsDefaultValueType)
                            {
                                isValid = TypeManager.IsEqual(ParameterType, DefaultValue.Type) ||
                                          (TypeManager.IsNullableType(ParameterType) &&
                                           ConvertExpression.ImplicitNullableConversion(
                                               rc, DefaultValue, ParameterType) != EmptyExpression.Null);
                            }
                            else
                            {
                                rc.ReportError(
                                    1736,
                                    string.Format(
                                        "The expression being assigned to optional parameter '{0}' must be a constant or default value.",
                                        Name),
                                    DefaultValue.Span);
                                isValid = true;
                            }

                            if (!isValid)
                            {
                                DefaultValue = null;
                                rc.ReportError(
                                    1763,
                                    string.Format(
                                        "Optional parameter '{0}' of type `{1}' can only be initialized with 'null'.",
                                        Name,
                                        GetSignatureForError()),
                                    Span);
                            }
                        }
                    }
                    else
                    {
                        ConstantExpression c = value.ConvertImplicitly(ParameterType);
                        if (c == null)
                        {
                            if (ParameterType == TypeManager.CoreTypes.Object)
                            {
                                rc.ReportError(
                                    1763,
                                    string.Format(
                                        "Optional parameter '{0}' of type '{1}' can only be initialized with 'null'.",
                                        Name,
                                        GetSignatureForError()),
                                    Span);
                            }
                            else
                            {
                                rc.ReportError(
                                    1750,
                                    string.Format(
                                        "Optional parameter value '{0}' cannot be converted to parameter type '{1}'.",
                                        value.Value,
                                        GetSignatureForError()),
                                    Span);
                            }
                            DefaultValue = null;
                        }
                    }
                }
            }

            if ((_modifierFlags & Modifier.IsByRef) != 0 &&
                TypeManager.IsSpecialType(ParameterType))
            {
                rc.ReportError(
                    1601,
                    string.Format(
                        "Method or delegate parameter cannot be of type '{0}'.",
                        GetSignatureForError()),
                    Span);
                return null;
            }

            _ = TypeManager.CheckTypeVariance(
                ParameterType,
                (_modifierFlags & Modifier.IsByRef) != 0 ? Variance.None : Variance.Contravariant,
                rc);

            if (TypeManager.IsGenericParameter(ParameterType))
            {
                return ParameterType;
            }

            if ((_modifierFlags & Modifier.This) != 0 &&
                (ParameterType.IsPointer || TypeManager.IsDynamicType(ParameterType)))
            {
                rc.ReportError(
                    1103,
                    string.Format(
                        "The extension method cannot be of type '{0}'.",
                        TypeManager.GetCSharpName(ParameterType)),
                    Span);
            }

            return ParameterType;
        }

        public virtual string GetSignatureForError()
        {
            string typeName = (ParameterType != null)
                               ? TypeManager.GetCSharpName(ParameterType)
                               : TypeName.GetSignatureForError();

            string mod = GetModifierSignature(_modifierFlags);
            return mod.Length > 0 ? string.Concat(mod, " ", typeName) : typeName;
        }
    }

    public abstract class ParametersCollection
    {
        public int Count => (Parameters == null) ? 0 : Parameters.Length;

        public bool HasParams { get; protected set; }

        public bool HasArglist { get; protected set; }

        public Type ExtensionMethodType => Count == 0 ? null : FixedParameters[0].HasExtensionMethodModifier ? Types[0] : null;

        public IParameterData[] FixedParameters => Parameters;

        public bool HasExtensionMethodType => Count == 0 ? false : FixedParameters[0].HasExtensionMethodModifier;

        public bool IsEmpty => Parameters.Length == 0;

        public Type[] Types { get; set; }

        protected IParameterData[] Parameters { get; set; }

        public int GetParameterIndexByName(string name)
        {
            return Parameters == null ? -1 : Parameters.FindIndex(o => o.Name == name);
        }

        public string GetSignatureForError()
        {
            StringBuilder sb = new StringBuilder("(");
            for (int i = 0; i < Count; ++i)
            {
                if (i != 0)
                {
                    _ = sb.Append(", ");
                }

                _ = sb.Append(ParameterDesc(i));
            }
            _ = sb.Append(')');
            return sb.ToString();
        }

        public string ParameterDesc(int pos)
        {
            Type[] types = Types;
            string typeName = ((types == null) || (types[pos] == null))
                               ? "dynamic"
                               : TypeManager.GetCSharpName(types[pos]);

            return FixedParameters[pos].HasExtensionMethodModifier ? "this " + typeName : typeName;
        }

        public ParametersCollection InflateTypes(Type[] genArguments, Type[] argTypes)
        {
            ParametersCollection p = (ParametersCollection)MemberwiseClone();
            Type[] types = Types;

            for (int i = 0; i < Count; ++i)
            {
                if (types[i].IsGenericType)
                {
                    Type[] genericArgumentsOpen = new Type[types[i].GetGenericTypeDefinition().GetGenericArguments().Length];
                    Type[] genericArguments = types[i].GetGenericArguments();

                    for (int j = 0; j < genericArgumentsOpen.Length; ++j)
                    {
                        genericArgumentsOpen[j] = genericArguments[j].IsGenericParameter ? argTypes[genericArguments[j].GenericParameterPosition] : genericArguments[j];
                    }

                    p.Types[i] = types[i].GetGenericTypeDefinition().MakeGenericType(genericArgumentsOpen);
                    continue;
                }

                if (!types[i].IsGenericParameter)
                {
                    continue;
                }

                Type genericArgument = argTypes[types[i].GenericParameterPosition];
                p.Types[i] = genericArgument;
                continue;
            }

            return p;
        }
    }

    /// <summary>Represents the methods parameters</summary>
    public class ParametersCompiled : ParametersCollection
    {
        public static readonly ParametersCompiled EmptyReadOnlyParameters = new ParametersCompiled();

        // Used by C# 2.0 delegates
        public static readonly ParametersCompiled Undefined = new ParametersCompiled();

        private ParametersCompiled()
        {
            Parameters = new Parameter[0];
            Types = Type.EmptyTypes;
        }

        private ParametersCompiled(Parameter[] parameters, Type[] types)
        {
            Parameters = parameters;
            Types = types;
        }

        public ParametersCompiled(params Parameter[] parameters)
        {
            Parameters = parameters ?? throw new ArgumentException("Use EmptyReadOnlyParameters");

            int count = parameters.Length;
            if (count == 0)
            {
                return;
            }

            if (count == 1)
            {
                HasParams = parameters[0].IsParamsArray;
                return;
            }

            for (int i = 0; i < count; i++)
            {
                string baseName = parameters[i].Name;
                HasParams |= parameters[i].IsParamsArray;

                for (int j = i + 1; j < count; j++)
                {
                    if (baseName != parameters[j].Name)
                    {
                        continue;
                    }

                    Debug.Assert(baseName != parameters[j].Name, "Duplicate parameter name: " + baseName);

                    i = j;
                }
            }
        }

        public ParametersCompiled(IEnumerable<Parameter> parameters, bool hasArgList)
            : this(parameters.ToArray())
        {
            HasArglist = hasArgList;
        }

        public ParametersCompiled(IEnumerable<Parameter> parameters)
            : this(parameters.ToArray()) { }

        public CallingConventions CallingConvention => CallingConventions.Standard;

        public Parameter this[int pos] => (Parameter)Parameters[pos];

        public static ParametersCompiled CreateFullyResolved(Parameter p, Type type)
        {
            return new ParametersCompiled(new[] { p }, new[] { type });
        }

        public static ParametersCompiled CreateFullyResolved(Parameter[] parameters, Type[] types)
        {
            return new ParametersCompiled(parameters, types);
        }

        public static ParametersCompiled MergeGenerated(
            ParametersCompiled userParams,
            bool checkConflicts,
            Parameter compilerParams,
            Type compilerTypes)
        {
            return MergeGenerated(
                userParams,
                checkConflicts,
                new[] { compilerParams },
                new[] { compilerTypes });
        }

        // Use this method when you merge compiler generated parameters with user parameters
        public static ParametersCompiled MergeGenerated(
            ParametersCompiled userParams,
            bool checkConflicts,
            Parameter[] compilerParams,
            Type[] compilerTypes)
        {
            Parameter[] allParams = new Parameter[userParams.Count + compilerParams.Length];

            userParams.FixedParameters.CopyTo(allParams, 0);

            Type[] allTypes;
            if (userParams.Types != null)
            {
                allTypes = new Type[allParams.Length];
                userParams.Types.CopyTo(allTypes, 0);
            }
            else
            {
                allTypes = null;
            }

            int index = 0;
            int lastFilled = userParams.Count;

            foreach (Parameter p in compilerParams)
            {
                for (int i = 0; i < lastFilled; ++i)
                {
                    while (p.Name == allParams[i].Name)
                    {
                        Debug.Assert(
                            !checkConflicts || (i >= userParams.Count),
                            string.Format(
                                "The parameter name '{0}' conflicts with a compiler generated name.",
                                p.Name));

                        p.Name = '_' + p.Name;
                    }
                }

                allParams[lastFilled] = p;

                if (allTypes != null)
                {
                    allTypes[lastFilled] = compilerTypes[index++];
                }

                ++lastFilled;
            }

            return new ParametersCompiled(allParams, allTypes) { HasParams = userParams.HasParams };
        }

        public void ResolveVariable()
        {
            for (int i = 0; i < FixedParameters.Length; ++i)
            {
                this[i].ResolveVariable(i);
            }
        }

        public ParametersCompiled Clone()
        {
            ParametersCompiled p = (ParametersCompiled)MemberwiseClone();
            IParameterData[] parameters = Parameters;

            p.Parameters = new IParameterData[parameters.Length];

            for (int i = 0; i < Count; ++i)
            {
                p.Parameters[i] = this[i].Clone();
            }

            return p;
        }

        public bool Resolve(ParseContext ec)
        {
            if (Types != null)
            {
                return true;
            }

            Types = new Type[Count];

            bool ok = true;

            for (int i = 0; i < FixedParameters.Length; ++i)
            {
                Parameter p = this[i];

                Type resolvedType = p.Resolve(ec);
                if (resolvedType == null)
                {
                    ok = false;
                    continue;
                }

                Types[i] = resolvedType;
            }

            return ok;
        }
    }

    //
    // Imported or resolved parameter information
    //
    public class ParameterData : IParameterData
    {
        private readonly Parameter.Modifier _modifiers;

        public ParameterData(string name, Parameter.Modifier modifiers)
        {
            Name = name;
            _modifiers = modifiers;
        }

        public ParameterData(string name, Parameter.Modifier modifiers, Expression defaultValue)
            : this(name, modifiers)
        {
            DefaultValue = defaultValue;
        }

        #region IParameterData Members
        public Expression DefaultValue { get; }

        public bool HasExtensionMethodModifier => (_modifiers & Parameter.Modifier.This) != 0;

        public bool HasDefaultValue => DefaultValue != null;

        public bool IsByRef { get; set; }

        public Parameter.Modifier ModFlags => _modifiers & ~Parameter.Modifier.This;

        public string Name { get; }

        private Parameter.Modifier _modifierFlags;
        public Parameter.Modifier ModifierFlags
        {
            get => _modifierFlags & ~Parameter.Modifier.This;
            set => _modifierFlags = value;
        }
        #endregion
    }
}