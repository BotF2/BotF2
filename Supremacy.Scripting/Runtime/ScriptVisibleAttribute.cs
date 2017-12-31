using System;
using System.Linq;
using System.Reflection;

namespace Supremacy.Scripting.Runtime
{
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Delegate |
        AttributeTargets.Interface |
        AttributeTargets.Struct |
        AttributeTargets.Enum,
        AllowMultiple = false,
        Inherited = true)]
    public class ScriptVisibleAttribute : Attribute {}

    [AttributeUsage(
        AttributeTargets.Assembly,
        AllowMultiple = true,
        Inherited = true)]
    public class ScriptVisibleNamespaceAttribute : Attribute
    {
        private readonly string _clrNamespace;
        private readonly AssemblyName _assemblyName;

        public ScriptVisibleNamespaceAttribute(string clrNamespace)
        {
            if (clrNamespace == null)
                throw new ArgumentNullException("clrNamespace");
            _clrNamespace = clrNamespace;
        }

        public ScriptVisibleNamespaceAttribute(string clrNamespace, string assemblyName)
            : this(clrNamespace)
        {
            if (assemblyName == null)
                throw new ArgumentNullException("assemblyName");
            _assemblyName = new AssemblyName(assemblyName);
        }

        public string ClrNamespace
        {
            get { return _clrNamespace; }
        }

        public AssemblyName AssemblyName
        {
            get { return _assemblyName; }
        }
    }

    public class ScriptVisibleNamespace
    {
        public ScriptVisibleNamespace(string clrNamespace, AssemblyName assemblyName)
        {
            if (clrNamespace == null)
                throw new ArgumentNullException("clrNamespace");

            ClrNamespace = clrNamespace;

            if (assemblyName == null)
                return;
            try { Assembly = Assembly.Load(assemblyName); }
            catch
            {
                Assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(o => o.GetName().Name == assemblyName.Name);
                 /* TODO: Log when imported into Supremacy solution. */
            }
        }

        public string ClrNamespace { get; private set; }
        public Assembly Assembly { get; private set; }
    }

    public static class ScriptVisibilityExtensions
    {
        public static bool IsScriptVisible(this Type type)
        {
            return type.GetCustomAttributes(typeof(ScriptVisibleAttribute), true).Any();
        }

        public static ScriptVisibleNamespace[] GetScriptVisibleNamespaces(this Assembly assembly)
        {
            return assembly.GetCustomAttributes(typeof(ScriptVisibleNamespaceAttribute), false)
                .OfType<ScriptVisibleNamespaceAttribute>()
                .Select(o => new ScriptVisibleNamespace(o.ClrNamespace, o.AssemblyName))
                .ToArray();
        }
    }
}