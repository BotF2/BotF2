// FilteredTextLogger.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.IO;

using Microsoft.Practices.Composite.Logging;

namespace Supremacy.Client.Logging
{
    // What is Microsoft.Practices.Composite.Logging? --> https://msdn.microsoft.com/en-us/library/ff921177.aspx

    public class FilteredTextLogger : TextLogger, IFilteredLogger
    {
        #region Constants
        public static readonly Category[] DebugCategories = new[]
                                                            {
                                                                Category.Exception,
                                                                Category.Warn,
                                                                Category.Info,
                                                                Category.Debug
                                                            };

        public static readonly Priority[] DebugPriorities = new[]
                                                            {
                                                                Priority.High,
                                                                Priority.Medium,
                                                                Priority.Low,
                                                                Priority.None
                                                            };

        public static readonly Category[] ReleaseCategories = new[]
                                                              {
                                                                  Category.Exception,
                                                                  Category.Warn,
                                                                  Category.Info
                                                              };

        public static readonly Priority[] ReleasePriorities = new[]
                                                              {
                                                                  Priority.High,
                                                                  Priority.Medium,
                                                                  Priority.Low
                                                              };
        #endregion

        #region Fields
        private Category[] _loggedCategories;
        private Priority[] _loggedPriorities;
        #endregion

        #region Constructors and Finalizers
        public FilteredTextLogger(TextWriter textWriter) : base(TextWriter.Synchronized(textWriter))
        {
            LoggedCategories = DebugCategories;
            LoggedPriorities = DebugPriorities;
        }
        #endregion

        #region IFilteredLogger Implementation
        public Category[] LoggedCategories
        {
            get { return _loggedCategories; }
            set
            {
                if (value == null)
                    value = DebugCategories;
                _loggedCategories = value;
            }
        }

        public Priority[] LoggedPriorities
        {
            get { return _loggedPriorities; }
            set
            {
                if (value == null)
                    value = DebugPriorities;
                _loggedPriorities = value;
            }
        }
        #endregion
    }
}