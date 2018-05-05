// NetUtility.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Net;
namespace Supremacy.Network
{
    public static class NetUtility
    {
        private static readonly TimeSpan CacheTimeout;
        private static Dictionary<string, Tuple<IPHostEntry, DateTime>> cache;

        static NetUtility()
        {
            cache = new Dictionary<string, Tuple<IPHostEntry, DateTime>>();
            CacheTimeout = new TimeSpan(0, 5, 0);
        }

        public static IPHostEntry Resolve(string host)
        {
            if ((cache.ContainsKey(host))
                && (cache[host].Item1 != null)
                && ((DateTime.Now - cache[host].Item2) <= CacheTimeout))
            {
                return cache[host].Item1;
            }
            else
            {
                IPHostEntry hostEntry = null;
                
                try
                {
                    hostEntry = Dns.GetHostEntry(host);
                }
                catch (Exception e) //ToDo: Just log or additional handling necessary?
                {
                    GameLog.LogException(e);
                }

                lock (cache)
                {
                    cache[host] = new Tuple<IPHostEntry, DateTime>(
                        hostEntry, DateTime.Now);
                    if (hostEntry != null)
                        cache[hostEntry.HostName] = cache[host];
                }

                if ((hostEntry != null) && (hostEntry.AddressList.Length > 0))
                {
                    return hostEntry;
                }
                else
                {
                    throw new NetException("Could not resolve host '"
                        + host + "'");
                }
            }
        }
    }
}
