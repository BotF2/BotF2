using Microsoft.Scripting;

namespace Supremacy.Scripting.Ast
{
    public static class CompilerErrors
    {
        public static readonly ErrorInfo StaticMemberCannotBeAccessedWithInstanceReference = new ErrorInfo(
            176,
            "Static member '{0}' cannot be accessed with an instance reference, qualify it with a type name instead.");

        public static readonly ErrorInfo InterpolatedExpressionCannotContainBraces = new ErrorInfo(
            1100,
            "Expression cannot contain '{'.",
            Severity.FatalError);

        public static readonly ErrorInfo StaticClassesCannotBeUsedAsGenericArguments = new ErrorInfo(
            718,
            "'{0}': static classes cannot be used as generic arguments.");

        public static readonly ErrorInfo TypeMayNotBeUsedAsGenericArgument = new ErrorInfo(
            306,
            "The type '{0}' may not be used as a type argument.");

        public static readonly ErrorInfo UnexpectedExpressionType = new ErrorInfo(
            118,
            "'{0}' is a '{1}', but a '{2}' was expected.");

        public static readonly ErrorInfo UnexpectedExpressionKind = new ErrorInfo(
            119,
            "Expression denotes a '{0}' where a `{1}' was expected.");

        public static readonly ErrorInfo MemberIsInaccessible = new ErrorInfo(
            122,
            "'{0}' is inaccessible due to its protection level.");

        public static readonly ErrorInfo FriendAssemblyNameNotMatching = new ErrorInfo(
            281,
            "Friend access was granted to '{0}', but the output assembly is named '{1}'.  " +
            "Try adding a reference to '{0}' or change the output assembly name to match it.");

        public static readonly ErrorInfo MemberIsObsolete = new ErrorInfo(
            619,
            "'{0}' is obsolete.",
            Severity.Warning);

        public static readonly ErrorInfo MemberIsObsoleteWarning = new ErrorInfo(
            612,
            "'{0}' is obsolete.",
            Severity.Warning);

        public static readonly ErrorInfo MemberIsObsoleteWithMessageWarning = new ErrorInfo(
            618,
            "'{0}' is obsolete: '{1}'.",
            Severity.Warning);

        public static readonly ErrorInfo InvalidNumberOfTypeArguments = new ErrorInfo(
            305,
            "Using the generic type '{0}' requires '{1}' type argument(s).");

        public static readonly ErrorInfo TypeOrNamespaceNotFound = new ErrorInfo(
            246,
            "The type or namespace name '{0}' could not be found.  Are you missing a using directive or an assembly reference?");

        public static readonly ErrorInfo LiteralValueMustBeBuiltinType = new ErrorInfo(
            1020,
            "Found a literal value but could not resolve a built-in type; encountered type '{0}'.");

        public static readonly ErrorInfo InvalidLiteralValue = new ErrorInfo(
            1021,
            "Invalid literal value '{0}'; expected literal of type '{1}'.");

        public static readonly ErrorInfo MissingIndexerValue = new ErrorInfo(
            1022, // TODO: Get a real error code and message.
            "Matching indexer does not exist on type '{0}'.");

        public static readonly ErrorInfo ConditionalOperandsBothHaveImplicitConversions = new ErrorInfo(
            172,
            "Type of conditional expression cannot be determined as '{0}' and '{1}' convert implicitly to each other.");

        public static readonly ErrorInfo ConditionalOperandsHaveNoImplicitConversion = new ErrorInfo(
            173,
            "Type of conditional expression cannot be determined because there is no implicit conversion between '{0}' and '{1}.'");

        public static readonly ErrorInfo UnreachableExpression = new ErrorInfo(
            429,
            "Unreachable expression code detected.");

        public static readonly ErrorInfo UnrecognizedEscapeSequence = new ErrorInfo(
            1009,
            "Unrecognized escape sequence.");
    }
}