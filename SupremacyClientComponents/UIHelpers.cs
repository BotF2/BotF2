using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using System.Diagnostics;

using Supremacy.Annotations;
using Supremacy.Client.Interop;

namespace Supremacy.Client
{
    /// <summary>
    /// Encapsulates operations and data relevant to the visual and logical trees.
    /// </summary>
    public static class UIHelpers
    {
        #region Constructor

        static UIHelpers()
        {
            // register the hyperlink click event so we could launch the browser.
            EventManager.RegisterClassHandler(
                typeof(Hyperlink),
                Hyperlink.ClickEvent,
                new RoutedEventHandler(OnHyperlinkClick));
        }

        #endregion

        #region Ensure Access

        /// <summary>
        /// Ensures the calling thread is the thread associated with the <see cref="Dispatcher"/>.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns></returns>
        public static bool EnsureAccess(MethodBase method)
        {
            return EnsureAccess((Dispatcher)null, method, null);
        }

        /// <summary>
        /// Ensures the calling thread is the thread associated with the <see cref="Dispatcher"/>.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static bool EnsureAccess(MethodBase method, params object[] parameters)
        {
            return EnsureAccess(null, method, null, parameters);
        }

        /// <summary>
        /// Ensures the calling thread is the thread associated with the <see cref="Dispatcher"/>.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="o">The object.</param>
        /// <returns></returns>
        public static bool EnsureAccess(MethodBase method, object o)
        {
            return EnsureAccess((Dispatcher)null, method, o);
        }

        /// <summary>
        /// Ensures the calling thread is the thread associated with the <see cref="Dispatcher"/>.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="o">The object.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static bool EnsureAccess(MethodBase method, object o, params object[] parameters)
        {
            return EnsureAccess(null, method, o, parameters);
        }

