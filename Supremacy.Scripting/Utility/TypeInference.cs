using System;
using System.Collections;
using System.Reflection;

using Supremacy.Scripting.Ast;
using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Utility
{
    abstract class TypeInferenceBase
    {
        protected readonly Arguments arguments;
        protected readonly int arg_count;

        protected TypeInferenceBase(Arguments arguments)
        {
            this.arguments = arguments;
            if (arguments != null)
                arg_count = arguments.Count;
        }

        public static TypeInferenceBase CreateInstance(Arguments arguments)
        {
            return new TypeInference(arguments);
        }

        public virtual int InferenceScore
        {
            get
            {
                return int.MaxValue;
            }
        }

        public abstract Type[] InferMethodArguments(ParseContext ec, MethodBase method);
        //		public abstract Type[] InferDelegateArguments (ParseContext ec, MethodBase method);
    }

    //
    // Implements C# type inference
    //
    class TypeInference : TypeInferenceBase
    {
        //
        // Tracks successful rate of type inference
        //
        int score = int.MaxValue;

        public TypeInference(Arguments arguments)
            : base(arguments)
        {
        }

        public override int InferenceScore
        {
            get
            {
                return score;
            }
        }

        /*
                public override Type[] InferDelegateArguments (ParseContext ec, MethodBase method)
                {
                    AParametersCollection pd = TypeManager.GetParameterData (method);
                    if (arg_count != pd.Count)
                        return null;

                    Type[] d_gargs = method.GetGenericArguments ();
                    TypeInferenceContext context = new TypeInferenceContext (d_gargs);

                    // A lower-bound inference is made from each argument type Uj of D
                    // to the corresponding parameter type Tj of M
                    for (int i = 0; i < arg_count; ++i) {
                        Type t = pd.Types [i];
                        if (!t.IsGenericParameter)
                            continue;

                        context.LowerBoundInference (arguments [i].Expr.Type, t);
                    }

                    if (!context.FixAllTypes (ec))
                        return null;

                    return context.InferredTypeArguments;
                }
        */
        public override Type[] InferMethodArguments(ParseContext ec, MethodBase method)
        {
            Type[] method_generic_args = method.GetGenericArguments();
            TypeInferenceContext context = new TypeInferenceContext(method_generic_args);
            if (!context.UnfixedVariableExists)
                return Type.EmptyTypes;

            ParametersCollection pd = TypeManager.GetParameterData(method);
            if (!InferInPhases(ec, context, pd))
                return null;

            return context.InferredTypeArguments;
        }

        //
        // Implements method type arguments inference
        //
        bool InferInPhases(ParseContext ec, TypeInferenceContext tic, ParametersCollection methodParameters)
        {
            int paramsArgumentsStart;
            if (methodParameters.HasParams)
            {
                paramsArgumentsStart = methodParameters.Count - 1;
            }
            else
            {
                paramsArgumentsStart = arg_count;
            }

            Type[] ptypes = methodParameters.Types;

            //
            // The first inference phase
            //
            Type methodParameter = null;
            for (int i = 0; i < arg_count; i++)
            {
                Argument a = arguments[i];
                if (a == null)
                    continue;

                if (i < paramsArgumentsStart)
                {
                    methodParameter = methodParameters.Types[i];
                }
                else if (i == paramsArgumentsStart)
                {
                    if (arg_count == paramsArgumentsStart + 1 && TypeManager.HasElementType(a.Value.Type))
                        methodParameter = methodParameters.Types[paramsArgumentsStart];
                    else
                        methodParameter = methodParameters.Types[paramsArgumentsStart].GetElementType();

                    ptypes = (Type[])ptypes.Clone();
                    ptypes[i] = methodParameter;
                }

                //
                // When a lambda expression, an anonymous method
                // is used an explicit argument type inference takes a place
                //
                var am = a.Value as LambdaExpression;
                if (am != null)
                {
                    if (am.ExplicitTypeInference(ec, tic, methodParameter))
                        --score;
                    continue;
                }

                if (a.IsByRef)
                {
                    score -= tic.ExactInference(a.Value.Type, methodParameter);
                    continue;
                }

                if (a.Value.Type == TypeManager.CoreTypes.Null)
                    continue;

                if (TypeManager.IsValueType(methodParameter))
                {
                    score -= tic.LowerBoundInference(a.Value.Type, methodParameter);
                    continue;
                }

                //
                // Otherwise an output type inference is made
                //
                score -= tic.OutputTypeInference(ec, a.Value, methodParameter);
            }

            //
            // Part of the second phase but because it happens only once
            // we don't need to call it in cycle
            //
            var fixedAny = false;
            if (!tic.FixIndependentTypeArguments(ec, ptypes, ref fixedAny))
                return false;

            return DoSecondPhase(ec, tic, ptypes, !fixedAny);
        }

        bool DoSecondPhase(ParseContext ec, TypeInferenceContext tic, Type[] methodParameters, bool fixDependent)
        {
            var fixedAny = false;
            if (fixDependent && !tic.FixDependentTypes(ec, ref fixedAny))
                return false;

            // If no further unfixed type variables exist, type inference succeeds
            if (!tic.UnfixedVariableExists)
                return true;

            if (!fixedAny && fixDependent)
                return false;

            // For all arguments where the corresponding argument output types
            // contain unfixed type variables but the input types do not,
            // an output type inference is made
            for (int i = 0; i < arg_count; i++)
            {

                // Align params arguments
                Type iType = methodParameters[i >= methodParameters.Length ? methodParameters.Length - 1 : i];

                if (!TypeManager.IsDelegateType(iType))
                {
                    if (TypeManager.DropGenericTypeArguments(iType) != TypeManager.CoreTypes.GenericExpression)
                        continue;

                    iType = iType.GetGenericArguments()[0];
                }

                var methodInfo = iType.GetMethod("Invoke");
                var returnType = methodInfo.ReturnType;

                if (returnType.IsGenericParameter)
                {
                    // Blablabla, because reflection does not work with dynamic types
                    var genericArguments = iType.GetGenericArguments();
                    returnType = genericArguments[returnType.GenericParameterPosition];
                }

                if (tic.IsReturnTypeNonDependent(ec, methodInfo, returnType))
                    score -= tic.OutputTypeInference(ec, arguments[i].Value, iType);
            }


            return DoSecondPhase(ec, tic, methodParameters, true);
        }
    }

    public class TypeInferenceContext
    {
        enum BoundKind
        {
            Exact = 0,
            Lower = 1,
            Upper = 2
        }

        class BoundInfo
        {
            public readonly Type Type;
            public readonly BoundKind Kind;

            public BoundInfo(Type type, BoundKind kind)
            {
                Type = type;
                Kind = kind;
            }

            public override int GetHashCode()
            {
                return Type.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                BoundInfo a = (BoundInfo)obj;
                return Type == a.Type && Kind == a.Kind;
            }
        }

        readonly Type[] unfixed_types;
        readonly Type[] fixed_types;
        readonly ArrayList[] bounds;
        bool failed;

        public TypeInferenceContext(Type[] typeArguments)
        {
            if (typeArguments.Length == 0)
                throw new ArgumentException("Empty generic arguments");

            fixed_types = new Type[typeArguments.Length];
            for (int i = 0; i < typeArguments.Length; ++i)
            {
                if (typeArguments[i].IsGenericParameter)
                {
                    if (bounds == null)
                    {
                        bounds = new ArrayList[typeArguments.Length];
                        unfixed_types = new Type[typeArguments.Length];
                    }
                    unfixed_types[i] = typeArguments[i];
                }
                else
                {
                    fixed_types[i] = typeArguments[i];
                }
            }
        }

        // 
        // Used together with AddCommonTypeBound fo implement
        // 7.4.2.13 Finding the best common type of a set of expressions
        //
        public TypeInferenceContext()
        {
            fixed_types = new Type[1];
            unfixed_types = new Type[1];
            unfixed_types[0] = typeof(Argument); // it can be any internal type
            bounds = new ArrayList[1];
        }

        public Type[] InferredTypeArguments
        {
            get
            {
                return fixed_types;
            }
        }

        public void AddCommonTypeBound(Type type)
        {
            AddToBounds(new BoundInfo(type, BoundKind.Lower), 0);
        }

        void AddToBounds(BoundInfo bound, int index)
        {
            //
            // Some types cannot be used as type arguments
            //
            if (bound.Type == TypeManager.CoreTypes.Void || bound.Type.IsPointer)
                return;

            ArrayList a = bounds[index];
            if (a == null)
            {
                a = new ArrayList();
                bounds[index] = a;
            }
            else
            {
                if (a.Contains(bound))
                    return;
            }

            //
            // SPEC: does not cover type inference using constraints
            //
            //if (TypeManager.IsGenericParameter (t)) {
            //    GenericConstraints constraints = TypeManager.GetTypeParameterConstraints (t);
            //    if (constraints != null) {
            //        //if (constraints.EffectiveBaseClass != null)
            //        //	t = constraints.EffectiveBaseClass;
            //    }
            //}
            a.Add(bound);
        }

        bool AllTypesAreFixed(Type[] types)
        {
            foreach (Type t in types)
            {
                if (t.IsGenericParameter)
                {
                    if (!IsFixed(t))
                        return false;
                    continue;
                }

                if (t.IsGenericType)
                    return AllTypesAreFixed(t.GetGenericArguments());
            }

            return true;
        }

        //
        // 26.3.3.8 Exact Inference
        //
        public int ExactInference(Type u, Type v)
        {
            // If V is an array type
            if (v.IsArray)
            {
                if (!u.IsArray)
                    return 0;

                if (u.GetArrayRank() != v.GetArrayRank())
                    return 0;

                return ExactInference(u.GetElementType(), v.GetElementType());
            }

            // If V is constructed type and U is constructed type
            if (v.IsGenericType && !v.IsGenericTypeDefinition)
            {
                if (!u.IsGenericType)
                    return 0;

                Type[] ga_u = u.GetGenericArguments();
                Type[] ga_v = v.GetGenericArguments();
                if (ga_u.Length != ga_v.Length)
                    return 0;

                int score = 0;
                for (int i = 0; i < ga_u.Length; ++i)
                    score += ExactInference(ga_u[i], ga_v[i]);

                return score > 0 ? 1 : 0;
            }

            // If V is one of the unfixed type arguments
            int pos = IsUnfixed(v);
            if (pos == -1)
                return 0;

            AddToBounds(new BoundInfo(u, BoundKind.Exact), pos);
            return 1;
        }

        public bool FixAllTypes(ParseContext ec)
        {
            for (int i = 0; i < unfixed_types.Length; ++i)
            {
                if (!FixType(ec, i))
                    return false;
            }
            return true;
        }

        //
        // All unfixed type variables Xi are fixed for which all of the following hold:
        // a, There is at least one type variable Xj that depends on Xi
        // b, Xi has a non-empty set of bounds
        // 
        public bool FixDependentTypes(ParseContext ec, ref bool fixed_any)
        {
            for (int i = 0; i < unfixed_types.Length; ++i)
            {
                if (unfixed_types[i] == null)
                    continue;

                if (bounds[i] == null)
                    continue;

                if (!FixType(ec, i))
                    return false;

                fixed_any = true;
            }

            return true;
        }

        //
        // All unfixed type variables Xi which depend on no Xj are fixed
        //
        public bool FixIndependentTypeArguments(ParseContext ec, Type[] methodParameters, ref bool fixed_any)
        {
            ArrayList types_to_fix = new ArrayList(unfixed_types);
            for (int i = 0; i < methodParameters.Length; ++i)
            {
                Type t = methodParameters[i];

                if (!TypeManager.IsDelegateType(t))
                {
                    if (TypeManager.DropGenericTypeArguments(t) != TypeManager.CoreTypes.GenericExpression)
                        continue;

                    t = t.GetGenericArguments()[0];
                }

                if (t.IsGenericParameter)
                    continue;

                MethodInfo invoke = TypeManager.GetDelegateInvokeMethod(ec, t, t);
                Type rtype = invoke.ReturnType;
                if (!rtype.IsGenericParameter && !rtype.IsGenericType)
                    continue;

				// Blablabla, because reflection does not work with dynamic types
				if (rtype.IsGenericParameter) {
					Type [] g_args = t.GetGenericArguments ();
					rtype = g_args [rtype.GenericParameterPosition];
				}
      
                // Remove dependent types, they cannot be fixed yet
                RemoveDependentTypes(types_to_fix, rtype);
            }

            foreach (Type t in types_to_fix)
            {
                if (t == null)
                    continue;

                int idx = IsUnfixed(t);
                if (idx >= 0 && !FixType(ec, idx))
                {
                    return false;
                }
            }

            fixed_any = types_to_fix.Count > 0;
            return true;
        }

        //
        // 26.3.3.10 Fixing
        //
        public bool FixType(ParseContext ec, int i)
        {
            // It's already fixed
            if (unfixed_types[i] == null)
                throw new InternalErrorException("Type argument has been already fixed");

            if (failed)
                return false;

            ArrayList candidates = (ArrayList)bounds[i];
            if (candidates == null)
                return false;

            if (candidates.Count == 1)
            {
                unfixed_types[i] = null;
                Type t = ((BoundInfo)candidates[0]).Type;
                if (t == TypeManager.CoreTypes.Null)
                    return false;

                fixed_types[i] = t;
                return true;
            }

            //
            // Determines a unique type from which there is
            // a standard implicit conversion to all the other
            // candidate types.
            //
            Type best_candidate = null;
            int candidates_count = candidates.Count;
            for (int ci = 0; ci < candidates_count; ++ci)
            {
                BoundInfo bound = (BoundInfo)candidates[ci];
                int cii;
                for (cii = 0; cii < candidates_count; ++cii)
                {
                    if (cii == ci)
                        continue;

                    BoundInfo cbound = (BoundInfo)candidates[cii];

                    // Same type parameters with different bounds
                    if (cbound.Type == bound.Type)
                    {
                        if (bound.Kind != BoundKind.Exact)
                            bound = cbound;

                        continue;
                    }

                    if (bound.Kind == BoundKind.Exact || cbound.Kind == BoundKind.Exact)
                    {
                        if (cbound.Kind != BoundKind.Exact)
                        {
                            if (!TypeUtils.IsImplicitlyConvertible(cbound.Type, bound.Type))
                                break;

                            continue;
                        }

                        if (bound.Kind != BoundKind.Exact)
                        {
                            if (!TypeUtils.IsImplicitlyConvertible(bound.Type, cbound.Type))
                                break;

                            bound = cbound;
                            continue;
                        }

                        break;
                    }

                    if (bound.Kind == BoundKind.Lower)
                    {
                        if (!TypeUtils.IsImplicitlyConvertible(cbound.Type, bound.Type))
                            break;
                    }
                    else
                    {
                        if (!TypeUtils.IsImplicitlyConvertible(bound.Type, cbound.Type))
                            break;
                    }
                }

                if (cii != candidates_count)
                    continue;

                if (best_candidate != null && best_candidate != bound.Type)
                    return false;

                best_candidate = bound.Type;
            }

            if (best_candidate == null)
                return false;

            unfixed_types[i] = null;
            fixed_types[i] = best_candidate;
            return true;
        }

        //
        // Uses inferred types to inflate delegate type argument
        //
        public Type InflateGenericArgument(Type parameter)
        {
            if (parameter.IsGenericParameter)
            {
                //
                // Inflate method generic argument (MVAR) only
                //
                if (parameter.DeclaringMethod == null)
                    return parameter;

                return fixed_types[parameter.GenericParameterPosition];
            }

            if (parameter.IsGenericType)
            {
                Type[] parameter_targs = parameter.GetGenericArguments();
                for (int ii = 0; ii < parameter_targs.Length; ++ii)
                {
                    parameter_targs[ii] = InflateGenericArgument(parameter_targs[ii]);
                }
                return parameter.GetGenericTypeDefinition().MakeGenericType(parameter_targs);
            }

            return parameter;
        }

        //
        // Tests whether all delegate input arguments are fixed and generic output type
        // requires output type inference 
        //
        public bool IsReturnTypeNonDependent(ParseContext ec, MethodInfo invoke, Type returnType)
        {
            if (returnType.IsGenericParameter)
            {
                if (IsFixed(returnType))
                    return false;
            }
            else if (returnType.IsGenericType)
            {
                if (TypeManager.IsDelegateType(returnType))
                {
                    invoke = TypeManager.GetDelegateInvokeMethod(ec, returnType, returnType);
                    return IsReturnTypeNonDependent(ec, invoke, invoke.ReturnType);
                }

                Type[] g_args = returnType.GetGenericArguments();

                // At least one unfixed return type has to exist 
                if (AllTypesAreFixed(g_args))
                    return false;
            }
            else
            {
                return false;
            }

            // All generic input arguments have to be fixed
            ParametersCollection d_parameters = TypeManager.GetParameterData(invoke);
            return AllTypesAreFixed(d_parameters.Types);
        }

        bool IsFixed(Type type)
        {
            return IsUnfixed(type) == -1;
        }

        int IsUnfixed(Type type)
        {
            if (!type.IsGenericParameter)
                return -1;

            //return unfixed_types[type.GenericParameterPosition] != null;
            for (int i = 0; i < unfixed_types.Length; ++i)
            {
                if (unfixed_types[i] == type)
                    return i;
            }

            return -1;
        }

        //
        // 26.3.3.9 Lower-bound Inference
        //
        public int LowerBoundInference(Type u, Type v)
        {
            return LowerBoundInference(u, v, false);
        }

        //
        // Lower-bound (false) or Upper-bound (true) inference based on inversed argument
        //
        int LowerBoundInference(Type u, Type v, bool inversed)
        {
            // If V is one of the unfixed type arguments
            int pos = IsUnfixed(v);
            if (pos != -1)
            {
                AddToBounds(new BoundInfo(u, inversed ? BoundKind.Upper : BoundKind.Lower), pos);
                return 1;
            }

            // If U is an array type
            if (u.IsArray)
            {
                int u_dim = u.GetArrayRank();
                Type v_i;
                Type u_i = u.GetElementType();

                if (v.IsArray)
                {
                    if (u_dim != v.GetArrayRank())
                        return 0;

                    v_i = v.GetElementType();

                    if (TypeManager.IsValueType(u_i))
                        return ExactInference(u_i, v_i);

                    return LowerBoundInference(u_i, v_i, inversed);
                }

                if (u_dim != 1)
                    return 0;

                if (v.IsGenericType)
                {
                    Type g_v = v.GetGenericTypeDefinition();
                    if ((g_v != TypeManager.CoreTypes.GenericListInterface) && (g_v != TypeManager.CoreTypes.GenericCollectionInterface) &&
                        (g_v != TypeManager.CoreTypes.GenericEnumerableInterface))
                        return 0;

                    v_i = v.GetGenericArguments()[0];
                    if (TypeManager.IsValueType(u_i))
                        return ExactInference(u_i, v_i);

                    return LowerBoundInference(u_i, v_i);
                }
            }
            else if (v.IsGenericType && !v.IsGenericTypeDefinition)
            {
                //
                // if V is a constructed type C<V1..Vk> and there is a unique type C<U1..Uk>
                // such that U is identical to, inherits from (directly or indirectly),
                // or implements (directly or indirectly) C<U1..Uk>
                //
                ArrayList u_candidates = new ArrayList();
                if (u.IsGenericType)
                    u_candidates.Add(u);

                for (Type t = u.BaseType; t != null; t = t.BaseType)
                {
                    if (t.IsGenericType && !t.IsGenericTypeDefinition)
                        u_candidates.Add(t);
                }

                // TODO: Implement GetGenericInterfaces only and remove
                // the if from foreach
                u_candidates.AddRange(TypeManager.GetInterfaces(u));

                Type open_v = v.GetGenericTypeDefinition();
                Type[] unique_candidate_targs = null;
                Type[] ga_v = v.GetGenericArguments();
                foreach (Type u_candidate in u_candidates)
                {
                    if (!u_candidate.IsGenericType || u_candidate.IsGenericTypeDefinition)
                        continue;

                    if (TypeManager.DropGenericTypeArguments(u_candidate) != open_v)
                        continue;

                    //
                    // The unique set of types U1..Uk means that if we have an interface I<T>,
                    // class U : I<int>, I<long> then no type inference is made when inferring
                    // type I<T> by applying type U because T could be int or long
                    //
                    if (unique_candidate_targs != null)
                    {
                        Type[] secondUniqueCandidateTargs = u_candidate.GetGenericArguments();
                        if (TypeManager.IsEqual(unique_candidate_targs, secondUniqueCandidateTargs))
                        {
                            unique_candidate_targs = secondUniqueCandidateTargs;
                            continue;
                        }

                        //
                        // This should always cause type inference failure
                        //
                        failed = true;
                        return 1;
                    }

                    unique_candidate_targs = u_candidate.GetGenericArguments();
                }

                if (unique_candidate_targs != null)
                {
                    Type[] gaOpenV = open_v.GetGenericArguments();
                    int score = 0;
                    for (int i = 0; i < unique_candidate_targs.Length; ++i)
                    {
                        Variance variance = TypeManager.GetTypeParameterVariance(gaOpenV[i]);

                        Type u_i = unique_candidate_targs[i];
                        if (variance == Variance.None || TypeManager.IsValueType(u_i))
                        {
                            if (ExactInference(u_i, ga_v[i]) == 0)
                                ++score;
                        }
                        else
                        {
                            bool upper_bound = (variance == Variance.Contravariant && !inversed) ||
                                (variance == Variance.Covariant && inversed);

                            if (LowerBoundInference(u_i, ga_v[i], upper_bound) == 0)
                                ++score;
                        }
                    }
                    return score;
                }
            }

            return 0;
        }

        //
        // 26.3.3.6 Output Type Inference
        //
        public int OutputTypeInference(ParseContext ec, Expression e, Type t)
        {
            // If e is a lambda or anonymous method with inferred return type
            var ame = e as LambdaExpression;
            if (ame != null)
            {
                Type rt = ame.InferReturnType(ec, this, t);
                MethodInfo invoke = t.GetMethod("Invoke");

                if (rt == null)
                {
                    ParametersCollection pd = TypeManager.GetParameterData(invoke);
                    return ame.Parameters.Count == pd.Count ? 1 : 0;
                }

                Type rtype = invoke.ReturnType;
				if (rt.IsGenericParameter)
				{
                    // Blablabla, because reflection does not work with dynamic types
                    Type[] g_args = t.GetGenericArguments();
                    rtype = g_args[rtype.GenericParameterPosition];
				}
   
                return LowerBoundInference(rt, rtype) + 1;
            }

            //
            // if E is a method group and T is a delegate type or expression tree type
            // return type Tb with parameter types T1..Tk and return type Tb, and overload
            // resolution of E with the types T1..Tk yields a single method with return type U,
            // then a lower-bound inference is made from U for Tb.
            //
            if (e is MethodGroupExpression)
            {
                // TODO: Or expression tree
                if (!TypeManager.IsDelegateType(t))
                    return 0;

                MethodInfo invoke = TypeManager.GetDelegateInvokeMethod(ec, t, t);
                Type rtype = invoke.ReturnType;
				// Blablabla, because reflection does not work with dynamic types
				Type [] g_args = t.GetGenericArguments ();
				rtype = g_args [rtype.GenericParameterPosition];

                if (!TypeManager.IsGenericType(rtype))
                    return 0;

                var mg = (MethodGroupExpression)e;
                Arguments args = Arguments.CreateDelegateMethodArguments(TypeManager.GetParameterData(invoke), e.Span);
                mg = mg.OverloadResolve(ec, ref args, true, e.Span);
                if (mg == null)
                    return 0;

                // TODO: What should happen when return type is of generic type ?
                throw new NotImplementedException();
                //				return LowerBoundInference (null, rtype) + 1;
            }

            //
            // if e is an expression with type U, then
            // a lower-bound inference is made from U for T
            //
            return LowerBoundInference(e.Type, t) * 2;
        }

        void RemoveDependentTypes(ArrayList types, Type returnType)
        {
            int idx = IsUnfixed(returnType);
            if (idx >= 0)
            {
                types[idx] = null;
                return;
            }

            if (returnType.IsGenericType)
            {
                foreach (Type t in returnType.GetGenericArguments())
                {
                    RemoveDependentTypes(types, t);
                }
            }
        }

        public bool UnfixedVariableExists
        {
            get
            {
                if (unfixed_types == null)
                    return false;

                foreach (Type ut in unfixed_types)
                    if (ut != null)
                        return true;
                return false;
            }
        }
    }
}