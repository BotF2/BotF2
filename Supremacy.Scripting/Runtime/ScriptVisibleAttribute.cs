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
    public class ScriptVisibleAttribute : Attribute { }

    [AttributeUsage(
        AttributeTargets.Assembly,
        AllowMultiple = true,
        Inherited = true)]
    public class ScriptVisibleNamespaceAttribute : Attribute
    {
        public ScriptVisibleNamespaceAttribute(string clrNamespace)
        {
            ClrNamespace = clrNamespace ?? throw new ArgumentNullException("clrNamespace");
        }

        public ScriptVisibleNamespaceAttribute(string clrNamespace, string assemblyName)
            : this(clrNamespace)
        {
            if (assemblyName == null)
            {
                throw new ArgumentNullException("assemblyName");
            }

            AssemblyName = new AssemblyName(assemblyName);
        }

        public string ClrNamespace { get; }

        public AssemblyName AssemblyName { get; }
    }

    public class ScriptVisibleNamespace
    {
        public ScriptVisibleNamespace(string clrNamespace, AssemblyName assemblyName)
        {
            ClrNamespace = clrNamespace ?? throw new ArgumentNullException("clrNamespace");

            if (assemblyName == null)
            {
                return;
            }

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