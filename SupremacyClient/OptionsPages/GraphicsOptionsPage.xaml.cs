﻿using System;

using Supremacy.Annotations;
using Supremacy.Resources;

namespace Supremacy.Client.OptionsPages
{
    /// <summary>
    /// Interaction logic for GraphicsOptionsPage.xaml
    /// </summary>
    public partial class GraphicsOptionsPage : IClientOptionsPage
    {
        private readonly IResourceManager _resourceManager;

        public GraphicsOptionsPage([NotNull] IResourceManager resourceManager)
        {
            _resourceManager = resourceManager ?? throw new ArgumentNullException("resourceManager");
            InitializeComponent();
        }

        public string Header => _resourceManager.GetString("SETTINGS_GRAPHICS_TAB");
    }
}