        /// <summary>
        /// Ensures the calling thread is the thread associated with the <see cref="Dispatcher"/>.
        /// </summary>
        /// <param name="dispatcher">The dispatcher.</param>
        /// <param name="method">The method.</param>
        /// <param name="o">The object.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static bool EnsureAccess(Dispatcher dispatcher, MethodBase method, object o, params object[] parameters)
        {
            if (dispatcher == null)
            {
                if (o is DispatcherObject dispatcherObject)
                {
                    dispatcher = dispatcherObject.Dispatcher;
                }
                else if (Application.Current != null)
                {
                    dispatcher = Application.Current.Dispatcher;
                }
                else
                {
                    return false;
                }
            }

            if (!dispatcher.CheckAccess())
            {
                _ = dispatcher.Invoke(
                    DispatcherPriority.Normal,
                    (Func<object, object[], object>)method.Invoke,
                    o,
                    parameters);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Ensures the calling thread is the thread associated with the <see cref="Dispatcher"/>.
        /// </summary>
        /// <param name="o">The object.</param>
        /// <param name="method">The method.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static bool EnsureAccess(this DispatcherObject o, MethodBase method, params object[] parameters)
        {
            return EnsureAccess(o.Dispatcher, method, o, parameters);
        }

        #endregion

        #region Find Elements
        /// <summary>
        /// Returns whether the specified <see cref="DependencyObject"/> is a <see cref="Visual"/> or
        ///  <see cref="Visual3D"/>.
        /// </summary>
        /// <param name="p">The <see cref="DependencyObject"/> to examine.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="DependencyObject"/> is a <see cref="Visual"/> or 
        /// <see cref="Visual3D"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsVisual(this DependencyObject p)
        {
            return (p is Visual) || (p is Visual3D);
        }

        public static DependencyObject GetVisualParent(this DependencyObject o)
        {
            if (o is FrameworkContentElement contentElement)
            {
                return contentElement.Parent ?? ContentOperations.GetParent(contentElement);
            }

            return VisualTreeHelper.GetParent(o);
        }

        public static DependencyObject GetLogicalParent(this DependencyObject o)
        {
            if (o == null)
            {
                return null;
            }

            DependencyObject logicalParent = LogicalTreeHelper.GetParent(o);
            if (logicalParent != null)
            {
                return logicalParent;
            }

            if (o is FrameworkElement frameworkElement)
            {
                return frameworkElement.TemplatedParent;
            }

            return null;
        }

        /// <summary>
        /// Finds the logical ancestor according to The condition.
        /// </summary>
        /// <param name="startElement">The start element.</param>
        /// <param name="predicate">The condition.</param>
        /// <returns></returns>
        public static DependencyObject FindLogicalAncestor(this DependencyObject startElement, Func<DependencyObject, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }

            DependencyObject o = startElement;

            while ((o != null) && !predicate(o))
            {
                o = o.GetLogicalParent();
            }

            return o;
        }

        /// <summary>
        /// Finds an element's logical ancestors of a specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startElement">The start element.</param>
        /// <returns>The logical ancestors.</returns>
        public static IEnumerable<T> FindLogicalAncestorsByType<T>(this DependencyObject startElement) where T : DependencyObject
        {
            if (startElement == null)
            {
                return Enumerable.Empty<T>();
            }

            return startElement.FindLogicalAncestorsByType<T>(null);
        }

        /// <summary>
        /// Finds an element's logical ancestors of a specified type that match the specified predicate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startElement">The start element.</param>
        /// <param name="predicate">The condition.</param>
        /// <returns>The matching logical ancestors.</returns>
        public static IEnumerable<T> FindLogicalAncestorsByType<T>(this DependencyObject startElement, Func<T, bool> predicate) where T : DependencyObject
        {
            if (startElement == null)
            {
                return Enumerable.Empty<T>();
            }

            return FindLogicalAncestorsByType(startElement, predicate, false);
        }

        /// <summary>
        /// Finds an element's logical ancestors of a specified type that match the specified predicate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startElement">The start element.</param>
        /// <param name="includeStartElement">Indicates whether <paramref name="startElement"/> may be included in the results.</param>
        /// <returns>The matching logical ancestors.</returns>
        public static IEnumerable<T> FindLogicalAncestorsByType<T>(this DependencyObject startElement, bool includeStartElement) where T : DependencyObject
        {
            if (startElement == null)
            {
                return Enumerable.Empty<T>();
            }

            return FindLogicalAncestorsByType<T>(startElement, null, includeStartElement);
        }

        /// <summary>
        /// Finds an element's logical ancestors of a specified type that match the specified predicate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startElement">The start element.</param>
        /// <param name="predicate">The condition.</param>
        /// <param name="includeStartElement">Indicates whether <paramref name="startElement"/> may be included in the results.</param>
        /// <returns>The matching logical ancestors.</returns>
        public static IEnumerable<T> FindLogicalAncestorsByType<T>(this DependencyObject startElement, Func<T, bool> predicate, bool includeStartElement) where T : DependencyObject
        {
            if (startElement == null)
            {
                yield break;
            }

            DependencyObject currentElement = startElement;

            while (currentElement != null)
            {
                if (includeStartElement)
                {
                    if ((currentElement is T resultCandidate) && ((predicate == null) || predicate(resultCandidate)))
                    {
                        yield return resultCandidate;
                    }
                }

                includeStartElement = true;

                currentElement = currentElement.GetLogicalParent();
            }
        }

        /// <summary>
        /// Finds an element's logical descendants of a specified type.
        /// </summary>
        /// <typeparam name="T">The type of descendant.</typeparam>
        /// <param name="startElement">The start element.</param>
        /// <returns>The logical descendants.</returns>
        public static IEnumerable<T> FindLogicalDescendantsByType<T>(this DependencyObject startElement) where T : DependencyObject
        {
            if (startElement == null)
            {
                return Enumerable.Empty<T>();
            }

            return FindLogicalDescendantsByType<T>(startElement, null);
        }

        /// <summary>
        /// Finds an element's logical descendants of a specified type that match the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of descendant.</typeparam>
        /// <param name="startElement">The start element.</param>
        /// <param name="predicate">The condition.</param>
        /// <returns>The logical descendants.</returns>
        public static IEnumerable<T> FindLogicalDescendantsByType<T>(this DependencyObject startElement, Func<T, bool> predicate) where T : DependencyObject
        {
            if (startElement == null)
            {
                return Enumerable.Empty<T>();
            }

            return FindLogicalDescendantsByType(startElement, predicate, false);
        }

        /// <summary>
        /// Finds an element's logical descendants of a specified type that match the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of descendant.</typeparam>
        /// <param name="startElement">The start element.</param>
        /// <param name="includeStartElement">Indicates whether <paramref name="startElement"/> may be included in the results.</param>
        /// <returns>The logical descendants.</returns>
        public static IEnumerable<T> FindLogicalDescendantsByType<T>(this DependencyObject startElement, bool includeStartElement) where T : DependencyObject
        {
            if (startElement == null)
            {
                return Enumerable.Empty<T>();
            }

            return FindLogicalDescendantsByType<T>(startElement, null, includeStartElement);
        }

        /// <summary>
        /// Finds an element's logical descendants of a specified type that match the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of descendant.</typeparam>
        /// <param name="startElement">The start element.</param>
        /// <param name="predicate">The condition.</param>
        /// <param name="includeStartElement">Indicates whether <paramref name="startElement"/> may be included in the results.</param>
        /// <returns>The logical descendants.</returns>
        public static IEnumerable<T> FindLogicalDescendantsByType<T>(this DependencyObject startElement, Func<T, bool> predicate, bool includeStartElement) where T : DependencyObject
        {
            if (startElement == null)
            {
                yield break;
            }

            if (includeStartElement && (startElement is T resultCandidate) && ((predicate == null) || predicate(resultCandidate)))
            {
                yield return resultCandidate;
            }

            foreach (DependencyObject logicalChild in startElement.GetLogicalChildren())
            {
                foreach (T result in FindLogicalDescendantsByType(logicalChild, predicate, true))
                {
                    yield return result;
                }
            }
        }

        /// <summary>
        /// Finds the logical ancestor by type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startElement">The start element.</param>
        /// <returns></returns>
        public static T FindLogicalAncestorByType<T>(this DependencyObject startElement) where T : DependencyObject
        {
            if (startElement == null)
            {
                return null;
            }

            return (T)FindLogicalAncestor(startElement, o => o is T);
        }

        /// <summary>
        /// Finds the logical root.
        /// </summary>
        /// <param name="startElement">The start element.</param>
        /// <returns></returns>
        public static DependencyObject FindLogicalRoot(this DependencyObject startElement)
        {
            if (startElement == null)
            {
                return null;
            }

            return startElement.FindLogicalAncestorsByType<DependencyObject>().FirstOrDefault();
        }

        /// <summary>
        /// Finds the visual ancestor according to The condition.
        /// </summary>
        /// <param name="startElement">The start element.</param>
        /// <param name="predicate">The condition.</param>
        /// <returns></returns>
        public static DependencyObject FindVisualAncestor(this DependencyObject startElement, Func<DependencyObject, bool> predicate)
        {
            if (startElement == null)
            {
                return null;
            }

            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }

            DependencyObject o = startElement;

            while ((o != null) && !predicate(o))
            {
                o = o.GetVisualParent();
            }

            return o;
        }

