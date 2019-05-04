using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Supremacy.Annotations;
using Supremacy.Game;
using Supremacy.Client.Context;

namespace Supremacy.Client.Services
{
    //public sealed class PlayerTarget2Service : IPlayerTarget2Service
    //{
    //    public static PlayerTarget2Service Instance = null;
    //    private readonly IAppContext _appContext;
    //    private readonly List<Order> _target2;

    //    public bool AutoTurnTarget2 { get; set; }

    //    public PlayerTarget2Service([NotNull] IAppContext appContext)
    //    {
    //        if (appContext == null)
    //            throw new ArgumentNullException("appContext");
    //        _appContext = appContext;
    //        _target2 = new List<Order>();
    //        Instance = this;
    //    }

    //    #region Implementation of IPlayerOrderService
    //    public ReadOnlyCollection<Order> Target2
    //    {
    //        get { return _target2.AsReadOnly(); }
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

    //        lock (_target2)
    //        {
    //            while ((_target2.Count > 0) && order.Overrides(_target2[_target2.Count - 1]))
    //                _target2.RemoveAt(_target2.Count - 1);
    //            _target2.Add(order);
    //        }
    //        OnOrdersChanged();
    //    }

    //    public bool RemoveOrder(Order order)
    //    {
    //        if (order == null)
    //            return false;
    //        bool result;
    //        lock (_target2)
    //        {
    //            result = _target2.Remove(order);
    //        }
    //        OnOrdersChanged();
    //        return result;
    //    }

    //    public void ClearTarget2()
    //    {
    //        lock (_target2)
    //        {
    //            _target2.Clear();
    //        }
    //        OnOrdersChanged();
    //    }
    //    #endregion

    //    public event EventHandler Target2Changed;

    //    private void OnOrdersChanged()
    //    {
    //        if (Target2Changed != null)
    //        {
    //            Target2Changed.Invoke(this, new EventArgs());
    //        }
    //    }
    //}
}