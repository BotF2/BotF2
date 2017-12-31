using System.Linq;

using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    public class ObjectCreationExpression : Expression
    {
        private readonly Arguments _arguments;
        private Expression _objectType;

        public ObjectCreationExpression()
        {
            _arguments = new Arguments();
        }

        public ObjectInitializerExpression Initializer { get; set; }

        public Expression ObjectType
        {
            get { return _objectType; }
            set { _objectType = value; }
        }

        public Arguments Arguments
        {
            get { return _arguments; }
        }

        public bool HasArguments
        {
            get { return (_arguments.Count != 0); }
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _objectType, prefix, postfix);

            for (var i = 0; i < _arguments.Count; i++)
            {
                var argument = _arguments[i];
                Walk(ref argument, prefix, postfix);
                _arguments[i] = argument;
            }
        }

        public override Expression DoResolve(Runtime.ParseContext parseContext)
        {
            var typeExpression = _objectType = _objectType.ResolveAsTypeStep(parseContext, false);
            if (typeExpression == null)
                return base.DoResolve(parseContext);

            _arguments.Resolve(parseContext);

            Type = typeExpression.Type;

            var initializer = Initializer;

            if (initializer == null)
            {
                return new NewExpression(
                    _objectType,
                    _arguments,
                    Span).Resolve(parseContext);
            }

            if (initializer is CollectionInitializerExpression)
            {
                var genericInterfaces = TypeManager.DropGenericTypeArguments(Type).GetInterfaces()
                    .Where(o => o.IsGenericType)
                    .Select(o => o.GetGenericTypeDefinition());

                if (!genericInterfaces.Contains(TypeManager.CoreTypes.GenericEnumerableInterface))
                {
/*
                    parseContext.ReportError(
                        1925,
                        string.Format(
                            "A field or property '{0}' cannot be initialized with a collection " +
                            "object initializer because type '{1}' does not implement '{2}' interface.",
                            typeExpression.GetSignatureForError(),
                            TypeManager.GetCSharpName(objectType),
                            TypeManager.GetCSharpName(TypeManager.CoreTypes.GenericEnumerableInterface)),
                        this.Span);
*/
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
                _arguments,
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
                _arguments.Dump(sw);
                sw.Write(")");
            }

            if (Initializer != null)
                DumpChild(Initializer, sw);
        }
    }
}