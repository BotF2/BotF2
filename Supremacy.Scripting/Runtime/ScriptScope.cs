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
        private readonly SymbolDocumentInfo _document;
        private readonly ScriptScope _parent;
        private readonly LambdaBuilder _block;
        private readonly Dictionary<string, ParameterExpression> _variables = new Dictionary<string, ParameterExpression>();

        public IEnumerable<ParameterExpression> Locals
        {
            get { return _variables.Values.OfType<ParameterExpression>().ToList(); }
        }

        public ScriptScope(ScriptScope parent, SymbolDocumentInfo document)
            : this(parent, null, document) { }

        public ScriptScope(ScriptScope parent, string name, SymbolDocumentInfo document)
        {
            _parent = parent;
            _block = Utils.Lambda(typeof(object), name ?? MakeLambdaName());
            if (parent != null && document == null)
                document = _parent.Document;
            _document = document;
        }

        public string MakeLambdaName()
        {
            if (_parent != null)
                return _parent.MakeLambdaName();
            return "lambda" + _lambdaCount++;
        }

        public string Name
        {
            get { return _block.Name; }
        }

        public ScriptScope Parent
        {
            get { return _parent; }
        }

        public ScriptScope TopScope
        {
            get
            {
                if (_parent == null)
                    return this;
                return _parent.TopScope;
            }
        }

        public SymbolDocumentInfo Document
        {
            get { return _document; }
        }

        public ParameterExpression CreateParameter(string name)
        {
            var variable = _block.Parameter(typeof(object), name);
            _variables[name] = variable;
            return variable;
        }

        public Expression GetOrMakeLocal(string name)
        {
            return GetOrMakeLocal(name, typeof(object));
        }

        public ParameterExpression GetOrMakeLocal(string name, Type type)
        {
            ParameterExpression variable;
            if (_variables.TryGetValue(name, out variable) && type.IsAssignableFrom(variable.Type))
                return variable;
            variable = _block.Parameter(type, name);
            _variables[name] = variable;
            return variable;
        }

        public ParameterExpression LookupName(string name)
        {
            ParameterExpression variable;
            if (_variables.TryGetValue(name, out variable))
                return variable;
            if (_parent != null)
                return _parent.LookupName(name);
            return null;
        }

        public ParameterExpression HiddenVariable(Type type, string name)
        {
            return _block.HiddenVariable(type, name);
        }

        public LambdaExpression FinishScope(Expression body, Type lambdaType)
        {
            _block.Body = EnsureExpressionReturnsObject(body);
            return _block.MakeLambda(lambdaType);
        }

        public LambdaExpression FinishScope(Expression body)
        {
            _block.Body = EnsureExpressionReturnsObject(body);
            if (_block.Body.Type != typeof(object))
                _block.ReturnType = _block.Body.Type;
            return _block.MakeLambda();
        }

        private static Expression EnsureExpressionReturnsObject(Expression body)
        {
            if (body.Type == typeof(void))
                return Expression.Block(body, MSAst.Default(typeof(object)));
            return body;
        }
    }
}