using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

using MSAst = System.Linq.Expressions.Expression;

using System.Linq;

namespace Supremacy.Scripting.Ast
{
    public class ArrayCreationExpression : Expression
    {
        private readonly List<Expression> _dimensions;
        private readonly List<Expression> _arrayData;

        private FullNamedExpression _baseType;
        private ArrayInitializerExpression _initializer;
        private int _resolvedDimensions;
        private Type _resolvedElementType;

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            var clone = target as ArrayCreationExpression;
            if (clone == null)
                return;

            clone._baseType = Clone(cloneContext, _baseType);
            clone._initializer = Clone(cloneContext, _initializer);
            clone._resolvedDimensions = _resolvedDimensions;
            clone._resolvedElementType = _resolvedElementType;
            
            clone.RankSpecifier = RankSpecifier;

            _dimensions.CloneTo(cloneContext, clone._dimensions);
            _arrayData.CloneTo(cloneContext, clone._arrayData);
        }

        protected List<Expression> ArrayData
        {
            get { return _arrayData; }
        }

        public Type ResolvedElementType
        {
            get { return _resolvedElementType; }
            protected set { _resolvedElementType = value; }
        }

        public int ResolvedDimensions
        {
            get { return _resolvedDimensions; }
            protected set { _resolvedDimensions = value; }
        }

        public ArrayCreationExpression()
        {
            _dimensions = new List<Expression>();
            _arrayData = new List<Expression>();
        }

        public FullNamedExpression BaseType
        {
            get { return _baseType; }
            set
            {
                var composedCast = value as ComposedCastExpression;
                if (composedCast != null)
                {
                    value = composedCast.Left;
                    RankSpecifier = composedCast.DimensionSpecifier + RankSpecifier;
                }
                _baseType = value;
            }
        }

        public virtual string RankSpecifier { get; set; }

        public ArrayInitializerExpression Initializer
        {
            get { return _initializer; }
            set { _initializer = value; }
        }

        public List<Expression> Dimensions
        {
            get { return _dimensions; }
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _baseType, prefix, postfix);
            WalkList(_dimensions, prefix, postfix);
            Walk(ref _initializer, prefix, postfix);
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write("new");

            if (BaseType != null)
            {
                sw.Write(" ");
                DumpChild(BaseType, sw);
            }

            sw.Write("[");

            var i = 0;
            foreach (var dimension in _dimensions)
            {
                if (i++ != 0)
                    sw.Write(", ");
                DumpChild(dimension, sw);
            }

            sw.Write("]");

            if (RankSpecifier != null)
                sw.Write(RankSpecifier.Substring(2));

            if (_initializer == null)
                return;

