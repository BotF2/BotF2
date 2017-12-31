// FilteredRandom.cs
//
// Copyright (c) 2003-2007 Steve Rabin, Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

//#define FR_RANGE_DIAGNOSTICS
//#define FR_GAUSSIAN_DIAGNOSTICS

using System;
using System.Diagnostics;

namespace Supremacy.Utility
{
    [Serializable]
    public class FilteredRandom : Random
    {
        #region Constants
        protected const int MaxRange = 256;
        protected const double InvalidNextValueGaussian = 100;
        #endregion

        #region Fields
        protected static readonly int[] InitialHistoryRange = {3, 8, 0, 6, 7, 9, 7, 0, 3, 5};
        protected static readonly double[] InitialHistoryReal = {0.9, 0.3, 0.1, 0.4, 0.6};
        protected static readonly double[] InitialHistoryGaussian = {0.9, 0.3, 0.6, 0.4, 0.8};

        private readonly int[] _rangeHistory;
        private readonly double[] _realHistory;
        private readonly double[] _gaussianHistory;

        private bool _change;
        private FilteredRandom _filteredRandomForReal;
        private FilteredRandom _filteredRandomForGaussian;
        private int _repeatingRunLength;
        private double _nextGaussian;
        #endregion

        #region Properties
        protected int[] RangeHistory
        {
            get { return _rangeHistory; }
        }

        protected double[] RealHistory
        {
            get { return _realHistory; }
        }

        protected double[] GaussianHistory
        {
            get { return _gaussianHistory; }
        }
        #endregion

        #region Constructors
        public FilteredRandom()
            : this(Environment.TickCount) {}

        public FilteredRandom(int seed)
            : base(seed)
        {
            _repeatingRunLength = 1;

            _rangeHistory = new int[InitialHistoryRange.Length];
            _realHistory = new double[InitialHistoryReal.Length];
            _gaussianHistory = new double[InitialHistoryGaussian.Length];

            for (int i = 0; i < InitialHistoryRange.Length; i++)
            {
                _rangeHistory[i] = InitialHistoryRange[i];
            }

            for (int i = 0; i < InitialHistoryReal.Length; i++)
            {
                _realHistory[i] = InitialHistoryReal[i];
            }

            _nextGaussian = InvalidNextValueGaussian;

            for (int i = 0; i < _gaussianHistory.Length; i++)
            {
                _gaussianHistory[i] = InitialHistoryGaussian[i];
            }
        }
        #endregion

        #region Properties
        public int RepeatingRunLength
        {
            get { return _repeatingRunLength; }
            set
            {
                if (_repeatingRunLength < 0)
                    throw new ArgumentOutOfRangeException("value", value, "value must be non-negative");
                _repeatingRunLength = value;
            }
        }
        #endregion

        #region Methods
        protected int Normalize(int value, int minimum, int maximum)
        {
            double result = ((maximum - minimum) / MaxRange);
            result += ((maximum - minimum) % MaxRange);
            result /= MaxRange;
            result += 1;
            result *= _rangeHistory[_rangeHistory.Length - 1];
            return (int)Math.Round(result);
        }

        public override int Next()
        {
            return Next(0, int.MaxValue);
        }

        public override int Next(int maxValue)
        {
            return Next(0, maxValue);
        }

