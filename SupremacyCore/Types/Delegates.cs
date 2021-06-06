// Delegates.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Universe;
using System.ComponentModel;

namespace Supremacy.Types
{
    public class ParameterEventArgs<T> : EventArgs
    {
        #region Fields
        private readonly T _parameter;
        #endregion

        #region Constructors
        public ParameterEventArgs(T parameter)
        {
            _parameter = parameter;
        }
        #endregion

        #region Properties
        public T Parameter => _parameter;
        #endregion
    }

    public class ParameterCancelEventArgs<T> : CancelEventArgs
    {
        #region Fields
        private readonly T _parameter;
        #endregion

        #region Constructors
        public ParameterCancelEventArgs(T parameter)
        {
            _parameter = parameter;
        }
        #endregion

        #region Properties
        public T Parameter => _parameter;
        #endregion
    }


    public delegate void DefaultEventHandler();

    public delegate void Function();

    public delegate void SetterFunction<T>(T value);

    public delegate void PropertySetter<T1, T2>(T1 value, T2 property);

    public delegate bool BoolFunction();

    public delegate bool? NullableBoolFunction();

    public delegate T Formula<T>(params T[] input);

    public delegate void SectorEventHandler(Sector sector);
}