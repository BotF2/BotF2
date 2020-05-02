using System;
using System.Collections.Generic;
using System.Linq;
using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

using MSAst = System.Linq.Expressions.Expression;

namespace Supremacy.Scripting.Ast
{
    public class LambdaExpression : Expression
    {
        private readonly List<LambdaParameter> _parameters;
        private Expression _body;
        private Expression _resolvedBody;

        public LambdaExpression()
        {
            _parameters = new List<LambdaParameter>();
        }

        public IList<LambdaParameter> Parameters => _parameters;

        public TopLevelScope Scope { get; set; }

        public Expression Body
        {
            get => _resolvedBody ?? _body;
            set
            {
                _body = value;
                _resolvedBody = null;
            }
        }

        public Type ReturnType { get; set; }
        public Type LambdaType { get; set; }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            for (int i = 0; i < _parameters.Count; i++)
            {
                LambdaParameter parameter = _parameters[i];
                Walk(ref parameter, prefix, postfix);
                _parameters[i] = parameter;
            }

            Walk(ref _body, prefix, postfix);
        }

        public override MSAst TransformCore(ScriptGenerator generator)
        {
            ScriptScope scope = generator.PushNewScope();
            try
            {
                for (int i = 0; i < Scope.Parameters.Count; i++)
                {
                    _ = Scope.Parameters[i].Transform(generator);
                }

                MSAst transformedBody = _body.Transform(generator);

                if ((ReturnType != null) && (ReturnType != TypeManager.CoreTypes.Object) && (transformedBody.Type != ReturnType))
                {
                    transformedBody = MSAst.Convert(transformedBody, ReturnType);
                }

                System.Linq.Expressions.LambdaExpression result = scope.FinishScope(transformedBody);

                return result;
            }
            finally
            {
                generator.PopScope();
            }
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            if (Scope == null)
            {
                bool parenthesizeParameters = (Parameters.Count != 1) ||
                                              Parameters[0].HasExplicitType;

                if (parenthesizeParameters)
                {
                    sw.Write("(");
                }

                for (int i = 0; i < Parameters.Count; i++)
                {
                    if (i != 0)
                    {
                        sw.Write(", ");
                    }

                    DumpChild(Parameters[i], sw);
                }

                if (parenthesizeParameters)
                {
                    sw.Write(")");
                }

                sw.Write(" => ");

                if (_body != null)
                {
                    _body.Dump(sw, indentChange);
                }
            }
            else
            {
                bool parenthesizeParameters = (Scope.Parameters.Count != 1) ||
                                          !(Scope.Parameters[0] is ImplicitLambdaParameter);

                if (parenthesizeParameters)
                {
                    sw.Write("(");
                }

                for (int i = 0; i < Scope.Parameters.Count; i++)
                {
                    if (i != 0)
                    {
                        sw.Write(", ");
                    }

                    sw.Write(Scope.Parameters[i].Name);
                }

                if (parenthesizeParameters)
                {
                    sw.Write(")");
                }

                sw.Write(" => ");

                if (_body != null)
                {
                    _body.Dump(sw, indentChange);
                }
            }
        }

        public bool HasExplicitParameters => (Scope != null) && (Scope.Parameters.Count > 0) && !(Scope.Parameters[0] is ImplicitLambdaParameter);

        private bool _resolved;

        public override Expression DoResolve(ParseContext ec)
        {
            if (_resolved)
            {
                return this;
            }

            _resolved = true;

            TopLevelScope scope = Scope;
            if (scope == null)
            {
                EnsureScope(ec);

                if (Body is QuoteExpression)
                {
                    return Body.DoResolve(ec);
                }

                _body = Body.Resolve(ec);

                if (_body == null)
                {
                    return null;
                }

                Type = _resolvedBody.Type;
            }

            if (HasExplicitParameters && !Scope.Parameters.Resolve(ec))
            {
                return null;
            }

            if (Body is QuoteExpression)
            {
                return Body.DoResolve(ec);
            }

            //            var currentScope = ec.CurrentScope;
            //            ec.CurrentScope = scope;
            //
            //            this.Body = this.Body.Resolve(ec);
            //
            //            ec.CurrentScope = currentScope;

            ExpressionClass = ExpressionClass.Value;
            return this;
        }