            sw.Write(" ");
            DumpChild(_initializer, sw);
        }

        public override MSAst TransformCore(ScriptGenerator generator)
        {
            if (_initializer != null)
            {
                return MSAst.NewArrayInit(
                    _resolvedElementType,
                    _arrayData.Select(o => o.Transform(generator)));
            }

            return MSAst.NewArrayBounds(
                _resolvedElementType,
                _dimensions.Select(o => o.Transform(generator)));
        }

        private bool ResolveArrayType(ParseContext ec)
        {
            if (_baseType == null)
            {
                ec.ReportError(
                    622, 
                    "Can only use array initializer expressions to assign to array types.  Try using a new expression instead.",
                    Span);

                return false;
            }

            var arrayQualifier = new StringBuilder();

            //
            // `In the first form allocates an array instace of the type that results
            // from deleting each of the individual expression from the expression list'
            //
            if (RankSpecifier == null ||  _dimensions.Count > 0)
            {
                arrayQualifier.Append("[");
                for (var i = _dimensions.Count - 1; i > 0; i--)
                    arrayQualifier.Append(",");
                arrayQualifier.Append("]");
            }

            if (RankSpecifier != null)
                arrayQualifier.Append(RankSpecifier);

            _baseType = _baseType.Resolve(ec) as FullNamedExpression;
            
            if (_baseType == null)
                return false;

            //
            // Lookup the type
            //
            TypeExpression arrayTypeExpr = new ComposedCastExpression(_baseType, arrayQualifier.ToString(), Span);
            arrayTypeExpr = arrayTypeExpr.ResolveAsTypeTerminal(ec, false);
            if (arrayTypeExpr == null)
                return false;

            Type = arrayTypeExpr.Type;
            
            _resolvedElementType = Type.GetElementType();
            _resolvedDimensions = Type.GetArrayRank();

            return true;
        }

        public override Expression DoResolve(ParseContext ec)
        {
            if (Type != null)
                return this;

            if (!ResolveArrayType(ec))
                return null;

            //
            // First step is to validate the initializers and fill
            // in any missing bits
            //
            if (!ResolveInitializers(ec))
                return null;

            for (var i = 0; i < _dimensions.Count; ++i)
            {
                var e = _dimensions[i].Resolve(ec);
                if (e == null)
                    continue;

                _dimensions[i] = e;
            }

            ExpressionClass = ExpressionClass.Value;
            return this;
        }

        protected bool ResolveInitializers(ParseContext ec)
        {
            if (_initializer == null)
                return _dimensions.Any();

            //
            // We use this to store all the date values in the order in which we
            // will need to store them in the byte blob later
            //
            var arrayData = new List<Expression>();
            var bounds = new System.Collections.Specialized.HybridDictionary();

            if (_dimensions != null && _dimensions.Count != 0)
                return CheckIndices(ec, _initializer.Values, 0, true, _resolvedDimensions);

            if (!CheckIndices(ec, _initializer.Values, 0, false, _resolvedDimensions))
                return false;

            //UpdateIndices();

            return true;
        }

        protected virtual Expression ResolveArrayElement(ParseContext ec, Expression element)
        {
            element = element.Resolve(ec);

            if (element == null)
                return null;

            return ConvertExpression.MakeImplicitConversion(
                ec,
                element,
                _resolvedElementType,
                Span);
        }

        private bool CheckIndices(ParseContext ec, IList<Expression> probe, int idx, bool explicitDimensions, int childBounds)
        {
            if (explicitDimensions)
            {
                var a = _dimensions[idx];
                a = a.Resolve(ec);
                if (a == null)
                    return false;

                var c = a as ConstantExpression;
                if (c != null)
                {
                    c = c.ConvertImplicitly(TypeManager.CoreTypes.Int32);
                }

                if (c == null)
                {
                    ec.ReportError(
                        150,
                        "A constant value is expected.",
                        a.Span);
                    return false;
                }

                var value = (int)c.Value;
                if (value != probe.Count)
                {
                    ec.ReportError(
                        847,
                        string.Format("An array initializer of length '{0}' was expected.", value),
                        Span);

                    return false;
                }

                //bounds[idx] = value;
            }

            for (var i = 0; i < probe.Count; ++i)
            {
                var o = probe[i];
                var subProbe = o as ArrayInitializerExpression;
                if (subProbe != null)
                {
                    if (idx + 1 >= _resolvedDimensions)
                    {
                        ec.ReportError(
                            623, 
                            "Array initializers can only be used in a variable or field initializer.  Try using a new expression instead.",
                            subProbe.Span);

                        return false;
                    }

                    var subProbeCheck = CheckIndices(ec, subProbe.Values, idx + 1, explicitDimensions, childBounds - 1);
                    if (!subProbeCheck)
                        return false;

                    probe[i] = new ArrayCreationExpression
                               {
                                   _baseType = _baseType,
                                   _initializer = subProbe,
                                   RankSpecifier = RankSpecifier.Substring(RankSpecifier.IndexOf(']') + 1),
                                   Span = subProbe.Span
                               }.Resolve(ec);
                }
                else if (childBounds > 1)
                {
                    ec.ReportError(
                        846,
                        "A nested array initializer was expected.",
                        o.Span);
                }
                else
                {
                    var element = ResolveArrayElement(ec, o);
                    if (element == null)
                        continue;

                    // Initializers with the default values can be ignored
                    var c = element as ConstantExpression;
                    if (c != null)
                    {
                        //if (c.IsDefaultInitializer(_resolvedElementType))
                        //{
                        //    element = null;
                        //}
                    }

                    _arrayData.Add(element);
                }
            }

            return true;
        }
    }

    public class ImplicitlyTypedArrayCreationExpression : ArrayCreationExpression
    {
        public override string RankSpecifier
        {
            get { return base.RankSpecifier; }
            set
            {
                if (value != null)
                {
                    if (value.Length > 2)
                    {
                        ResolvedDimensions = 0;
                        while (value[++ResolvedDimensions] == ',')
                            continue;
                    }
                    else
                    {
                        ResolvedDimensions = 1;
                    }
                }
                base.RankSpecifier = value;
            }
        }

        public override Expression DoResolve(ParseContext ec)
        {
            if (Type != null)
                return this;

            if (!ResolveInitializers(ec))
                return null;

            if (ResolvedElementType == null || 
                ResolvedElementType == TypeManager.CoreTypes.Null ||
                ResolvedElementType == TypeManager.CoreTypes.Void ||
                (Dimensions.Any() && Dimensions.Count != ResolvedDimensions))
            {
                OnErrorNoBestType(ec);
                return null;
            }

            //
            // At this point we found common base type for all initializer elements
            // but we have to be sure that all static initializer elements are of
            // same type
            //
            UnifyInitializerElement(ec);

            Type = TypeManager.GetConstructedType(ResolvedElementType, RankSpecifier);
            ExpressionClass = ExpressionClass.Value;

            return this;
        }

        protected void UnifyInitializerElement(ParseContext ec)
        {
            for (var i = 0; i < ArrayData.Count; ++i)
            {
                var e = ArrayData[i];
                if (e == null)
                    continue;
                ArrayData[i] = ConvertExpression.MakeImplicitConversion(
                    ec,
                    e,
                    ResolvedElementType,
                    SourceSpan.None);
            }
        }

        protected override Expression ResolveArrayElement(ParseContext ec, Expression element)
        {
            element = element.Resolve(ec);
            if (element == null)
                return null;

            if (ResolvedElementType == null)
            {
                if ((element.Type != TypeManager.CoreTypes.Null) && !(element is LambdaExpression))
                    ResolvedElementType = element.Type;

                return element;
            }

            if (ConvertExpression.ImplicitConversionExists(ec, element, ResolvedElementType))
            {
                return element;
            }

            if (TypeUtils.IsImplicitlyConvertible(ResolvedElementType, element.Type, true))
            {
                ResolvedElementType = element.Type;
                return element;
            }

            OnErrorNoBestType(ec);
            return null;
        }

        private void OnErrorNoBestType(ParseContext ec)
        {
            ec.ReportError(
                826,
                "The type of an implicitly typed array cannot be inferred from the initializer.  Try specifying array type explicitly.",
                Span);
        }
    }

    public class ArrayInitializerExpression : Expression
    {
        private readonly List<Expression> _values;

        public ArrayInitializerExpression()
        {
            _values = new List<Expression>();
        }

        public IList<Expression> Values
        {
            get { return _values; }
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            WalkList(_values, prefix, postfix);
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            var clone = target as ArrayInitializerExpression;
            if (clone == null)
                return;

            _values.CloneTo(cloneContext, clone._values);
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write("{");

            var i = 0;
            foreach (var value in _values)
            {
                if (i++ != 0)
                    sw.Write(",");
                
                sw.Write(" ");
                
                DumpChild(value, sw);
            }

            if (i != 0)
                sw.Write(" ");

            sw.Write("}");
        }
    }
}