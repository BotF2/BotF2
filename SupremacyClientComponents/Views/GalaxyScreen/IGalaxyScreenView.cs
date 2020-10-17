// IGalaxyScreenView.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Client.Context;

namespace Supremacy.Client.Views
{
    public interface IGameScreenView<TPresentationModel> : IView
    {
        #region Properties and Indexers
        IAppContext AppContext { get; set; }
        TPresentationModel Model { get; set; }
        #endregion

        #region Public and Protected Methods
        void OnCreated();
        void OnDestroyed();
        #endregion
    }

    public interface IGalaxyScreenView : IGameScreenView<GalaxyScreenPresentationModel> { }

    //public interface ISinglePlayerStartScreen : ISinglePlayerStartScreen<EncyclopediaScreenPresentationModel> { }

    public interface IColonyScreenView : IGameScreenView<ColonyScreenPresentationModel> { }

    public interface IAssetsScreenView : IGameScreenView<AssetsScreenPresentationModel> { }

    public interface IDiplomacyScreenView : IGameScreenView<DiplomacyScreenPresentationModel> { }

    public interface INewDiplomacyScreenView : IGameScreenView<DiplomacyScreenViewModel> { }

    public interface IScienceScreenView : IGameScreenView<ScienceScreenPresentationModel> { }

    public interface IEncyclopediaScreenView : IGameScreenView<EncyclopediaScreenPresentationModel> { }

    //public interface IIntelScreenView : IGameScreenView<IntelScreenPresentationModel> { }

    public interface ISystemAssaultScreenView : IGameScreenView<SystemAssaultScreenViewModel>
    {
        bool IsActive { get; set; }
    }
}