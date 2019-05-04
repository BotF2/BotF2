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
    //public interface IPlayerTarget1Service
    //{
    //    ReadOnlyCollection<Order> Target1 { get; }
    //    void AddOrder(Order target1);
    //    bool RemoveOrder(Order target1);
    //    void ClearTarget1();

    //    bool AutoTurnTarget1 { get; set; }

    //    event EventHandler Target1Changed;
    //}

    //public static class PlayerTarget1ServiceExtensions
    //{
    //    public static int RemoveOrders([NotNull] this IPlayerTarget1Service orderService, [NotNull] IEnumerable<Order> target1)
    //    {
    //        if (orderService == null)
    //            throw new ArgumentNullException("target1Service");
    //        if (target1 == null)
    //            throw new ArgumentNullException("target1");

    //        return target1.Count(orderService.RemoveOrder);
    //    }

    //    public static int RemoveOrders<TOrder>([NotNull] this IPlayerTarget1Service orderService, [NotNull] Func<TOrder, bool> predicate)
    //        where TOrder : Order
    //    {
    //        if (orderService == null)
    //            throw new ArgumentNullException("target1Service");
    //        if (predicate == null)
    //            throw new ArgumentNullException("predicate");

    //        var removedCount = 0;
    //        var target1 = orderService.Target1;

    //        for (var i = 0; i < target1.Count; i++)
    //        {
    //            var target = target1[i] as TOrder;
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