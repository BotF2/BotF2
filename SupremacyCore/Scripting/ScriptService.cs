using System;

using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;

using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting
{
    public class ScriptService : IScriptService
    {
        private static ScriptService _instance;

        public static ScriptService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ScriptService();
                }

                return _instance;
            }
        }

        public ScriptService()
        {
            string languageName = typeof(ScriptLanguageContext).AssemblyQualifiedName;
            ScriptRuntimeSetup scriptRuntimeSetup = new ScriptRuntimeSetup { LanguageSetups = { new LanguageSetup(languageName, "Q#") } };
            ScriptEngine scriptEngine = ScriptRuntime.CreateRemote(AppDomain.CurrentDomain, scriptRuntimeSetup).GetEngineByTypeName(languageName);

            Context = (ScriptLanguageContext)HostingHelpers.GetLanguageContext(scriptEngine);
            _ = Context.DomainManager.LoadAssembly(typeof(Game.GameContext).Assembly);
        }

        #region Implementation of IScriptService
        public ScriptLanguageContext Context { get; }
        #endregion
    }
}