using System.ComponentModel;

using Microsoft.Scripting;

namespace Supremacy.Scripting.Ast
{
    public class TypeNameExpression : FullNamedExpression
    {
        public TypeNameExpression()
        {
            TypeArguments = new TypeArguments();
        }

        public TypeNameExpression(string name, TypeArguments typeArguments, SourceSpan span)
        {
            TypeArguments = typeArguments ?? new TypeArguments();
            Name = name;
            Span = span;
        }

        [DefaultValue(null)]
        public string Name { get; set; }

        public TypeArguments TypeArguments { get; }

        [DefaultValue(false)]
        public bool HasTypeArguments => TypeArguments.Count != 0;
    }
}