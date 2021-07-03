using System;
using System.Reflection;

using Microsoft.Scripting;

using Supremacy.Annotations;

using System.Linq;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    /// <summary>
    ///   This class denotes an expression which evaluates to a member
    ///   of a struct or a class.
    /// </summary>
    public abstract class MemberExpression : Expression
    {
        /// <summary>
        ///   The name of this member.
        /// </summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        ///   Whether this is an instance member.
        /// </summary>
        public abstract bool IsInstance
        {
            get;
        }

        /// <summary>
        ///   Whether this is a static member.
        /// </summary>
        public abstract bool IsStatic
        {
            get;
        }

        /// <summary>
        ///   The type which declares this member.
        /// </summary>
        public abstract Type DeclaringType
        {
            get;
        }

        public override bool IsPrimaryExpression => true;

        /// <summary>
        ///   The instance expression associated with this member, if it's a
        ///   non-static member.
        /// </summary>
        public Expression InstanceExpression;

        // TODO: possible optimalization
        // Cache resolved constant result in FieldBuilder <-> expression map
        public virtual MemberExpression ResolveMemberAccess(
            ParseContext ec,
            Expression left,
            SourceSpan loc,
            NameExpression original)
        {
            //
            // Precondition:
            //   original == null || original.Resolve (...) ==> left
            //

            if (left is TypeExpression)
            {
                left = left.ResolveAsBaseTerminal(ec, false);
                if (left == null)
                {
                    return null;
                }

                // TODO: Same problem as in class.cs, TypeTerminal does not
                // always do all necessary checks
                ObsoleteAttribute obsoleteAttribute = left.Type.GetCustomAttributes(typeof(ObsoleteAttribute), true)
                    .Cast<ObsoleteAttribute>()
                    .FirstOrDefault();
                if (obsoleteAttribute != null)
                {
                    ErrorInfo error = obsoleteAttribute.IsError
                        ? CompilerErrors.MemberIsObsolete
                        : string.IsNullOrEmpty(obsoleteAttribute.Message)
                        ? CompilerErrors.MemberIsObsoleteWarning
                        : CompilerErrors.MemberIsObsoleteWithMessageWarning;
                    ec.ReportError(
                        error,
                        Span,
                        Name,
                        obsoleteAttribute.Message);
                }

                if (left is GenericTypeExpression ct && !ct.CheckConstraints(ec))
                {
                    return null;
                }

                if (!IsStatic)
                {
                    ec.ReportError(
                        120,
                        string.Format(
                            "An object reference is required to access non-static member '{0}'.",
                            Name),
                        Severity.Error,
                        Span);

                    return null;
                }

                return this;
            }

            if (!IsInstance)
            {
                return original != null && original.IdenticalNameAndTypeName(ec, left, loc) ? this : ResolveExtensionMemberAccess(ec, left);
            }

            InstanceExpression = left;
            return this;
        }

        protected virtual MemberExpression ResolveExtensionMemberAccess(ParseContext ec, Expression left)
        {
            ec.ReportError(
                CompilerErrors.StaticMemberCannotBeAccessedWithInstanceReference,
                Span,
                GetSignatureForError());

            return this;
        }

        public virtual void SetTypeArguments(ParseContext ec, TypeArguments ta)
        {
            ec.ReportError(
                307,
                string.Format(
                    "The property '{0}' cannot be used with type arguments",
                    GetSignatureForError()),
                Severity.Error,
                Span);
        }
    }

    public class ConstantMemberExpression : MemberExpression
    {
        private FieldInfo _field;

        public ConstantMemberExpression([NotNull] FieldInfo field, SourceSpan span = default)
        {
            _field = field ?? throw new ArgumentNullException("field");

            Span = span;
        }

        public override string Name => _field.Name;

        public override bool IsInstance => !_field.IsStatic;

        public override bool IsStatic => _field.IsStatic;

        public override Type DeclaringType => _field.DeclaringType;

        public override MemberExpression ResolveMemberAccess(ParseContext ec, Expression left, SourceSpan loc, NameExpression original)
        {
            _field = TypeManager.GetGenericFieldDefinition(_field);

            IConstant ic = TypeManager.GetConstant(_field);
            if (ic == null)
            {
                if (_field.IsLiteral)
                {
                    ic = new ExternalConstant(_field);
                }
                else
                {
                    ic = ExternalConstant.CreateDecimal(_field);
                    // HACK: decimal field was not resolved as constant
                    if (ic == null)
                    {
                        return new FieldExpression(_field, loc).ResolveMemberAccess(ec, left, loc, original);
                    }
                }

                TypeManager.RegisterConstant(_field, ic);
            }

            return base.ResolveMemberAccess(ec, left, loc, original);
        }

        public override Expression DoResolve(ParseContext ec)
        {
            IConstant ic = TypeManager.GetConstant(_field);
            if (ic.ResolveValue())
            {
                ic.CheckObsoleteness(ec, Span);
            }

            return ic.CreateConstantReference(Span);
        }
    }
}