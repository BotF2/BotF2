// ChanceTree.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;

namespace Supremacy.Utility
{
    public class Chance<TItem>
    {
        public int GrowthWeight { get; set; }
        public int CurrentWeight { get; set; }
        public Chance<TItem> Left { get; set; }
        public Chance<TItem> Right { get; set; }
        public TItem Item { get; set; }

        public Chance(int growthWeight, int currentWeight, TItem item)
        {
            GrowthWeight = growthWeight;
            CurrentWeight = currentWeight;
            Item = item;
        }
    }

    public class ChanceTree<TItem>
    {
        private readonly List<Chance<TItem>> _chances;
        private Chance<TItem> _root;
        private int _count;

        public ChanceTree()
        {
            _chances = new List<Chance<TItem>>();
        }

        protected IList<Chance<TItem>> Chances
        {
            get { return _chances; }
        }

        public bool IsEmpty
        {
            get { return _count == 0 || _chances.TrueForAll(o => o.CurrentWeight <= 0); }
        }

        public void AddChance(int chance, TItem item)
        {
            ++_count;

            if (_root == null)
            {
                _root = new Chance<TItem>(chance, chance, item);
                _chances.Add(_root);
                return;
            }

            var current = _root;
            var child = new Chance<TItem>(chance, chance, item);

            _chances.Add(child);

            while (true)
            {
                current.GrowthWeight += chance;

                if (current.Left == null)
                {
                    current.Left = child;
                    break;
                }

                if (current.Right == null)
                {
                    current.Right = child;
                    break;
                }

                if (current.Left.GrowthWeight > current.Right.GrowthWeight)
                    current = current.Right;
                else
                    current = current.Left;
            }
        }

        public TItem Get()
        {
            if (_count == 0)
                return default(TItem);
            return _chances[FindNode(RandomProvider.Shared.Next(_chances[0].GrowthWeight))].Item;
        }

        public TItem Take()
        {
            if (_count == 0)
                return default(TItem);

            var probability = RandomProvider.Shared.Next(_chances[0].GrowthWeight);
            var index = FindNode(probability);
            var chance = _chances[index];

            ModifyChances(index, -chance.CurrentWeight);

            return chance.Item;
        }

        protected void ModifyChances(int index, int modifyBy)
        {
            // Get the prior growth
            var priorGrowth = _chances[index].GrowthWeight;

            _chances[index].CurrentWeight += modifyBy;
            _chances[index].GrowthWeight += modifyBy;

            if (_chances[index].CurrentWeight <= 0)
            {
                --_count;

                if (_count != index)
                {
                    // First remove the last node from the list
                    var swapIndex = _count;

                    while (swapIndex > 0)
                    {
                        swapIndex = (swapIndex - 1) >> 1;
                        _chances[swapIndex].GrowthWeight -= _chances[_count].CurrentWeight;
                    }

                    // Swap the last with the current
                    _chances[index] = _chances[_count];

                    // Get the new growth weight
                    var child = (index << 1) + 1;
                    _chances[index].GrowthWeight = _chances[index].CurrentWeight;

                    if (child < _count)
                        _chances[index].GrowthWeight += _chances[child++].GrowthWeight;
                    if (child < _count)
                        _chances[index].GrowthWeight += _chances[child].GrowthWeight;
                }
            }

            // Feed back up the tree
            var feedWeight = _chances[index].GrowthWeight - priorGrowth;
            if (feedWeight == 0)
                return;

            // Parent Weight
            var pIndex = index;
            while (pIndex > 0)
            {
                pIndex = (pIndex - 1) >> 1;
                _chances[pIndex].GrowthWeight += feedWeight;
            }
        }

        protected int FindNode(int chance)
        {
            var index = 0;
            var prior = 0;

            while (true)
            {
                prior += _chances[index].CurrentWeight;
                
                if (chance < prior)
                    return index;

                var nextIndex = (index << 1) + 1;
                if (nextIndex < _count && chance < (prior + _chances[nextIndex].GrowthWeight))
                {
                    index = nextIndex;
                    continue;
                }

                prior += _chances[index].GrowthWeight;
                index++;
            }
        }
    }
}