        public override int Next(int minValue, int maxValue)
        {
            int escape = 0; //Escapes while loop if sequence becomes overconstrained
            int range = (maxValue - minValue) % MaxRange; //The acceptable range of values
            bool change = true; //If the sequence was alterred and must be reexamined by each rule

            Debug.Assert(range > 2, "range arg must be >= 3");

#if FR_RANGE_DIAGNOSTICS
            Console.Write("FilteredRandom.Next({0}, {1}):", minValue, maxValue);
#endif

            for (int i = 0; i < _rangeHistory.Length - 1; i++)
            {
                //move history down
                _rangeHistory[i] = _rangeHistory[i + 1];
            }

            while (change && escape < 50)
            {
                change = false;
                escape++;

                //Get random value based on chance
                _rangeHistory[_rangeHistory.Length - 1] = (base.Next() % range);

                //Allow correct number of repeating numbers as specified
                int j = 2;
                int runlength = 1;
                while (_rangeHistory[_rangeHistory.Length - 1] == _rangeHistory[_rangeHistory.Length - j])
                {
                    runlength++;
                    if (runlength > _repeatingRunLength)
                    {
#if FR_RANGE_DIAGNOSTICS
                        Console.Write(" ({0})", Normalize(_rangeHistory[_rangeHistory.Length - 1], minValue, maxValue));
#endif
                        change = true;
                        break;
                    }

                    j++;
                    if (j > _rangeHistory.Length)
                    {
                        break;
                    }
                }
                if (change)
                {
                    continue;
                }

                //Check for more than 3 in the last 10
                if (range >= 8)
                {
                    int count = 1;
                    for (int i = 1; i < 10; i++)
                    {
                        if (_rangeHistory[_rangeHistory.Length - 1 - i] == _rangeHistory[_rangeHistory.Length - 1])
                        {
                            count++;
                        }
                    }
                    if (count > 3)
                    {
#if FR_RANGE_DIAGNOSTICS
                        Console.Write(" [{0}]", Normalize(_rangeHistory[_rangeHistory.Length - 1], minValue, maxValue));
#endif
                        change = true;
                        continue;
                    }
                }
                else if (range >= 5)
                {
                    //Check for more than 4 in the last 10
                    int count = 1;
                    for (int i = 1; i < 10; i++)
                    {
                        if (_rangeHistory[_rangeHistory.Length - 1 - i] == _rangeHistory[_rangeHistory.Length - 1])
                        {
                            count++;
                        }
                    }
                    if (count > 4)
                    {
#if FR_RANGE_DIAGNOSTICS
                        Console.Write(" [{0}]", Normalize(_rangeHistory[_rangeHistory.Length - 1], minValue, maxValue));
#endif
                        change = true;
                        continue;
                    }
                }
                else
                {
                    //Check for more than 5 in the last 10
                    int count = 1;
                    for (int i = 1; i < 10; i++)
                    {
                        if (_rangeHistory[_rangeHistory.Length - 1 - i] == _rangeHistory[_rangeHistory.Length - 1])
                        {
                            count++;
                        }
                    }
                    if (count > 5)
                    {
#if FR_RANGE_DIAGNOSTICS
                        Console.Write(" [{0}]", Normalize(_rangeHistory[_rangeHistory.Length - 1], minValue, maxValue));
#endif
                        change = true;
                        continue;
                    }
                }

                //Check for more than 2 in a counting sequence
                if (range > 6)
                {
                    if (((_rangeHistory[_rangeHistory.Length - 1] + 1 == _rangeHistory[_rangeHistory.Length - 2])
                            && (_rangeHistory[_rangeHistory.Length - 1] + 2 == _rangeHistory[_rangeHistory.Length - 3]))
                        || ((_rangeHistory[_rangeHistory.Length - 1] == _rangeHistory[_rangeHistory.Length - 2] + 1)
                            && (_rangeHistory[_rangeHistory.Length - 1] == _rangeHistory[_rangeHistory.Length - 3] + 2)))
                    {
#if FR_RANGE_DIAGNOSTICS				
                        Console.Write(" {{{0}}}", Normalize(_rangeHistory[_rangeHistory.Length - 1], minValue, maxValue));
#endif
                        change = true;
                        continue;
                    }
                }
                else
                {
                    //Check for more than 3 in a counting sequence
                    if (((_rangeHistory[_rangeHistory.Length - 1] + 1 == _rangeHistory[_rangeHistory.Length - 2])
                            && (_rangeHistory[_rangeHistory.Length - 1] + 2 == _rangeHistory[_rangeHistory.Length - 3])
                            && (_rangeHistory[_rangeHistory.Length - 1] + 3 == _rangeHistory[_rangeHistory.Length - 4]))
                        || ((_rangeHistory[_rangeHistory.Length - 1] == _rangeHistory[_rangeHistory.Length - 2] + 1)
                            && (_rangeHistory[_rangeHistory.Length - 1] == _rangeHistory[_rangeHistory.Length - 3] + 2)
                            && (_rangeHistory[_rangeHistory.Length - 1] == _rangeHistory[_rangeHistory.Length - 4] + 3)))
                    {
#if FR_RANGE_DIAGNOSTICS				
                        Console.Write(" {{{0}}}", Normalize(_rangeHistory[_rangeHistory.Length - 1], minValue, maxValue));
#endif
                        change = true;
                        continue;
                    }
                }

                //Check for no more than 4 in a row being at the bottom of the range or the top of the range
                if (range > 8)
                {
                    if (((_rangeHistory[_rangeHistory.Length - 1] < (range / 2))
                            && (_rangeHistory[_rangeHistory.Length - 2] < (range / 2))
                            && (_rangeHistory[_rangeHistory.Length - 3] < (range / 2))
                            && (_rangeHistory[_rangeHistory.Length - 4] < (range / 2))
                            && (_rangeHistory[_rangeHistory.Length - 5] < (range / 2)))
                        || ((_rangeHistory[_rangeHistory.Length - 1] >= (range / 2))
                            && (_rangeHistory[_rangeHistory.Length - 2] >= (range / 2))
                            && (_rangeHistory[_rangeHistory.Length - 3] >= (range / 2))
                            && (_rangeHistory[_rangeHistory.Length - 4] >= (range / 2))
                            && (_rangeHistory[_rangeHistory.Length - 5] >= (range / 2))))
                    {
#if FR_RANGE_DIAGNOSTICS
                        Console.Write(" ${0}$", Normalize(_rangeHistory[_rangeHistory.Length - 1], minValue, maxValue));
#endif
                        change = true;
                        continue;
                    }
                }

                //Check for two pairs right next to each other (like 2255)
                if ((_rangeHistory[_rangeHistory.Length - 1] == _rangeHistory[_rangeHistory.Length - 2])
                    && (_rangeHistory[_rangeHistory.Length - 3] == _rangeHistory[_rangeHistory.Length - 4]))
                {
#if FR_RANGE_DIAGNOSTICS
                    Console.Write(" #{0}#", Normalize(_rangeHistory[_rangeHistory.Length - 1], minValue, maxValue));
#endif
                    change = true;
                    continue;
                }

                //Check for a motif of 2 repeating immediately
                if (range > 3)
                {
                    if ((_rangeHistory[_rangeHistory.Length - 1] == _rangeHistory[_rangeHistory.Length - 3])
                        && (_rangeHistory[_rangeHistory.Length - 2] == _rangeHistory[_rangeHistory.Length - 4]))
                    {
#if FR_RANGE_DIAGNOSTICS
                        Console.Write(" -{0}-", Normalize(_rangeHistory[_rangeHistory.Length - 1], minValue, maxValue));
#endif
                        change = true;
                        continue;
                    }
                }

                //Check for a motif (or mirror motif) of 3 repeating in the last 10
                if (range > 5)
                {
                    for (int i = 3; i < 7; i++)
                    {
                        if (((_rangeHistory[_rangeHistory.Length - 1] == _rangeHistory[_rangeHistory.Length - 1 - i])
                                && (_rangeHistory[_rangeHistory.Length - 2] == _rangeHistory[_rangeHistory.Length - 1 - i - 1])
                                && (_rangeHistory[_rangeHistory.Length - 3] == _rangeHistory[_rangeHistory.Length - 1 - i - 2]))
                            || ((_rangeHistory[_rangeHistory.Length - 1] == _rangeHistory[_rangeHistory.Length - 1 - i - 2])
                                && (_rangeHistory[_rangeHistory.Length - 2] == _rangeHistory[_rangeHistory.Length - 1 - i - 1])
                                && (_rangeHistory[_rangeHistory.Length - 3] == _rangeHistory[_rangeHistory.Length - 1 - i])))
                        {
#if FR_RANGE_DIAGNOSTICS
                            Console.Write(" *{0}*", Normalize(_rangeHistory[_rangeHistory.Length - 1], minValue, maxValue));
#endif
                            change = true;
                            break;
                        }
                    }
                    if (change)
                    {
                        continue;
                    }
                }
            }

#if FR_RANGE_DIAGNOSTICS
            Console.WriteLine(" ... result = {0}", Normalize(_rangeHistory[_rangeHistory.Length - 1], minValue, maxValue));
#endif
            return Normalize(_rangeHistory[_rangeHistory.Length - 1], minValue, maxValue);
        }

