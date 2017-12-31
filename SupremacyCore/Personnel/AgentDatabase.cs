using System;
using System.Diagnostics;
using System.Xaml;

using Supremacy.Collections;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Resources;
using Supremacy.VFS;

namespace Supremacy.Personnel
{
    [Serializable]
    public class AgentDatabase : KeyedCollectionBase<Civilization, AgentProfileCollection>
    {
        public AgentDatabase()
            : base(o => o.Owner) {}

        public static AgentDatabase Load()
        {
            var gameContext = GameContext.Current;
            if (gameContext == null)
                gameContext = GameContext.Create(GameOptionsManager.LoadDefaults(), false);

            // works     GameLog.Client.GameData.DebugFormat("AgentDatabase.cs: AgentDatabase loading...");

            GameContext.PushThreadContext(gameContext);

            try
            {
                IVirtualFileInfo fileInfo;

                Debug.Assert(PersonnelConstants.Instance != null);

                if (!ResourceManager.VfsService.TryGetFileInfo(new Uri("vfs:///Resources/Data/Agents.xaml"), out fileInfo))
                    return null;

                if (!fileInfo.Exists)
                    return null;

                using (var stream = fileInfo.OpenRead())
                {
                    return (AgentDatabase)XamlServices.Load(stream);
                }
            }
            finally
            {
                GameContext.PopThreadContext();
            }
        }
    }
}