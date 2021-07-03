// ValueModifier.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Linq.Expressions;

namespace Supremacy.Types
{
    [Serializable]
    public class ValueModifier : ValueModifier<int> { }

    [Serializable]
    public class ValueModifier<T> where T : IConvertible
    {
        protected const byte CompoundMultiplierFlag = 1 << 0;
        protected const byte ApplyOffsetFirstFlag = 1 << 1;

        // ReSharper disable StaticFieldInGenericType
        private static readonly Func<T, T, Percentage, T> _compoundMultiplyExpression;
        private static readonly Func<T, T, Percentage, T> _standardMultiplyExpression;
        private static readonly Func<T, T, int, T> _offsetExpression;
        // ReSharper restore StaticFieldInGenericType

        private byte _flags;
        private int _offset;
        private Percentage _multiplier;

        static ValueModifier()
        {
            ParameterExpression originalValue = Expression.Parameter(typeof(T), "originalValue");
            ParameterExpression currentValue = Expression.Parameter(typeof(T), "currentValue");
            ParameterExpression multiplier = Expression.Parameter(typeof(Percentage), "multiplier");
            ParameterExpression offset = Expression.Parameter(typeof(int), "offset");

            _compoundMultiplyExpression = Expression.Lambda<Func<T, T, Percentage, T>>(
                Expression.Convert(
                    Expression.Multiply(
                        Expression.Convert(currentValue, typeof(decimal)),
                        Expression.Convert(multiplier, typeof(decimal))),
                    typeof(T)),
                originalValue,
                currentValue,
                multiplier).Compile();

            _standardMultiplyExpression = Expression.Lambda<Func<T, T, Percentage, T>>(
                Expression.Convert(
                    Expression.Add(
                        Expression.Convert(currentValue, typeof(decimal)),
                        Expression.Subtract(
                            Expression.Multiply(
                                Expression.Convert(originalValue, typeof(decimal)),
                                Expression.Convert(multiplier, typeof(decimal))),
                            Expression.Convert(originalValue, typeof(decimal)))),
                    typeof(T)),
                originalValue,
                currentValue,
                multiplier).Compile();

            _offsetExpression = Expression.Lambda<Func<T, T, int, T>>(
                Expression.Convert(
                    Expression.Add(
                        Expression.Convert(currentValue, typeof(decimal)),
                        Expression.Convert(offset, typeof(decimal))),
                    typeof(T)),
                originalValue,
                currentValue,
                offset).Compile();
        }

        protected void SetFlag(byte flag, bool value)
        {
            if (value)
            {
                _flags |= flag;
            }
            else
            {
                _flags &= (byte)~flag;
            }
        }

        protected bool IsFlagSet(byte flag)
        {
            return (_flags & flag) == flag;
        }

        public bool HasCompoundMultiplier
        {
            get => IsFlagSet(CompoundMultiplierFlag);
            set => SetFlag(CompoundMultiplierFlag, value);
        }

        public bool IsOffsetAppliedFirst
        {
            get => IsFlagSet(ApplyOffsetFirstFlag);
            set => SetFlag(ApplyOffsetFirstFlag, value);
        }

        public int Offset
        {
            get => _offset;
            set => _offset = value;
        }

        public Percentage Multiplier
        {
            get => _multiplier;
            set => _multiplier = value;
        }

        public T Apply(T originalValue)
        {
            return Apply(originalValue, originalValue);
        }

        public T Apply(T originalValue, T currentValue)
        {
            if (IsOffsetAppliedFirst)
            {
                return ApplyMultiplier(originalValue, ApplyOffset(originalValue, currentValue));
            }

            return ApplyOffset(originalValue, ApplyMultiplier(originalValue, currentValue));
        }

        protected T ApplyOffset(T originalValue, T currentValue)
        {
            return _offsetExpression(originalValue, currentValue, _offset);
        }

        protected T ApplyMultiplier(T originalValue, T currentValue)
        {
            if (HasCompoundMultiplier)
            {
                _ = _compoundMultiplyExpression(originalValue, currentValue, _multiplier);
            }

            return _standardMultiplyExpression(originalValue, currentValue, _multiplier);
        }

        public ValueModifier()
            : this(0, 1.0f) { }

        public ValueModifier(int offset, Percentage efficiency)
        {
            _offset = offset;
            _multiplier = efficiency;
        }
    }
}