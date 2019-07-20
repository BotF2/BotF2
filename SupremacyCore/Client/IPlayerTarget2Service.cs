// IPlayerOrderService.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Supremacy.Annotations;
using Supremacy.Game;

namespace Supremacy.Client
{
    //public interface IPlayerTarget2Service
    //{
    //    ReadOnlyCollection<Order> Target2 { get; }
    //    void AddOrder(Order target);
    //    bool RemoveOrder(Order target);
    //    void ClearTarget2();

    //    bool AutoTurnTarget2 { get; set; }

    //    event EventHandler Target2Changed;
    //}

    //public static class PlayerTarget2ServiceExtensions
    //{
    //    public static int RemoveOrders([NotNull] this IPlayerTarget2Service orderService, [NotNull] IEnumerable<Order> target2)
    //    {
    //        if (orderService == null)
    //            throw new ArgumentNullException("orderService");
    //        if (target2 == null)
    //            throw new ArgumentNullException("target2");

    //        return target2.Count(orderService.RemoveOrder);
    //    }

    //    public static int RemoveOrders<TOrder>([NotNull] this IPlayerTarget2Service orderService, [NotNull] Func<TOrder, bool> predicate)
    //        where TOrder : Order
    //    {
    //        if (orderService == null)
    //            throw new ArgumentNullException("orderService");
    //        if (predicate == null)
    //            throw new ArgumentNullException("predicate");

    //        var removedCount = 0;
    //        var target2 = orderService.Target2;

    //        for (var i = 0; i < target2.Count; i++)
    //        {
    //            var target = target2[i] as TOrder;
    //            if (target == null)
    //                continue;
    //            if (!predicate(target))
    //                continue;
    //            if (!orderService.RemoveOrder(target))
    //                continue;
    //            --i;
    //            ++removedCount;
    //        }

    //        return removedCount;
    //    }
    //}
}