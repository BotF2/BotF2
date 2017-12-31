using System;
using System.ComponentModel;

namespace Supremacy.Xna
{
    [Serializable]
    [ImmutableObject(true)]
    public struct XnaGraphicsOptions
    {
        private readonly bool _enableDepthStencil;
        private readonly bool _preferAnisotropicFiltering;
        private readonly bool _preferMultiSampling;

        public bool EnableDepthStencil
        {
            get { return _enableDepthStencil; }
        }

        public bool PreferAnisotropicFiltering
        {
            get { return _preferAnisotropicFiltering; }
        }

        public bool PreferMultiSampling
        {
            get { return _preferMultiSampling; }
        }

        public XnaGraphicsOptions(bool enableDepthStencil, bool preferAnisotropicFiltering, bool preferMultiSampling)
        {
            _enableDepthStencil = enableDepthStencil;
            _preferAnisotropicFiltering = preferAnisotropicFiltering;
            _preferMultiSampling = preferMultiSampling;
        }
    }
}