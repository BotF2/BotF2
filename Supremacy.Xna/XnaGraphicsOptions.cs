using System;
using System.ComponentModel;

namespace Supremacy.Xna
{
    [Serializable]
    [ImmutableObject(true)]
    public struct XnaGraphicsOptions
    {
        public bool EnableDepthStencil { get; }

        public bool PreferAnisotropicFiltering { get; }

        public bool PreferMultiSampling { get; }

        public XnaGraphicsOptions(bool enableDepthStencil, bool preferAnisotropicFiltering, bool preferMultiSampling)
        {
            EnableDepthStencil = enableDepthStencil;
            PreferAnisotropicFiltering = preferAnisotropicFiltering;
            PreferMultiSampling = preferMultiSampling;
        }
    }
}