        /// <summary>
        /// Finds the visual ancestor by type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startElement">The start element.</param>
        /// <returns></returns>
        public static T FindVisualAncestorByType<T>(this DependencyObject startElement) where T : DependencyObject
        {
            if (startElement == null)
            {
                return null;
            }

            return (T)FindVisualAncestor(startElement, o => o is T);
        }

        /// <summary>
        /// Finds an element's visual ancestors of a specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startElement">The start element.</param>
        /// <returns>The visual ancestors.</returns>
        public static IEnumerable<T> FindVisualAncestorsByType<T>(this DependencyObject startElement) where T : DependencyObject
        {
            if (startElement == null)
            {
                return Enumerable.Empty<T>();
            }

            return startElement.FindVisualAncestorsByType<T>(null);
        }

        /// <summary>
        /// Finds an element's visual ancestors of a specified type that match the specified predicate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startElement">The start element.</param>
        /// <param name="predicate">The condition.</param>
        /// <returns>The matching visual ancestors.</returns>
        public static IEnumerable<T> FindVisualAncestorsByType<T>(this DependencyObject startElement, Func<T, bool> predicate) where T : DependencyObject
        {
            if (startElement == null)
            {
                return Enumerable.Empty<T>();
            }

            return FindVisualAncestorsByType(startElement, predicate, false);
        }