        private void EnsureScope(ParseContext ec)
        {
            if (Scope != null)
            {
                return;
            }

            ParametersCompiled parameters = HasExplicitParameters
                ? new ParametersCompiled(
                    Parameters.Select(o => new Parameter(o.Name, null, o.Span)
                    {
                        ParameterType = o.Type.Resolve(ec).Type
                    }))
                : new ParametersCompiled(Parameters.Select(o => new ImplicitLambdaParameter(o.Name, null, o.Span)));
            TopLevelScope scope = Scope = new TopLevelScope(ec.Compiler, ec.CurrentScope, parameters, Span.Start);
                
            for (int i = 0; i < parameters.Count; i++)
            {
                parameters[i].Scope = scope;
            }
        }

        public bool ExplicitTypeInference(ParseContext ec, TypeInferenceContext typeInference, Type delegateType)
        {
            if (!HasExplicitParameters)
            {
                return false;
            }

            if (!TypeManager.IsDelegateType(delegateType))
            {
                if (TypeManager.DropGenericTypeArguments(delegateType) != TypeManager.CoreTypes.GenericExpression)
                {
                    return false;
                }

                delegateType = delegateType.GetGenericArguments()[0];
                if (!TypeManager.IsDelegateType(delegateType))
                {
                    return false;
                }
            }

            ParametersCollection delegateParams = TypeManager.GetDelegateParameters(ec, delegateType);
            if (delegateParams.Count != Scope.Parameters.Count)
            {
                return false;
            }

            for (int i = 0; i < Scope.Parameters.Count; ++i)
            {
                Type iType = delegateParams.Types[i];
                if (!TypeManager.IsGenericParameter(iType))
                {
                    if (!TypeManager.HasElementType(iType))
                    {
                        continue;
                    }

                    if (!TypeManager.IsGenericParameter(iType.GetElementType()))
                    {
                        continue;
                    }
                }
                _ = typeInference.ExactInference(Scope.Parameters.Types[i], iType);
            }

            return true;
        }

        public Type InferReturnType(ParseContext ec, TypeInferenceContext typeInferenceContext, Type type)
        {
            int parameterCount = Scope.Parameters.Count;
            System.Reflection.MethodInfo invokeMethod = TypeManager.GetDelegateInvokeMethod(ec, null, type);
            
            if ((invokeMethod.GetParameters().Length != Scope.Parameters.Count) || typeInferenceContext.InferredTypeArguments.Length < parameterCount)
            {
                return TypeManager.CoreTypes.Object;
            }

            ParametersCompiled oldParameters = Scope.Parameters;

            Scope.Parameters = oldParameters.Clone();

            for (int i = 0; i < parameterCount; i++)
            {
                if (typeInferenceContext.InferredTypeArguments[i] != null)
                {
                    if (!typeInferenceContext.InferredTypeArguments[i].IsGenericParameter)
                    {
                        Scope.Parameters[i].ParameterType = typeInferenceContext.InferredTypeArguments[i];
                    }
                }
            }

            //_scope.Parameters.Resolve(ec);

            Scope oldScope = ec.CurrentScope;
            
            ec.CurrentScope = Scope;

            Expression resolvedBody = _body.Resolve(ec);
            
            ec.CurrentScope = oldScope;

            Scope.Parameters = oldParameters;
            Type = resolvedBody.Type;

            for (int i = 0; i < parameterCount; i++)
            {
                if ((typeInferenceContext.InferredTypeArguments[i] != null) && typeInferenceContext.InferredTypeArguments[i].IsGenericParameter)
                {
                    typeInferenceContext.InferredTypeArguments[i] = Scope.Parameters[i].ParameterType;
                        //_scope.Parameters[i].ParameterType = typeInferenceContext.InferredTypeArguments[i];
                }
            }

            return Type;
        }

