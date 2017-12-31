// CivilizationMapData.h
//
// Copyright (c) 2008 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

#pragma once

#include "MapLocation.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Diagnostics;

using namespace Supremacy::Universe;

namespace Supremacy
{
    namespace Game
    {
        [Serializable]
        public ref class CivilizationMapData sealed
        {
public:
            CivilizationMapData(int width, int height)
            {
                if (width > MapLocation::MaxValue)
                    throw gcnew ArgumentException("width cannot be greater than MapLocation::MaxValue");
                if (width < MapLocation::MinValue)
                    throw gcnew ArgumentException("width cannot be less than MapLocation::MinValue");
                if (height > MapLocation::MaxValue)
                    throw gcnew ArgumentException("height cannot be greater than MapLocation::MaxValue");
                if (height < MapLocation::MinValue)
                    throw gcnew ArgumentException("height cannot be less than MapLocation::MinValue");
                _mapData = gcnew array<unsigned short, 2>(width, height);
            }
            
            __forceinline int GetFuelRange(MapLocation location)
            {
                return (_mapData[location.X,location.Y] & FuelRangeMask);
            }

            __forceinline int GetScanStrength(MapLocation location)
            {
                return ((_mapData[location.X,location.Y] & ScanStrengthMask) >> ScanStrengthOffset) - ScanStrengthAdjustment;
            }
            
            __forceinline bool IsExplored(MapLocation location)
            {
                return ((_mapData[location.X,location.Y] & ExploredMask) != 0);
            }
            
            __forceinline bool IsScanned(MapLocation location)
            {
                return ((_mapData[location.X,location.Y] & ScannedMask) != 0);
            }
            
            __forceinline void ResetScanStrengthAndFuelRange()
            {
                int maxX = _mapData->GetLength(0);
                int maxY = _mapData->GetLength(1);

                for (int x = 0 ; (x < maxX); x++)
                {
                    for (int y = 0 ; (y < maxY); y++)
                    {
                        _mapData[x, y] = ((_mapData[x, y] & ~ScanStrengthMask & ~FuelRangeMask) |
                                          (MaxFuelRange << FuelRangeOffset) |
                                          (ScanStrengthAdjustment << ScanStrengthOffset));
                    }
                }
            }
            
            __forceinline void SetExplored(MapLocation location, bool value)
            {
                if (value)
                {
                    SetScanned(location, true);
                    _mapData[location.X, location.Y] |= ExploredMask;
                }
                else
                {
                    _mapData[location.X,location.Y] &= ~ExploredMask;
                }
            }
            
            __forceinline void SetExplored(MapLocation location, bool value, int radius)
            {
                int startX = Max(0, location.X - radius);
                int startY = Max(0, location.Y - radius);
                int endX = Min(_mapData->GetLength(0) - 1, location.X + radius);
                int endY = Min(_mapData->GetLength(1) - 1, location.Y + radius);

                for (int x = startX; x <= endX; x++)
                {
                    for (int y = startY; y <= endY; y++)
                    {
                        SetExplored(MapLocation(x, y), value);
                    }
                }
            }
            
            __forceinline void SetFuelRange(MapLocation location, int value)
            {
                _mapData[location.X, location.Y] = (_mapData[location.X, location.Y] & ~FuelRangeMask) | (Min(value, MaxFuelRange) << FuelRangeOffset);
            }
            
            __forceinline void SetScanned(MapLocation location, bool value)
            {
                if (value)
                {
                    _mapData[location.X, location.Y] |= ScannedMask;
                }
                else
                {
                    _mapData[location.X,location.Y] &= ~ScannedMask;
                }
            }

            __forceinline void SetScanned(MapLocation location, bool value, int radius)
            {
                int startX = Max(0, location.X - radius);
                int startY = Max(0, location.Y - radius);
                int endX = Min(_mapData->GetLength(0) - 1, location.X + radius);
                int endY = Min(_mapData->GetLength(1) - 1, location.Y + radius);

                for (int x = startX; x <= endX; x++)
                {
                    for (int y = startY; y <= endY; y++)
                    {
                        SetScanned(MapLocation(x, y), value);
                    }
                }
            }
            
            __forceinline void SetScanStrength(MapLocation location, int value)
            {
                if (value < MinScanStrength)
                    value = MinScanStrength;

                if (value > MaxScanStrength)
                    value = MaxScanStrength;

                int adjustedValue = Min(value + ScanStrengthAdjustment, MaxAdjustedScanStrength);

                _mapData[location.X,location.Y] = ((_mapData[location.X,location.Y] & ~ScanStrengthMask) | 
                                                   (adjustedValue << ScanStrengthOffset));
                
                if (value > 0)
                    SetScanned(location, true);
            }
            
            __forceinline void UpgradeFuelRange(MapLocation location, int value)
            {
                if (value < GetFuelRange(location))
                {
                    SetFuelRange(location, value);
                }
            }
            
            __forceinline void UpgradeFuelRange(MapLocation location, int value, int radius)
            {
                int startX = Max(0, location.X - radius);
                int startY = Max(0, location.Y - radius);
                int endX = Min(_mapData->GetLength(0) - 1, location.X + radius);
                int endY = Min(_mapData->GetLength(1) - 1, location.Y + radius);

                for (int x = startX; x <= endX; x++)
                {
                    for (int y = startY; y <= endY; y++)
                    {
                        UpgradeFuelRange(MapLocation(x, y), value);
                    }
                }
            }
            
            __forceinline void UpgradeScanStrength(MapLocation location, int value)
            {
                if ((value > GetScanStrength(location)))
                {
                    SetScanStrength(location, value);
                }
            }
            
            __forceinline void UpgradeScanStrength(MapLocation location, int value, int radius)
            {
                int startX = Max(0, location.X - radius);
                int startY = Max(0, location.Y - radius);
                int endX = Min(_mapData->GetLength(0) - 1, location.X + radius);
                int endY = Min(_mapData->GetLength(1) - 1, location.Y + radius);
                for (int x = startX; x <= endX; x++)
                {
                    for (int y = startY; y <= endY; y++)
                    {
                        UpgradeScanStrength(MapLocation(x, y), value);
                    }
                }
            }
            
            __forceinline void UpgradeScanStrength(MapLocation location, int value, int radius, int falloff, int minValue)
            {
                int startX = Max(0, location.X - radius);
                int startY = Max(0, location.Y - radius);
                int endX = Min(_mapData->GetLength(0) - 1, location.X + radius);
                int endY = Min(_mapData->GetLength(1) - 1, location.Y + radius);

                falloff = Abs(falloff);

                for (int x = startX; x <= endX; x++)
                {
                    for (int y = startY; y <= endY; y++)
                    {
                        MapLocation targetLocation = MapLocation(x, y) ;
                        UpgradeScanStrength(targetLocation, Max(minValue, (value - (falloff * MapLocation::GetDistance(location, targetLocation)))));
                    }
                }
            }

            void ApplyScanInterference(array<int, 2>^ interference)
            {
                int width = _mapData->GetLength(0);
                int height = _mapData->GetLength(1);

                if (interference->GetLength(0) != width ||
                    interference->GetLength(1) != height)
                {
                    throw gcnew ArgumentException("Interference array dimensions must match map dimensions.", "interference");
                }

                MapLocation location;

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        location = MapLocation(x, y);
                        this->SetScanStrength(location, GetScanStrength(location) + interference[x, y]);
                    }
                }
            }