        /// <summary>
        /// Finds an element's visual ancestors of a specified type that match the specified predicate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startElement">The start element.</param>
        /// <param name="includeStartElement">Indicates whether <paramref name="startElement"/> may be included in the results.</param>
        /// <returns>The matching visual ancestors.</returns>
        public static IEnumerable<T> FindVisualAncestorsByType<T>(this DependencyObject startElement, bool includeStartElement) where T : DependencyObject
        {
            if (startElement == null)
            {
                return Enumerable.Empty<T>();
            }

            return FindVisualAncestorsByType<T>(startElement, null, includeStartElement);
        }

        /// <summary>
        /// Finds an element's visual ancestors of a specified type that match the specified predicate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startElement">The start element.</param>
        /// <param name="predicate">The condition.</param>
        /// <param name="includeStartElement">Indicates whether <paramref name="startElement"/> may be included in the results.</param>
        /// <returns>The matching visual ancestors.</returns>
        public static IEnumerable<T> FindVisualAncestorsByType<T>(this DependencyObject startElement, Func<T, bool> predicate, bool includeStartElement) where T : DependencyObject
        {
            if (startElement == null)
            {
                yield break;
            }

            DependencyObject currentElement = startElement;

            while (true)
            {
                if (includeStartElement)
                {
                    if ((currentElement is T resultCandidate) && ((predicate == null) || predicate(resultCandidate)))
                    {
                        yield return resultCandidate;
                    }
                }

                includeStartElement = true;

                DependencyObject parentElement = GetVisualParent(currentElement);
                if (parentElement == null)
                {
                    break;
                }

                currentElement = parentElement;
            }
        }

        /// <summary>
        /// Finds the visual descendant.
        /// </summary>
        /// <param name="startElement">The start element.</param>
        /// <param name="predicate">The condition.</param>
        /// <returns></returns>
        public static DependencyObject FindVisualDescendant(this DependencyObject startElement, Func<DependencyObject, bool> predicate)
        {
            if (startElement == null)
            {
                return null;
            }

            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }

            return startElement.FindVisualDescendantsByType(predicate).FirstOrDefault();
        }

        /// <summary>
        /// Finds an element's visual descendants of a specified type.
        /// </summary>
        /// <typeparam name="T">The type of descendant.</typeparam>
        /// <param name="startElement">The start element.</param>
        /// <returns>The visual descendants.</returns>
        public static IEnumerable<T> FindVisualDescendantsByType<T>(this DependencyObject startElement) where T : DependencyObject
        {
            if (startElement == null)
            {
                return Enumerable.Empty<T>();
            }

            return FindVisualDescendantsByType<T>(startElement, null);
        }

        /// <summary>
        /// Finds an element's visual descendants of a specified type that match the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of descendant.</typeparam>
        /// <param name="startElement">The start element.</param>
        /// <param name="predicate">The condition.</param>
        /// <returns>The visual descendants.</returns>
        public static IEnumerable<T> FindVisualDescendantsByType<T>(this DependencyObject startElement, Func<T, bool> predicate) where T : DependencyObject
        {
            if (startElement == null)
            {
                return Enumerable.Empty<T>();
            }

            return FindVisualDescendantsByType(startElement, predicate, false);
        }

        /// <summary>
        /// Finds an element's visual descendants of a specified type that match the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of descendant.</typeparam>
        /// <param name="startElement">The start element.</param>
        /// <param name="includeStartElement">Indicates whether <paramref name="startElement"/> may be included in the results.</param>
        /// <returns>The visual descendants.</returns>
        public static IEnumerable<T> FindVisualDescendantsByType<T>(this DependencyObject startElement, bool includeStartElement) where T : DependencyObject
        {
            if (startElement == null)
            {
                return Enumerable.Empty<T>();
            }

            return FindVisualDescendantsByType<T>(startElement, null, includeStartElement);
        }

