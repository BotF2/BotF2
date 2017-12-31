namespace Supremacy.Scripting.Ast
{
    public enum ExpressionClass : byte
    {
        Invalid,
        Value,
        Variable,
        Namespace,
        Type,
        TypeParameter,
        MethodGroup,
        PropertyAccess,
        EventAccess,
        IndexerAccess,
        Nothing,
    }
}