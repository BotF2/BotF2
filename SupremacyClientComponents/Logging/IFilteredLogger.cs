// IFilteredLogger.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Microsoft.Practices.Composite.Logging;

namespace Supremacy.Client.Logging
{
    public interface IFilteredLogger : ILoggerFacade
    {
        Category[] LoggedCategories { get; set; }
        Priority[] LoggedPriorities { get; set; }
    }
}