        /// <summary>
        /// Finds an element's visual descendants of a specified type that match the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of descendant.</typeparam>
        /// <param name="startElement">The start element.</param>
        /// <param name="predicate">The condition.</param>
        /// <param name="includeStartElement">Indicates whether <paramref name="startElement"/> may be included in the results.</param>
        /// <returns>The visual descendants.</returns>
        public static IEnumerable<T> FindVisualDescendantsByType<T>(this DependencyObject startElement, Func<T, bool> predicate, bool includeStartElement) where T : DependencyObject
        {
            if (startElement == null)
            {
                yield break;
            }

            if (includeStartElement && (startElement is T resultCandidate) && ((predicate == null) || predicate(resultCandidate)))
            {
                yield return resultCandidate;
            }

            foreach (DependencyObject visualChild in startElement.GetVisualChildren())
            {
                foreach (T result in FindVisualDescendantsByType(visualChild, predicate, true))
                {
                    yield return result;
                }
            }
        }

        public static DependencyObject FindFirstFocusableDescendant(this DependencyObject startElement)
        {
            return startElement.FindFirstFocusableDescendant<DependencyObject>();
        }

        public static DependencyObject FindFirstFocusableDescendant(this DependencyObject startElement, bool includeStartElement)
        {
            return startElement.FindFirstFocusableDescendant<DependencyObject>(includeStartElement);
        }

        public static DependencyObject FindLastFocusableDescendant(this DependencyObject startElement)
        {
            return startElement.FindLastFocusableDescendant<DependencyObject>();
        }

        public static DependencyObject FindLastFocusableDescendant(this DependencyObject startElement, bool includeStartElement)
        {
            return startElement.FindLastFocusableDescendant<DependencyObject>(includeStartElement);
        }

        public static T FindFirstFocusableDescendant<T>(this DependencyObject startElement) where T : DependencyObject
        {
            if (startElement == null)
            {
                return null;
            }

            return startElement.FindFirstFocusableDescendant<T>(false);
        }

        public static T FindFirstFocusableDescendant<T>(this DependencyObject startElement, bool includeStartElement) where T : DependencyObject
        {
            if (startElement == null)
            {
                return null;
            }

            return startElement.FindVisualDescendantsByType<T>(
                o => (bool)o.GetValue(UIElement.FocusableProperty) &&
                     (bool)o.GetValue(UIElement.IsVisibleProperty)).FirstOrDefault();
        }

        public static T FindLastFocusableDescendant<T>(this DependencyObject startElement) where T : DependencyObject
        {
            if (startElement == null)
            {
                return null;
            }

            return startElement.FindLastFocusableDescendant<T>(false);
        }

        public static T FindLastFocusableDescendant<T>(this DependencyObject startElement, bool includeStartElement) where T : DependencyObject
        {
            if (startElement == null)
            {
                return null;
            }

            foreach (DependencyObject child in GetVisualChildren(startElement, true))
            {
                T childElement = FindLastFocusableDescendant<T>(child, true);
                if (childElement != null)
                {
                    return childElement;
                }

                if ((bool)child.GetValue(UIElement.FocusableProperty) &&
                    (bool)child.GetValue(UIElement.VisibilityProperty))
                {
                    return childElement;
                }
            }
            return null;
        }

        public static DependencyObject FindNextFocusable(DependencyObject startElement, DependencyObject rootElement)
        {
            return FindNextFocusable<DependencyObject>(startElement, rootElement);
        }

        public static bool IsVisualAncestorOf(this DependencyObject sourceElement, DependencyObject targetElement)
        {
            DependencyObject currentElement = targetElement;
            while (currentElement != null)
            {
                if (currentElement == sourceElement)
                {
                    return true;
                }

                currentElement = GetVisualParent(currentElement);
            }
            return false;
        }

        public static bool IsVisualDescendantOf(this DependencyObject sourceElement, DependencyObject targetElement)
        {
            DependencyObject currentElement = sourceElement;
            while (currentElement != null)
            {
                if (currentElement == targetElement)
                {
                    return true;
                }

                currentElement = GetVisualParent(currentElement);
            }
            return false;
        }

