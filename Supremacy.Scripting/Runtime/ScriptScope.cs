using System;
using System.Collections.Generic;
using System.Linq;

using System.Linq.Expressions;

using Microsoft.Scripting.Ast;

using MSAst = System.Linq.Expressions.Expression;

namespace Supremacy.Scripting.Runtime
{
    internal class ScriptScope
    {
        private int _lambdaCount;
        private readonly LambdaBuilder _block;
        private readonly Dictionary<string, ParameterExpression> _variables = new Dictionary<string, ParameterExpression>();

        public IEnumerable<ParameterExpression> Locals => _variables.Values.OfType<ParameterExpression>().ToList();

        public ScriptScope(ScriptScope parent, SymbolDocumentInfo document)
            : this(parent, null, document) { }

        public ScriptScope(ScriptScope parent, string name, SymbolDocumentInfo document)
        {
            Parent = parent;
            _block = Utils.Lambda(typeof(object), name ?? MakeLambdaName());
            if (parent != null && document == null)
            {
                document = Parent.Document;
            }

            Document = document;
        }

        public string MakeLambdaName()
        {
            return Parent != null ? Parent.MakeLambdaName() : "lambda" + _lambdaCount++;
        }

        public string Name => _block.Name;

        public ScriptScope Parent { get; }

        public ScriptScope TopScope => Parent == null ? this : Parent.TopScope;

        public SymbolDocumentInfo Document { get; }

        public ParameterExpression CreateParameter(string name)
        {
            ParameterExpression variable = _block.Parameter(typeof(object), name);
            _variables[name] = variable;
            return variable;
        }

        public MSAst GetOrMakeLocal(string name)
        {
            return GetOrMakeLocal(name, typeof(object));
        }

        public ParameterExpression GetOrMakeLocal(string name, Type type)
        {
            if (_variables.TryGetValue(name, out ParameterExpression variable) && type.IsAssignableFrom(variable.Type))
            {
                return variable;
            }

            variable = _block.Parameter(type, name);
            _variables[name] = variable;
            return variable;
        }

        public ParameterExpression LookupName(string name)
        {
            if (_variables.TryGetValue(name, out ParameterExpression variable))
            {
                return variable;
            }

            return Parent?.LookupName(name);
        }

        public ParameterExpression HiddenVariable(Type type, string name)
        {
            return _block.HiddenVariable(type, name);
        }

        public LambdaExpression FinishScope(MSAst body, Type lambdaType)
        {
            _block.Body = EnsureExpressionReturnsObject(body);
            return _block.MakeLambda(lambdaType);
        }

        public LambdaExpression FinishScope(MSAst body)
        {
            _block.Body = EnsureExpressionReturnsObject(body);
            if (_block.Body.Type != typeof(object))
            {
                _block.ReturnType = _block.Body.Type;
            }

            return _block.MakeLambda();
        }

        private static MSAst EnsureExpressionReturnsObject(MSAst body)
        {
            return body.Type == typeof(void) ? MSAst.Block(body, MSAst.Default(typeof(object))) : body;
        }
    }
}