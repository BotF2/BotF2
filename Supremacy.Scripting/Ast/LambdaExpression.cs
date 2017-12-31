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
        private TopLevelScope _scope;
        private Expression _body;
        private Expression _resolvedBody;

        public LambdaExpression()
        {
            _parameters = new List<LambdaParameter>();
        }

        public IList<LambdaParameter> Parameters
        {
            get { return _parameters; }
        }

        public TopLevelScope Scope
        {
            get { return _scope; }
            set { _scope = value; }
        }

        public Expression Body
        {
            get { return _resolvedBody ?? _body; }
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
            for (var i = 0; i < _parameters.Count; i++)
            {
                var parameter = _parameters[i];
                Walk(ref parameter, prefix, postfix);
                _parameters[i] = parameter;
            }

            Walk(ref _body, prefix, postfix);
        }

        public override MSAst TransformCore(ScriptGenerator generator)
        {
            var scope = generator.PushNewScope();
            try
            {
                for (int i = 0; i < _scope.Parameters.Count; i++)
                    _scope.Parameters[i].Transform(generator);


                var transformedBody = _body.Transform(generator);

                if ((ReturnType != null) && (ReturnType != TypeManager.CoreTypes.Object) && (transformedBody.Type != ReturnType))
                    transformedBody = MSAst.Convert(transformedBody, ReturnType);

                var result = scope.FinishScope(transformedBody);

                return result;
            }
            finally
            {
                generator.PopScope();
            }
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            if (_scope == null)
            {
                var parenthesizeParameters = ((Parameters.Count != 1) ||
                                              Parameters[0].HasExplicitType);

                if (parenthesizeParameters)
                    sw.Write("(");

                for (var i = 0; i < Parameters.Count; i++)
                {
                    if (i != 0)
                        sw.Write(", ");

                    DumpChild(Parameters[i], sw);
                }

                if (parenthesizeParameters)
                    sw.Write(")");

                sw.Write(" => ");

                if (_body != null)
                    _body.Dump(sw, indentChange);
            }
            else
            {
                var parenthesizeParameters = ((_scope.Parameters.Count != 1) ||
                                          !(_scope.Parameters[0] is ImplicitLambdaParameter));

                if (parenthesizeParameters)
                    sw.Write("(");

                for (var i = 0; i < _scope.Parameters.Count; i++)
                {
                    if (i != 0)
                        sw.Write(", ");

                    sw.Write(_scope.Parameters[i].Name);
                }

                if (parenthesizeParameters)
                    sw.Write(")");

                sw.Write(" => ");

                if (_body != null)
                    _body.Dump(sw, indentChange);
            }
        }

        public bool HasExplicitParameters
        {
            get { return ((_scope != null) && (_scope.Parameters.Count > 0) && !(_scope.Parameters[0] is ImplicitLambdaParameter)); }
        }

        private bool _resolved;

        public override Expression DoResolve(ParseContext ec)
        {
            if (_resolved)
                return this;

            _resolved = true;

            var scope = Scope;
            if (scope == null)
            {
                EnsureScope(ec);

                if (Body is QuoteExpression)
                    return Body.DoResolve(ec);

                _body = Body.Resolve(ec);

                if (_body == null)
                    return null;

                Type = _resolvedBody.Type;
            }

            if (HasExplicitParameters && !_scope.Parameters.Resolve(ec))
                return null;

            if (Body is QuoteExpression)
                return Body.DoResolve(ec);

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
            if (_scope != null)
                return;

            ParametersCompiled parameters;

            if (HasExplicitParameters)
            {
                parameters = new ParametersCompiled(
                    Parameters.Select(o => new Parameter(o.Name, null, o.Span)
                                                {
                                                    ParameterType = o.Type.Resolve(ec).Type
                                                }));
            }
            else
            {
                parameters = new ParametersCompiled(Parameters.Select(o => new ImplicitLambdaParameter(o.Name, null, o.Span)));
            }

            var scope = Scope = new TopLevelScope(ec.Compiler, ec.CurrentScope, parameters, Span.Start);
                
            for (int i = 0; i < parameters.Count; i++)
                parameters[i].Scope = scope;
        }

        public bool ExplicitTypeInference(ParseContext ec, TypeInferenceContext typeInference, Type delegateType)
        {
            if (!HasExplicitParameters)
                return false;

            if (!TypeManager.IsDelegateType(delegateType))
            {
                if (TypeManager.DropGenericTypeArguments(delegateType) != TypeManager.CoreTypes.GenericExpression)
                    return false;

                delegateType = delegateType.GetGenericArguments()[0];
                if (!TypeManager.IsDelegateType(delegateType))
                    return false;
            }

            var delegateParams = TypeManager.GetDelegateParameters(ec, delegateType);
            if (delegateParams.Count != _scope.Parameters.Count)
                return false;

            for (int i = 0; i < _scope.Parameters.Count; ++i)
            {
                var iType = delegateParams.Types[i];
                if (!TypeManager.IsGenericParameter(iType))
                {
                    if (!TypeManager.HasElementType(iType))
                        continue;

                    if (!TypeManager.IsGenericParameter(iType.GetElementType()))
                        continue;
                }
                typeInference.ExactInference(_scope.Parameters.Types[i], iType);
            }

            return true;
        }

        public Type InferReturnType(ParseContext ec, TypeInferenceContext typeInferenceContext, Type type)
        {
            var parameterCount = _scope.Parameters.Count;
            var invokeMethod = TypeManager.GetDelegateInvokeMethod(ec, null, type);
            
            if ((invokeMethod.GetParameters().Length != _scope.Parameters.Count) || typeInferenceContext.InferredTypeArguments.Length < parameterCount)
                return TypeManager.CoreTypes.Object;

            var oldParameters = _scope.Parameters;

            _scope.Parameters = oldParameters.Clone();

            for (int i = 0; i < parameterCount; i++)
            {
                if (typeInferenceContext.InferredTypeArguments[i] != null)
                {
                    if (!typeInferenceContext.InferredTypeArguments[i].IsGenericParameter)
                        _scope.Parameters[i].ParameterType = typeInferenceContext.InferredTypeArguments[i];
                }
            }

            //_scope.Parameters.Resolve(ec);

            var oldScope = ec.CurrentScope;
            
            ec.CurrentScope = _scope;
            
            var resolvedBody = _body.Resolve(ec);
            
            ec.CurrentScope = oldScope;

            _scope.Parameters = oldParameters;
            Type = resolvedBody.Type;

            for (int i = 0; i < parameterCount; i++)
            {
                if ((typeInferenceContext.InferredTypeArguments[i] != null) && typeInferenceContext.InferredTypeArguments[i].IsGenericParameter)
                {
                    typeInferenceContext.InferredTypeArguments[i] = _scope.Parameters[i].ParameterType;
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
                return null;

            var delegateParameters = TypeManager.GetParameterData(delegateType.GetMethod("Invoke"));

            if (HasExplicitParameters)
            {
                if (!VerifyExplicitParameters(ec, delegateType, delegateParameters))
                    return null;

                return _scope.Parameters.Clone();
            }

            //
            // If L has an implicitly typed parameter list we make implicit parameters explicit
            // Set each parameter of L is given the type of the corresponding parameter in D
            //
            if (!VerifyParameterCompatibility(ec, delegateType, delegateParameters, ec.IsInProbingMode))
                return null;

            var ptypes = new Type[_scope.Parameters.Count];
            for (int i = 0; i < delegateParameters.Count; i++)
            {
                // D has no ref or out parameters
                if ((delegateParameters.FixedParameters[i].ModifierFlags & Parameter.Modifier.IsByRef) != 0)
                    return null;

                Type dParam = delegateParameters.Types[i];

				// Blablabla, because reflection does not work with dynamic types
				if (dParam.IsGenericParameter)
					dParam = delegateType.GetGenericArguments () [dParam.GenericParameterPosition];

                //
                // When type inference context exists try to apply inferred type arguments
                //
                if (tic != null)
                    dParam = tic.InflateGenericArgument(dParam);

                ptypes[i] = dParam;
                ((ImplicitLambdaParameter)_scope.Parameters.FixedParameters[i]).ParameterType = dParam;
            }

            // TODO : FIX THIS
            //ptypes.CopyTo(_scope.Parameters.Types, 0);
            
            // TODO: STOP DOING THIS
            _scope.Parameters.Types = ptypes;

            return _scope.Parameters;
        }

        protected bool VerifyExplicitParameters(ParseContext ec, Type delegateType, ParametersCollection parameters)
        {
            if (VerifyParameterCompatibility(ec, delegateType, parameters, ec.IsInProbingMode))
                return true;

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
            if (_scope.Parameters.Count != invokePd.Count)
            {
                if (ignoreErrors)
                    return false;

                ec.ReportError(
                    1593,
                    string.Format(
                        "Delegate '{0}' does not take '{1}' arguments.",
                        TypeManager.GetCSharpName(delegateType),
                        _scope.Parameters.Count),
                    Span);

                return false;
            }

            var hasImplicitParameters = !HasExplicitParameters;
            var error = false;

            for (int i = 0; i < _scope.Parameters.Count; ++i)
            {
                var pMod = invokePd.FixedParameters[i].ModifierFlags;
                if (_scope.Parameters.FixedParameters[i].ModifierFlags != pMod && pMod != Parameter.Modifier.Params)
                {
                    if (ignoreErrors)
                        return false;

                    if (pMod == Parameter.Modifier.None)
                    {
                        ec.ReportError(
                            1677,
                            string.Format(
                                "Parameter '{0}' should not be declared with the '{1}' keyword.",
                                (i + 1),
                                Parameter.GetModifierSignature(_scope.Parameters.FixedParameters[i].ModifierFlags)),
                            Span);
                    }
                    else
                    {
                        ec.ReportError(
                            1676,
                            string.Format(
                                "Parameter '{0}' must be declared with the '{1}' keyword.",
                                (i + 1),
                                Parameter.GetModifierSignature(pMod)),
                            Span);
                    }
                    error = true;
                }

                if (hasImplicitParameters)
                    continue;

                Type type = invokePd.Types[i];

                // We assume that generic parameters are always inflated
                if (TypeManager.IsGenericParameter(type))
                    continue;

                if (TypeManager.HasElementType(type) && TypeManager.IsGenericParameter(type.GetElementType()))
                    continue;

                if (invokePd.Types[i] != _scope.Parameters.Types[i])
                {
                    if (ignoreErrors)
                        return false;

                    ec.ReportError(
                        1678,
                        string.Format(
                            "Parameter '{0}' is declared as type '{1}' but should be '{2}'",
                            (i + 1),
                            TypeManager.GetCSharpName(_scope.Parameters.Types[i]),
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
		public bool ImplicitStandardConversionExists (ParseContext ec, Type delegateType)
		{
		    var expressionTreeConversion = false;
            if (!TypeManager.IsDelegateType(delegateType))
            {
                if (TypeManager.DropGenericTypeArguments(delegateType) != TypeManager.CoreTypes.GenericExpression)
                    return false;

                delegateType = delegateType.GetGenericArguments()[0];
                expressionTreeConversion = true;
            }

		    var invokeMethod = delegateType.GetMethod("Invoke");// TypeManager.GetDelegateInvokeMethod(ec, null, delegateType);
            var returnType = invokeMethod.ReturnType;

            if (returnType.IsGenericParameter)
            {
                var genericArguments = delegateType.GetGenericArguments();
                returnType = genericArguments[returnType.GenericParameterPosition];
            }

            EnsureScope(ec);

            var delegateParameters = invokeMethod.GetParameters();
            if (delegateParameters.Length != _scope.Parameters.Count)
                return false;

		    var lambdaParameters = ResolveParameters(ec, null, delegateType);

		    int i = 0;
            var result = delegateParameters.All(delegateParameter => TypeUtils.AreAssignable(delegateParameter.ParameterType, lambdaParameters[i++].ParameterType));

            if (result)
            {
                var oldScope = ec.CurrentScope;
                ec.CurrentScope = _scope;
                var resolvedBody = _body.Resolve(ec);
                ec.CurrentScope = oldScope;

                if (!TypeUtils.AreAssignable(returnType, resolvedBody.Type))
                    return false;

                _body = resolvedBody;
                _resolvedBody = (expressionTreeConversion) ? new QuoteExpression(this) : resolvedBody;

                Type = _body.Type;
                LambdaType = delegateType;
            }

		    return result;
/*
            using (ec.With(ParseContext.Options.InferReturnType, false))
            {
                using (ec.Set(ParseContext.Options.ProbingMode))
                {
                    return Compatible(ec, delegateType) != null;
                }
            }
*/
		}

/*
        protected Expression CreateExpressionTree(ParseContext ec, Type delegateType)
        {
            if (ec.IsInProbingMode)
                return this;

            BlockContext bc = new ScopeS(ec.MemberContext, ec.CurrentBlock.Explicit, TypeManager.CoreTypes.Void);
            Expression args = _scope.Parameters.CreateExpressionTree(bc, loc);
            Expression expr = Block.CreateExpressionTree(ec);
            if (expr == null)
                return null;

            Arguments arguments = new Arguments(2);
            arguments.Add(new Argument(expr));
            arguments.Add(new Argument(args));
            return CreateExpressionFactoryCall(
                ec,
                "Lambda",
                new TypeArguments(new TypeExpression(delegateType, loc)),
                arguments);
        }
*/

/*
        protected Type CompatibleChecks(ParseContext ec, Type delegateType)
        {
            if (TypeManager.IsDelegateType(delegateType))
                return delegateType;

            if (TypeManager.DropGenericTypeArguments(delegateType) == TypeManager.CoreTypes.Expression)
            {
                delegateType = delegateType.GetGenericArguments()[0];
                if (TypeManager.IsDelegateType(delegateType))
                    return delegateType;

                ec.ReportError(
                    835,
                    string.Format(
                        "Cannot convert `{0}' to an expression tree of non-delegate type `{1}'",
                        GetSignatureForError(),
                        TypeManager.GetCSharpName(delegateType)),
                    this.Span);

                return null;
            }

            ec.ReportError(
                1660,
                string.Format(
                    "Cannot convert `{0}' to non-delegate type `{1}'",
                    GetSignatureForError(),
                    TypeManager.GetCSharpName(delegateType)),
                this.Span);
            return null;
        }

        public Expression Compatible(ParseContext ec, Type type)
        {
            Expression am = (Expression)_compatibles[type];
            if (am != null)
                return am;

            Type delegate_type = CompatibleChecks(ec, type);
            if (delegate_type == null)
                return null;

            //
            // At this point its the first time we know the return type that is 
            // needed for the anonymous method.  We create the method here.
            //

            var invoke_mb = delegate_type.GetMethod("Invoke");
            Type return_type = invoke_mb.ReturnType;

			Type[] g_args = delegate_type.GetGenericArguments ();
			if (return_type.IsGenericParameter)
				return_type = g_args [return_type.GenericParameterPosition];

            //
            // Second: the return type of the delegate must be compatible with 
            // the anonymous type.   Instead of doing a pass to examine the block
            // we satisfy the rule by setting the return type on the EmitContext
            // to be the delegate type return type.
            //

            var body = CompatibleMethodBody(ec, null, return_type, delegate_type);
            if (body == null)
                return null;

            try
            {
                am = body.Compatible(ec);
            }
            catch (Exception e)
            {
                throw new InternalErrorException(e, this.Span);
            }

            if (!ec.IsInProbingMode)
                _compatibles.Add(type, am ?? EmptyExpression.Null);

            return am;
        }
*/
    }
}