        /// <summary>
        /// Finds the ancestor <see cref="Popup"/> of a UI element.
        /// </summary>
        /// <param name="popupDescendant">The UI element for which the ancestor <see cref="Popup"/> should be found.</param>
        /// <returns>The ancestor <see cref="Popup"/> of <paramref name="popupDescendant"/>.</returns>
        public static Popup FindPopup(this DependencyObject popupDescendant)
        {
            if (popupDescendant == null)
            {
                return null;
            }

            if (popupDescendant is Popup popup)
            {
                return popup;
            }

            DependencyObject popupRoot = popupDescendant.FindVisualRoot() ?? popupDescendant;
            return popupRoot.FindLogicalAncestorByType<Popup>();
        }

        public static bool IsLogicalAncestorOf(this DependencyObject sourceElement, DependencyObject targetElement)
        {
            DependencyObject currentElement = targetElement;
            while (currentElement != null)
            {
                if (currentElement == sourceElement)
                {
                    return true;
                }

                currentElement = GetLogicalParent(currentElement);
            }
            return false;
        }

        public static bool IsLogicalDescendantOf(this DependencyObject sourceElement, DependencyObject targetElement)
        {
            DependencyObject currentElement = sourceElement;
            while (currentElement != null)
            {
                if (currentElement == targetElement)
                {
                    return true;
                }

                currentElement = GetLogicalParent(currentElement);
            }

            Popup popup = sourceElement.FindPopup();
            if (popup != null)
            {
                return popup.IsLogicalDescendantOf(targetElement);
            }

            return false;
        }

        /// <summary>
        /// Returns a <see cref="UIElement"/> value that represents the next visual object that is focusable. 
        /// </summary>
        /// <param name="startElement">The visual used as a basis for the search.</param>
        /// <param name="rootElement">The root visual used to limit the scope of the search.</param>
        /// <returns>A <see cref="UIElement"/> value that represents the next visual object that is focusable.</returns>
        public static T FindNextFocusable<T>(DependencyObject startElement, DependencyObject rootElement) where T : DependencyObject
        {
            if (startElement == null)
            {
                return null;
            }

            T descendantElement = FindFirstFocusableDescendant<T>(startElement);
            if (descendantElement != null)
            {
                return descendantElement;
            }

            DependencyObject currentElement = startElement;

            while ((currentElement != null) && (currentElement != rootElement))
            {
                DependencyObject sibling = FindNextSibling(currentElement);

                while (sibling != null)
                {
                    if ((sibling is T siblingCast) &&
                        (bool)sibling.GetValue(UIElement.FocusableProperty) &&
                        (bool)sibling.GetValue(UIElement.IsVisibleProperty))
                    {
                        return siblingCast;
                    }

                    T childElement = FindFirstFocusableDescendant<T>(sibling);
                    if (childElement != null)
                    {
                        return childElement;
                    }

                    sibling = FindNextSibling(sibling);
                }

                // None found, so move up the visual tree
                currentElement = GetVisualParent(currentElement);
            }

            return null;
        }

        /// <summary>
        /// Returns a <see cref="UIElement"/> value that represents the previous visual object that is focusable. 
        /// </summary>
        /// <param name="startElement">The visual used as a basis for the search.</param>
        /// <param name="rootElement">The root visual used to limit the scope of the search.</param>
        /// <returns>A <see cref="UIElement"/> value that represents the previous visual object that is focusable.</returns>
        public static T FindPreviousFocusable<T>(DependencyObject startElement, DependencyObject rootElement) where T : DependencyObject
        {
            if (startElement == null)
            {
                return null;
            }

            DependencyObject currentElement = startElement;

            while ((currentElement != null) && (currentElement != rootElement))
            {
                DependencyObject sibling = FindPreviousSibling(currentElement);

                while (sibling != null)
                {
                    T child = FindLastFocusableDescendant<T>(sibling);
                    if (child != null)
                    {
                        return child;
                    }

                    if ((sibling is T siblingCast) &&
                        (bool)sibling.GetValue(UIElement.FocusableProperty) &&
                        (bool)sibling.GetValue(UIElement.IsVisibleProperty))
                    {
                        return siblingCast;
                    }

                    sibling = FindPreviousSibling(sibling) as UIElement;
                }

                currentElement = VisualTreeHelper.GetParent(currentElement) as UIElement;
            }

            return null;
        }

