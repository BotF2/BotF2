// PlayerOrdersMessage.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Supremacy.Game;
using Supremacy.IO;
using Supremacy.Utility;

namespace Supremacy.WCF
{
    [DataContract]
    public class PlayerOrdersMessage
    {
        [DataMember]
        private string _buffer;

        public IList<Order> Orders
        {
            get
            {
                IList<Order> orders = StreamUtility.Read<IList<Order>>(Convert.FromBase64String(_buffer));
                foreach (Order item in orders)
                {
                    GameLog.Core.SaveLoadDetails.DebugFormat("Order: owner = {0}, IsExecuted = {1}", item.Owner, item.IsExecuted);
                }
/*                GameLog.Core.SaveLoad.DebugFormat("{0}", gamelogText)*/;
                return StreamUtility.Read<IList<Order>>(Convert.FromBase64String(_buffer));
            }
        }

        [DataMember]
        public bool AutoTurn { get; set; }

        public PlayerOrdersMessage(IList<Order> orders, bool autoTurn)
        {
            if (orders == null)
            {
                throw new ArgumentNullException("data");
            }

            _buffer = Convert.ToBase64String(StreamUtility.Write(orders));
            AutoTurn = autoTurn;
        }
    }
}
