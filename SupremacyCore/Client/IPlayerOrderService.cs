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
    public interface IPlayerOrderService
    {
        ReadOnlyCollection<Order> Orders { get; }
        void AddOrder(Order order);
        bool RemoveOrder(Order order);
        void ClearOrders();

        bool AutoTurn { get; set; }

        event EventHandler OrdersChanged;
    }

    public static class PlayerOrderServiceExtensions
    {
        public static int RemoveOrders([NotNull] this IPlayerOrderService orderService, [NotNull] IEnumerable<Order> orders)
        {
            if (orderService == null)
                throw new ArgumentNullException("orderService");
            if (orders == null)
                throw new ArgumentNullException("orders");

            return orders.Count(orderService.RemoveOrder);
        }

        public static int RemoveOrders<TOrder>([NotNull] this IPlayerOrderService orderService, [NotNull] Func<TOrder, bool> predicate)
            where TOrder : Order
        {
            if (orderService == null)
                throw new ArgumentNullException("orderService");
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            var removedCount = 0;
            var orders = orderService.Orders;

            for (var i = 0; i < orders.Count; i++)
            {
                var order = orders[i] as TOrder;
                if (order == null)
                    continue;
                if (!predicate(order))
                    continue;
                if (!orderService.RemoveOrder(order))
                    continue;
                --i;
                ++removedCount;
            }

            return removedCount;
        }
    }
}