        /// <summary>
        /// Returns a <see cref="DependencyObject"/> value that represents the the previous sibling visual object.
        /// </summary>
        /// <param name="startElement">The visual whose sibling is returned.</param>
        /// <returns>A <see cref="DependencyObject"/> value that represents the previous sibling visual object.</returns>
        private static DependencyObject FindPreviousSibling(DependencyObject startElement)
        {
            if (startElement == null)
            {
                return null;
            }

            DependencyObject parent = GetVisualParent(startElement);
            if (parent == null)
            {
                return null;
            }

            DependencyObject previous = null;

            foreach (DependencyObject child in GetVisualChildren(parent))
            {
                if (child == startElement)
                {
                    break;
                }

                previous = child;
            }

            return previous;
        }

        /// <summary>
        /// Returns a <see cref="DependencyObject"/> value that represents the the next sibling visual object.
        /// </summary>
        /// <param name="startElement">The visual whose sibling is returned.</param>
        /// <returns>A <see cref="DependencyObject"/> value that represents the next sibling visual object.</returns>
        private static DependencyObject FindNextSibling(DependencyObject startElement)
        {
            if (startElement == null)
            {
                return null;
            }

            bool foundSource = false;

            foreach (DependencyObject child in GetVisualChildren(startElement))
            {
                if (child == startElement)
                {
                    foundSource = true;
                    continue;
                }

                if (!foundSource)
                {
                    continue;
                }

                return child;
            }

            return null;
        }

        /// <summary>
        /// Finds the visual descendant by type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startElement">The start element.</param>
        /// <returns></returns>
        public static T FindVisualDescendantByType<T>(this DependencyObject startElement) where T : DependencyObject
        {
            return FindVisualDescendantsByType<T>(startElement).FirstOrDefault();
        }

        /// <summary>
        /// Finds the visual root.
        /// </summary>
        /// <param name="startElement">The start element.</param>
        /// <returns></returns>
        public static DependencyObject FindVisualRoot(this DependencyObject startElement)
        {
            return startElement.FindVisualAncestorsByType<DependencyObject>().FirstOrDefault();
        }

        /// <summary>
        /// Gets the visual children.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <returns></returns>
        public static IEnumerable<DependencyObject> GetVisualChildren(this DependencyObject parent)
        {
            return GetVisualChildren(parent, false);
        }

