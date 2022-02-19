using System.Collections.Generic;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

using System.Linq;

namespace Supremacy.Scripting.Ast
{
    public class Scope
    {
        private IDictionary<string, Parameter> _variables;
        private List<Scope> _children;
        private List<Scope> _anonymousChildren;

        public CompilerContext CompilerContext { get; }

        public SourceLocation StartLocation { get; }

        public SourceLocation EndLocation { get; set; }

        public Scope Parent { get; set; }
        public ExplicitScope Explicit { get; set; }
        public TopLevelScope TopLevel { get; set; }

        public bool IsDescendantOf(Scope scope)
        {
            if (scope == null)
            {
                return false;
            }

            if (scope == this)
            {
                return false;
            }

            for (Scope current = this; current != null; current = current.Parent)
            {
                if (current == scope)
                {
                    return true;
                }
            }

            return false;
        }

        public Scope(CompilerContext compilerContext, Scope parent)
            : this(compilerContext, parent, SourceLocation.None, SourceLocation.None)
        {
            if (compilerContext != null)
            {
                CompilerContext = compilerContext;
            }
        }

        public Scope(CompilerContext compilerContext, TopLevelScope parent, TopLevelScope source)
            : this(compilerContext, parent, source.StartLocation, SourceLocation.None)
        {
            if (compilerContext != null)
            {
                CompilerContext = compilerContext;
            }

            _children = source._children;
            _variables = source._variables;
        }

        public Scope(CompilerContext compilerContext, Scope parent, SourceLocation startLocation, SourceLocation endLocation)
        {
            if (parent != null)
            {
                parent.AddChild(this);

                TopLevel = parent.TopLevel;
                Explicit = parent.Explicit;
            }

            StartLocation = startLocation;
            CompilerContext = compilerContext ?? (parent?.CompilerContext);

            EndLocation = endLocation;
            Parent = parent;
        }

        private void AddChild(Scope scope)
        {
            if (_children == null)
            {
                _children = new List<Scope>();
            }

            _children.Add(scope);
        }

        public void AddAnonymousChild(TopLevelScope b)
        {
            if (_anonymousChildren == null)
            {
                _anonymousChildren = new List<Scope>();
            }

            _anonymousChildren.Add(b);
        }

        public void SetEndLocation(SourceLocation location)
        {
            if ((EndLocation != SourceLocation.None) && (EndLocation != location))
            {
                throw new InvalidImplementationException("End location of scope has already been set.");
            }

            EndLocation = location;
        }

        public Parameter GetVariable(string name)
        {
            for (Scope scope = this; scope != null; scope = scope.Parent)
            {
                if (scope._variables == null)
                {
                    continue;
                }

                Parameter variable = scope._variables[name];
                if (variable != null)
                {
                    return variable;
                }
            }
            return null;
        }

        protected virtual bool CheckParentConflictName(TopLevelScope scope, string name, SourceSpan span)
        {
            if (scope != null)
            {
                Expression e = scope.GetParameterReference(name, span);
                if (e != null)
                {
                    ParameterReference pr = e as ParameterReference;
                    if ((this is QueryScope) &&
                        (((pr != null) && (pr.Parameter is QueryScope.ImplicitQueryParameter)) ||
                         e is MemberAccessExpression))
                    {
                        CompilerContext.Errors.Add(
                            CompilerContext.SourceUnit,
                            "Duplicate variable name: " + name,
                            span,
                            255,
                            Severity.Error);
                        return false;
                    }
                }
            }
            return true;
        }

        public T Clone<T>(CloneContext cloneContext) where T : Scope
        {
            T clone = (T)MemberwiseClone();
            CloneTo(cloneContext, clone);
            return clone;
        }

        protected virtual void CloneTo(CloneContext cloneContext, Scope target)
        {
            cloneContext.AddBlockMap(this, target);

            target.TopLevel = (TopLevelScope)cloneContext.LookupBlock(TopLevel);
            target.Explicit = (ExplicitScope)cloneContext.LookupBlock(Explicit);

            if (Parent != null)
            {
                target.Parent = cloneContext.RemapBlockCopy(Parent);
            }

            if (_variables != null)
            {
                target._variables = new Dictionary<string, Parameter>();

                foreach (KeyValuePair<string, Parameter> keyValuePair in _variables)
                {
                    Parameter newlocal = Ast.Clone(cloneContext, keyValuePair.Value);
                    target._variables[keyValuePair.Key] = newlocal;
                    cloneContext.AddVariableMap(keyValuePair.Value, newlocal);
                }
            }

            if (target._children != null)
            {
                target._children = _children.Select(o => cloneContext.LookupBlock(o)).ToList();
            }
        }

        public bool CheckInvariantMeaningInBlock(string name, Expression e, SourceSpan span)
        {
            Scope b = this;
            IKnownVariable kvi = b.Explicit.GetKnownVariable(name);
            while (kvi == null)
            {
                b = b.Explicit.Parent;
                if (b == null)
                {
                    return true;
                }

                kvi = b.Explicit.GetKnownVariable(name);
            }

            if (kvi.Scope == b)
            {
                return true;
            }

            // Is kvi.Block nested inside 'b'
            if (b.Explicit != kvi.Scope.Explicit)
            //if (kvi.Scope.Explicit.IsDescendantOf(b.Explicit))
            {
                //
                // If a variable by the same name it defined in a nested block of this
                // block, we violate the invariant meaning in a block.
                //
                if (b == this)
                {
                    CompilerContext.Errors.Add(
                        CompilerContext.SourceUnit,
                        string.Format("'{0}' conflicts with a declaration in a child block.", name),
                        span,
                        135,
                        Severity.Error);
                    return false;
                }

                //
                // It's ok if the definition is in a nested subblock of b, but not
                // nested inside this block -- a definition in a sibling block
                // should not affect us.
                //
                return true;
            }

            //
            // Block 'b' and kvi.Block are the same textual block.
            // However, different variables are extant.
            //
            // Check if the variable is in scope in both blocks.  We use
            // an indirect check that depends on AddVariable doing its
            // part in maintaining the invariant-meaning-in-block property.
            //
            if (e is ParameterReference)
            {
                return true;
            }

            if (this is TopLevelScope)
            {
                CompilerContext.Errors.Add(
                    CompilerContext.SourceUnit,
                    string.Format("A local variable '{0}' cannot be used before it is declared.", name),
                    span,
                    841,
                    Severity.Error);
                return false;
            }

            //
            // Even though we detected the error when the name is used, we
            // treat it as if the variable declaration was in error.
            //
            OnAlreadyDeclaredError(kvi.Span, name, "parent or current");
            return false;
        }

        protected virtual void OnAlreadyDeclaredError(SourceSpan span, string name, string reason)
        {
            if (reason == null)
            {
                OnAlreadyDeclaredError(span, name);
                return;
            }

            const string errorFormat = "A local variable named '{0}' cannot be declared " +
                                       "in this scope because it would give a different meaning " +
                                       "to '{0}', which is already used in a '{1}' scope " +
                                       "to denote something else.";

            CompilerContext.Errors.Add(
                CompilerContext.SourceUnit,
                string.Format(
                    errorFormat, name, reason),
                span,
                841,
                Severity.Error);
        }

        protected virtual void OnAlreadyDeclaredError(SourceSpan span, string name)
        {
            CompilerContext.Errors.Add(
                CompilerContext.SourceUnit,
                string.Format(
                    "A local variable named '{0}' is already defined in this scope.",
                    name),
                span,
                841,
                Severity.Error);
        }
    }
}