using Supremacy.Entities;
using Supremacy.Utility;

namespace Supremacy.Resources
{
    public class MasterResources
    {
        private static CivDatabase m_CivDatabase;

        public static CivDatabase CivDB
        {
            get
            {
                if (m_CivDatabase == null)
                {
                    GameLog.Core.GameData.Debug("Loading master copy of civilization database...");
                    m_CivDatabase = CivDatabase.Load();
                    GameLog.Core.GameData.Debug("Master civilization database loaded");
                }
                return m_CivDatabase;
            }
            private set { }
        }
    }
}
