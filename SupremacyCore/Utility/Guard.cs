using System;
using System.Collections;
using System.Linq;

using Supremacy.Annotations;
using Supremacy.Scripting.Utility;

namespace Supremacy.Utility
{
    public static class Guard
    {
        /// <summary>
        /// Checks a string argument to ensure it isn't null or empty
        /// </summary>
        /// <param name="argumentValue">The argument value to check.</param>
        /// <param name="argumentName">The name of the argument.</param>
        /// <exception cref="ArgumentException">
        /// Thrown is <paramref name="argumentValue"/> is <c>null</c> or an empty string.
        /// </exception>
        [NotNull]
        [AssertionMethod]
        public static string ArgumentNotNullOrEmpty(
            [AssertionCondition(AssertionConditionType.IsNotNull)] string argumentValue,
            [InvokerParameterName] string argumentName)
        {
            if (string.IsNullOrEmpty(argumentValue))
                throw new ArgumentException(SR.ArgumentException_ValueMustBeNonEmptyString, argumentName);
            return argumentValue;
        }

        /// <summary>
        /// Checks a string argument to ensure it isn't null, empty, or composed entirely of whitespace.
        /// </summary>
        /// <param name="argumentValue">The argument value to check.</param>
        /// <param name="argumentName">The name of the argument.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="argumentValue"/> is <c>null</c>, empty, or composed entirely of whitespace.
        /// </exception>
        [NotNull]
        [AssertionMethod]
        public static string ArgumentNotNullOrWhiteSpace(
            [AssertionCondition(AssertionConditionType.IsNotNull)] string argumentValue,
            [InvokerParameterName] string argumentName)
        {
            if (string.IsNullOrWhiteSpace(argumentValue))
                throw new ArgumentException(SR.ArgumentException_ValueMustBeNonEmptyString, argumentName);
            return argumentValue;
        }

        /// <summary>
        /// Checks an argument to ensure it isn't null
        /// </summary>
        /// <param name="argumentValue">The argument value to check.</param>
        /// <param name="argumentName">The name of the argument.</param>
        /// <exception cref="ArgumentNullException">argumentValue is null</exception>
        [NotNull]
        [AssertionMethod]
        public static T ArgumentNotNull<T>(
            [AssertionCondition(AssertionConditionType.IsNotNull)] T argumentValue,
            [InvokerParameterName] string argumentName)
        {
            if (ReferenceEquals(argumentValue, null))
                throw new ArgumentNullException(argumentName);
            return argumentValue;
        }

        /// <summary>
        /// Checks an argument to ensure is assignable to type <typeparam name="TTarget"/>.
        /// </summary>
        /// <param name="argumentValue">The argument value to check.</param>
        /// <param name="argumentName">The name of the argument.</param>
        /// <exception cref="ArgumentException">argumentValue is not assignable to type <typeparam name="TTarget"/></exception>
        [NotNull]
        [AssertionMethod]
        public static T ArgumentAssignableAs<T, TTarget>(
            [AssertionCondition(AssertionConditionType.IsNotNull)] T argumentValue,
            [InvokerParameterName] string argumentName)
        {
            if (argumentValue is TTarget)
                return argumentValue;

            throw new ArgumentException(
                string.Format(
                    "Value must be assignable to type '{0}'.",
                    typeof(TTarget).FullName));
        }

        /// <summary>
        /// Checks an Enum argument to ensure that its value is defined by the specified Enum type.
        /// </summary>
        /// <param name="enumType">The Enum type the value should correspond to.</param>
        /// <param name="value">The value to check for.</param>
        /// <param name="argumentName">The name of the argument holding the value.</param>
        public static void ArgumentIsDefinedEnum(Type enumType, object value, [InvokerParameterName] string argumentName)
        {
            if (Enum.IsDefined(enumType, value))
                return;

            throw new ArgumentException(
                string.Format(
                    SR.ArgumentException_UndefinedEnumValueFormat,
                    enumType.Name),
                argumentName);
        }

        public static TEnum ArgumentIsEnum<TEnum>(TEnum value, [InvokerParameterName] string argumentName, bool mustBeDefined = false, bool allowNullable = true)
        {
            Type type = typeof(TEnum);
            if (type.IsEnum)
            {
                if (!mustBeDefined || Enum.IsDefined(type, value))
                    return value;
            }
            else
            {
                if (type.IsNullableType(out type) &&
                    type.IsEnum)
                {
                    if (!mustBeDefined ||
                        (!ReferenceEquals(value, null) && Enum.IsDefined(type, value)))
                    {
                        return value;
                    }
                }
            }

            throw new ArgumentException(
                SR.ArgumentException_ParameterMustBeEnumType,
                argumentName);
        }

        public static IEnumerable ArgumentElementType<TElement>(
            [NotNull] IEnumerable collection,
            [InvokerParameterName] string argumentName,
            bool exactMatch = false,
            bool allowNull = false)
        {
            if (collection == null && !allowNull)
                throw new ArgumentNullException("collection");

            Type constraintType = typeof(TElement);

            if (exactMatch)
            {
                if (collection.Cast<object>().All(o => o.GetType() == constraintType))
                    return collection;

                throw new ArgumentException(
                    string.Format(
                        SR.ArgumentException_ElementExactTypeMismatch,
                        argumentName,
                        constraintType.Name),
                    argumentName);
            }

            if (collection.Cast<object>().All(o => o is TElement))
                return collection;

            throw new ArgumentException(
                string.Format(
                    SR.ArgumentException_ElementTypeMismatch,
                    argumentName,
                    constraintType.Name),
                argumentName);
        }
    }
}