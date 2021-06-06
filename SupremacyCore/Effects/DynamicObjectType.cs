using System;
using System.Collections;
using System.Diagnostics;

using Supremacy.Annotations;

namespace Supremacy.Effects
{
    [Serializable]
    public sealed class DynamicObjectType
    {
        /// <summary> 
        ///     Retrieve a DynamicObjectType that represents a given system (CLR) type
        /// </summary> 
        /// <param name="systemType">The system (CLR) type to convert</param>
        /// <returns>
        ///     A DynamicObjectType that represents the system (CLR) type (will create
        ///     a new one if doesn't exist) 
        /// </returns>
        public static DynamicObjectType FromSystemType([NotNull] Type systemType)
        {
            if (systemType == null)
                throw new ArgumentNullException("systemType");

            if (!typeof(DynamicObject).IsAssignableFrom(systemType))
            {
                throw new ArgumentException(
                    string.Format(
                        "Type '{0}' does not derive from DynamicObject.",
                        systemType.FullName));
            }

            return FromSystemTypeInternal(systemType);
        }

        /// <summary> 
        ///     Helper method for the public FromSystemType call but without
        ///     the expensive IsAssignableFrom parameter validation. 
        /// </summary>
        internal static DynamicObjectType FromSystemTypeInternal(Type systemType)
        {
            Debug.Assert(
                systemType != null && typeof(DynamicObject).IsAssignableFrom(systemType),
                "Invalid systemType argument");

            DynamicObjectType result;

            lock (DynamicProperty.Synchronized)
            {
                /*
                 * Recursive routine to (set up if necessary) and use the DTypeFromCLRType
                 * hashtable that is used for the actual lookup.
                 */
                result = FromSystemTypeRecursive(systemType);
            }

            return result;
        }

        /*
         * The caller must wrap this routine inside a locked block.  This recursive routine manipulates
         * the static hashtable DTypeFromCLRTypeand it must not be allowed to do this across multiple
         * threads  simultaneously.
         */
        private static DynamicObjectType FromSystemTypeRecursive(Type systemType)
        {
            DynamicObjectType dType = (DynamicObjectType)ClrTypeMappings[systemType];

            if (dType != null)
                return dType;

            dType = new DynamicObjectType { _systemType = systemType };

            ClrTypeMappings[systemType] = dType;

            if (systemType != typeof(DynamicObject))
                dType._baseDType = FromSystemTypeRecursive(systemType.BaseType);

            dType._id = DynamicObjectTypeCount++;

            return dType;
        }

        /// <summary> 
        ///     Zero-based unique identifier for constant-time array lookup operations 
        /// </summary>
        /// <remarks> 
        ///     There is no guarantee on this value. It can vary between application runs.
        /// </remarks>
        public int Id => _id;

        /// <summary>
        /// The system (CLR) type that this DynamicObjectType represents 
        /// </summary> 
        public Type SystemType => _systemType;

        /// <summary>
        /// The DynamicObjectType of the base class
        /// </summary>
        public DynamicObjectType BaseType => _baseDType;

        /// <summary>
        ///     Returns the name of the represented system (CLR) type 
        /// </summary>
        public string Name => SystemType.Name;

        /// <summary>
        ///     Determines whether the specifed object is an instance of the current DynamicObjectType 
        /// </summary>
        /// <param name="dependencyObject">The object to compare with the current Type</param>
        /// <returns>
        ///     true if the current DynamicObjectType is in the inheritance hierarchy of the 
        ///     object represented by the o parameter. false otherwise.
        /// </returns> 
        public bool IsInstanceOfType(DynamicObject dependencyObject)
        {
            if (dependencyObject != null)
            {
                DynamicObjectType dynamicObjectType = dependencyObject.DynamicObjectType;
                do
                {
                    if (dynamicObjectType.Id == Id)
                        return true;

                    dynamicObjectType = dynamicObjectType._baseDType;
                }
                while (dynamicObjectType != null);
            }
            return false;
        }

        /// <summary> 
        ///     Determines whether the current DynamicObjectType derives from the
        ///     specified DynamicObjectType
        /// </summary>
        /// <param name="dynamicObjectType">The DynamicObjectType to compare 
        ///     with the current DynamicObjectType</param>
        /// <returns> 
        ///     true if the DynamicObjectType represented by the dType parameter and the 
        ///     current DynamicObjectType represent classes, and the class represented
        ///     by the current DynamicObjectType derives from the class represented by 
        ///     c; otherwise, false. This method also returns false if dType and the
        ///     current Type represent the same class.
        /// </returns>
        public bool IsSubclassOf(DynamicObjectType dynamicObjectType)
        {
            // Check for null and return false, since this type is never a subclass of null. 
            if (dynamicObjectType != null)
            {
                // A DynamicObjectType isn't considered a subclass of itself, so start with base type 
                DynamicObjectType dynamicObjectType1 = _baseDType;

                while (dynamicObjectType1 != null)
                {
                    if (dynamicObjectType1.Id == dynamicObjectType.Id)
                        return true;

                    dynamicObjectType1 = dynamicObjectType1._baseDType;
                }
            }
            return false;
        }

        /// <summary> 
        ///     Serves as a hash function for a particular type, suitable for use in
        ///     hashing algorithms and data structures like a hash table 
        /// </summary>
        /// <returns>The DynamicObjectType's Id</returns>
        public override int GetHashCode()
        {
            return _id;
        }

        // DynamicObjectType may not be constructed outside of FromSystemType
        private DynamicObjectType() { }

        private int _id;
        private Type _systemType;
        private DynamicObjectType _baseDType;

        private static readonly Hashtable ClrTypeMappings = new Hashtable();
        private static int DynamicObjectTypeCount;
    }
}