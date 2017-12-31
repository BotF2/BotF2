using System;
using System.Collections.Generic;
using System.Diagnostics;

using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    public class ObjectInitializerExpression : Expression
    {
        private readonly List<Expression> _initializers;
        private Type _targetType;

        public ObjectInitializerExpression()
        {
            _initializers = new List<Expression>();
        }

        public IList<Expression> Initializers
        {
            get { return _initializers; }
        }

        public override Expression DoResolve(Runtime.ParseContext parseContext)
        {
            List<string> elementNames = null;
            var isCollectionInitialization = false;

            for (int i = 0; i < _initializers.Count; ++i)
            {
                var initializer = _initializers[i];
                var elementInitializer = initializer as ElementInitializer;

                if (i == 0)
                {
                    if (elementInitializer != null)
                    {
                        elementNames = new List<string>(_initializers.Count) { elementInitializer.MemberName };
                    }
                    else
                    {
                        if (!TypeManager.ImplementsInterface(
                            parseContext.CurrentInitializerVariable.Type,
                            TypeManager.CoreTypes.GenericEnumerableInterface))
                        {
                            parseContext.ReportError(
                                1922,
                                string.Format(
                                    "A field or property '{0}' cannot be initialized with a collection " +
                                    "initializer because type '{1}' does not implement '{2}' interface.",
                                    parseContext.CurrentInitializerVariable.GetSignatureForError(),
                                    TypeManager.GetCSharpName(parseContext.CurrentInitializerVariable.Type),
                                    TypeManager.GetCSharpName(TypeManager.CoreTypes.GenericEnumerableInterface)),
                                Span);

                            return null;
                        }
                        isCollectionInitialization = true;
                    }
                }
                else
                {
                    if (isCollectionInitialization != (elementInitializer == null))
                    {
                        parseContext.ReportError(
                            747,
                            string.Format(
                                "Inconsistent '{0}' member declaration.",
                                isCollectionInitialization ? "collection initializer" : "object initializer"),
                            initializer.Span);

                        continue;
                    }

                    if (!isCollectionInitialization)
                    {
                        Debug.Assert(elementNames != null);
                        
                        if (elementNames.Contains(elementInitializer.MemberName))
                        {
                            parseContext.ReportError(
                                1912,
                                string.Format(
                                    "An object initializer includes more than one member '{0}' initialization.",
                                    elementInitializer.MemberName),
                                elementInitializer.Span);
                        }
                        else
                        {
                            elementNames.Add(elementInitializer.MemberName);
                        }
                    }
                }

                _initializers[i] = initializer.Resolve(parseContext);
            }

            _targetType = parseContext.CurrentInitializerVariable.Type;

            if (isCollectionInitialization)
            {
                if (TypeManager.HasElementType(_targetType))
                {
                    parseContext.ReportError(
                        1925,
                        string.Format(
                            "Cannot initialize object of type '{0}' with a collection initializer.",
                            TypeManager.GetCSharpName(_targetType)),
                        Span);
                }
            }

            ExpressionClass = ExpressionClass.Variable;

            return this;
        }

        public override void Dump(Runtime.SourceWriter sw, int indentChange)
        {
            sw.Write(" {");

            for (var i = 0; i < _initializers.Count; i++)
            {
                if (i != 0)
                    sw.Write(", ");
                else
                    sw.Write(" ");
                DumpChild(_initializers[i], sw, indentChange);
            }

            sw.Write((_initializers.Count == 0) ? "}" : " }");
        }
    }
}