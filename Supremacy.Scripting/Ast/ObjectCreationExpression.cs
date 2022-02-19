using System.Linq;

using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    public class ObjectCreationExpression : Expression
    {
        private Expression _objectType;

        public ObjectCreationExpression()
        {
            Arguments = new Arguments();
        }

        public ObjectInitializerExpression Initializer { get; set; }

        public Expression ObjectType
        {
            get => _objectType;
            set => _objectType = value;
        }

        public Arguments Arguments { get; }

        public bool HasArguments => Arguments.Count != 0;

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _objectType, prefix, postfix);

            for (int i = 0; i < Arguments.Count; i++)
            {
                Argument argument = Arguments[i];
                Walk(ref argument, prefix, postfix);
                Arguments[i] = argument;
            }
        }

        public override Expression DoResolve(Runtime.ParseContext parseContext)
        {
            Expression typeExpression = _objectType = _objectType.ResolveAsTypeStep(parseContext, false);
            if (typeExpression == null)
            {
                return base.DoResolve(parseContext);
            }

            Arguments.Resolve(parseContext);

            Type = typeExpression.Type;

            ObjectInitializerExpression initializer = Initializer;

            if (initializer == null)
            {
                return new NewExpression(
                    _objectType,
                    Arguments,
                    Span).Resolve(parseContext);
            }

            if (initializer is CollectionInitializerExpression)
            {
                System.Collections.Generic.IEnumerable<System.Type> genericInterfaces = TypeManager.DropGenericTypeArguments(Type).GetInterfaces()
                    .Where(o => o.IsGenericType)
                    .Select(o => o.GetGenericTypeDefinition());

                if (!genericInterfaces.Contains(TypeManager.CoreTypes.GenericEnumerableInterface))
                {
                    parseContext.ReportError(
                        1925,
                        string.Format(
                            "Cannot initialize object of type '{0}' with a collection initializer.",
                            TypeManager.GetCSharpName(Type)),
                        Span);

                }
            }

            return new NewInitExpression(
                _objectType,
                Arguments,
                initializer,
                Span).Resolve(parseContext);
        }

        public override void Dump(Runtime.SourceWriter sw, int indentChange)
        {
            sw.Write("new ");

            DumpChild(_objectType, sw, indentChange);

            if (HasArguments)
            {
                sw.Write("(");
                Arguments.Dump(sw);
                sw.Write(")");
            }

            if (Initializer != null)
            {
                DumpChild(Initializer, sw);
            }
        }
    }
}