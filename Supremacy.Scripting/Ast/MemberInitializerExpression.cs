using System.Reflection;

using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class MemberInitializerExpression : Expression
    {
        private Expression _value;

        public string MemberName { get; set; }

        public Expression Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _value, prefix, postfix);
        }

        public Expression DoResolve(ParseContext parseContext, Expression leftSide)
        {
            var memberName = MemberName;
            var memberInfo = (MemberInfo)leftSide.Type.GetProperty(memberName) ??
                             leftSide.Type.GetField(memberName);
            
            return new NewInitMemberBinding(
                memberInfo,
                _value).Resolve(parseContext);
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write(MemberName);
            sw.Write(" = ");
            DumpChild(Value, sw, indentChange);
        }
    }
}