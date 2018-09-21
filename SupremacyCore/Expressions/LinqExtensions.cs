// LinqExtensions.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Utility;
using System.Xml.Linq;

namespace System.Linq
{
    public static class LinqExtensions
    {
        #region Methods
        public static T ValueOrDefault<T>(this XElement source)
        {
            if (source != null)
            {
                try
                {
                    return (T)Convert.ChangeType(source, typeof(T));
                }
                catch (Exception e)
                {
                    GameLog.Core.General.Error(e);
                }
            }
            return default(T);
        }

        public static T ValueOrDefault<T>(this XAttribute source)
        {
            if (source != null)
            {
                try
                {
                    return (T)Convert.ChangeType(source, typeof(T));
                }
                catch (Exception e)
                {
                    GameLog.Core.General.Error(e);
                }
            }
            return default(T);
        }

        public static T ValueOrDefault<T>(this XElement source, T defaultValue)
        {
            if (source != null)
            {
                try
                {
                    return (T)Convert.ChangeType(source, typeof(T));
                }
                catch (Exception e)
                {
                    GameLog.Core.General.Error(e);
                }
            }
            return defaultValue;
        }

        public static T ValueOrDefault<T>(this XAttribute source, T defaultValue)
        {
            if (source != null)
            {
                try
                {
                    return (T)Convert.ChangeType(source, typeof(T));
                }
                catch (Exception e)
                {
                    GameLog.Core.General.Error(e);
                }
            }
            return defaultValue;
        }
        #endregion
    }
}