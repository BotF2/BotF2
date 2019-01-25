using System;

namespace Supremacy.Client
{
    public interface IClientApplication
    {
        Version Version { get; }
        bool IsShuttingDown { get; }
        IClientCommandLineArguments CommandLineArguments { get; }

        void DoEvents();
        bool LoadDefaultResources();
        bool LoadThemeResources(string theme);
        bool LoadThemeResourcesShipyard(string themeShipyard);
        bool LoadThemeResourcesNewShipSelectionView(string themeShipyard); // a new bool or could we use the one above?
    }
}