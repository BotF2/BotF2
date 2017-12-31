namespace Supremacy.Expressions.Ast
{
    public interface IQueryExpression
    {
        RangeDeclaration VariableName { get; set; }
        IExpression Initializer { get; set; }
        IQueryBody Body { get; set; }
    }
}