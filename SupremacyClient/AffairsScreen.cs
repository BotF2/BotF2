// AffairsScreen.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.ComponentModel;
using System.Windows;

using Microsoft.Practices.Unity;

using Supremacy.Annotations;
using Supremacy.Client.Views;
using Supremacy.Economy;
using Supremacy.Game;
using Supremacy.Utility;

namespace Supremacy.Client
{
    public sealed class AffairsScreen : GameScreen<PersonnelScreenPresentationModel>, IPersonnelScreenView, IWeakEventListener
    {
        private PersonnelPool _personnelPool;

        public AffairsScreen([NotNull] IUnityContainer container) : base(container) {}

        public override void RefreshScreen()
        {
            base.RefreshScreen();

            if (_personnelPool != null)
            {
                foreach (PersonnelCategory pc in EnumUtilities.GetValues<PersonnelCategory>())
                    PropertyChangedEventManager.RemoveListener(_personnelPool.Distribution[pc], this, string.Empty);
            }

            _personnelPool = AppContext.LocalPlayerEmpire.Personnel;

            if (_personnelPool != null)
            {
                foreach (PersonnelCategory pc in EnumUtilities.GetValues<PersonnelCategory>())
                    PropertyChangedEventManager.AddListener(_personnelPool.Distribution[pc], this, string.Empty);
            }
        }

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (_personnelPool != null)
            {
                PlayerOrderService.AddOrder(
                    new SetPersonnelDistributionOrder(AppContext.LocalPlayer.Empire, _personnelPool.Distribution));
            }
            return true;
        }
    }
}
