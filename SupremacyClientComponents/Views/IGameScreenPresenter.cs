// IGameScreenPresenter.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Annotations;

namespace Supremacy.Client.Views
{
    public interface IPresenter : IInteractionNode
    {
        /// <summary>
        /// Directs this presenter to initialize itself.
        /// </summary>
        /// <remarks>
        /// Implementations of this method should perform any operations that should be completed when
        /// the screen is first created.  This includes hooking up command handlers, event subscription, etc.
        /// </remarks>
        void Run();

        /// <summary>
        /// Directs this presenter to perform its final cleanup.
        /// </summary>
        /// <remarks>
        /// Implementations of this method should perform any operations that should be completed when
        /// the screen is destroyed.  This includes unhooking any command handlers, event subscription, etc.
        /// </remarks>
        void Terminate();
    }

    public interface IGameScreenPresenter<out TPresentationModel, out TView> : IPresenter 
        where TView : IGameScreenView<TPresentationModel>
    {
        /// <summary>
        /// Gets the view.
        /// </summary>
        /// <value>The view.</value>
        [NotNull]
        TView View { get; }

        /// <summary>
        /// Gets the presentation model.
        /// </summary>
        /// <value>The presentation model.</value>
        [NotNull]
        TPresentationModel Model { get; }
    }

    public class DiplomacyScreenPresentationModel {}
    public class ScienceScreenPresentationModel {}
    public class IntelScreenPresentationModel {}

    public interface IDiplomacyScreenPresenter : IGameScreenPresenter<DiplomacyScreenPresentationModel, IDiplomacyScreenView> {}
    public interface IScienceScreenPresenter : IGameScreenPresenter<ScienceScreenPresentationModel, IScienceScreenView> {}
    //public interface IIntelScreenPresenter : IGameScreenPresenter<IntelScreenPresentationModel, IIntelScreenView> {}
    public interface IAssetsScreenPresenter : IGameScreenPresenter<AssetsScreenPresentationModel, IAssetsScreenView> {}
}