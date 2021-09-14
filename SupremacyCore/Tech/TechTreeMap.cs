// TechTreeMap.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using Supremacy.Entities;

namespace Supremacy.Tech
{
    [Serializable]
    public class TechTreeMap
    {
        private readonly Dictionary<int, TechTree> _map;

        private TechTree _default;

        public TechTree Default
        {
            get => _default;
            set => _default = value;
        }

        public TechTreeMap()
        {
            _map = new Dictionary<int, TechTree>();
        }

        public TechTree this[Civilization civ]
        {
            get
            {
                if (civ == null)
                {
                    throw new ArgumentNullException("civ");
                }

                return this[civ.CivID];
            }
            set
            {
                if (civ == null)
                {
                    throw new ArgumentNullException("civ");
                }

                this[civ.CivID] = value;
            }
        }

        public TechTree this[int civId]
        {
            get
            {
                if (!_map.TryGetValue(civId, out TechTree value))
                {
                    value = Default;
                }

                return value;
            }
            set
            {
                _ = _map.Remove(civId);
                if (value != null)
                {
                    _map[civId] = value;
                }
            }
        }
    }
}