        /// <summary>
        /// Gets the visual children.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="reverseOrder">Whether the visual children should be returned in reverse order.</param>
        /// <returns></returns>
        public static IEnumerable<DependencyObject> GetVisualChildren(this DependencyObject parent, bool reverseOrder)
        {
            if (parent is Popup popup)
            {
                yield return popup.Child;
                yield break;
            }

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; ++i)
            {
                yield return VisualTreeHelper.GetChild(parent, i);
            }
        }

        /// <summary>
        /// Gets the visual children.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <returns></returns>
        public static IEnumerable<DependencyObject> GetLogicalChildren(this DependencyObject parent)
        {
            return GetLogicalChildren(parent, false);
        }

        /// <summary>
        /// Gets the visual children.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="reverseOrder">Whether the logical children should be returned in reverse order.</param>
        /// <returns></returns>
        public static IEnumerable<DependencyObject> GetLogicalChildren(this DependencyObject parent, bool reverseOrder)
        {
            if (parent is Popup popup)
            {
                yield return popup.Child;
                yield break;
            }

            foreach (DependencyObject child in LogicalTreeHelper.GetChildren(parent).OfType<DependencyObject>())
            {
                yield return child;
            }
        }

        #endregion

        #region DPI

        private static Matrix _dpiTransformToDevice;
        /// <summary>
        /// Gets a matrix that transforms the coordinates of this target to the device that is associated with the rendering destination.
        /// </summary>
        public static Matrix DpiTransformToDevice
        {
            get
            {
                EnsureDpiData();
                return _dpiTransformToDevice;
            }
        }

        private static Matrix _dpiTransformFromDevice;
        /// <summary>
        /// Gets a matrix that transforms the coordinates of the device that is associated with the rendering destination of this target.
        /// </summary>
        public static Matrix DpiTransformFromDevice
        {
            get
            {
                EnsureDpiData();
                return _dpiTransformFromDevice;
            }
        }

        private static double? _dpiX;
        /// <summary>
        /// Gets the system horizontal dots per inch (dpi).
        /// </summary>
        public static double DpiX
        {
            get
            {
                EnsureDpiData();
                return _dpiX.Value;
            }
        }

        private static double? _dpiY;
        /// <summary>
        /// Gets the system vertical dots per inch (dpi).
        /// </summary>
        public static double DpiY
        {
            get
            {
                EnsureDpiData();
                return _dpiX.Value;
            }
        }

        /// <summary>
        /// Safely gets the system DPI. Using <see cref="PresentationSource"/> will not work in partial trust.
        /// </summary>
        [SecurityCritical]
        private static void EnsureDpiData()
        {
            if (_dpiX.HasValue)
            {
                return;
            }

            HandleRef desktopWindow = new HandleRef(null, IntPtr.Zero);

            IntPtr deviceContext = NativeMethods.GetDC(desktopWindow);
            if (deviceContext == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            try
            {
                int dpi = NativeMethods.GetDeviceCaps(new HandleRef(null, deviceContext), 90);
                _dpiX = dpi;
                _dpiY = dpi;
            }
            finally
            {
                _ = NativeMethods.ReleaseDC(desktopWindow, new HandleRef(null, deviceContext));
            }

            _dpiTransformToDevice = Matrix.Identity;
            _dpiTransformToDevice.Scale(_dpiX.Value / 96d, _dpiY.Value / 96d);

            _dpiTransformFromDevice = Matrix.Identity;
            _dpiTransformFromDevice.Scale(96d / _dpiX.Value, 96d / _dpiY.Value);
        }

        #endregion

        #region ItemsControl

        /// <summary>
        /// Gets the generated containers of all items in an <see cref="ItemsControl"/>.
        /// </summary>
        /// <param name="itemsControl">The items control.</param>
        /// <returns></returns>
        public static IEnumerable<DependencyObject> GetItemContainers(this ItemsControl itemsControl)
        {
            for (int i = 0; i < itemsControl.Items.Count; i++)
            {
                yield return itemsControl.ItemContainerGenerator.ContainerFromIndex(i);
            }
        }

        #endregion

        #region Launch Browser

        private static int _launchBrowserRequests;
        private const int MaxBrowserRequests = 3;

        /// <summary>
        /// Gets or sets a value indicating whether clicking a <see cref="Hyperlink"/> that has a URI
        /// automatically launches the browser.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the browser is launched automatically; otherwise, <c>false</c>. The default is <c>false</c>.
        /// </value>
        public static bool IsAutomaticBrowserLaunchEnabled { get; set; }

        /// <summary>
        /// Launches the browser.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <remarks>Provides accidental click flood protection.</remarks>
        public static void LaunchBrowser(Uri uri)
        {
            if (!uri.IsAbsoluteUri)
            {
                return;
            }

            if (_launchBrowserRequests >= MaxBrowserRequests)
            {
                return;
            }

            _ = Interlocked.Increment(ref _launchBrowserRequests);
            _ = ThreadPool.QueueUserWorkItem(LaunchBrowserCallback, uri);
        }

        private static void LaunchBrowserCallback(object state)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = ((Uri)state).AbsoluteUri,
                };

                _ = Process.Start(startInfo);
            }
            finally
            {
                _ = Interlocked.Decrement(ref _launchBrowserRequests);
            }
        }

        private static void OnHyperlinkClick(object sender, RoutedEventArgs e)
        {
            if (!IsAutomaticBrowserLaunchEnabled)
            {
                return;
            }

            Uri uri = ((Hyperlink)e.Source).NavigateUri;
            if (uri != null)
            {
                LaunchBrowser(uri);
            }
        }

        #endregion

        #region Default Style Key Retrieval
        public static object GetDefaultStyleKey([NotNull] this FrameworkElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return FrameworkElementHelper.GetDefaultStyleKey(element);
        }

        [UsedImplicitly]
        private sealed class FrameworkElementHelper : FrameworkElement
        {
            internal static object GetDefaultStyleKey([NotNull] FrameworkElement element)
            {
                if (element == null)
                {
                    throw new ArgumentNullException("element");
                }

                return element.GetValue(DefaultStyleKeyProperty);
            }
        }
        #endregion
    }
}