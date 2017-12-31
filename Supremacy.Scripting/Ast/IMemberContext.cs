using System;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public interface IMemberContext
    {
        int CompilerErrorCount { get; }
        CompilerContext Compiler { get; }
        ExtensionMethodGroupExpression LookupExtensionMethod(Type extensionType, string name, SourceSpan location);
        FullNamedExpression LookupNamespaceOrType(string name, SourceSpan location, bool ignoreAmbiguousReferences, int genericArity);
        FullNamedExpression LookupNamespaceAlias(string name);
    }
}