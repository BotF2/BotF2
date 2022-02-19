// StateManager.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Markup;

using Supremacy.Annotations;
using Supremacy.Utility;

namespace Supremacy.Types
{
    public sealed class StateManager<TStateEnum> where TStateEnum : State
    {
        private int _currentState;
        private readonly StateTransition<TStateEnum>[] _transitionMap;

        public event EventHandler StateChanged;

        public StateManager(TStateEnum startState, params StateTransition<TStateEnum>[] transitionMap)
        {
            _currentState = startState.Value;
            _transitionMap = transitionMap;
        }

        public TStateEnum CurrentState => (TStateEnum)State.Get(typeof(TStateEnum), _currentState);

        public bool TryChange(TStateEnum desiredState)
        {
            int currentState = _currentState;
            int desiredStateValue = desiredState.Value;

            if (currentState == desiredStateValue)
            {
                return true;
            }

            foreach (StateTransition<TStateEnum> transition in _transitionMap.Where(t => t.From == currentState && t.To == desiredStateValue))
            {
                if (Interlocked.CompareExchange(ref _currentState, transition.To, currentState) == currentState)
                {
                    StateChanged.Raise(this);
                    return true;
                }

                if (transition.Disposition == StateChangeDisposition.Optional)
                {
                    return false;
                }

                throw new InvalidOperationException("Mandatory state transition couldn't occur.");
            }

            throw new InvalidOperationException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Illegal state transition from {0} to {1}.",
                    State.Get(typeof(TStateEnum), currentState),
                    desiredState));
        }
    }

    public enum StateChangeDisposition
    {
        MustHappen,
        Optional
    }

    public struct StateTransition<TStateEnum> : IEquatable<StateTransition<TStateEnum>> where TStateEnum : State
    {
        private readonly int _to;

        public StateTransition([NotNull] TStateEnum from, [NotNull] TStateEnum to, StateChangeDisposition disposition)
        {
            if (@from == null)
            {
                throw new ArgumentNullException("from");
            }

            if (to == null)
            {
                throw new ArgumentNullException("to");
            }

            From = from.Value;
            _to = to.Value;
            Disposition = disposition;
        }

        public int From { get; }

        public int To => _to;

        public StateChangeDisposition Disposition { get; }

        #region IEquatable<StateTransition<TStateEnum>> Members

        public bool Equals(StateTransition<TStateEnum> other)
        {
            return From == other.From &&
                   _to == other._to &&
                   Disposition == other.Disposition;
        }

        #endregion

        public override bool Equals(object obj)
        {
            StateTransition<TStateEnum>? other = obj as StateTransition<TStateEnum>?;
            if (other.HasValue)
            {
                return Equals(other);
            }

            return false;
        }

        public static bool operator ==(StateTransition<TStateEnum> st1, StateTransition<TStateEnum> st2)
        {
            return st1.Equals(st2);
        }

        public static bool operator !=(StateTransition<TStateEnum> st1, StateTransition<TStateEnum> st2)
        {
            return !st1.Equals(st2);
        }

        public override int GetHashCode()
        {
            return From | (_to << 16);
        }
    }

    [TypeConverter(typeof(StateConverter<State>))]
    public abstract class State : IEquatable<State>, IConvertible
    {
        private const BindingFlags ValueFieldFlags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static;

        protected State(int value)
        {
            Value = value;
        }

        public string Name => ToString();

        public int Value { get; }

        public int Toint()
        {
            return this;
        }

        public static implicit operator int(State state)
        {
            return state.Value;
        }

        public override string ToString()
        {
            FieldInfo field = GetFieldInfo();
            if (field != null)
            {
                return field.Name;
            }

            return Value.ToString();
        }

        internal FieldInfo GetFieldInfo()
        {
            return (
                       from f in GetType().GetFields(ValueFieldFlags)
                       where typeof(State).IsAssignableFrom(f.FieldType)
                       let value = (State)f.GetValue(null)
                       where value == Value
                       select f
                   ).FirstOrDefault();
        }

        internal static IEnumerable<FieldInfo> GetValueFields(Type type)
        {
            return from f in type.GetFields(ValueFieldFlags)
                   where typeof(State).IsAssignableFrom(f.FieldType)
                   select f;
        }

        internal static IEnumerable<State> GetValues(Type type)
        {
            return from f in GetValueFields(type)
                   select (State)f.GetValue(null);
        }

        internal static State Get(Type type, int value)
        {

            if (TryGet(type, value, out State state))
            {
                return state;
            }

            throw new InvalidOperationException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "The '{0}' type doesn't define a static field with a numeric value of '{1}'.",
                    type.Name,
                    value));
        }

        internal static bool TryGet(Type type, int value, out State state)
        {
            state = GetValues(type).FirstOrDefault(o => o.Value == value);
            return state != null;
        }

        internal static State Parse(Type type, string value)
        {

            if (TryParse(type, value, out State result))
            {
                return result;
            }

            throw new InvalidOperationException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "The '{0}' type doesn't define a static field with the name '{1}'.",
                    type.Name,
                    value));
        }

        internal static bool TryParse(Type type, string value, out State state)
        {
            state = (
                         from f in GetValueFields(type)
                         where f.Name == value
                         select (State)f.GetValue(null)
                     ).FirstOrDefault();

            return state != null;
        }

        public bool Equals(State other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is null)
            {
                return false;
            }

            if (other.GetType() != GetType())
            {
                return false;
            }

            return other.Value == Value;
        }

        public sealed override bool Equals(object obj)
        {
            return Equals(obj as State);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public static bool operator ==(State left, State right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(State left, State right)
        {
            return !Equals(left, right);
        }

        TypeCode IConvertible.GetTypeCode()
        {
            return Value.GetTypeCode();
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToBoolean(provider);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToChar(provider);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToSByte(provider);
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToByte(provider);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToInt16(provider);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToUInt16(provider);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToInt32(provider);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToUInt32(provider);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToInt64(provider);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToUInt64(provider);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToSingle(provider);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToDouble(provider);
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToDecimal(provider);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToDateTime(provider);
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            return Value.ToString(provider);
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            return ((IConvertible)Value).ToType(conversionType, provider);
        }
    }

    public sealed class StateConverter<TStateEnum> : TypeConverter where TStateEnum : State
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(int) || sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(MarkupExtension) ||
                   destinationType == typeof(int) ||
                   destinationType == typeof(string) ||
                   base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            Type type = typeof(TStateEnum);


            if (context != null &&
                context.PropertyDescriptor != null &&
                context.PropertyDescriptor.PropertyType.IsSubclassOf(type))
            {
                type = context.PropertyDescriptor.PropertyType;
            }


            State state;

            int? intValue = TryConvert.ToInt32(value);
            if (intValue.HasValue)
            {
                if (State.TryGet(type, intValue.Value, out state))
                {
                    return state;
                }

                return null;
            }

            if (value is string stringValue)
            {
                if (State.TryParse(type, stringValue, out state))
                {
                    return state;
                }

                return null;
            }

            return null;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            State state = value as State;
            if (state != null)
            {
                if (destinationType == typeof(MarkupExtension))
                {
                    if (context is IValueSerializerContext serializerContext)
                    {
                        ValueSerializer typeSerializer = serializerContext.GetValueSerializerFor(typeof(Type));
                        if (typeSerializer != null)
                        {
                            return new StaticExtension(
                                typeSerializer.ConvertToString(state.GetType(), serializerContext) +
                                "." +
                                state.Name);
                        }
                    }

                    return new StaticExtension
                    {
                        MemberType = state.GetType(),
                        Member = state.Name
                    };
                }

                if (destinationType == typeof(int))
                {
                    return state.Value;
                }

                if (destinationType == typeof(string))
                {
                    return state.ToString();
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}