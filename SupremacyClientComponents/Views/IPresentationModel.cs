// IPresentationModel.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

namespace Supremacy.Client.Views
{
    /// <summary>
    /// Base interface for all presentation models in a Presenter-View-Presentation Model
    /// (P-V-PM) architecture.
    /// </summary>
    public interface IPresentationModel
    {
        /// <summary>
        /// Notifies the presentation model (PM) that it has been loaded into a view.
        /// This provides an opportunity for the PM to hook into requisite services and
        /// register command and event handlers.
        /// </summary>
        void NotifyLoaded();

        /// <summary>
        /// Notifies the presentation model (PM) that it has been unloaded from a view.
        /// This provides an opportunity for the PM to unhook from requisite services and
        /// unregister command and event handlers.
        /// </summary>
        void NotifyUnloaded();
    }
}