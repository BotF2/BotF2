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

        public AnonymousTypeClass(AnonymousObjectInitializer initializer, Type type)
        {
            if (initializer == null)
            {
                throw new ArgumentNullException("initializer");
            }

            Type = type;
            _fieldLookup = initializer.MemberDeclarators.ToDictionary(
                o => o.Name,
                o => type.GetField(o.Name));
        }

        public Type Type { get; }

        public MemberInfo this[string name] => _fieldLookup[name];
    }
}