using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting
{
    public interface IScriptService
    {
        ScriptLanguageContext Context { get; }
    }
}