using System;

namespace Supremacy.Scripting.Runtime
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module, AllowMultiple = true)]
    public sealed class ScriptNamespaceAliasAttribute : Attribute
    {
        public ScriptNamespaceAliasAttribute(string alias, string @namespace)
        {
            Alias = alias;
            Namespace = @namespace;
        }

        public string Alias { get; }

        public string Namespace { get; }
    }
}