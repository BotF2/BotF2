using System.Reflection;

using Microsoft.Scripting.Actions;

using System.Linq;

namespace Supremacy.Scripting.Utility
{
    public static class TrackerExtensions
    {
        private static readonly FieldInfo CtorField = typeof(ConstructorTracker).GetField(
            "_ctor",
            BindingFlags.NonPublic | BindingFlags.Instance);

        public static MemberInfo GetMemberInfo(this MemberTracker tracker)
        {
            switch (tracker.MemberType)
            {
                case TrackerTypes.None:
                    return null;
                
                case TrackerTypes.Constructor:
                    return (ConstructorInfo)CtorField.GetValue(tracker);
                
                case TrackerTypes.Event:
                    return ((EventTracker)tracker).Event;
                
                case TrackerTypes.Field:
                    return ((FieldTracker)tracker).Field;
                
                case TrackerTypes.Method:
                    return ((MethodTracker)tracker).Method;

                case TrackerTypes.Property:
                    if (tracker is ReflectedPropertyTracker propertyTracker)
                    {
                        return propertyTracker.Property;
                    }

                    return null;

                case TrackerTypes.Type:
                    return ((TypeTracker)tracker).Type;

                case TrackerTypes.Namespace:
                    return null;

                case TrackerTypes.MethodGroup:
                    MethodBase[] methodBases = ((MethodGroup)tracker).GetMethodBases();
                    if (methodBases.Length == 1)
                    {
                        return methodBases[0];
                    }

                    return null;

                case TrackerTypes.TypeGroup:
                    System.Collections.Generic.IEnumerable<System.Type> types = ((TypeGroup)tracker).Types;
                    if (!types.Skip(1).Any())
                    {
                        return types.Single();
                    }

                    return null;

                case TrackerTypes.Custom:
                    return null;

                case TrackerTypes.Bound:
                    return ((BoundMemberTracker)tracker).BoundTo.GetMemberInfo();

                default:
                case TrackerTypes.All:
                    return null;
            }
        } 
    }
}