        private static Type GetFuncType(int argumentPlusReturnCount)
        {
            switch (argumentPlusReturnCount)
            {
                #region Generated Delegate Microsoft Scripting Scripting Func Types

                // *** BEGIN GENERATED CODE ***
                // generated by function: gen_delegate_func from: generate_dynsites.py

                case 1: return typeof(Func<>);
                case 2: return typeof(Func<,>);
                case 3: return typeof(Func<,,>);
                case 4: return typeof(Func<,,,>);
                case 5: return typeof(Func<,,,,>);
                case 6: return typeof(Func<,,,,,>);
                case 7: return typeof(Func<,,,,,,>);
                case 8: return typeof(Func<,,,,,,,>);
                case 9: return typeof(Func<,,,,,,,,>);
                case 10: return typeof(Func<,,,,,,,,,>);
                case 11: return typeof(Func<,,,,,,,,,,>);
                case 12: return typeof(Func<,,,,,,,,,,,>);
                case 13: return typeof(Func<,,,,,,,,,,,,>);
                case 14: return typeof(Func<,,,,,,,,,,,,,>);
                case 15: return typeof(Func<,,,,,,,,,,,,,,>);
                case 16: return typeof(Func<,,,,,,,,,,,,,,,>);
                case 17: return typeof(Func<,,,,,,,,,,,,,,,,>);

                // *** END GENERATED CODE ***

                #endregion

                default: return null;
            }
        }

        protected ParametersCompiled ResolveParameters(ParseContext ec, TypeInferenceContext tic, Type delegateType)
        {
            if (!TypeManager.IsDelegateType(delegateType))
            {
                return null;
            }

            ParametersCollection delegateParameters = TypeManager.GetParameterData(delegateType.GetMethod("Invoke"));

            if (HasExplicitParameters)
            {
                return !VerifyExplicitParameters(ec, delegateType, delegateParameters) ? null : Scope.Parameters.Clone();
            }

            //
            // If L has an implicitly typed parameter list we make implicit parameters explicit
            // Set each parameter of L is given the type of the corresponding parameter in D
            //
            if (!VerifyParameterCompatibility(ec, delegateType, delegateParameters, ec.IsInProbingMode))
            {
                return null;
            }

            Type[] ptypes = new Type[Scope.Parameters.Count];
            for (int i = 0; i < delegateParameters.Count; i++)
            {
                // D has no ref or out parameters
                if ((delegateParameters.FixedParameters[i].ModifierFlags & Parameter.Modifier.IsByRef) != 0)
                {
                    return null;
                }

                Type dParam = delegateParameters.Types[i];

                // Blablabla, because reflection does not work with dynamic types
                if (dParam.IsGenericParameter)
                {
                    dParam = delegateType.GetGenericArguments()[dParam.GenericParameterPosition];
                }

                //
                // When type inference context exists try to apply inferred type arguments
                //
                if (tic != null)
                {
                    dParam = tic.InflateGenericArgument(dParam);
                }

                ptypes[i] = dParam;
                ((ImplicitLambdaParameter)Scope.Parameters.FixedParameters[i]).ParameterType = dParam;
            }

            // TODO : FIX THIS
            //ptypes.CopyTo(_scope.Parameters.Types, 0);
            
            // TODO: STOP DOING THIS
            Scope.Parameters.Types = ptypes;

            return Scope.Parameters;
        }

        protected bool VerifyExplicitParameters(ParseContext ec, Type delegateType, ParametersCollection parameters)
        {
            if (VerifyParameterCompatibility(ec, delegateType, parameters, ec.IsInProbingMode))
            {
                return true;
            }

            if (!ec.IsInProbingMode)
            {
                ec.ReportError(
                    1661,
                    string.Format(
                        "Cannot convert '{0}' to delegate type '{1}' since there is a parameter mismatch.",
                        GetSignatureForError(),
                        TypeManager.GetCSharpName(delegateType)),
                    Span);
            }

            return false;
        }

