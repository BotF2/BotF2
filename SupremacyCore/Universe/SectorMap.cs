// SectorMap.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

namespace Supremacy.Universe
{
    [Serializable]
    public class SectorMap
    {
        public const int MaxWidth = 256;
        public const int MaxHeight = 256;

        private readonly Sector[,] _sectors;

        /// <summary>
        /// Gets the width of a <see cref="SectorMap"/>.
        /// </summary>
        /// <value>The width.</value>
        public int Width => _sectors.GetLength(0);

        /// <summary>
        /// Gets the height of a <see cref="SectorMap"/>.
        /// </summary>
        /// <value>The height.</value>
        public int Height => _sectors.GetLength(1);

        /// <summary>
        /// Gets the <see cref="Sector"/> in the <see cref="SectorMap"/> at the specified location
        /// </summary>
        /// <param name="location">Map Location</param>
        /// <returns>The Sector if found, null otherwise</returns>
        public Sector this[MapLocation location]
        {
            get { return this[location.X, location.Y]; }
        }

        /// <summary>
        /// Gets the <see cref="Sector"/> in the <see cref="SectorMap"/> at the specified location
        /// </summary>
        /// <param name="x">X-Coordinate</param>
        /// <param name="y">Y-Coordinate</param>
        /// <returns>The Sector if found, null otherwise</returns>
        public Sector this[int x, int y]
        {
            get
            {
                if ((x < 0) || (y < 0) || (x >= Width) || (y >= Height))
                {
                    return null;
                }
                return _sectors[x, y];
            }
        }

        /// <summary>
        /// Gets the galactic quadrant of a <see cref="Sector"/>.
        /// </summary>
        /// <param name="sector">The sector.</param>
        /// <returns></returns>
        public Quadrant GetQuadrant(Sector sector)
        {
            if (sector == null)
                throw new ArgumentNullException("sector");
            return GetQuadrant(sector.Location);
        }

        /// <summary>
        /// Gets the galactic quadrant of a <see cref="MapLocation"/>.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        public Quadrant GetQuadrant(MapLocation location)
        {
            if (location.X < (Width / 2))
            {
                if (location.Y < (Height / 2))
                    return Quadrant.Gamma;
                return Quadrant.Alpha;
            }
            if (location.Y < (Height / 2))
                return Quadrant.Delta;
            return Quadrant.Beta;
        }

        public void Reset()
        {
            int width = _sectors.GetLength(0);
            int height = _sectors.GetLength(1);
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                    _sectors[x, y].Reset();
            }
        }

        /// <summary>
        /// Constructs a new <see cref="SectorMap"/> and initializes all its <see cref="Sector"/>s.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public SectorMap(int width, int height)
        {
            _sectors = new Sector[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _sectors[x, y] = new Sector(new MapLocation(x, y));
                }
            }
        }
    }
}
