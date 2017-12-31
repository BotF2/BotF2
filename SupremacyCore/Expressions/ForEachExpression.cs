// 
// ForEachExpression.cs
// 
// Copyright (c) 2013-2013 Mike Strobel
// 
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
// 
// All other rights reserved.
// 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Supremacy.Expressions
{
    public class ForEachExpression : Expression
    {
        private readonly ParameterExpression _variable;
        private readonly Expression _enumerable;
        private readonly Expression _body;

        private readonly LabelTarget _breakTarget;
        private readonly LabelTarget _continueTarget;

        public new ParameterExpression Variable
        {
            get { return _variable; }
        }

        public Expression Enumerable
        {
            get { return _enumerable; }
        }

        public Expression Body
        {
            get { return _body; }
        }

        public LabelTarget BreakTarget
        {
            get { return _breakTarget; }
        }

        public LabelTarget ContinueTarget
        {
            get { return _continueTarget; }
        }

        public override Type Type
        {
            get
            {
                if (_breakTarget != null)
                    return _breakTarget.Type;

                return typeof(void);
            }
        }

        internal ForEachExpression(ParameterExpression variable, Expression enumerable, Expression body, LabelTarget breakTarget, LabelTarget continueTarget)
        {
            _variable = variable;
            _enumerable = enumerable;
            _body = body;
            _breakTarget = breakTarget;
            _continueTarget = continueTarget;
        }

        public ForEachExpression Update(
            ParameterExpression variable, Expression enumerable, Expression body, LabelTarget breakTarget, LabelTarget continueTarget)
        {
            if (_variable == variable && _enumerable == enumerable && _body == body && _breakTarget == breakTarget && _continueTarget == continueTarget)
                return this;

            return CustomExpression.ForEach(variable, enumerable, body, continueTarget, breakTarget);
        }

        public override Expression Reduce()
        {
            // Avoid allocating an unnecessary enumerator for arrays.
            if (_enumerable.Type.IsArray)
                return ReduceForArray();

            return ReduceForEnumerable();
        }

        private Expression ReduceForArray()
        {
            var innerLoopBreak = Label("inner_loop_break");
            var innerLoopContinue = Label("inner_loop_continue");

            var @continue = _continueTarget ?? Label("continue");
            var @break = _breakTarget ?? Label("break");

            var index = Variable(typeof(int), "i");

            return Block(
                new[] { index, _variable },
                Assign(index, Constant(0)),
                Loop(
                    Block(
                        IfThen(
                            IsFalse(
                                LessThan(
                                    index,
                                    ArrayLength(_enumerable))),
                            Break(innerLoopBreak)),
                        Assign(
                            _variable,
                            Convert(
                                ArrayIndex(
                                    _enumerable,
                                    index),
                                _variable.Type)),
                        _body,
                        Label(@continue),
                        PreIncrementAssign(index)),
                    innerLoopBreak,
                    innerLoopContinue),
                Label(@break));
        }

        private Expression ReduceForEnumerable()
        {
            MethodInfo getEnumerator;
            MethodInfo moveNext;
            MethodInfo getCurrent;

            ResolveEnumerationMembers(out getEnumerator, out moveNext, out getCurrent);

            var enumeratorType = getEnumerator.ReturnType;

            var enumerator = Variable(enumeratorType);

            var innerLoopContinue = Label("inner_loop_continue");
            var innerLoopBreak = Label("inner_loop_break");
            var @continue = _continueTarget ?? Label("continue");
            var @break = _breakTarget ?? Label("break");

            Expression variableInitializer;

            if (_variable.Type.IsAssignableFrom(getCurrent.ReturnType))
                variableInitializer = Property(enumerator, getCurrent);
            else
                variableInitializer = Convert(Property(enumerator, getCurrent), _variable.Type);

            Expression loop = Block(
                new[] { _variable },
                Goto(@continue),
                Loop(
                    Block(
                        Assign(_variable, variableInitializer),
                        _body,
                        Label(@continue),
                        Condition(
                            Call(enumerator, moveNext),
                            Goto(innerLoopContinue),
                            Goto(innerLoopBreak))),
                    innerLoopBreak,
                    innerLoopContinue),
                Label(@break));

            var dispose = CreateDisposeOperation(enumeratorType, enumerator);

            return Block(
                new[] { enumerator },
                Assign(enumerator, Call(_enumerable, getEnumerator)),
                dispose != null
                    ? TryFinally(loop, dispose)
                    : loop);
        }

        private void ResolveEnumerationMembers(
            out MethodInfo getEnumerator,
            out MethodInfo moveNext,
            out MethodInfo getCurrent)
        {
            Type itemType;
            Type enumerableType;
            Type enumeratorType;

            if (TryGetGenericEnumerableArgument(out itemType))
            {
                enumerableType = typeof(IEnumerable<>).MakeGenericType(itemType);
                enumeratorType = typeof(IEnumerator<>).MakeGenericType(itemType);
            }
            else
            {
                enumerableType = typeof(IEnumerable);
                enumeratorType = typeof(IEnumerator);
            }

            moveNext = typeof(IEnumerator).GetMethod("MoveNext");
            getCurrent = enumeratorType.GetProperty("Current").GetGetMethod();
            getEnumerator = _enumerable.Type.GetMethod("GetEnumerator", BindingFlags.Public | BindingFlags.Instance);

            //
            // We want to avoid unnecessarily boxing an enumerator if it's a value type.  Look
            // for a GetEnumerator() method that conforms to the rules of the C# 'foreach'
            // pattern.  If we don't find one, fall back to IEnumerable[<T>].GetEnumerator().
            //

            if (getEnumerator == null || !enumeratorType.IsAssignableFrom(getEnumerator.ReturnType))
            {
                getEnumerator = enumerableType.GetMethod("GetEnumerator");
            }
        }

        private static Expression CreateDisposeOperation(Type enumeratorType, ParameterExpression enumerator)
        {
            var dispose = typeof(IDisposable).GetMethod("Dispose");

            if (typeof(IDisposable).IsAssignableFrom(enumeratorType))
            {
                //
                // We know the enumerator implements IDisposable, so skip the type check.
                //
                return Call(enumerator, dispose);
            }

            if (enumeratorType.IsValueType)
            {
                //
                // The enumerator is a value type and doesn't implement IDisposable; we needn't
                // bother with a check at all.
                //
                return null;
            }

            //
            // We don't know whether the enumerator implements IDisposable or not.  Emit a
            // runtime check.
            //

            var disposable = Variable(typeof(IDisposable));

            return Block(
                new[] { disposable },
                Assign(disposable, TypeAs(enumerator, typeof(IDisposable))),
                IfThen(
                    ReferenceNotEqual(disposable, Constant(null)),
                    Call(
                        disposable,
                        "Dispose",
                        Type.EmptyTypes)));
        }

        private bool TryGetGenericEnumerableArgument(out Type argument)
        {
            argument = null;

            foreach (var iface in _enumerable.Type.GetInterfaces())
            {
                if (!iface.IsGenericType)
                    continue;

                var definition = iface.GetGenericTypeDefinition();
                if (definition != typeof(IEnumerable<>))
                    continue;

                argument = iface.GetGenericArguments()[0];
                if (_variable.Type.IsAssignableFrom(argument))
                    return true;
            }

            return false;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            return Update(
                (ParameterExpression)visitor.Visit(_variable),
                visitor.Visit(_enumerable),
                visitor.Visit(_body),
                _breakTarget,
                _continueTarget);
        }
    }

    public abstract class CustomExpression
    {
        public static ForEachExpression ForEach(ParameterExpression variable, Expression enumerable, Expression body)
        {
            return ForEach(variable, enumerable, body, null);
        }

        public static ForEachExpression ForEach(ParameterExpression variable, Expression enumerable, Expression body, LabelTarget breakTarget)
        {
            return ForEach(variable, enumerable, body, breakTarget, null);
        }

        public static ForEachExpression ForEach(
            ParameterExpression variable, Expression enumerable, Expression body, LabelTarget breakTarget, LabelTarget continueTarget)
        {
            if (variable == null)
                throw new ArgumentNullException("variable");
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");
            if (body == null)
                throw new ArgumentNullException("body");

            if (!typeof(IEnumerable).IsAssignableFrom(enumerable.Type))
                throw new ArgumentException("The enumerable must implement at least IEnumerable", "enumerable");

            if (continueTarget != null && continueTarget.Type != typeof(void))
                throw new ArgumentException("Continue label target must be void", "continueTarget");

            return new ForEachExpression(variable, enumerable, body, breakTarget, continueTarget);
        }
    }
}
