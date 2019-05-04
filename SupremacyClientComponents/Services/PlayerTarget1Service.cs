using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Supremacy.Annotations;
using Supremacy.Game;
using Supremacy.Client.Context;

namespace Supremacy.Client.Services
{
    //public sealed class PlayerTarget1Service : IPlayerTarget1Service
    //{
    //    public static PlayerTarget1Service Instance = null;
    //    private readonly IAppContext _appContext;
    //    private readonly List<Order> _target1;

    //    public bool AutoTurnTarget1 { get; set; }

    //    public PlayerTarget1Service([NotNull] IAppContext appContext)
    //    {
    //        if (appContext == null)
    //            throw new ArgumentNullException("appContext");
    //        _appContext = appContext;
    //        _target1 = new List<Order>();
    //        Instance = this;
    //    }

    //    #region Implementation of IPlayerOrderService
    //    public ReadOnlyCollection<Order> Target1
    //    {
    //        get { return _target1.AsReadOnly(); }
    //    }

    //    public void AddOrder(Order order)
    //    {
    //        if (order == null)
    //            return;

    //        var localPlayer = _appContext.LocalPlayer;
    //        if (localPlayer == null)
    //            return;

    //        var owner = localPlayer.Empire;
    //        if (owner == null)
    //            return;

    //        order.Owner = owner;

    //        lock (_target1)
    //        {
    //            while ((_target1.Count > 0) && order.Overrides(_target1[_target1.Count - 1]))
    //                _target1.RemoveAt(_target1.Count - 1);
    //            _target1.Add(order);
    //        }
    //        OnOrdersChanged();
    //    }

    //    public bool RemoveOrder(Order order)
    //    {
    //        if (order == null)
    //            return false;
    //        bool result;
    //        lock (_target1)
    //        {
    //            result = _target1.Remove(order);
    //        }
    //        OnOrdersChanged();
    //        return result;
    //    }

    //    public void ClearTarget1()
    //    {
    //        lock (_target1)
    //        {
    //            _target1.Clear();
    //        }
    //        OnOrdersChanged();
    //    }
    //    #endregion

    //    public event EventHandler Target1Changed;

    //    private void OnOrdersChanged()
    //    {
    //        if (Target1Changed != null)
    //        {
    //            Target1Changed.Invoke(this, new EventArgs());
    //        }
    //    }
    //}
}