            void CombineFrom(CivilizationMapData^ other, bool combineVisibility, bool combineFuelRange)
            {
                int width = _mapData->GetLength(0);
                int height = _mapData->GetLength(1);

                if (other == nullptr)
                    throw gcnew ArgumentNullException("other");

                if (!combineVisibility && !combineFuelRange)
                    return;

                MapLocation location;

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        location = MapLocation(x, y);

                        unsigned short currentData = _mapData[x, y];
                        unsigned short otherData = other->_mapData[x, y];
                        
                        if (combineVisibility)
                        {
                            currentData |= (otherData & (ExploredMask | ScannedMask));

                            unsigned short scanStrength = Max(
                                ((currentData & ScanStrengthMask) >> ScanStrengthOffset) - ScanStrengthAdjustment,
                                ((otherData & ScanStrengthMask) >> ScanStrengthOffset) - ScanStrengthAdjustment);

                            scanStrength = Min(scanStrength + ScanStrengthAdjustment, MaxAdjustedScanStrength);

                            currentData = ((currentData & ~ScanStrengthMask) |  (scanStrength << ScanStrengthOffset));
                        }

                        if (combineFuelRange)
                        {
                            unsigned short fuelRange = Min(currentData & FuelRangeMask, otherData & FuelRangeMask);
                                
                            currentData = (currentData & ~FuelRangeMask) | (Min(fuelRange, MaxFuelRange) << FuelRangeOffset);
                        }

                        _mapData[x, y] = currentData;
                    }
                }
            }

            literal int MaxScanStrength = 31;
            literal int MinScanStrength = -32;

private:
            initonly array<unsigned short, 2>^ _mapData;

            literal int ExploredMask = 16384;
            literal int FuelRangeMask = 255;
            literal int FuelRangeOffset = 0;
            literal int MaxFuelRange = 255;
            literal int MaxAdjustedScanStrength = 63;
            literal int ScanStrengthAdjustment = 32;
            literal int ScannedMask = 32768;
            literal int ScanStrengthMask = 16128;
            literal int ScanStrengthOffset = 8;
        };
    }
}