        public override void NextBytes(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = (byte)(Next() % 0x100);
        }

        protected override double Sample()
        {
            int i;
            int escape = 0;

            for (i = 0; i < _realHistory.Length - 1; i++)
            {
                //Move history down
                _realHistory[i] = _realHistory[i + 1];
            }

            _change = true;
            while (_change && escape < 50)
            {
                _change = false;
                escape++;

                //Get the whole number from a filtered random source
                double whole = Next(10);

                //Allow a repeating run of two
                if (_filteredRandomForReal == null)
                    _filteredRandomForReal = new FilteredRandom();

                _filteredRandomForReal.RepeatingRunLength = 2;

                //Let the fractional be completely random
                double fractional = base.Next() * (1.0 / int.MaxValue);

                //Combine the whole and fractional
                double candidate = whole + fractional;

                //Move the number into the [0,1] range
                candidate = candidate * 0.1;

                if (candidate < 0.0)
                {
                    //Ensure lower bound
                    candidate = 0.0;
                }
                else if (candidate > 1.0)
                {
                    //Ensure upper bound
                    candidate = 1.0;
                }

                _realHistory[_realHistory.Length - 1] = candidate;

                //Check that the last 3 numbers were more than 0.1 away from each other
                double diff1_2 = _realHistory[_realHistory.Length - 1] - _realHistory[_realHistory.Length - 2];
                double diff1_3 = _realHistory[_realHistory.Length - 1] - _realHistory[_realHistory.Length - 3];
                double diff2_3 = _realHistory[_realHistory.Length - 2] - _realHistory[_realHistory.Length - 3];

                if (((diff1_2 <= 0.1) && (diff1_2 >= -0.1))
                    && ((diff1_3 <= 0.1) && (diff1_3 >= -0.1))
                    && ((diff2_3 <= 0.1) && (diff2_3 >= -0.1)))
                {
                    _change = true;
                    continue;
                }

                //Check that the last 2 numbers are more than 0.02 away from each other
                double diff = _realHistory[_realHistory.Length - 1] - _realHistory[_realHistory.Length - 2];
                if (diff <= 0.02 && diff >= -0.02)
                {
                    _change = true;
                    continue;
                }

                //Check that the last 5 numbers don't make an increasing/decreasing sequence
                if (((_realHistory[_realHistory.Length - 1] > _realHistory[_realHistory.Length - 2])
                        && (_realHistory[_realHistory.Length - 2] > _realHistory[_realHistory.Length - 3])
                        && (_realHistory[_realHistory.Length - 3] > _realHistory[_realHistory.Length - 4])
                        && (_realHistory[_realHistory.Length - 4] > _realHistory[_realHistory.Length - 5]))
                    || ((_realHistory[_realHistory.Length - 1] < _realHistory[_realHistory.Length - 2])
                        && (_realHistory[_realHistory.Length - 2] < _realHistory[_realHistory.Length - 3])
                        && (_realHistory[_realHistory.Length - 3] < _realHistory[_realHistory.Length - 4])
                        && (_realHistory[_realHistory.Length - 4] < _realHistory[_realHistory.Length - 5])))
                {
                    _change = true;
                    continue;
                }
            }
            return (_realHistory[_realHistory.Length - 1]);
        }
        #endregion

