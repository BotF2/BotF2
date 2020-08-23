using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    public enum ArgumentType
    {
        None = 0,
        Ref = 1,			// ref modifier used
        Out = 2,			// out modifier used
        Default = 3,		// argument created from default parameter value
        //DynamicTypeName = 4	// System.Type argument for dynamic binding
    }

    public class Argument : Ast
    {
        private Expression _value = EmptyExpression.Null;

        public Argument() { }

        public Argument(Expression value)
        {
            _value = value;
        }

        public ArgumentType ArgumentType
        {
            get; set;
        }

        public Parameter.Modifier Modifier
        {
            get
            {
                switch (ArgumentType)
                {
                    case ArgumentType.Out:
                        return Parameter.Modifier.Out;

                    case ArgumentType.Ref:
                        return Parameter.Modifier.Ref;

                    default:
                        return Parameter.Modifier.None;
                }
            }
        }

        public bool IsByRef => ArgumentType == ArgumentType.Out || ArgumentType == ArgumentType.Ref;

        public bool IsDefaultArgument => ArgumentType == ArgumentType.Default;

        public Expression Value
        {
            get => _value;
            set => _value = value ?? EmptyExpression.Null;
        }

        public override SourceSpan Span => (_value == null) ? SourceSpan.None : _value.Span;

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            if (!(target is Argument clone))
            {
                return;
            }

            clone._value = Clone(cloneContext, _value);
            clone.ArgumentType = ArgumentType;
            clone.FileName = FileName;
            clone.Span = Span;
        }

        public string GetSignatureForError()
        {
            return Value.ExpressionClass == ExpressionClass.MethodGroup ? Value.ExpressionClassName : TypeManager.GetCSharpName(Value.Type);
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            DumpChild(_value, sw, indentChange);
        }

        public void Resolve(ParseContext ec)
        {
            if (Value == EmptyExpression.Null)
            {
                return;
            }

            Value = Value.Resolve(ec);
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _value, prefix, postfix);
        }
    }

    public class NamedArgument : Argument
    {
        public string Name { get; set; }

        public NamedArgument(string name, Expression value)
            : base(value)
        {
            Name = name;
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            if (!(target is NamedArgument clone))
            {
                return;
            }

            clone.Name = Name;
        }
    }
}