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

using MSAst = System.Linq.Expressions;

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
        private Type _parameterType;

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
            get { return _parameterType ?? TypeManager.CoreTypes.Object; }
            set { _parameterType = value; }
        }

        internal Type ParameterTypeInternal
        {
            get { return _parameterType; }
            set { _parameterType = value; }
        }

        public int Index { get; private set; }

        public virtual bool IsParamsArray
        {
            get { return false; }
        }

        public Scope Scope { get; set; }

        #region IParameterData Members
        public Expression DefaultValue { get; set; }

        public bool HasExtensionMethodModifier
        {
            get { return (_modifierFlags & Modifier.This) == Modifier.This; }
        }

        public bool HasDefaultValue { get; set; }
        public string Name { get; set; }

        public bool IsByRef
        {
            get { return ParameterType.IsByRef; }
        }

        public Modifier ModifierFlags
        {
            get { return _modifierFlags & ~Modifier.This; }
        }
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

            var targetParameter = target as Parameter;
            if (targetParameter == null)
                return;

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
                return ParameterType;

            var texpr = TypeName.ResolveAsTypeTerminal(rc, false);
            if (texpr == null)
                return null;

            ParameterType = texpr.Type;

            // Ignore all checks for dummy members
            if (DefaultValue != null)
            {
                DefaultValue = DefaultValue.Resolve(rc);
                if (DefaultValue != null)
                {
                    var value = DefaultValue as ConstantExpression;
                    if (value == null)
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
                        var c = value.ConvertImplicitly(ParameterType);
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

            TypeManager.CheckTypeVariance(
                ParameterType,
                (_modifierFlags & Modifier.IsByRef) != 0 ? Variance.None : Variance.Contravariant,
                rc);

            if (TypeManager.IsGenericParameter(ParameterType))
                return ParameterType;

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
            var typeName = (ParameterType != null)
                               ? TypeManager.GetCSharpName(ParameterType)
                               : TypeName.GetSignatureForError();

            var mod = GetModifierSignature(_modifierFlags);
            if (mod.Length > 0)
                return String.Concat(mod, " ", typeName);

            return typeName;
        }
    }

    public abstract class ParametersCollection
    {
        private IParameterData[] _parameters;

        public int Count
        {
            get { return (Parameters == null) ? 0 : Parameters.Length; }
        }

        public bool HasParams { get; protected set; }

        public bool HasArglist { get; protected set; }

        public Type ExtensionMethodType
        {
            get
            {
                if (Count == 0)
                    return null;
                return FixedParameters[0].HasExtensionMethodModifier ? Types[0] : null;
            }
        }

        public IParameterData[] FixedParameters
        {
            get { return Parameters; }
        }

        public bool HasExtensionMethodType
        {
            get
            {
                if (Count == 0)
                    return false;

                return FixedParameters[0].HasExtensionMethodModifier;
            }
        }

        public bool IsEmpty
        {
            get { return Parameters.Length == 0; }
        }

        public Type[] Types { get; set; }

        protected IParameterData[] Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

        public int GetParameterIndexByName(string name)
        {
            if (_parameters == null)
                return -1;

            return _parameters.FindIndex(o => o.Name == name);
        }

        public string GetSignatureForError()
        {
            var sb = new StringBuilder("(");
            for (var i = 0; i < Count; ++i)
            {
                if (i != 0)
                    sb.Append(", ");
                sb.Append(ParameterDesc(i));
            }
            sb.Append(')');
            return sb.ToString();
        }

        public string ParameterDesc(int pos)
        {
            var types = Types;
            var typeName = ((types == null) || (types[pos] == null))
                               ? "dynamic"
                               : TypeManager.GetCSharpName(types[pos]);

            if (FixedParameters[pos].HasExtensionMethodModifier)
                return "this " + typeName;

            return typeName;
        }

        public ParametersCollection InflateTypes(Type[] genArguments, Type[] argTypes)
        {
            var p = (ParametersCollection)MemberwiseClone();
            var types = Types;

            for (var i = 0; i < Count; ++i)
            {
                if (types[i].IsGenericType)
                {
                    var genericArgumentsOpen = new Type[types[i].GetGenericTypeDefinition().GetGenericArguments().Length];
                    var genericArguments = types[i].GetGenericArguments();

                    for (var j = 0; j < genericArgumentsOpen.Length; ++j)
                    {
                        if (genericArguments[j].IsGenericParameter)
                            genericArgumentsOpen[j] = argTypes[genericArguments[j].GenericParameterPosition];
                        else
                            genericArgumentsOpen[j] = genericArguments[j];
                    }

                    p.Types[i] = types[i].GetGenericTypeDefinition().MakeGenericType(genericArgumentsOpen);
                    continue;
                }

                if (!types[i].IsGenericParameter)
                    continue;

                var genericArgument = argTypes[types[i].GenericParameterPosition];
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
            if (parameters == null)
                throw new ArgumentException("Use EmptyReadOnlyParameters");

            Parameters = parameters;

            var count = parameters.Length;
            if (count == 0)
                return;

            if (count == 1)
            {
                HasParams = parameters[0].IsParamsArray;
                return;
            }

            for (var i = 0; i < count; i++)
            {
                var baseName = parameters[i].Name;
                HasParams |= parameters[i].IsParamsArray;

                for (var j = i + 1; j < count; j++)
                {
                    if (baseName != parameters[j].Name)
                        continue;

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
            : this(parameters.ToArray()) {}

        public CallingConventions CallingConvention
        {
            get { return CallingConventions.Standard; }
        }

        public Parameter this[int pos]
        {
            get { return (Parameter)Parameters[pos]; }
        }

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
            var allParams = new Parameter[userParams.Count + compilerParams.Length];

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

            var index = 0;
            var lastFilled = userParams.Count;

            foreach (var p in compilerParams)
            {
                for (var i = 0; i < lastFilled; ++i)
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
                    allTypes[lastFilled] = compilerTypes[index++];

                ++lastFilled;
            }

            return new ParametersCompiled(allParams, allTypes) { HasParams = userParams.HasParams };
        }

        public void ResolveVariable()
        {
            for (var i = 0; i < FixedParameters.Length; ++i)
                this[i].ResolveVariable(i);
        }

        public ParametersCompiled Clone()
        {
            var p = (ParametersCompiled)MemberwiseClone();
            var parameters = Parameters;

            p.Parameters = new IParameterData[parameters.Length];

            for (var i = 0; i < Count; ++i)
                p.Parameters[i] = this[i].Clone();

            return p;
        }

        public bool Resolve(ParseContext ec)
        {
            if (Types != null)
                return true;

            Types = new Type[Count];

            var ok = true;

            for (var i = 0; i < FixedParameters.Length; ++i)
            {
                Parameter p = this[i];

                var resolvedType = p.Resolve(ec);
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
        private readonly Expression _defaultValue;
        private readonly string _name;

        public ParameterData(string name, Parameter.Modifier modifiers)
        {
            _name = name;
            _modifiers = modifiers;
        }

        public ParameterData(string name, Parameter.Modifier modifiers, Expression defaultValue)
            : this(name, modifiers)
        {
            _defaultValue = defaultValue;
        }

        #region IParameterData Members
        public Expression DefaultValue
        {
            get { return _defaultValue; }
        }

        public bool HasExtensionMethodModifier
        {
            get { return (_modifiers & Parameter.Modifier.This) != 0; }
        }

        public bool HasDefaultValue
        {
            get { return _defaultValue != null; }
        }

        public bool IsByRef { get; set; }

        public Parameter.Modifier ModFlags
        {
            get { return _modifiers & ~Parameter.Modifier.This; }
        }

        public string Name
        {
            get { return _name; }
        }

        private Parameter.Modifier _modifierFlags;
        public Parameter.Modifier ModifierFlags
        {
            get { return _modifierFlags & ~Parameter.Modifier.This; }
            set { _modifierFlags = value; }
        }
        #endregion
    }
}