        public virtual double NextGaussian()
        {
            bool change = true;

#if FR_GAUSSIAN_DIAGNOSTICS
            Console.Write("FilteredRandom.NextGaussian():");
#endif

            for (int i = 0; i < _gaussianHistory.Length - 1; i++)
            {
                //Move history down
                _gaussianHistory[i] = _gaussianHistory[i + 1];
            }

            while (change)
            {
                change = false;

                if (_nextGaussian == InvalidNextValueGaussian)
                {
                    //Gaussian random number generator adapted from Everett Carter's 
                    //article "Generating Gaussian Random Numbers" (www.taygeta.com/random/gaussian.html)
                    double x1, x2, w, y1, y2;

                    if (_filteredRandomForGaussian == null)
                        _filteredRandomForGaussian = new FilteredRandom();

                    do
                    {
                        x1 = 2.0 * _filteredRandomForGaussian.NextDouble() - 1.0;
                        x2 = 2.0 * _filteredRandomForGaussian.NextDouble() - 1.0;
                        w = x1 * x1 + x2 * x2;
                    } while (w >= 1.0);

                    w = Math.Sqrt((-2.0 * Math.Log10(w)) / w);

                    //Generate two random numbers at a time, store one for
                    //the next time genrand is called.
                    y1 = (x1 * w) / 1.5;
                    y2 = (x2 * w) / 1.5;
                    if (y1 > 1.0)
                    {
                        y1 = 1.0;
                    }
                    if (y1 < -1.0)
                    {
                        y1 = -1.0;
                    }
                    if (y2 > 1.0)
                    {
                        y2 = 1.0;
                    }
                    if (y2 < -1.0)
                    {
                        y2 = -1.0;
                    }
                    _nextGaussian = y2; //The stored random value for the next time
                    _gaussianHistory[_gaussianHistory.Length - 1] = y1;
                }
                else
                {
                    _gaussianHistory[_gaussianHistory.Length - 1] = _nextGaussian;
                    _nextGaussian = InvalidNextValueGaussian;
                }

                //Check that the there are not more than 3 numbers in a row above or below zero.
                if (((_gaussianHistory[_gaussianHistory.Length - 1] < 0.0)
                        && (_gaussianHistory[_gaussianHistory.Length - 2] < 0.0)
                        && (_gaussianHistory[_gaussianHistory.Length - 3] < 0.0)
                        && (_gaussianHistory[_gaussianHistory.Length - 4] < 0.0))
                    || ((_gaussianHistory[_gaussianHistory.Length - 1] > 0.0)
                        && (_gaussianHistory[_gaussianHistory.Length - 2] > 0.0)
                        && (_gaussianHistory[_gaussianHistory.Length - 3] > 0.0)
                        && (_gaussianHistory[_gaussianHistory.Length - 4] > 0.0)))
                {
#if FR_GAUSSIAN_DIAGNOSTICS
                    Console.Write(" a");
#endif
                    change = true;
                    continue;
                }

                //Check that the last 3 numbers were more than 0.1 away from each other
                double diff1_2 = _gaussianHistory[_gaussianHistory.Length - 1] - _gaussianHistory[_gaussianHistory.Length - 2];
                double diff1_3 = _gaussianHistory[_gaussianHistory.Length - 1] - _gaussianHistory[_gaussianHistory.Length - 3];
                double diff2_3 = _gaussianHistory[_gaussianHistory.Length - 2] - _gaussianHistory[_gaussianHistory.Length - 3];

                if ((diff1_2 <= 0.1 && diff1_2 >= -0.1)
                    && (diff1_3 <= 0.1 && diff1_3 >= -0.1)
                    && (diff2_3 <= 0.1 && diff2_3 >= -0.1))
                {
#if FR_GAUSSIAN_DIAGNOSTICS
                    Console.Write(" b");
#endif
                    change = true;
                    continue;
                }

                //Check that the last 2 numbers are more than 0.02 away from each other
                double diff = _gaussianHistory[_gaussianHistory.Length - 1] - _gaussianHistory[_gaussianHistory.Length - 2];
                if (diff <= 0.02 && diff >= -0.02)
                {
#if FR_GAUSSIAN_DIAGNOSTICS
                    Console.Write(" c");
#endif
                    change = true;
                    continue;
                }
                
                //Check that the last 5 numbers don't make an increasing/decreasing sequence
                if (((_gaussianHistory[_gaussianHistory.Length - 1] > _gaussianHistory[_gaussianHistory.Length - 2])
                        && (_gaussianHistory[_gaussianHistory.Length - 2] > _gaussianHistory[_gaussianHistory.Length - 3])
                        && (_gaussianHistory[_gaussianHistory.Length - 3] > _gaussianHistory[_gaussianHistory.Length - 4])
                        && (_gaussianHistory[_gaussianHistory.Length - 4] > _gaussianHistory[_gaussianHistory.Length - 5]))
                    || ((_gaussianHistory[_gaussianHistory.Length - 1] < _gaussianHistory[_gaussianHistory.Length - 2])
                        && (_gaussianHistory[_gaussianHistory.Length - 2] < _gaussianHistory[_gaussianHistory.Length - 3])
                        && (_gaussianHistory[_gaussianHistory.Length - 3] < _gaussianHistory[_gaussianHistory.Length - 4])
                        && (_gaussianHistory[_gaussianHistory.Length - 4] < _gaussianHistory[_gaussianHistory.Length - 5])))
                {
#if FR_GAUSSIAN_DIAGNOSTICS
                    Console.Write(" d");
#endif
                    change = true;
                    continue;
                }
            }

#if FR_GAUSSIAN_DIAGNOSTICS
            Console.WriteLine(" ... result = {0}", _gaussianHistory[_gaussianHistory.Length - 1]);
#endif
            return (_gaussianHistory[_gaussianHistory.Length - 1]);
        }

        public override double NextDouble()
        {
            return Sample();
        }
    }
}