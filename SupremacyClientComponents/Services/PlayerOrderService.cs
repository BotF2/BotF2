using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Supremacy.Annotations;
using Supremacy.Game;
using Supremacy.Client.Context;

namespace Supremacy.Client.Services
{
    public sealed class PlayerOrderService : IPlayerOrderService
    {
        public static PlayerOrderService Instance = null;
        private readonly IAppContext _appContext;
        private readonly List<Order> _orders;

        public bool AutoTurn { get; set; }

        public PlayerOrderService([NotNull] IAppContext appContext)
        {
            if (appContext == null)
                throw new ArgumentNullException("appContext");
            _appContext = appContext;
            _orders = new List<Order>();
            Instance = this;
        }

        #region Implementation of IPlayerOrderService
        public ReadOnlyCollection<Order> Orders
        {
            get { return _orders.AsReadOnly(); }
        }

        public void AddOrder(Order order)
        {
            if (order == null)
                return;

            var localPlayer = _appContext.LocalPlayer;
            if (localPlayer == null)
                return;

            var owner = localPlayer.Empire;
            if (owner == null)
                return;

            order.Owner = owner;

            lock (_orders)
            {
                while ((_orders.Count > 0) && order.Overrides(_orders[_orders.Count - 1]))
                    _orders.RemoveAt(_orders.Count - 1);
                _orders.Add(order);
            }
            OnOrdersChanged();
        }

        public bool RemoveOrder(Order order)
        {
            if (order == null)
                return false;
            bool result;
            lock (_orders)
            {
                result = _orders.Remove(order);
            }
            OnOrdersChanged();
            return result;
        }

        public void ClearOrders()
        {
            lock (_orders)
            {
                _orders.Clear();
            }
            OnOrdersChanged();
        }
        #endregion

        public event EventHandler OrdersChanged;

        private void OnOrdersChanged()
        {
            if (OrdersChanged != null)
            {
                OrdersChanged.Invoke(this, new EventArgs());
            }
        }
    }
}