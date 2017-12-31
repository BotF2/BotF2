using System;
using System.Linq;
using System.Reflection;

using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    /// <summary>
    ///   Abstract base class for type parameter constraints.
    ///   The type parameter can come from a generic type definition or from reflection.
    /// </summary>
    public abstract class GenericConstraints
    {
        public abstract string TypeParameter { get; }

        public abstract GenericParameterAttributes Attributes { get; }

        public bool HasConstructorConstraint
        {
            get { return (Attributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0; }
        }

        public bool HasReferenceTypeConstraint
        {
            get { return (Attributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0; }
        }

        public bool HasValueTypeConstraint
        {
            get { return (Attributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0; }
        }

        public virtual bool HasClassConstraint
        {
            get { return ClassConstraint != null; }
        }

        public abstract Type ClassConstraint { get; }

        public abstract Type[] InterfaceConstraints { get; }

        public abstract Type EffectiveBaseClass { get; }

        // <summary>
        //   Returns whether the type parameter is "known to be a reference type".
        // </summary>
        public virtual bool IsReferenceType
        {
            get
            {
                if (HasReferenceTypeConstraint)
                    return true;
                if (HasValueTypeConstraint)
                    return false;

                if (ClassConstraint != null)
                {
                    if (ClassConstraint.IsValueType)
                        return false;

                    if (ClassConstraint != TypeManager.CoreTypes.Object)
                        return true;
                }

                return (from t in InterfaceConstraints
                        where t.IsGenericParameter
                        select TypeManager.GetTypeParameterConstraints(t)).Any(gc => (gc != null) && gc.IsReferenceType);
            }
        }

        // <summary>
        //   Returns whether the type parameter is "known to be a value type".
        // </summary>
        public virtual bool IsValueType
        {
            get
            {
                if (HasValueTypeConstraint)
                    return true;
                if (HasReferenceTypeConstraint)
                    return false;

                if (ClassConstraint != null)
                {
                    if (!TypeManager.IsValueType(ClassConstraint))
                        return false;

                    if (ClassConstraint != TypeManager.CoreTypes.ValueType)
                        return true;
                }

                return (from t in InterfaceConstraints
                        where t.IsGenericParameter
                        select TypeManager.GetTypeParameterConstraints(t)).Any(gc => (gc != null) && gc.IsValueType);
            }
        }
    }

    public class ReflectionConstraints : GenericConstraints
    {
        private readonly GenericParameterAttributes _attributes;
        private readonly Type _baseType;
        private readonly Type _classConstraint;
        private readonly Type[] _interfaceConstraints;
        private readonly string _name;

        private ReflectionConstraints(string name, Type[] constraints, GenericParameterAttributes attributes)
        {
            _name = name;
            _attributes = attributes;

            var interfaceConstraintsPos = 0;
            if ((attributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
            {
                _baseType = TypeManager.CoreTypes.ValueType;
                interfaceConstraintsPos = 1;
            }
            else if ((attributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
            {
                if (constraints.Length > 0 && constraints[0].IsClass)
                {
                    _classConstraint = _baseType = constraints[0];
                    interfaceConstraintsPos = 1;
                }
                else
                {
                    _baseType = TypeManager.CoreTypes.Object;
                }
            }
            else
            {
                _baseType = TypeManager.CoreTypes.Object;
            }

            if (constraints.Length > interfaceConstraintsPos)
            {
                if (interfaceConstraintsPos == 0)
                {
                    _interfaceConstraints = constraints;
                }
                else
                {
                    _interfaceConstraints = new Type[constraints.Length - interfaceConstraintsPos];
                    Array.Copy(constraints, interfaceConstraintsPos, _interfaceConstraints, 0, _interfaceConstraints.Length);
                }
            }
            else
            {
                _interfaceConstraints = Type.EmptyTypes;
            }
        }

        public override string TypeParameter
        {
            get { return _name; }
        }

        public override GenericParameterAttributes Attributes
        {
            get { return _attributes; }
        }

        public override Type ClassConstraint
        {
            get { return _classConstraint; }
        }

        public override Type EffectiveBaseClass
        {
            get { return _baseType; }
        }

        public override Type[] InterfaceConstraints
        {
            get { return _interfaceConstraints; }
        }

        public static GenericConstraints GetConstraints(Type t)
        {
            var constraints = t.GetGenericParameterConstraints();
            var attrs = t.GenericParameterAttributes;
            if (constraints.Length == 0 && attrs == GenericParameterAttributes.None)
                return null;
            return new ReflectionConstraints(t.Name, constraints, attrs);
        }
    }
}