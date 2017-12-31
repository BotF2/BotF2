using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Supremacy.Scripting.Ast;

namespace Supremacy.Scripting.Utility
{
    internal class AnonymousTypeClass
    {
        private readonly Dictionary<string, FieldInfo> _fieldLookup;
        private readonly Type _type;

        public AnonymousTypeClass(AnonymousObjectInitializer initializer, Type type)
        {
            if (initializer == null)
                throw new ArgumentNullException("initializer");

            _type = type;
            _fieldLookup = initializer.MemberDeclarators.ToDictionary(
                o => o.Name,
                o => type.GetField(o.Name));
        }

        public Type Type
        {
            get { return _type; }
        }

        public MemberInfo this[string name]
        {
            get { return _fieldLookup[name]; }
        }
    }
}