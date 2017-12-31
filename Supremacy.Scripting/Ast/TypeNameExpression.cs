using System.ComponentModel;

using Microsoft.Scripting;

namespace Supremacy.Scripting.Ast
{
    public class TypeNameExpression : FullNamedExpression
    {
        private readonly TypeArguments _typeArguments;

        public TypeNameExpression()
        {
            _typeArguments = new TypeArguments();
        }

        public TypeNameExpression(string name, TypeArguments typeArguments, SourceSpan span)
        {
            _typeArguments = typeArguments ?? new TypeArguments();
            Name = name;
            Span = span;
        }

        [DefaultValue(null)]
        public string Name { get; set; }

        public TypeArguments TypeArguments
        {
            get { return _typeArguments; }
        }

        [DefaultValue(false)]
        public bool HasTypeArguments
        {
            get { return (TypeArguments.Count != 0); }
        }
    }
}