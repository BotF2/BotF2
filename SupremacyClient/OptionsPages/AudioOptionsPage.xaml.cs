﻿using System;
using Supremacy.Annotations;
using Supremacy.Resources;

namespace Supremacy.Client.OptionsPages
{
    /// <summary>
    /// Interaction logic for AudioOptionsPage.xaml
    /// </summary>
    public partial class AudioOptionsPage : IClientOptionsPage
    {
        private readonly IResourceManager _resourceManager;

        public AudioOptionsPage(
            [NotNull] IResourceManager resourceManager)
        {
            _resourceManager = resourceManager ?? throw new ArgumentNullException("resourceManager");

            InitializeComponent();
        }

        public string Header => _resourceManager.GetString("SETTINGS_AUDIO_TAB");
    }
}
