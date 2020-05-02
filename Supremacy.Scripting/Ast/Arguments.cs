using System;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;

using MSAst = System.Linq.Expressions.Expression;

using System.Linq;

namespace Supremacy.Scripting.Ast
{
    public class Arguments : IList<Argument>
    {
        private readonly MSAst[] _emptyTransform = new MSAst[0];

#pragma warning disable 649
        private readonly List<Argument> _arguments;
        private List<Argument> _reordered;
#pragma warning restore 649

        public Arguments()
        {
            _arguments = new List<Argument>();
        }

        public Arguments(int capacity)
        {
            _arguments = new List<Argument>(capacity);
        }

        public int Add(Argument arg)
        {
            _arguments.Add(arg);
            return _arguments.Count - 1;
        }

        void ICollection<Argument>.Clear()
        {
            _arguments.Clear();
        }

        bool ICollection<Argument>.Contains(Argument item)
        {
            return _arguments.Contains(item);
        }

        void ICollection<Argument>.CopyTo(Argument[] array, int arrayIndex)
        {
            _arguments.CopyTo(array, arrayIndex);
        }

        public bool Remove(Argument item)
        {
            return _arguments.Remove(item);
        }

        public void AddRange(Arguments args)
        {
            _arguments.AddRange(args._arguments);
        }

        public void AddRange(IEnumerable<Argument> arguments)
        {
            _arguments.AddRange(arguments);
        }

        public void MarkReorderedArgument(NamedArgument a)
        {
            //
            // Constant expression can have no effect on left-to-right execution
            //
            if (a.Value is ConstantExpression)
            {
                return;
            }

            if (_reordered == null)
            {
                _reordered = new List<Argument>();
            }

            _reordered.Add(a);
        }

        public void Dump(SourceWriter sw)
        {
            int i = 0;
            foreach (Argument argument in _arguments)
            {
                if (i++ != 0)
                {
                    sw.Write(", ");
                }

                if (argument is NamedArgument)
                {
                    sw.Write(((NamedArgument)argument).Name + ": ");
                }

                argument.Dump(sw);
            }
        }

        public Arguments Clone(CloneContext cloneContext)
        {
            Arguments clone = new Arguments(_arguments.Count);
            clone.AddRange(_arguments.Select(o => Ast.Clone(cloneContext, o)));
            return clone;
        }

        void ICollection<Argument>.Add(Argument item)
        {
            _ = Add(item);
        }

        public int Count => _arguments.Count;

        public bool IsReadOnly => false;

        public IEnumerator<Argument> GetEnumerator()
        {
            return _arguments.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _arguments.GetEnumerator();
        }

        public int IndexOf(Argument item)
        {
            return _arguments.IndexOf(item);
        }

        public void Insert(int index, Argument argument)
        {
            _arguments.Insert(index, argument);
        }

        public static MSAst[] Transform(Arguments arguments, ScriptGenerator generator)
		{
            return arguments == null ? (new MSAst[0]) : arguments.Transform(generator);
        }

        public MSAst[] Transform(ScriptGenerator generator)
        {
            if (_arguments.Count == 0)
            {
                return _emptyTransform;
            }

            if (_reordered != null)
            {
                throw new NotImplementedException();
            }

            MSAst[] transformedArguments = new MSAst[_arguments.Count];
            for (int i = 0; i < _arguments.Count; i++)
                transformedArguments[i] = _arguments[i].Value.Transform(generator);

            return transformedArguments;
        }

        //
        // Returns dynamic when at least one argument is of dynamic type
        //
        public void Resolve(ParseContext ec)
        {
            foreach (Argument argument in _arguments)
            {
                argument.Resolve(ec);
            }
        }

        public void RemoveAt(int index)
        {
            _arguments.RemoveAt(index);
        }

        public Argument this[int index]
        {
            get => _arguments[index];
            set => _arguments[index] = value;
        }

        public static Arguments CreateDelegateMethodArguments(ParametersCollection pd, SourceSpan location)
        {
            Arguments delegateArguments = new Arguments(pd.Count);
            for (int i = 0; i < pd.Count; ++i)
            {
                ArgumentType typeModifier;
                Type argumentType = pd.Types[i];
                switch (pd.FixedParameters[i].ModifierFlags)
                {
                    case Parameter.Modifier.Ref:
                        typeModifier = ArgumentType.Ref;
                        //atype = atype.GetElementType ();
                        break;
                    case Parameter.Modifier.Out:
                        typeModifier = ArgumentType.Out;
                        //atype = atype.GetElementType ();
                        break;
                    default:
                        typeModifier = 0;
                        break;
                }
                _ = delegateArguments.Add(new Argument(new TypeExpression(argumentType) { Span = location })
                    {
                        ArgumentType = typeModifier
                    });
            }
            return delegateArguments;
        }
    }
}