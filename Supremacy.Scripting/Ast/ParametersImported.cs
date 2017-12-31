using System;
using System.Reflection;
using System.Linq;

using Microsoft.Scripting;

using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    public class ParametersImported : ParametersCollection
    {
        ParametersImported(ParametersCollection param, Type[] types)
        {
            Parameters = param.FixedParameters;
            Types = types;
            HasArglist = param.HasArglist;
            HasParams = param.HasParams;
        }

        ParametersImported(IParameterData[] parameters, Type[] types, bool hasArglist, bool hasParams)
        {
            Parameters = parameters;
            Types = types;
            HasArglist = hasArglist;
            HasParams = hasParams;
        }

        public ParametersImported(IParameterData[] param, Type[] types)
        {
            Parameters = param;
            Types = types;
        }

        public static ParametersCollection Create(MethodBase method)
        {
            return Create(method.GetParameters(), method);
        }

        //
        // Generic method parameters importer, param is shared between all instances
        //
        public static ParametersCollection Create(ParametersCollection param, MethodBase method)
        {
            if (param.IsEmpty)
                return param;

            var parameters = method.GetParameters();
            
            var types = parameters
                .Select(p => p.ParameterType)
                .Select(t => t.IsByRef ? t.GetElementType() : t)
                .ToArray();

            return new ParametersImported(param, types);
        }

        //
        // Imports SRE parameters
        //
        public static ParametersCollection Create(ParameterInfo[] pi, MethodBase method)
        {
            const TypeAttributes staticClassAttribute = TypeAttributes.Abstract | TypeAttributes.Sealed;
            int varargs = method != null && (method.CallingConvention & CallingConventions.VarArgs) != 0 ? 1 : 0;

            if (pi.Length == 0 && varargs == 0)
                return ParametersCompiled.EmptyReadOnlyParameters;

            var types = new Type[pi.Length + varargs];
            var par = new IParameterData[pi.Length + varargs];
            var isParams = false;
            var extensionAttribute = TypeManager.PredefinedAttributes.Extension;
            var paramAttributre = TypeManager.PredefinedAttributes.ParamArray;

            for (int i = 0; i < pi.Length; i++)
            {
                types[i] = pi[i].ParameterType;

                ParameterInfo p = pi[i];
                Parameter.Modifier mod = 0;
                Expression defaultValue = null;
                if (types[i].IsByRef)
                {
                    //if ((p.Attributes & (ParameterAttributes.Out | ParameterAttributes.In)) == ParameterAttributes.Out)
                    //    mod = Parameter.Modifier.OUT;
                    //else
                    //    mod = Parameter.Modifier.REF;

                    //
                    // Strip reference wrapping
                    //
                    types[i] = types[i].GetElementType();
                }
                else if (i == 0 && method != null && method.IsStatic &&
                         (method.DeclaringType.Attributes & staticClassAttribute) == staticClassAttribute &&
                         method.IsDefined(extensionAttribute, false))
                {
                    mod = Parameter.Modifier.This;
                }
                else
                {
                    if (i >= pi.Length - 2 && types[i].IsArray)
                    {
                        if (p.IsDefined(paramAttributre, false))
                        {
                            mod = Parameter.Modifier.Params;
                            isParams = true;
                        }
                    }

                    if (!isParams && p.IsOptional)
                    {
                        var value = p.DefaultValue;
                        if (value == Missing.Value)
                        {
                            defaultValue = EmptyExpression.Null;
                        }
                        else if (value == null)
                        {
                            defaultValue = new LiteralExpression
                                            {
                                                Kind = LiteralKind.Null,
                                                Span = SourceSpan.None
                                            };
                        }
                        else
                        {
                            Activator.CreateInstance(
                                typeof(ConstantExpression<>).MakeGenericType(value.GetType()),
                                value);
                        }
                    }
                }

                par[i] = new ParameterData(p.Name, mod, defaultValue) { ModifierFlags = mod };
            }

            if (varargs != 0)
            {
                //par[par.Length - 1] = new ArglistParameter(Location.Null);
                //types[types.Length - 1] = InternalType.Arglist;
                throw new NotSupportedException("__arglist parameters are not supported.");
            }

            return method != null
                       ? new ParametersImported(par, types, varargs != 0, isParams)
                       : new ParametersImported(par, types);
        }
    }
}