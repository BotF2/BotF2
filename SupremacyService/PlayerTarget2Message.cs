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

namespace Supremacy.WCF
{
    //[DataContract]
    //public class PlayerTarget2Message
    //{
    //    [DataMember]
    //    private string _buffer;

    //    public IList<Order> Target2
    //    {
    //        get { return StreamUtility.Read<IList<Order>>(Convert.FromBase64String(_buffer)); }
    //    }

    //    [DataMember]
    //    public bool AutoTurnTarget2 { get; set; }

    //    public PlayerTarget2Message(IList<Order> orders, bool autoTurn)
    //    {
    //        if (orders == null)
    //            throw new ArgumentNullException("data");
    //        _buffer = Convert.ToBase64String(StreamUtility.Write(orders));
    //        AutoTurnTarget2 = autoTurn;
    //    }
    //}
}
