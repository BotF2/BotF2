using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class TypeArguments : IEnumerable<FullNamedExpression>
    {
        public static readonly TypeArguments Empty = new TypeArguments();

        private readonly List<FullNamedExpression> _arguments;
        private Type[] _resolvedTypes;

        public Type[] ResolvedTypes
        {
            get { return _resolvedTypes; }
        }

        public TypeArguments()
        {
            _arguments = new List<FullNamedExpression>();
        }

        public void Add(FullNamedExpression type)
        {
            if (this == Empty)
                throw new InvalidOperationException("Cannot modify TypeArguments.Empty.");
            if (type == null)
                throw new ArgumentNullException("type");
            _arguments.Add(type);
        }

        public void Add(TypeArguments newArguments)
        {
            if (this == Empty)
                throw new InvalidOperationException("Cannot modify TypeArguments.Empty.");
            if (newArguments == null)
                throw new ArgumentNullException("newArguments");
            _arguments.AddRange(newArguments._arguments);
        }

        public int Count
        {
            get { return _arguments.Count; }
        }

        public FullNamedExpression this[int index]
        {
            get { return _arguments[index]; }
        }

        public TypeArguments(params FullNamedExpression[] arguments)
        {
            _arguments = new List<FullNamedExpression>(arguments);
        }

        public IEnumerator<FullNamedExpression> GetEnumerator()
        {
            return _arguments.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Resolve(ParseContext parseContext)
        {
            if (this == Empty)
                return true;

            if (_resolvedTypes != null)
                return true;

            _resolvedTypes = new Type[_arguments.Count];

            bool success = true;

            for (int i = 0; i < _arguments.Count; i++)
            {
                var typeExpression = _arguments[i].ResolveAsTypeTerminal(parseContext, false);
                if (typeExpression == null)
                {
                    success = false;
                    continue;
                }

                _resolvedTypes[i] = typeExpression.Type;

                if (typeExpression.Type.IsSealed && typeExpression.Type.IsAbstract)
                {
                    parseContext.ReportError(
                        CompilerErrors.StaticClassesCannotBeUsedAsGenericArguments,
                        typeExpression.Span,
                        typeExpression.Type.FullName);

                    success = false;
                }

                if (typeExpression.Type.IsPointer/* || TypeManager.IsSpecialType(typeExpression.Type)*/)
                {
                    parseContext.ReportError(
                        CompilerErrors.TypeMayNotBeUsedAsGenericArgument,
                        typeExpression.Span,
                        typeExpression.Type.FullName);

                    success = false;
                }
            }

            if (!success)
                _resolvedTypes = Type.EmptyTypes;

            return success;
        }

		public TypeArguments Clone ()
		{
            if (this == Empty)
                return this;
		    return new TypeArguments(_arguments.ToArray());
		}

        public TypeArguments Clone(CloneContext cloneContext)
        {
            var clone = new TypeArguments{ _resolvedTypes = _resolvedTypes };
            _arguments.CloneTo(cloneContext, clone._arguments);
            return clone;
        }

        public string GetSignatureForError()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < Count; ++i)
            {
                if (i != 0)
                    sb.Append(',');

                sb.Append(_arguments[i].GetSignatureForError());
            }
            return sb.ToString();
        }
    }
}