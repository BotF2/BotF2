using System;

using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;

using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting
{
    public class ScriptService : IScriptService
    {
        private readonly ScriptLanguageContext _context;

        private static ScriptService _instance;

        public static ScriptService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ScriptService();
                return _instance;
            }
        }

        public ScriptService()
        {
            var languageName = typeof(ScriptLanguageContext).AssemblyQualifiedName;
            var scriptRuntimeSetup = new ScriptRuntimeSetup { LanguageSetups = { new LanguageSetup(languageName, "Q#") } };
            var scriptEngine = ScriptRuntime.CreateRemote(AppDomain.CurrentDomain, scriptRuntimeSetup).GetEngineByTypeName(languageName);

            _context = (ScriptLanguageContext)HostingHelpers.GetLanguageContext(scriptEngine);
            _context.DomainManager.LoadAssembly(typeof(Game.GameContext).Assembly);
        }

        #region Implementation of IScriptService
        public ScriptLanguageContext Context => _context;
        #endregion
    }
}