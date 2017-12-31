using System;

namespace Supremacy.Scripting.Runtime
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module, AllowMultiple = true)]
    public sealed class ScriptNamespaceAliasAttribute : Attribute
    {
        private readonly string _alias;
        private readonly string _namespace;

        public ScriptNamespaceAliasAttribute(string alias, string @namespace)
        {
            _alias = alias;
            _namespace = @namespace;
        }

        public string Alias
        {
            get { return _alias; }
        }

        public string Namespace
        {
            get { return _namespace; }
        }
    }
}