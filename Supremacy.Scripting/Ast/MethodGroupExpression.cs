using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    public class MethodGroupExpression : MemberExpression
    {
        public interface IErrorHandler
        {
            bool AmbiguousCall(ParseContext ec, MethodBase ambiguous);
            bool NoExactMatch(ParseContext ec, MethodBase method);
        }

        private readonly bool _hasInaccessibleCandidatesOnly;
        private readonly Type _queriedType;
        private MethodBase _bestCandidate;
        private Type _delegateType;


        public MethodGroupExpression(MemberInfo[] members, Type type, SourceSpan span, bool inacessibleCandidatesOnly)
            : this(members, type, span)
        {
            _hasInaccessibleCandidatesOnly = inacessibleCandidatesOnly;
        }

        public MethodGroupExpression(MemberInfo[] members, Type type, SourceSpan span)
        {
            Methods = members.Cast<MethodBase>().ToArray();
            _queriedType = type;

            Span = span;
            ExpressionClass = ExpressionClass.MethodGroup;
        }

        internal MethodGroupExpression()
        {
            // For cloning purposes only.
        }

        public override Type DeclaringType => _queriedType;

        public Type DelegateType
        {
            set => _delegateType = value;
        }

        public bool IdenticalTypeName { get; private set; }

        public override string GetSignatureForError()
        {
            return BestCandidate != null ? TypeManager.GetCSharpSignature(BestCandidate) : TypeManager.GetCSharpSignature(Methods[0]);
        }

        public override string Name => Methods[0].Name;

        public override bool IsInstance => BestCandidate != null ? !BestCandidate.IsStatic : Methods.Any(mb => !mb.IsStatic);

        public override bool IsStatic => BestCandidate != null ? BestCandidate.IsStatic : Methods.Any(mb => mb.IsStatic);

        public TypeArguments TypeArguments { get; set; }

        public MethodBase[] Methods { get; set; }

        public IErrorHandler CustomErrorHandler { get; set; }

        public MethodBase BestCandidate
        {
            get => _bestCandidate;
            set
            {
                _bestCandidate = value;
                MethodInfo methodInfo = value as MethodInfo;
                Type = methodInfo?.ReturnType;
            }
        }

        public static explicit operator ConstructorInfo(MethodGroupExpression mg)
        {
            return (ConstructorInfo)mg.BestCandidate;
        }

        public static explicit operator MethodInfo(MethodGroupExpression mg)
        {
            return (MethodInfo)mg.BestCandidate;
        }

        //
        //  7.4.3.3  Better conversion from expression
        //  Returns :   1    if a->p is better,
        //              2    if a->q is better,
        //              0 if neither is better
        //
        private static int BetterExpressionConversion(ParseContext ec, Argument a, Type p, Type q)
        {
            Type argumentType = a.Value.Type;

            if (a.Value is LambdaExpression)
            {
                // Uwrap delegate from Expression<T>
                if (TypeManager.DropGenericTypeArguments(p) == TypeManager.CoreTypes.GenericExpression)
                {
                    p = p.GetGenericArguments()[0];
                }

                if (TypeManager.DropGenericTypeArguments(q) == TypeManager.CoreTypes.GenericExpression)
                {
                    q = q.GetGenericArguments()[0];
                }

                p = TypeManager.GetDelegateInvokeMethod(ec, null, p).ReturnType;
                q = TypeManager.GetDelegateInvokeMethod(ec, null, q).ReturnType;

                if (p == TypeManager.CoreTypes.Void && q != TypeManager.CoreTypes.Void)
                {
                    return 2;
                }

                if (q == TypeManager.CoreTypes.Void && p != TypeManager.CoreTypes.Void)
                {
                    return 1;
                }
            }

            return argumentType == p ? 1 : argumentType == q ? 2 : BetterTypeConversion(ec, p, q);
        }

        //
        // 7.4.3.4  Better conversion from type
        //
        public static int BetterTypeConversion(ParseContext ec, Type p, Type q)
        {
            if (p == null || q == null)
            {
                throw new InternalErrorException("BetterTypeConversion got a null conversion");
            }

            if (p == TypeManager.CoreTypes.Int32)
            {
                if (q == TypeManager.CoreTypes.UInt32 || q == TypeManager.CoreTypes.UInt64)
                {
                    return 1;
                }
            }
            else if (p == TypeManager.CoreTypes.Int64)
            {
                if (q == TypeManager.CoreTypes.UInt64)
                {
                    return 1;
                }
            }
            else if (p == TypeManager.CoreTypes.SByte)
            {
                if (q == TypeManager.CoreTypes.Byte || q == TypeManager.CoreTypes.UInt16 ||
                    q == TypeManager.CoreTypes.UInt32 || q == TypeManager.CoreTypes.UInt64)
                {
                    return 1;
                }
            }
            else if (p == TypeManager.CoreTypes.UInt16)
            {
                if (q == TypeManager.CoreTypes.UInt16 || q == TypeManager.CoreTypes.UInt32 ||
                    q == TypeManager.CoreTypes.UInt64)
                {
                    return 1;
                }
            }

            if (q == TypeManager.CoreTypes.Int32)
            {
                if (p == TypeManager.CoreTypes.UInt32 || p == TypeManager.CoreTypes.UInt64)
                {
                    return 2;
                }
            }
            if (q == TypeManager.CoreTypes.Int64)
            {
                if (p == TypeManager.CoreTypes.UInt64)
                {
                    return 2;
                }
            }
            else if (q == TypeManager.CoreTypes.SByte)
            {
                if (p == TypeManager.CoreTypes.Byte || p == TypeManager.CoreTypes.UInt16 ||
                    p == TypeManager.CoreTypes.UInt32 || p == TypeManager.CoreTypes.UInt64)
                {
                    return 2;
                }
            }
            if (q == TypeManager.CoreTypes.UInt16)
            {
                if (p == TypeManager.CoreTypes.UInt16 || p == TypeManager.CoreTypes.UInt32 ||
                    p == TypeManager.CoreTypes.UInt64)
                {
                    return 2;
                }
            }

            bool pToQ = TypeUtils.IsImplicitlyConvertible(p, q);
            bool qToP = TypeUtils.IsImplicitlyConvertible(q, p);

            return pToQ && !qToP ? 1 : qToP && !pToQ ? 2 : 0;
        }

        /// <summary>
        ///   Determines "Better function" between candidate
        ///   and the current best match
        /// </summary>
        /// <remarks>
        ///    Returns a boolean indicating :
        ///     false if candidate ain't better
        ///     true  if candidate is better than the current best match
        /// </remarks>
        private static bool BetterFunction(
            ParseContext parseContext,
            Arguments arguments,
            int argumentCount,
            MethodBase candidate,
            bool candidateParams,
            MethodBase best,
            bool bestParams)
        {
            ParametersCollection candidateParameters = TypeManager.GetParameterData(candidate);
            ParametersCollection bestParameters = TypeManager.GetParameterData(best);

            bool betterAtLeastOne = false;
            bool same = true;
            for (int j = 0, cIndex = 0, bIndex = 0; j < argumentCount; ++j, ++cIndex, ++bIndex)
            {
                Argument a = arguments[j];

                // Provided default argument value is never better
                if (a.IsDefaultArgument && candidateParams == bestParams)
                {
                    return false;
                }

                Type candidateType = candidateParameters.Types[cIndex];
                Type bestType = bestParameters.Types[bIndex];

                if (candidateParams && candidateParameters.FixedParameters[cIndex].ModifierFlags == Parameter.Modifier.Params)
                {
                    candidateType = candidateType.GetElementType();
                    --cIndex;
                }

                if (bestParams && bestParameters.FixedParameters[bIndex].ModifierFlags == Parameter.Modifier.Params)
                {
                    bestType = bestType.GetElementType();
                    --bIndex;
                }

                if (candidateType.Equals(bestType))
                {
                    continue;
                }

                same = false;
                int result = BetterExpressionConversion(parseContext, a, candidateType, bestType);

                // for each argument, the conversion to 'ct' should be no worse than 
                // the conversion to 'bt'.
                if (result == 2)
                {
                    return false;
                }

                // for at least one argument, the conversion to 'ct' should be better than 
                // the conversion to 'bt'.
                if (result != 0)
                {
                    betterAtLeastOne = true;
                }
            }

            if (betterAtLeastOne)
            {
                return true;
            }

            //
            // This handles the case
            //
            //   Add (float f1, float f2, float f3);
            //   Add (params decimal [] foo);
            //
            // The call Add (3, 4, 5) should be ambiguous.  Without this check, the
            // first candidate would've chosen as better.
            //
            if (!same)
            {
                return false;
            }

            //
            // The two methods have equal parameter types.  Now apply tie-breaking rules
            //
            if (best.IsGenericMethod)
            {
                if (!candidate.IsGenericMethod)
                {
                    return true;
                }
            }
            else if (candidate.IsGenericMethod)
            {
                return false;
            }

            //
            // This handles the following cases:
            //
            //   Trim () is better than Trim (params char[] chars)
            //   Concat (string s1, string s2, string s3) is better than
            //     Concat (string s1, params string [] srest)
            //   Foo (int, params int [] rest) is better than Foo (params int [] rest)
            //
            if (!candidateParams && bestParams)
            {
                return true;
            }

            if (candidateParams && !bestParams)
            {
                return false;
            }

            int candidateParameterCount = candidateParameters.Count;
            int bestParameterCount = bestParameters.Count;

            if (candidateParameterCount != bestParameterCount)
            {
                // can only happen if (candidate_params && best_params)
                return candidateParameterCount > bestParameterCount && bestParameters.HasParams;
            }

            //
            // now, both methods have the same number of parameters, and the parameters have the same types
            // Pick the "more specific" signature
            //

            MethodBase originalCandidate = TypeManager.DropGenericMethodArguments(candidate);
            MethodBase originalBest = TypeManager.DropGenericMethodArguments(best);

            ParametersCollection originalCandidateParameters = TypeManager.GetParameterData(originalCandidate);
            ParametersCollection originalBestParameters = TypeManager.GetParameterData(originalBest);

            bool specificAtLeastOnce = false;
            for (int j = 0; j < candidateParameterCount; ++j)
            {
                Type candidateParameterType = originalCandidateParameters.Types[j];
                Type bestParameterType = originalBestParameters.Types[j];

                if (candidateParameterType.Equals(bestParameterType))
                {
                    continue;
                }

                Type specific = MoreSpecific(candidateParameterType, bestParameterType);
                if (specific == bestParameterType)
                {
                    return false;
                }

                if (specific == candidateParameterType)
                {
                    specificAtLeastOnce = true;
                }
            }

            return specificAtLeastOnce;

            // FIXME: handle lifted operators
            // ...
        }

        protected override MemberExpression ResolveExtensionMemberAccess(ParseContext ec, Expression left)
        {
            if (!IsStatic)
            {
                return base.ResolveExtensionMemberAccess(ec, left);
            }

            //
            // When left side is an expression and at least one candidate method is 
            // static, it can be extension method
            //
            InstanceExpression = left;
            return this;
        }

        public override MemberExpression ResolveMemberAccess(
            ParseContext ec,
            Expression left,
            SourceSpan loc,
            NameExpression original)
        {
            if (!(left is TypeExpression) &&
                original != null && original.IdenticalNameAndTypeName(ec, left, loc))
            {
                IdenticalTypeName = true;
            }

            return base.ResolveMemberAccess(ec, left, loc, original);
        }

        public override Expression DoResolve(ParseContext ec)
        {
            if (InstanceExpression != null)
            {
                InstanceExpression = InstanceExpression.DoResolve(ec);
                if (InstanceExpression == null)
                {
                    return null;
                }
            }

            return this;
        }

        public void ReportUsageError(ParseContext ec)
        {
            ec.ReportError(
                654,
                "Method '" + DeclaringType + "." + Name + "()' is referenced without parentheses.",
                Severity.Error,
                Span);
        }

        private void OnErrorAmbiguousCall(ParseContext ec, MethodBase ambiguous)
        {
            if (CustomErrorHandler != null && CustomErrorHandler.AmbiguousCall(ec, ambiguous))
            {
                return;
            }

            ec.ReportError(
                121,
                string.Format(
                    "The call is ambiguous between the following methods or properties: '{0}' and '{1}'.",
                    TypeManager.GetCSharpSignature(ambiguous),
                    TypeManager.GetCSharpSignature(BestCandidate)),
                Severity.Error,
                Span);
        }

        protected virtual void OnErrorInvalidArguments(
            ParseContext parseContext,
            SourceSpan location,
            int argumentIndex,
            MethodBase method,
            Argument argument,
            ParametersCollection expectedParameters,
            Type argumentType)
        {
            ExtensionMethodGroupExpression emg = this as ExtensionMethodGroupExpression;

            if (TypeManager.IsDelegateType(method.DeclaringType))
            {
                parseContext.ReportError(
                    1594,
                    string.Format(
                        "Delegate '{0}' has some invalid arguments.",
                        TypeManager.GetCSharpName(method.DeclaringType)),
                    location);
            }
            else
            {
                if (emg != null)
                {
                    parseContext.ReportError(
                        1928,
                        string.Format(
                            "Type '{0}' does not contain a member '{1}' and the best extension method overload '{2}' has some invalid arguments.",
                            emg.ExtensionExpression.GetSignatureForError(),
                            emg.Name,
                            TypeManager.GetCSharpSignature(method)),
                        location);
                }
                else
                {
                    parseContext.ReportError(
                        1502,
                        string.Format(
                            "The best overloaded method match for '{0}' has some invalid arguments.",
                            TypeManager.GetCSharpSignature(method)),
                        location);
                }
            }

            Parameter.Modifier mod = argumentIndex >= expectedParameters.Count ? 0 : expectedParameters.FixedParameters[argumentIndex].ModifierFlags;

            string index = (argumentIndex + 1).ToString();
            if (((mod & (Parameter.Modifier.Ref | Parameter.Modifier.Out)) ^
                (argument.Modifier & (Parameter.Modifier.Ref | Parameter.Modifier.Out))) != 0)
            {
                if ((mod & Parameter.Modifier.IsByRef) == 0)
                {
                    parseContext.ReportError(
                        1615,
                        string.Format(
                            "Argument '#{0}' does not require '{1}' modifier.  Consider removing '{1}' modifier.",
                            index,
                            Parameter.GetModifierSignature(argument.Modifier)),
                        location);
                }
                else
                {
                    parseContext.ReportError(
                        1620,
                        string.Format(
                            "Argument '#{0}' is missing '{1}' modifier.",
                            index,
                            Parameter.GetModifierSignature(mod)),
                        location);
                }
            }
            else
            {
                string p1 = argument.GetSignatureForError();
                string p2 = TypeManager.GetCSharpName(argumentType);

                if (argumentIndex == 0 && emg != null)
                {
                    parseContext.ReportError(
                        1929,
                        string.Format("Extension method instance type '{0}' cannot be converted to '{1}'.", p1, p2),
                        location);
                }
                else
                {
                    parseContext.ReportError(
                        1503,
                        string.Format("Argument '#{0}' cannot convert '{1}' expression to type '{2}'.", index, p1, p2),
                        location);
                }
            }
        }

        public void Error_ValueCannotBeConverted(ParseContext ec, SourceSpan loc, Type target, bool expl)
        {
            ec.ReportError(
                428,
                string.Format(
                    "Cannot convert method group `{0}' to non-delegate type `{1}'. Consider using parentheses to invoke the method.",
                    Name,
                    TypeManager.GetCSharpName(target)),
                loc);
        }

        private void OnErrorArgumentCountWrong(ParseContext ec, int argCount)
        {
            ec.ReportError(
                1501,
                string.Format(
                    "No overload for method '{0}' takes '{1}' arguments.",
                    Name,
                    argCount),
                Span);
        }

        protected virtual int GetApplicableParametersCount(MethodBase method, ParametersCollection parameters)
        {
            return parameters.Count;
        }

        public static bool IsAncestralType(Type firstType, Type secondType)
        {
            return firstType != secondType &&
                (TypeManager.IsSubclassOf(secondType, firstType) ||
                TypeManager.ImplementsInterface(secondType, firstType));
        }

        ///
        /// Determines if the candidate method is applicable (section 14.4.2.1)
        /// to the given set of arguments
        /// A return value rates candidate method compatibility,
        /// 0 = the best, int.MaxValue = the worst
        ///
        public int IsApplicable(
            ParseContext parseContext,
            ref Arguments arguments,
            int argumentCount,
            ref MethodBase method,
            ref bool paramsExpandedForm)
        {
            MethodBase candidate = method;
            ParametersCollection pd = TypeManager.GetParameterData(candidate);

            int paramCount = GetApplicableParametersCount(candidate, pd);
            int optionalCount = 0;

            if (argumentCount != paramCount)
            {
                for (int i = 0; i < pd.Count; ++i)
                {
                    if (!pd.FixedParameters[i].HasDefaultValue)
                    {
                        continue;
                    }

                    optionalCount = pd.Count - i;
                    break;
                }

                int argsGap = Math.Abs(argumentCount - paramCount);
                if (optionalCount != 0)
                {
                    if (argsGap > optionalCount)
                    {
                        return int.MaxValue - 10000 + argsGap - optionalCount;
                    }

                    // Readjust expected number when params used
                    if (pd.HasParams)
                    {
                        optionalCount--;
                        if (argumentCount < paramCount)
                        {
                            paramCount--;
                        }
                    }
                    else if (argumentCount > paramCount)
                    {
                        return int.MaxValue - 10000 + argsGap;
                    }
                }
                else if (argumentCount != paramCount)
                {
                    if (!pd.HasParams)
                    {
                        return int.MaxValue - 10000 + argsGap;
                    }

                    if (argumentCount < paramCount - 1)
                    {
                        return int.MaxValue - 10000 + argsGap;
                    }
                }

                // Initialize expanded form of a method with 1 params parameter
                paramsExpandedForm = paramCount == 1 && pd.HasParams;

                // Resize to fit optional arguments
                if (optionalCount != 0)
                {
                    Arguments resized;
                    if (arguments == null)
                    {
                        resized = new Arguments(optionalCount);
                    }
                    else
                    {
                        resized = new Arguments(paramCount);
                        resized.AddRange(arguments);
                    }

                    for (int i = argumentCount; i < paramCount; ++i)
                    {
                        resized.Add(null);
                    }

                    arguments = resized;
                }
            }

            if (argumentCount > 0)
            {
                //
                // Shuffle named arguments to the right positions if there are any
                //
                if (arguments[argumentCount - 1] is NamedArgument)
                {
                    argumentCount = arguments.Count;

                    for (int i = 0; i < argumentCount; ++i)
                    {
                        bool argMoved = false;
                        while (true)
                        {
                            if (!(arguments[i] is NamedArgument namedArgument))
                                break;

                            int index = pd.GetParameterIndexByName(namedArgument.Name);

                            // Named parameter not found or already reordered
                            if (index <= i)
                            {
                                break;
                            }

                            // When using parameters which should not be available to the user
                            if (index >= paramCount)
                            {
                                break;
                            }

                            if (!argMoved)
                            {
                                arguments.MarkReorderedArgument(namedArgument);
                                argMoved = true;
                            }

                            Argument temp = arguments[index];
                            arguments[index] = arguments[i];
                            arguments[i] = temp;

                            if (temp == null)
                            {
                                break;
                            }
                        }
                    }
                }
                else
                {
                    argumentCount = arguments.Count;
                }
            }
            else if (arguments != null)
            {
                argumentCount = arguments.Count;
            }

            //
            // 1. Handle generic method using type arguments when specified or type inference
            //
            if (candidate.IsGenericMethod)
            {
                if (TypeArguments != null)
                {
                    Type[] genericArguments = candidate.GetGenericArguments();
                    if (genericArguments.Length != TypeArguments.Count)
                        return int.MaxValue - 20000 + Math.Abs(TypeArguments.Count - genericArguments.Length);

                    // TODO: Don't create new method, create Parameters only
                    method = ((MethodInfo)candidate).MakeGenericMethod(TypeArguments.ResolvedTypes);
                    candidate = method;
                    pd = TypeManager.GetParameterData(candidate);
                }
                else
                {
                    int score = TypeManager.InferTypeArguments(parseContext, arguments, ref candidate);
                    if (score != 0)
                    {
                        return score - 20000;
                    }

                    if (candidate.IsGenericMethodDefinition)
                    {
                        throw new InternalErrorException(
                            "A generic method '{0}' definition took part in overload resolution.",
                            TypeManager.GetCSharpSignature(candidate));
                    }

                    pd = TypeManager.GetParameterData(candidate);
                }
            }
            else
            {
                if (TypeArguments != null)
                {
                    return int.MaxValue - 15000;
                }
            }

            //
            // 2. Each argument has to be implicitly convertible to method parameter
            //
            method = candidate;
            Parameter.Modifier modifiers = 0;
            Type parameterType = null;


            for (int i = 0; i < argumentCount; i++)
            {
                Debug.Assert(arguments != null);

                Argument argument = arguments[i];
                if (argument == null)
                {
                    if (!pd.FixedParameters[i].HasDefaultValue)
                    {
                        throw new InternalErrorException();
                    }

                    Expression constant = pd.FixedParameters[i].DefaultValue as ConstantExpression ??
                                   new DefaultValueExpression(new TypeExpression(pd.Types[i], Span))
                                   {
                                       Span = Span
                                   }.Resolve(parseContext);

                    arguments[i] = new Argument(constant) { ArgumentType = ArgumentType.Default };
                    continue;
                }

                if (modifiers != Parameter.Modifier.Params)
                {
                    modifiers = pd.FixedParameters[i].ModifierFlags & ~(Parameter.Modifier.OutMask | Parameter.Modifier.RefMask);
                    parameterType = pd.Types[i];
                }
                else
                {
                    paramsExpandedForm = true;
                }

                Parameter.Modifier argumentModifier = argument.Modifier & ~(Parameter.Modifier.OutMask | Parameter.Modifier.RefMask);
                int score = 1;

                if (!paramsExpandedForm)
                {
                    score = IsArgumentCompatible(
                        parseContext,
                        argumentModifier,
                        argument,
                        modifiers & ~Parameter.Modifier.Params,
                        parameterType);
                }

                if (score != 0 && (modifiers & Parameter.Modifier.Params) != 0 && _delegateType == null)
                {
                    Debug.Assert(parameterType != null);

                    // It can be applicable in expanded form
                    score = IsArgumentCompatible(
                        parseContext,
                        argumentModifier,
                        argument,
                        0,
                        parameterType.GetElementType());

                    if (score == 0)
                    {
                        paramsExpandedForm = true;
                    }
                }

                if (score == 0)
                {
                    continue;
                }

                if (paramsExpandedForm)
                {
                    ++score;
                }

                return ((argumentCount - i) * 2) + score;
            }

            if (argumentCount != paramCount)
            {
                paramsExpandedForm = true;
            }

            return 0;
        }

        private static int IsArgumentCompatible(
            ParseContext ec,
            Parameter.Modifier argumentModifier,
            Argument argument,
            Parameter.Modifier parameterModifier,
            Type parameter)
        {
            //
            // Types have to be identical when ref or out modifer is used 
            //
            if (argumentModifier != 0 || parameterModifier != 0)
            {
                if (TypeManager.HasElementType(parameter))
                {
                    parameter = parameter.GetElementType();
                }

                Type argumentType = argument.Value.Type;

                if (TypeManager.HasElementType(argumentType))
                {
                    argumentType = argumentType.GetElementType();
                }

                if (argumentType != parameter)
                {
                    return 2;
                }
            }
            else
            {
                if (!TypeManager.ImplicitConversionExists(ec, argument.Value, parameter))
                {
                    return 2;
                }
            }

            return (argumentModifier != parameterModifier) ? 1 : 0;
        }

        public static bool IsOverride(MethodBase candidateMethod, MethodBase baseMethod)
        {
            if (!IsAncestralType(baseMethod.DeclaringType, candidateMethod.DeclaringType))
            {
                return false;
            }

            ParametersCollection candidateParameters = TypeManager.GetParameterData(candidateMethod);
            ParametersCollection baseParameters = TypeManager.GetParameterData(baseMethod);

            if (candidateParameters.Count != baseParameters.Count)
            {
                return false;
            }

            for (int j = 0; j < candidateParameters.Count; ++j)
            {
                Parameter.Modifier candidateModifiers = candidateParameters.FixedParameters[j].ModifierFlags;
                Parameter.Modifier baseModifiers = baseParameters.FixedParameters[j].ModifierFlags;
                Type candidateType = candidateParameters.Types[j];
                Type baseType = baseParameters.Types[j];

                if (candidateModifiers != baseModifiers || candidateType != baseType)
                {
                    return false;
                }
            }

            return true;
        }

        public static MethodGroupExpression MakeUnionSet(MethodGroupExpression methodGroup1, MethodGroupExpression methodGroup2, SourceSpan loc)
        {
            if (methodGroup1 == null)
            {
                return methodGroup2 ?? null;
            }

            return methodGroup2 == null
                ? methodGroup1
                : new MethodGroupExpression(
                methodGroup2.Methods.Where(m => !TypeManager.ArrayContainsMethod(methodGroup1.Methods, m, false)).ToArray(),
                null,
                loc);
        }

        private static Type MoreSpecific(Type p, Type q)
        {
            if (TypeManager.IsGenericParameter(p) && !TypeManager.IsGenericParameter(q))
            {
                return q;
            }

            if (!TypeManager.IsGenericParameter(p) && TypeManager.IsGenericParameter(q))
            {
                return p;
            }

            if (TypeManager.HasElementType(p))
            {
                Type pe = p.GetElementType();
                Type qe = q.GetElementType();
                Type specific = MoreSpecific(pe, qe);
                if (specific == pe)
                {
                    return p;
                }

                if (specific == qe)
                {
                    return q;
                }
            }
            else if (TypeManager.IsGenericType(p))
            {
                Type[] pArgs = p.GetGenericArguments();
                Type[] qArgs = q.GetGenericArguments();

                bool pSpecificAtLeastOnce = false;
                bool qSpecificAtLeastOnce = false;

                for (int i = 0; i < pArgs.Length; i++)
                {
                    Type specific = MoreSpecific(pArgs[i], qArgs[i]);
                    if (specific == pArgs[i])
                    {
                        pSpecificAtLeastOnce = true;
                    }

                    if (specific == qArgs[i])
                    {
                        qSpecificAtLeastOnce = true;
                    }
                }

                if (pSpecificAtLeastOnce && !qSpecificAtLeastOnce)
                {
                    return p;
                }

                if (!pSpecificAtLeastOnce && qSpecificAtLeastOnce)
                {
                    return q;
                }
            }

            return null;
        }

        /// <summary>
        ///   Find the Applicable Function Members (7.4.2.1)
        ///
        ///   me: Method Group expression with the members to select.
        ///       it might contain constructors or methods (or anything
        ///       that maps to a method).
        ///
        ///   Arguments: ArrayList containing resolved Argument objects.
        ///
        ///   loc: The location if we want an error to be reported, or a Null
        ///        location for "probing" purposes.
        ///
        ///   Returns: The MethodBase (either a ConstructorInfo or a MethodInfo)
        ///            that is the best match of me on Arguments.
        ///
        /// </summary>
        public virtual MethodGroupExpression OverloadResolve(
            ParseContext ec,
            ref Arguments arguments,
            bool mayFail,
            SourceSpan loc)
        {
            Type applicableType = null;
            List<MethodBase> candidates = new List<MethodBase>(2);
            List<MethodBase> candidateOverrides = null;

            //
            // Used to keep a map between the candidate
            // and whether it is being considered in its
            // normal or expanded form
            //
            // false is normal form, true is expanded form
            //
            Hashtable candidateToForm = null;
            Hashtable candidatesExpanded = null;

            Arguments candidateArgs = arguments;

            int argCount = arguments != null ? arguments.Count : 0;
            _ = Methods.Length;
            int methodCount;
            //
            // Methods marked 'override' don't take part in 'applicable_type'
            // computation, nor in the actual overload resolution.
            // However, they still need to be emitted instead of a base virtual method.
            // So, we salt them away into the 'candidate_overrides' array.
            //
            // In case of reflected methods, we replace each overriding method with
            // its corresponding base virtual method.  This is to improve compatibility
            // with non-C# libraries which change the visibility of overrides (#75636)
            //
            {
                int j = 0;
                for (int i = 0; i < Methods.Length; ++i)
                {
                    MethodBase m = Methods[i];
                    if (TypeManager.IsOverride(m))
                    {
                        if (candidateOverrides == null)
                        {
                            candidateOverrides = new List<MethodBase>();
                        }

                        candidateOverrides.Add(m);
                        m = TypeManager.TryGetBaseDefinition(m);
                    }
                    if (m != null)
                    {
                        Methods[j++] = m;
                    }
                }
                methodCount = j;
            }

            //
            // First we construct the set of applicable methods
            //
            bool isSorted = true;
            int bestCandidateRate = int.MaxValue;

            for (int i = 0; i < methodCount; i++)
            {
                Type declaringType = Methods[i].DeclaringType;

                //
                // If we have already found an applicable method
                // we eliminate all base types (Section 14.5.5.1)
                //
                if (applicableType != null && IsAncestralType(declaringType, applicableType))
                {
                    continue;
                }

                //
                // Check if candidate is applicable (section 14.4.2.1)
                //
                bool paramsExpandedForm = false;
                int candidateRate = IsApplicable(
                    ec,
                    ref candidateArgs,
                    argCount,
                    ref Methods[i],
                    ref paramsExpandedForm);

                if (candidateRate < bestCandidateRate)
                {
                    bestCandidateRate = candidateRate;
                    BestCandidate = Methods[i];
                }

                if (paramsExpandedForm)
                {
                    if (candidateToForm == null)
                    {
                        candidateToForm = new PtrHashtable();
                    }

                    MethodBase candidate = Methods[i];
                    candidateToForm[candidate] = candidate;
                }

                if (candidateArgs != arguments)
                {
                    if (candidatesExpanded == null)
                    {
                        candidatesExpanded = new Hashtable(2);
                    }

                    candidatesExpanded.Add(Methods[i], candidateArgs);
                    candidateArgs = arguments;
                }

                if (candidateRate != 0 || _hasInaccessibleCandidatesOnly)
                {
                    continue;
                }

                candidates.Add(Methods[i]);

                if (applicableType == null)
                {
                    applicableType = declaringType;
                }
                else if (applicableType != declaringType)
                {
                    isSorted = false;
                    if (IsAncestralType(applicableType, declaringType))
                    {
                        applicableType = declaringType;
                    }
                }
            }

            int candidateTop = candidates.Count;
            if (applicableType == null)
            {
                //
                // When we found a top level method which does not match and it's 
                // not an extension method. We start extension methods lookup from here
                //
                if (InstanceExpression != null)
                {
                    ExtensionMethodGroupExpression extensionMethod = ec.LookupExtensionMethod(Type, Name, loc);
                    if (extensionMethod != null)
                    {
                        extensionMethod.ExtensionExpression = InstanceExpression;
                        extensionMethod.SetTypeArguments(ec, TypeArguments);
                        return extensionMethod.OverloadResolve(ec, ref arguments, mayFail, loc);
                    }
                }

                if (mayFail)
                {
                    return null;
                }

                //
                // Okay so we have failed to find exact match so we
                // return error info about the closest match
                //
                if (BestCandidate != null)
                {
                    if (CustomErrorHandler != null && !_hasInaccessibleCandidatesOnly &&
                        CustomErrorHandler.NoExactMatch(ec, BestCandidate))
                    {
                        return null;
                    }

                    ParametersCollection parameterData = TypeManager.GetParameterData(BestCandidate);
                    bool candidateParams = candidateToForm != null && candidateToForm.Contains(BestCandidate);
                    if (argCount == parameterData.Count || parameterData.HasParams)
                    {
                        if (BestCandidate.IsGenericMethodDefinition)
                        {
                            if (TypeArguments == null)
                            {
                                ec.ReportError(
                                    411,
                                    string.Format(
                                        "The type arguments for method '{0}' cannot be inferred from " +
                                        "the usage.  Try specifying the type arguments explicitly.",
                                        TypeManager.GetCSharpSignature(BestCandidate)),
                                    loc);
                                return null;
                            }

                            Type[] g_args = BestCandidate.GetGenericArguments();
                            if (TypeArguments.Count != g_args.Length)
                            {
                                ec.ReportError(
                                    305,
                                    string.Format(
                                        "Using the generic method '{0}' requires '{1}' type argument(s).",
                                        TypeManager.GetCSharpSignature(BestCandidate),
                                        g_args.Length),
                                    loc);
                                return null;
                            }
                        }
                        else
                        {
                            if (TypeArguments != null && !BestCandidate.IsGenericMethod)
                            {
                                Debugger.Break();
                                // TODO: Error_TypeArgumentsCannotBeUsed(ec.Report, loc);
                                return null;
                            }
                        }

                        if (_hasInaccessibleCandidatesOnly)
                        {
                            if (InstanceExpression != null &&
                                TypeManager.IsNestedFamilyAccessible(typeof(TypeManager), BestCandidate.DeclaringType))
                            {
                                // Although a derived class can access protected members of
                                // its base class it cannot do so through an instance of the
                                // base class (CS1540).  If the qualifier_type is a base of the
                                // ec.CurrentType and the lookup succeeds with the latter one,
                                // then we are in this situation.
                                Debugger.Break();
                                // TODO: Error_CannotAccessProtected(ec, loc, _bestCandidate, _queriedType, ec.CurrentType);
                            }
                            else
                            {
                                ec.ReportError(
                                    CompilerErrors.MemberIsInaccessible,
                                    loc,
                                    GetSignatureForError());
                            }
                        }

                        if (!VerifyArgumentsCompat(ec, ref arguments, argCount, BestCandidate, candidateParams, mayFail, loc))
                        {
                            return null;
                        }

                        if (_hasInaccessibleCandidatesOnly)
                        {
                            return null;
                        }

                        throw new InternalErrorException(
                            "VerifyArgumentsCompat didn't find any problem with rejected candidate " + BestCandidate);
                    }
                }

                //
                // We failed to find any method with correct argument count
                //
                if (Name == ConstructorInfo.ConstructorName)
                {
                    ec.ReportError(
                        1729,
                        string.Format(
                            "The type '{0}' does not contain a constructor that takes `{1}' arguments.",
                            TypeManager.GetCSharpName(_queriedType),
                            argCount),
                        loc);
                }
                else
                {
                    OnErrorArgumentCountWrong(ec, argCount);
                }

                return null;
            }

            if (!isSorted)
            {
                //
                // At this point, applicable_type is _one_ of the most derived types
                // in the set of types containing the methods in this MethodGroup.
                // Filter the candidates so that they only contain methods from the
                // most derived types.
                //

                int finalized = 0; // Number of finalized candidates

                do
                {
                    // Invariant: applicable_type is a most derived type

                    // We'll try to complete Section 14.5.5.1 for 'applicable_type' by 
                    // eliminating all it's base types.  At the same time, we'll also move
                    // every unrelated type to the end of the array, and pick the next
                    // 'applicable_type'.

                    Type nextApplicableType = null;
                    int j = finalized; // where to put the next finalized candidate
                    int k = finalized; // where to put the next undiscarded candidate
                    for (int i = finalized; i < candidateTop; ++i)
                    {
                        MethodBase candidate = candidates[i];
                        Type declaringType = candidate.DeclaringType;

                        if (declaringType == applicableType)
                        {
                            candidates[k++] = candidates[j];
                            candidates[j++] = candidates[i];
                            continue;
                        }

                        if (IsAncestralType(declaringType, applicableType))
                        {
                            continue;
                        }

                        if (nextApplicableType != null &&
                            IsAncestralType(declaringType, nextApplicableType))
                        {
                            continue;
                        }

                        candidates[k++] = candidates[i];

                        if (nextApplicableType == null ||
                            IsAncestralType(nextApplicableType, declaringType))
                        {
                            nextApplicableType = declaringType;
                        }
                    }

                    applicableType = nextApplicableType;
                    finalized = j;
                    candidateTop = k;
                }
                while (applicableType != null);
            }

            //
            // Now we actually find the best method
            //

            BestCandidate = candidates[0];
            bool methodParams = candidateToForm != null && candidateToForm.Contains(BestCandidate);

            //
            // TODO: Broken inverse order of candidates logic does not work with optional
            // parameters used for method overrides and I am not going to fix it for SRE
            //
            if (candidatesExpanded != null && candidatesExpanded.Contains(BestCandidate))
            {
                candidateArgs = (Arguments)candidatesExpanded[BestCandidate];
                argCount = candidateArgs.Count;
            }

            for (int ix = 1; ix < candidateTop; ix++)
            {
                MethodBase candidate = candidates[ix];

                if (candidate == BestCandidate)
                {
                    continue;
                }

                bool candidateParams = candidateToForm != null && candidateToForm.Contains(candidate);

                if (BetterFunction(
                    ec,
                    candidateArgs,
                    argCount,
                    candidate,
                    candidateParams,
                    BestCandidate,
                    methodParams))
                {
                    BestCandidate = candidate;
                    methodParams = candidateParams;
                }
            }
            //
            // Now check that there are no ambiguities i.e the selected method
            // should be better than all the others
            //
            MethodBase ambiguous = null;
            for (int ix = 1; ix < candidateTop; ix++)
            {
                MethodBase candidate = candidates[ix];

                if (candidate == BestCandidate)
                {
                    continue;
                }

                bool candidateParams = candidateToForm != null && candidateToForm.Contains(candidate);
                if (!BetterFunction(
                         ec,
                         candidateArgs,
                         argCount,
                         BestCandidate,
                         methodParams,
                         candidate,
                         candidateParams))
                {
                    ambiguous = candidate;
                }
            }

            if (ambiguous != null)
            {
                OnErrorAmbiguousCall(ec, ambiguous);
                return this;
            }

            //
            // If the method is a virtual function, pick an override closer to the LHS type.
            //
            if (BestCandidate.IsVirtual)
            {
                if (TypeManager.IsOverride(BestCandidate))
                {
                    throw new InternalErrorException(
                        "Should not happen.  An 'override' method took part in overload resolution: " + BestCandidate);
                }

                if (candidateOverrides != null)
                {
                    Type[] genericArgs = null;
                    bool genericOverride = false;

                    if (BestCandidate.IsGenericMethod)
                    {
                        genericArgs = BestCandidate.GetGenericArguments();
                    }

                    foreach (MethodBase candidate in candidateOverrides)
                    {
                        if (candidate.IsGenericMethod)
                        {
                            if (genericArgs == null)
                            {
                                continue;
                            }

                            if (genericArgs.Length != candidate.GetGenericArguments().Length)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (genericArgs != null)
                            {
                                continue;
                            }
                        }

                        if (IsOverride(candidate, BestCandidate))
                        {
                            genericOverride = true;
                            BestCandidate = candidate;
                        }
                    }

                    if (genericOverride && genericArgs != null)
                    {
                        BestCandidate = ((MethodInfo)BestCandidate).MakeGenericMethod(genericArgs);
                    }
                }
            }

            //
            // And now check if the arguments are all
            // compatible, perform conversions if
            // necessary etc. and return if everything is
            // all right
            //
            if (!VerifyArgumentsCompat(
                     ec,
                     ref candidateArgs,
                     argCount,
                     BestCandidate,
                     methodParams,
                     mayFail,
                     loc))
            {
                return null;
            }

            if (BestCandidate == null)
            {
                return null;
            }

            MethodBase finalMethod = TypeManager.DropGenericMethodArguments(BestCandidate);

            if (finalMethod.IsGenericMethodDefinition &&
                !ConstraintChecker.CheckConstraints(ec, finalMethod, BestCandidate, loc))
            {
                return null;
            }

            arguments = candidateArgs;
            return this;
        }

        public override void SetTypeArguments(ParseContext ec, TypeArguments ta)
        {
            TypeArguments = ta;
        }

        public bool VerifyArgumentsCompat(
            ParseContext ec,
            ref Arguments arguments,
            int argCount,
            MethodBase method,
            bool choseParamsExpanded,
            bool mayFail,
            SourceSpan loc)
        {
            ParametersCollection parmeterData = TypeManager.GetParameterData(method);
            int parameterCount = GetApplicableParametersCount(method, parmeterData);

            int errors = ec.CompilerErrorCount;
            Parameter.Modifier parameterModifier = 0;
            Type pt = null;
            int argumentIndex = 0, argumentPosition = 0;
            Argument a = null;
            List<Expression> paramsInitializers = null;
            bool hasUnsafeArg = method is MethodInfo ? ((MethodInfo)method).ReturnType.IsPointer : false;

            for (; argumentIndex < argCount; argumentIndex++, ++argumentPosition)
            {
                a = arguments[argumentIndex];
                if (parameterModifier != Parameter.Modifier.Params)
                {
                    parameterModifier = parmeterData.FixedParameters[argumentIndex].ModifierFlags;
                    pt = parmeterData.Types[argumentIndex];
                    hasUnsafeArg |= pt.IsPointer;

                    if (parameterModifier == Parameter.Modifier.Params)
                    {
                        if (choseParamsExpanded)
                        {
                            paramsInitializers = new List<Expression>(argCount - argumentIndex);
                            pt = pt.GetElementType();
                        }
                    }
                }

                //
                // Types have to be identical when ref or out modifer is used 
                //
                if (a.Modifier != 0 || (parameterModifier & ~Parameter.Modifier.Params) != 0)
                {
                    if ((parameterModifier & ~Parameter.Modifier.Params) != a.Modifier)
                    {
                        break;
                    }

                    if (!TypeManager.IsEqual(a.Value.Type, pt))
                    {
                        break;
                    }

                    continue;
                }
                else
                {
                    if (a is NamedArgument namedArgument)
                    {
                        int nameIndex = parmeterData.GetParameterIndexByName(namedArgument.Name);
                        if (nameIndex < 0 || nameIndex >= parameterCount)
                        {
                            if (DeclaringType != null && TypeManager.IsDelegateType(DeclaringType))
                            {
                                ec.ReportError(
                                    1746,
                                    string.Format(
                                        "The delegate '{0}' does not contain a parameter named '{1}'.",
                                        TypeManager.GetCSharpName(DeclaringType),
                                        namedArgument.Name),
                                    namedArgument.Span);
                            }
                            else
                            {
                                ec.ReportError(
                                    1739,
                                    string.Format(
                                        "The best overloaded method match for '{0}' does not contain a parameter named '{1}'.",
                                        TypeManager.GetCSharpSignature(method),
                                        namedArgument.Name),
                                    namedArgument.Span);
                            }
                        }
                        else if (arguments[nameIndex] != a)
                        {
                            ec.ReportError(
                                1744,
                                string.Format(
                                    "Named argument '{0}' cannot be used for a parameter which has positional argument specified.",
                                    namedArgument.Name),
                                namedArgument.Span);
                        }
                    }
                }

                if (_delegateType != null && !IsTypeCovariant(a.Value, pt))
                {
                    break;
                }

                Expression conversion = ConvertExpression.MakeImplicitConversion(ec, a.Value, pt, loc).Resolve(ec);
                if (conversion == null)
                {
                    break;
                }

                //
                // Convert params arguments to an array initializer
                //
                if (paramsInitializers != null)
                {
                    // we choose to use 'a.Expr' rather than 'conv' so that
                    // we don't hide the kind of expression we have (esp. CompoundAssign.Helper)
                    paramsInitializers.Add(a.Value);
                    arguments.RemoveAt(argumentIndex--);
                    --argCount;
                    continue;
                }

                // Update the argument with the implicit conversion
                a.Value = conversion;
            }

            if (argumentIndex != argCount)
            {
                if (!mayFail && ec.CompilerErrorCount == errors)
                {
                    if (CustomErrorHandler != null)
                    {
                        CustomErrorHandler.NoExactMatch(ec, BestCandidate);
                    }
                    else
                    {
                        OnErrorInvalidArguments(ec, loc, argumentPosition, method, a, parmeterData, pt);
                    }
                }
                return false;
            }

            //
            // Fill not provided arguments required by params modifier
            //
            if (paramsInitializers == null && parmeterData.HasParams && argCount + 1 == parameterCount)
            {
                if (arguments == null)
                {
                    arguments = new Arguments(1);
                }

                pt = parmeterData.Types[parameterCount - 1];
                pt = pt.GetElementType();
                hasUnsafeArg |= pt.IsPointer;
                paramsInitializers = new List<Expression>(0);
            }

            //
            // Append an array argument with all params arguments
            //
            if (paramsInitializers != null)
            {
                ArrayInitializerExpression initializer = new ArrayInitializerExpression();
                foreach (Expression paramsInitializer in paramsInitializers)
                {
                    initializer.Values.Add(paramsInitializer);
                }

                _ = arguments.Add(
                    new Argument(
                        new ArrayCreationExpression
                        {
                            RankSpecifier = "[]",
                            BaseType = new TypeExpression(pt, loc),
                            Initializer = initializer
                        }.Resolve(ec)));
                argCount++;
            }

            if (argCount < parameterCount)
            {
                if (!mayFail)
                {
                    OnErrorArgumentCountWrong(ec, argCount);
                }

                return false;
            }

            if (hasUnsafeArg)
            {
                return false;
            }

            return true;
        }

        public static bool IsTypeCovariant(Expression a, Type b)
        {
            //
            // For each value parameter (a parameter with no ref or out modifier), an 
            // identity conversion or implicit reference conversion exists from the
            // parameter type in D to the corresponding parameter type in M
            //
            return a.Type == b ? true : TypeUtils.IsImplicitlyConvertible(a.Type, b);
        }
    }
}