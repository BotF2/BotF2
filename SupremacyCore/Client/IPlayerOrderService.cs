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
            {
                throw new ArgumentNullException(nameof(orderService));
            }

            if (orders == null)
            {
                throw new ArgumentNullException(nameof(orders));
            }

            return orders.Count(orderService.RemoveOrder);
        }

        public static int RemoveOrders<TOrder>([NotNull] this IPlayerOrderService orderService, [NotNull] Func<TOrder, bool> predicate)
            where TOrder : Order
        {
            if (orderService == null)
            {
                throw new ArgumentNullException(nameof(orderService));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            int removedCount = 0;
            ReadOnlyCollection<Order> orders = orderService.Orders;

            for (int i = 0; i < orders.Count; i++)
            {
                if (!(orders[i] is TOrder order))
                {
                    continue;
                }

                if (!predicate(order))
                {
                    continue;
                }

                if (!orderService.RemoveOrder(order))
                {
                    continue;
                }

                --i;
                ++removedCount;
            }

            return removedCount;
        }
    }
}