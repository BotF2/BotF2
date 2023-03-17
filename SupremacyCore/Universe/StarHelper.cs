// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Game;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Supremacy.Universe
{
    public static class StarHelper
    {
        private static readonly Dictionary<StarType, int[,,]> InterferenceFrames;

        static StarHelper()
        {
            InterferenceFrames = new Dictionary<StarType, int[,,]>
                                 {
                                     {
                                         StarType.RadioPulsar,
                                         new[,,]
                                         {
                                             {
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {  -2,  -4,  -8, -16, -32,   0,   0,   0,   0  },
                                                 {  -2,  -4,  -8, -16,   0,   0,   0,   0,   0  },
                                                 {   0,  -2,  -4,   0,   0,   0,   0,   0,   0  },
                                                 {   0,  -2,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  }
                                             },
                                             {
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0, -32,   0,   0,   0,   0  },
                                                 {   0,   0,   0, -16, -16,   0,   0,   0,   0  },
                                                 {   0,   0,  -4,  -8,  -8,   0,   0,   0,   0  },
                                                 {   0,  -2,  -2,  -4,  -4,   0,   0,   0,   0  },
                                                 {   0,   0,   0,  -2,  -2,   0,   0,   0,   0  }
                                             },
                                             {
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0, -32,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0, -16, -16,   0,   0,   0  },
                                                 {   0,   0,   0,   0,  -8,  -8,  -4,   0,   0  },
                                                 {   0,   0,   0,   0,  -4,  -4,  -2,  -2,   0  },
                                                 {   0,   0,   0,   0,  -2,  -2,   0,   0,   0  }
                                             },
                                             {
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0, -32, -16,  -8,  -4,  -2  },
                                                 {   0,   0,   0,   0,   0, -16,  -8,  -4,  -2  },
                                                 {   0,   0,   0,   0,   0,   0,  -4,  -2,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,  -2,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  }
                                             },
                                             {
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,  -2,   0  },
                                                 {   0,   0,   0,   0,   0,   0,  -4,  -2,   0  },
                                                 {   0,   0,   0,   0,   0, -16,  -8,  -4,  -2  },
                                                 {   0,   0,   0,   0, -32, -16,  -8,  -4,  -2  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  }
                                             },
                                             {
                                                 {   0,   0,   0,   0,  -2,  -2,   0,   0,   0  },
                                                 {   0,   0,   0,   0,  -4,  -4,  -2,  -2,   0  },
                                                 {   0,   0,   0,   0,  -8,  -8,  -4,   0,   0  },
                                                 {   0,   0,   0,   0, -16, -16,   0,   0,   0  },
                                                 {   0,   0,   0,   0, -32,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  }
                                             },
                                             {
                                                 {   0,   0,   0,  -2,  -2,   0,   0,   0,   0  },
                                                 {   0,  -2,  -2,  -4,  -4,   0,   0,   0,   0  },
                                                 {   0,   0,  -4,  -8,  -8,   0,   0,   0,   0  },
                                                 {   0,   0,   0, -16, -16,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0, -32,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  }
                                             },
                                             {
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,  -2,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,  -2,  -4,   0,   0,   0,   0,   0,   0  },
                                                 {  -2,  -4,  -8, -16,   0,   0,   0,   0,   0  },
                                                 {  -2,  -4,  -8, -16, -32,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0,   0,   0,   0,   0  }
                                             }
                                         }
                                     },

                                     {
                                         StarType.XRayPulsar,
                                         new[,,]
                                         {
                                             {
                                                 {   0,   0,   0,   0,   0  },
                                                 {   0, -16, -16, -16,   0  },
                                                 {   0, -16, -32, -16,   0  },
                                                 {   0, -16, -16, -16,   0  },
                                                 {   0,   0,   0,   0,   0  }
                                             },
                                             {
                                                 {   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0  },
                                                 {   0,   0, -16,   0,   0  },
                                                 {   0,   0,   0,   0,   0  },
                                                 {   0,   0,   0,   0,   0  }
                                             },
                                             {
                                                 {   0,   0,   0,   0,   0  },
                                                 {   0, -16, -16, -16,   0  },
                                                 {   0, -16, -32, -16,   0  },
                                                 {   0, -16, -16, -16,   0  },
                                                 {   0,   0,   0,   0,   0  }
                                             },
                                             {
                                                 {   0, -16, -16, -16,   0  },
                                                 { -16, -32, -32, -32, -16  },
                                                 { -16, -32, -48, -32, -16  },
                                                 { -16, -32, -32, -32, -16  },
                                                 {   0, -16, -16, -16,   0  }
                                             }
                                         }
                                     },

                                     {
                                         StarType.BlackHole,
                                         new [,,] { { { -8 } } }
                                     },

                                     {
                                         StarType.Wormhole,
                                         new [,,] { { { -8 } } }
                                     },

                                     {
                                         StarType.Nebula,
                                         new [,,] { { { -4 } } }
                                     },

                                     {
                                         StarType.NeutronStar,
                                         new [,,] { { { -8 } } }
                                     },

                                     {
                                         StarType.Quasar,
                                         new[,,]
                                         {
                                             {
                                                 {   -8,  -8,  -8  },
                                                 {   -8, -32,  -8  },
                                                 {   -8,  -8,  -8  }
                                             }
                                         }
                                     }
                                 };
        }

        #region Methods

        public static bool CanAddPlanet(StarSystem starSystem, PlanetType planetType, PlanetSize planetSize)
        {
            if (starSystem == null)
            {
                return false;
            }

            return CanAddPlanet(starSystem.StarType, starSystem.Planets, planetType, planetSize);
        }

        public static bool CanAddPlanet(
            StarType starType,
            IEnumerable<Planet> existingPlanets,
            PlanetType planetType,
            PlanetSize planetSize)
        {
            if (!(Attribute.GetCustomAttribute(
                typeof(StarType).GetField(starType.ToString()),
                typeof(SupportsPlanetsAttribute)) is SupportsPlanetsAttribute attribute))
            {
                return false;
            }

            if (attribute.IsAllowedTypesDefined)
            {
                bool isPlanetTypeAllowed = attribute.AllowedTypes.Any(allowedType => allowedType == planetType);
                if (!isPlanetTypeAllowed)
                {
                    return false;
                }
            }

            if (attribute.IsAllowedSizesDefined)
            {
                bool isPlanetSizeAllowed = attribute.AllowedSizes.Any(allowedSize => allowedSize == planetSize);
                if (!isPlanetSizeAllowed)
                {
                    return false;
                }
            }

            if (existingPlanets != null)
            {
                int currentPlanetCount = existingPlanets.Count();
                if (currentPlanetCount > attribute.MaxNumberOfPlanets)
                {
                    return false;
                }
            }

            return true;
        }

        public static int MaxNumberOfPlanets(StarType starType)
        {
            SupportsPlanetsAttribute supportsPlanetsAttribute = starType.GetAttribute<StarType, SupportsPlanetsAttribute>();
            return supportsPlanetsAttribute != null ? supportsPlanetsAttribute.MaxNumberOfPlanets : StarSystem.MaxPlanetsPerSystem;
        }

        public static int MaxNumberOfPlanets(StarSystem starSystem)
        {
            if (starSystem == null)
            {
                return 0;
            }

            return MaxNumberOfPlanets(starSystem.StarType);
        }

        public static bool SupportsPlanets(StarSystem starSystem)
        {
            if (starSystem == null)
            {
                return false;
            }

            switch (starSystem.StarType)
            {
                case StarType.White:
                case StarType.Blue:
                case StarType.Yellow:
                case StarType.Orange:
                case StarType.Red:
                case StarType.Nebula:
                    return true;
                default:
                    return false;
            }
        }

        public static bool CanPlaceStar(
            StarType starType,
            MapLocation location,
            [NotNull] IIndexedEnumerable<MapLocation> homeLocations)
        {
            if (homeLocations == null)
            {
                throw new ArgumentNullException("homeLocations");
            }

            if (homeLocations.Count == 0)
            {
                return true;
            }

            if (!InterferenceFrames.TryGetValue(starType, out int[,,] interferenceFrames))
            {
                return true;
            }

            if (starType.SupportsPlanets()) 
                return true;

            int minDistance = GalaxyGenerator.MinHomeworldDistanceFromInterference;
            if (minDistance > 0)
            {
                // TODO
                /*
                                minDistance += Math.Max(
                                    interferenceFrames.GetLength(1) / 2,
                                    interferenceFrames.GetLength(2) / 2);
                */
            }

            return homeLocations.All(o => MapLocation.GetDistance(o, location) >= minDistance);
        }

        public static bool SupportsPlanets(this StarType starType)
        {
            return starType.MatchAttribute(SupportsPlanetsAttribute.Default);
        }

        public static void ApplySensorInterference(
            [NotNull] int[,] interference,
            [NotNull] StarSystem starSystem)
        {
            if (interference == null)
            {
                throw new ArgumentNullException("interference");
            }

            if (starSystem == null)
            {
                throw new ArgumentNullException("starSystem");
            }


            if (!InterferenceFrames.TryGetValue(starSystem.StarType, out int[,,] interferenceFrames))
            {
                return;
            }

            int frameCount = interferenceFrames.GetLength(0);

            /*
             * Rather than deriving the frame number directly from the turn number, we mix things up
             * a bit and base it off the turn number and the star's object ID.  We do this to avoid,
             * for example, the rotation of every pulsar in the galaxy being in sync.
             */
            int i = (GameContext.Current.TurnNumber + (starSystem.ObjectID % frameCount)) % frameCount;

            MapLocation origin = starSystem.Location;
            SectorMap map = GameContext.Current.Universe.Map;

            int maxX = map.Width - 1;
            int maxY = map.Height - 1;

            int startX = origin.X - interferenceFrames.GetLength(2) / 2;
            int endX = Math.Min(origin.X + interferenceFrames.GetLength(2) / 2, maxX);
            int startY = origin.Y - interferenceFrames.GetLength(2) / 2;
            int endY = Math.Min(origin.Y + interferenceFrames.GetLength(2) / 2, maxY);

            for (int y = startY; y <= endY; y++)
            {
                if (y < 0)
                {
                    continue;
                }

                for (int x = startX; x <= endX; x++)
                {
                    if (x < 0)
                    {
                        continue;
                    }

                    int value = interferenceFrames[i, y - startY, x - startX];
                    if (value >= 0)
                    {
                        continue;
                    }

                    int current = interference[x, y];
                    if (value < current)
                    {
                        interference[x, y] = value;
                    }
                }
            }
        }

        #endregion
    }
}