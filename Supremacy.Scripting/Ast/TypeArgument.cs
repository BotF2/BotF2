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

        public Type[] ResolvedTypes { get; private set; }

        public TypeArguments()
        {
            _arguments = new List<FullNamedExpression>();
        }

        public void Add(FullNamedExpression type)
        {
            if (this == Empty)
            {
                throw new InvalidOperationException("Cannot modify TypeArguments.Empty.");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            _arguments.Add(type);
        }

        public void Add(TypeArguments newArguments)
        {
            if (this == Empty)
            {
                throw new InvalidOperationException("Cannot modify TypeArguments.Empty.");
            }

            if (newArguments == null)
            {
                throw new ArgumentNullException("newArguments");
            }

            _arguments.AddRange(newArguments._arguments);
        }

        public int Count => _arguments.Count;

        public FullNamedExpression this[int index] => _arguments[index];

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
            {
                return true;
            }

            if (ResolvedTypes != null)
            {
                return true;
            }

            ResolvedTypes = new Type[_arguments.Count];

            bool success = true;

            for (int i = 0; i < _arguments.Count; i++)
            {
                TypeExpression typeExpression = _arguments[i].ResolveAsTypeTerminal(parseContext, false);
                if (typeExpression == null)
                {
                    success = false;
                    continue;
                }

                ResolvedTypes[i] = typeExpression.Type;

                if (typeExpression.Type.IsSealed && typeExpression.Type.IsAbstract)
                {
                    parseContext.ReportError(
                        CompilerErrors.StaticClassesCannotBeUsedAsGenericArguments,
                        typeExpression.Span,
                        typeExpression.Type.FullName);

                    success = false;
                }

                if (typeExpression.Type.IsPointer)
                {
                    parseContext.ReportError(
                        CompilerErrors.TypeMayNotBeUsedAsGenericArgument,
                        typeExpression.Span,
                        typeExpression.Type.FullName);

                    success = false;
                }
            }

            if (!success)
            {
                ResolvedTypes = Type.EmptyTypes;
            }

            return success;
        }

        public TypeArguments Clone()
		{
            return this == Empty ? (this) : new TypeArguments(_arguments.ToArray());
        }

        public TypeArguments Clone(CloneContext cloneContext)
        {
            TypeArguments clone = new TypeArguments { ResolvedTypes = ResolvedTypes };
            _arguments.CloneTo(cloneContext, clone._arguments);
            return clone;
        }

        public string GetSignatureForError()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Count; ++i)
            {
                if (i != 0)
                {
                    sb.Append(',');
                }

                sb.Append(_arguments[i].GetSignatureForError());
            }
            return sb.ToString();
        }
    }
}