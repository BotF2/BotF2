using System;
using System.Collections.Generic;

using Microsoft.Scripting;

using Supremacy.Annotations;
using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    public class ConstantExpression : Expression
    {
        private static readonly Dictionary<Type, object> _defaultValueLookup = new Dictionary<Type, object>();

        private object _value;

        protected internal ConstantExpression() { }

        public ConstantExpression([NotNull] Type type, object value)
        {
            _value = value;
            Type = type ?? throw new ArgumentNullException("type");
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            if (!(target is ConstantExpression clone))
            {
                return;
            }

            clone._value = _value;
        }

        public static ConstantExpression Create(Type type, object value, SourceSpan location = default(SourceSpan))
        {
            return Activator.CreateInstance(
                       typeof(ConstantExpression<>).MakeGenericType(type),
                       value,
                       location) as ConstantExpression;
        }

        //
        // Returns true iff 1) the stack type of this is one of Object, 
        // int32, int64 and 2) this == 0 or this == null.
        //
        public virtual bool IsZeroInteger => IsDefaultValue &&
                       (Type.IsIntegerOrBoolean() || (Type == TypeManager.CoreTypes.Char));

        public virtual object Value => _value;

        public override bool IsNull => !Type.IsValueType && _value is null;

        public override bool IsPrimaryExpression => true;

        public virtual bool IsDefaultValue
        {
            get
            {
                if (!Type.IsValueType)
                {
                    return _value is null;
                }

                if (!_defaultValueLookup.TryGetValue(Type, out object defaultValue))
                {
                    defaultValue = Activator.CreateInstance(Type);
                    _defaultValueLookup[Type] = defaultValue;
                }

                return Equals(_value, defaultValue);
            }
        }

        public override System.Linq.Expressions.Expression TransformCore(ScriptGenerator generator)
        {
            return System.Linq.Expressions.Expression.Constant(
                _value,
                Type);
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            bool quote = Type == TypeManager.CoreTypes.String;

            if (quote)
            {
                sw.Write("\"");
            }

            sw.Write(_value);

            if (quote)
            {
                sw.Write("\"");
            }
        }

        public virtual ConstantExpression ConvertExplicitly(bool inCheckedContext, Type targetType)
        {
            return new EmptyConstantCastExpression(this, targetType);
        }

        public virtual ConstantExpression ConvertImplicitly(Type targetType)
        {
            if (TypeManager.IsEqual(Type, targetType))
            {
                return this;
            }

            return !TypeUtils.IsImplicitNumericConversion(Type, targetType) ? null : new EmptyConstantCastExpression(this, targetType, true);
        }

        /// <summary>
        ///   Attempts to do a compile-time folding of a constant cast.
        /// </summary>
        public ConstantExpression TryReduce(ParseContext ec, Type targetType, SourceSpan location)
        {
            try
            {
                return TryReduce(targetType);
            }
            catch (OverflowException)
            {
                OnErrorValueCannotBeConverted(ec, location, targetType, false);
                return new ConstantExpression(targetType, targetType.IsValueType ? Activator.CreateInstance(targetType) : null);
            }
        }

        private ConstantExpression TryReduce(Type targetType)
        {
            if (Type == targetType)
            {
                Type thisType = GetType();

                if (!thisType.IsGenericType && !TypeManager.IsEnumType(targetType))
                {
                    switch (Type.GetTypeCode(targetType))
                    {
                        case TypeCode.Boolean:
                            return new ConstantExpression<bool>((bool)_value) { Span = Span };

                        case TypeCode.Char:
                            return new ConstantExpression<char>((char)_value) { Span = Span };

                        case TypeCode.SByte:
                            return new ConstantExpression<sbyte>((sbyte)_value) { Span = Span };

                        case TypeCode.Byte:
                            return new ConstantExpression<byte>((byte)_value) { Span = Span };

                        case TypeCode.Int16:
                            return new ConstantExpression<short>((short)_value) { Span = Span };

                        case TypeCode.UInt16:
                            return new ConstantExpression<ushort>((ushort)_value) { Span = Span };

                        case TypeCode.Int32:
                            return new ConstantExpression<int>((int)_value) { Span = Span };

                        case TypeCode.UInt32:
                            return new ConstantExpression<uint>((uint)_value) { Span = Span };

                        case TypeCode.Int64:
                            return new ConstantExpression<long>((long)_value) { Span = Span };

                        case TypeCode.UInt64:
                            return new ConstantExpression<ulong>((ulong)_value) { Span = Span };

                        case TypeCode.Single:
                            return new ConstantExpression<float>((float)_value) { Span = Span };

                        case TypeCode.Double:
                            return new ConstantExpression<double>((double)_value) { Span = Span };

                        case TypeCode.Decimal:
                            return new ConstantExpression<decimal>((decimal)_value) { Span = Span };

                        case TypeCode.String:
                            return new ConstantExpression<string>((string)_value) { Span = Span };
                    }
                }
                return this;
            }

            if (TypeManager.IsEnumType(targetType))
            {
                ConstantExpression c = TryReduce(Enum.GetUnderlyingType(targetType));
                return c == null ? null : new EnumConstantExpression(c, targetType);
            }

            return ConvertExplicitly(false, targetType);
        }
    }

    public class ConstantExpression<T> : ConstantExpression
    {
        public ConstantExpression(T value)
            : base(typeof(T), value) { }

        public ConstantExpression(T value, SourceSpan span)
            : this(value)
        {
            Span = span;
        }

        internal ConstantExpression()
        {
            // For cloning purposes only.
        }

        public new T Value => (T)base.Value;

        public override bool IsDefaultValue => typeof(T).IsValueType ? Equals(Value, default(T)) : Value == null;
    }
}