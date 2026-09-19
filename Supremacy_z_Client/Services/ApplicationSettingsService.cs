// ApplicationSettingsService.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.Configuration;

namespace Supremacy.Client.Services
{
    internal class ApplicationSettingsService : IApplicationSettingsService
    {
        #region Implementation of IApplicationSettingsService
        public ApplicationSettingsBase Settings => Properties.Settings.Default;
        #endregion
    }
}