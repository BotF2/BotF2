using System;
using System.ComponentModel;
using System.Windows;

using Supremacy.Annotations;
using Supremacy.Utility;

namespace Supremacy.Client
{
    /// <summary>
    /// Encapsulates methods for dealing with dependency objects and properties.
    /// </summary>
    public static class DependencyHelpers
    {
        /// <summary>
        /// Gets the <see cref="DependencyProperty"/> specified by <paramref name="propertyName"/> for the target type specified by <paramref name="targetType"/>.
        /// </summary>
        /// <param name="targetType">The type of object for which the property is registered.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The <see cref="DependencyProperty"/> specified by <paramref name="propertyName"/> for the target type specified by <paramref name="targetType"/></returns>
        [CanBeNull]
        public static DependencyProperty GetDependencyProperty([NotNull] Type targetType, [NotNull] string propertyName)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            DependencyPropertyDescriptor propertyDescriptor = DependencyPropertyDescriptor.FromName(
                propertyName,
                targetType,
                targetType);

            if (propertyDescriptor != null)
            {
                return propertyDescriptor.DependencyProperty;
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="DependencyProperty"/> specified by <paramref name="propertyName"/> for the object specified by <paramref name="target"/>.
        /// </summary>
        /// <param name="target">The object for which the property is registerted.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The dependency property specified by <paramref name="propertyName"/> for the object specified by <paramref name="target"/>.</returns>
        [CanBeNull]
        public static DependencyProperty GetDependencyProperty([NotNull] this DependencyObject target, [NotNull] string propertyName)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            return GetDependencyProperty(target.GetType(), propertyName);
        }

        /// <summary>
        /// Determines whether the default value of the specified dependency property is set on a <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="property">The property.</param>
        /// <returns>
        /// <c>true</c> if the default value is set; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasDefaultValue([NotNull] this DependencyObject target, [NotNull] DependencyProperty property)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            return DependencyPropertyHelper.GetValueSource(target, property).BaseValueSource == BaseValueSource.Default;
        }

        /// <summary>
        /// Sets the value of the <paramref name="property"/> only if no value has been set or inherited.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="target">The object on which the property should be set.</param>
        /// <param name="property">The property to set.</param>
        /// <param name="value">The value to be set.</param>
        /// <returns><c>true</c> if <paramref name="value"/> was set; otherwise, <c>false</c>.</returns>
        public static bool SetIfDefault<T>([NotNull] this DependencyObject target, [NotNull] DependencyProperty property, T value)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            if (!target.HasDefaultValue(property))
            {
                return false;
            }

            target.SetValue(property, value);
            return true;
        }

        /// <summary>
        /// Gets the detailed value source of <see cref="property"/> on <see cref="target"/>.
        /// </summary>
        /// <param name="target">The object for which to get the detailed property value source.</param>
        /// <param name="property">The property for which the detailed value source should be retrieved.</param>
        /// <returns>The detailed value source of <see cref="property"/> on <see cref="target"/>.</returns>
        public static ValueSource GetValueSource([NotNull] this DependencyObject target, [NotNull] DependencyProperty property)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            GameLog.Client.GameData.DebugFormat("Civilization.cs: DependencyPropertyHelper: target={0}, property={1}", target, property);
            return DependencyPropertyHelper.GetValueSource(target, property);
        }

        /// <summary>
        /// Gets the base value source of <see cref="property"/> on <see cref="target"/>.
        /// </summary>
        /// <param name="target">The object for which to get the base property value source.</param>
        /// <param name="property">The property for which the base value source should be retrieved.</param>
        /// <returns>The base value source of <see cref="property"/> on <see cref="target"/>.</returns>
        public static BaseValueSource GetBaseValueSource([NotNull] this DependencyObject target, [NotNull] DependencyProperty property)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            return DependencyPropertyHelper.GetValueSource(target, property).BaseValueSource;
        }

        public static void TryFreeze([NotNull] this Freezable freezable)
        {
            if (freezable == null)
            {
                throw new ArgumentNullException("freezable");
            }

            if (freezable.CanFreeze)
            {
                freezable.Freeze();
            }
        }
    }
}