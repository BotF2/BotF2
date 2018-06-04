using Supremacy.Universe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supremacy.Game
{
    [Serializable]
    public class CivilizationMapData
    {
        public const int MaxScanStrength = 31;
        public const int MinScanStrength = -32;

        private int[,] _mapData;

        private int ExploredMask = 16384;
        private int FuelRangeMask = 255;
        private int FuelRangeOffset = 0;
        private int MaxFuelRange = 255;
        private int MaxAdjustedScanStrength = 63;
        private int ScanStrengthAdjustment = 32;
        private int ScannedMask = 32768;
        private int ScanStrengthMask = 16128;
        private int ScanStrengthOffset = 8;

        public CivilizationMapData(int width, int height)
        {
            if (width > MapLocation.MaxValue)
                throw new ArgumentException("width cannot be greater than MapLocation.MaxValue");
            if (width < MapLocation.MinValue)
                throw new ArgumentException("width cannot be less than MapLocation.MinValue");
            if (height > MapLocation.MaxValue)
                throw new ArgumentException("height cannot be greater than MapLocation.MaxValue");
            if (height < MapLocation.MinValue)
                throw new ArgumentException("height cannot be less than MapLocation.MinValue");
            _mapData = new int[width, height];
        }

        public int GetFuelRange(MapLocation location)
        {
            return (_mapData[location.X, location.Y] & FuelRangeMask);
        }

        public int GetScanStrength(MapLocation location)
        {
            return ((_mapData[location.X, location.Y] & ScanStrengthMask) >> ScanStrengthOffset) - ScanStrengthAdjustment;
        }

        public bool IsExplored(MapLocation location)
        {
            return ((_mapData[location.X, location.Y] & ExploredMask) != 0);
        }

        public bool IsScanned(MapLocation location)
        {
            return ((_mapData[location.X, location.Y] & ScannedMask) != 0);
        }

        public void ResetScanStrengthAndFuelRange()
        {
            int maxX = _mapData.GetLength(0);
            int maxY = _mapData.GetLength(1);

            for (int x = 0; (x < maxX); x++)
            {
                for (int y = 0; (y < maxY); y++)
                {
                    _mapData[x, y] = ((_mapData[x, y] & ~ScanStrengthMask & ~FuelRangeMask) |
                                      (MaxFuelRange << FuelRangeOffset) |
                                      (ScanStrengthAdjustment << ScanStrengthOffset));
                }
            }
        }

        public void SetExplored(MapLocation location, bool value)
        {
            if (value)
            {
                SetScanned(location, true);
                _mapData[location.X, location.Y] |= ExploredMask;
            }
            else
            {
                _mapData[location.X, location.Y] &= ~ExploredMask;
            }
        }

        public void SetExplored(MapLocation location, bool value, int radius)
        {
            int startX = Math.Max(0, location.X - radius);
            int startY = Math.Max(0, location.Y - radius);
            int endX = Math.Min(_mapData.GetLength(0) - 1, location.X + radius);
            int endY = Math.Min(_mapData.GetLength(1) - 1, location.Y + radius);

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    SetExplored(new MapLocation(x, y), value);
                }
            }
        }

        public void SetFuelRange(MapLocation location, int value)
        {
            _mapData[location.X, location.Y] = (_mapData[location.X, location.Y] & ~FuelRangeMask) | (Math.Min(value, MaxFuelRange) << FuelRangeOffset);
        }

        public void SetScanned(MapLocation location, bool value)
        {
            if (value)
            {
                _mapData[location.X, location.Y] |= ScannedMask;
            }
            else
            {
                _mapData[location.X, location.Y] &= ~ScannedMask;
            }
        }

        public void SetScanned(MapLocation location, bool value, int radius)
        {
            int startX = Math.Max(0, location.X - radius);
            int startY = Math.Max(0, location.Y - radius);
            int endX = Math.Min(_mapData.GetLength(0) - 1, location.X + radius);
            int endY = Math.Min(_mapData.GetLength(1) - 1, location.Y + radius);

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    SetScanned(new MapLocation(x, y), value);
                }
            }
        }

        public void SetScanStrength(MapLocation location, int value)
        {
            if (value < MinScanStrength)
                value = MinScanStrength;

            if (value > MaxScanStrength)
                value = MaxScanStrength;

            int adjustedValue = Math.Min(value + ScanStrengthAdjustment, MaxAdjustedScanStrength);

            _mapData[location.X, location.Y] = ((_mapData[location.X, location.Y] & ~ScanStrengthMask) |
                                               (adjustedValue << ScanStrengthOffset));

            if (value > 0)
                SetScanned(location, true);
        }

        public void UpgradeFuelRange(MapLocation location, int value)
        {
            if (value < GetFuelRange(location))
            {
                SetFuelRange(location, value);
            }
        }

        public void UpgradeFuelRange(MapLocation location, int value, int radius)
        {
            int startX = Math.Max(0, location.X - radius);
            int startY = Math.Max(0, location.Y - radius);
            int endX = Math.Min(_mapData.GetLength(0) - 1, location.X + radius);
            int endY = Math.Min(_mapData.GetLength(1) - 1, location.Y + radius);

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    UpgradeFuelRange(new MapLocation(x, y), value);
                }
            }
        }

        public void UpgradeScanStrength(MapLocation location, int value)
        {
            if ((value > GetScanStrength(location)))
            {
                SetScanStrength(location, value);
            }
        }

        public void UpgradeScanStrength(MapLocation location, int value, int radius)
        {
            int startX = Math.Max(0, location.X - radius);
            int startY = Math.Max(0, location.Y - radius);
            int endX = Math.Min(_mapData.GetLength(0) - 1, location.X + radius);
            int endY = Math.Min(_mapData.GetLength(1) - 1, location.Y + radius);
            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    UpgradeScanStrength(new MapLocation(x, y), value);
                }
            }
        }

        public void UpgradeScanStrength(MapLocation location, int value, int radius, int falloff, int minValue)
        {
            int startX = Math.Max(0, location.X - radius);
            int startY = Math.Max(0, location.Y - radius);
            int endX = Math.Min(_mapData.GetLength(0) - 1, location.X + radius);
            int endY = Math.Min(_mapData.GetLength(1) - 1, location.Y + radius);

            falloff = Math.Abs(falloff);

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    MapLocation targetLocation = new MapLocation(x, y);
                    UpgradeScanStrength(targetLocation, Math.Max(minValue, (value - (falloff * MapLocation.GetDistance(location, targetLocation)))));
                }
            }
        }

        public void ApplyScanInterference(int[,] interference)
        {
            int width = _mapData.GetLength(0);
            int height = _mapData.GetLength(1);

            if (interference.GetLength(0) != width ||
                interference.GetLength(1) != height)
            {
                throw new ArgumentException("Interference array dimensions must match map dimensions.", "interference");
            }

            MapLocation location;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    location = new MapLocation(x, y);
                    SetScanStrength(location, GetScanStrength(location) + interference[x, y]);
                }
            }
        }

        void CombineFrom(CivilizationMapData other, bool combineVisibility, bool combineFuelRange)
        {
            int width = _mapData.GetLength(0);
            int height = _mapData.GetLength(1);

            if (other == null)
                throw new ArgumentNullException("other");

            if (!combineVisibility && !combineFuelRange)
                return;

            MapLocation location;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    location = new MapLocation(x, y);

                    int currentData = _mapData[x, y];
                    int otherData = other._mapData[x, y];

                    if (combineVisibility)
                    {
                        currentData |= (otherData & (ExploredMask | ScannedMask));

                        int scanStrength = Math.Max(
                            ((currentData & ScanStrengthMask) >> ScanStrengthOffset) - ScanStrengthAdjustment,
                            ((otherData & ScanStrengthMask) >> ScanStrengthOffset) - ScanStrengthAdjustment);

                        scanStrength = Math.Min(scanStrength + ScanStrengthAdjustment, MaxAdjustedScanStrength);

                        currentData = ((currentData & ~ScanStrengthMask) | (scanStrength << ScanStrengthOffset));
                    }

                    if (combineFuelRange)
                    {
                        int fuelRange = Math.Min(currentData & FuelRangeMask, otherData & FuelRangeMask);

                        currentData = (currentData & ~FuelRangeMask) | (Math.Min(fuelRange, MaxFuelRange) << FuelRangeOffset);
                    }

                    _mapData[x, y] = currentData;
                }
            }
        }
    }
}