        protected bool VerifyParameterCompatibility(ParseContext ec, Type delegateType, ParametersCollection invokePd, bool ignoreErrors)
        {
            if (Scope.Parameters.Count != invokePd.Count)
            {
                if (ignoreErrors)
                {
                    return false;
                }

                ec.ReportError(
                    1593,
                    string.Format(
                        "Delegate '{0}' does not take '{1}' arguments.",
                        TypeManager.GetCSharpName(delegateType),
                        Scope.Parameters.Count),
                    Span);

                return false;
            }

            bool hasImplicitParameters = !HasExplicitParameters;
            bool error = false;

            for (int i = 0; i < Scope.Parameters.Count; ++i)
            {
                Parameter.Modifier pMod = invokePd.FixedParameters[i].ModifierFlags;
                if (Scope.Parameters.FixedParameters[i].ModifierFlags != pMod && pMod != Parameter.Modifier.Params)
                {
                    if (ignoreErrors)
                    {
                        return false;
                    }

                    if (pMod == Parameter.Modifier.None)
                    {
                        ec.ReportError(
                            1677,
                            string.Format(
                                "Parameter '{0}' should not be declared with the '{1}' keyword.",
                                i + 1,
                                Parameter.GetModifierSignature(Scope.Parameters.FixedParameters[i].ModifierFlags)),
                            Span);
                    }
                    else
                    {
                        ec.ReportError(
                            1676,
                            string.Format(
                                "Parameter '{0}' must be declared with the '{1}' keyword.",
                                i + 1,
                                Parameter.GetModifierSignature(pMod)),
                            Span);
                    }
                    error = true;
                }

                if (hasImplicitParameters)
                {
                    continue;
                }

                Type type = invokePd.Types[i];

                // We assume that generic parameters are always inflated
                if (TypeManager.IsGenericParameter(type))
                {
                    continue;
                }

                if (TypeManager.HasElementType(type) && TypeManager.IsGenericParameter(type.GetElementType()))
                {
                    continue;
                }

                if (invokePd.Types[i] != Scope.Parameters.Types[i])
                {
                    if (ignoreErrors)
                    {
                        return false;
                    }

                    ec.ReportError(
                        1678,
                        string.Format(
                            "Parameter '{0}' is declared as type '{1}' but should be '{2}'",
                            i + 1,
                            TypeManager.GetCSharpName(Scope.Parameters.Types[i]),
                            TypeManager.GetCSharpName(invokePd.Types[i])),
                        Span);

                    error = true;
                }
            }

            return !error;
        }

        //
        // Returns true if the body of lambda expression can be implicitly
        // converted to the delegate of type `delegate_type'
        //
        public bool ImplicitStandardConversionExists(ParseContext ec, Type delegateType)
		{
            bool expressionTreeConversion = false;
            if (!TypeManager.IsDelegateType(delegateType))
            {
                if (TypeManager.DropGenericTypeArguments(delegateType) != TypeManager.CoreTypes.GenericExpression)
                {
                    return false;
                }

                delegateType = delegateType.GetGenericArguments()[0];
                expressionTreeConversion = true;
            }

            System.Reflection.MethodInfo invokeMethod = delegateType.GetMethod("Invoke");// TypeManager.GetDelegateInvokeMethod(ec, null, delegateType);
            Type returnType = invokeMethod.ReturnType;

            if (returnType.IsGenericParameter)
            {
                Type[] genericArguments = delegateType.GetGenericArguments();
                returnType = genericArguments[returnType.GenericParameterPosition];
            }

            EnsureScope(ec);

            System.Reflection.ParameterInfo[] delegateParameters = invokeMethod.GetParameters();
            if (delegateParameters.Length != Scope.Parameters.Count)
            {
                return false;
            }

            ParametersCompiled lambdaParameters = ResolveParameters(ec, null, delegateType);

            int i = 0;
            bool result = delegateParameters.All(delegateParameter => TypeUtils.AreAssignable(delegateParameter.ParameterType, lambdaParameters[i++].ParameterType));

            if (result)
            {
                Scope oldScope = ec.CurrentScope;
                ec.CurrentScope = Scope;
                Expression resolvedBody = _body.Resolve(ec);
                ec.CurrentScope = oldScope;

                if (!TypeUtils.AreAssignable(returnType, resolvedBody.Type))
                {
                    return false;
                }

                _body = resolvedBody;
                _resolvedBody = expressionTreeConversion ? new QuoteExpression(this) : resolvedBody;

                Type = _body.Type;
                LambdaType = delegateType;
            }

            return result;
		}
    }
}