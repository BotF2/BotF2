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
                    GameLog.Print("Loading master copy of civilization database...");
                    m_CivDatabase = CivDatabase.Load();
                    GameLog.Print("Master civilization database loaded");
                }
                return m_CivDatabase;
            }
            private set { }
        }
    }
}
