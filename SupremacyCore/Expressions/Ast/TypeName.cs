using System.Collections.Generic;

namespace Supremacy.Expressions.Ast
{
    public class TypeName
    {
        private readonly IList<TypeArgument> _typeArguments;

        public TypeName()
        {
            _typeArguments = new List<TypeArgument>();
        }

        public string Name { get; set; }

        public bool IsBuiltinType { get; set; }

        public IList<TypeArgument> TypeArguments
        {
            get { return _typeArguments; }
        }
    }
}