// AppContextAwareValueConverter.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Globalization;
using System.Threading;
using System.Windows.Data;

using Microsoft.Practices.ServiceLocation;

using Supremacy.Resources;
using Supremacy.Client.Context;

namespace Supremacy.Client
{
    public abstract class AppContextAwareValueConverter : IValueConverter
    {
        private static IResourceManager s_resourceManager;
        private static IAppContext s_appContext;


        protected IResourceManager ResourceManager
        {
            get
            {
                IResourceManager resourceManager = s_resourceManager;
                if (resourceManager == null)
                {
                    IResourceManager newResourceManager = ServiceLocator.Current.GetInstance<IResourceManager>();
                    resourceManager = Interlocked.CompareExchange(ref s_resourceManager, resourceManager, null) ?? newResourceManager;
                }
                return resourceManager;
            }
        }

        protected IAppContext AppContext
        {
            get
            {
                IAppContext appContext = s_appContext;
                if (appContext == null)
                {
                    IAppContext newAppContext = ServiceLocator.Current.GetInstance<IAppContext>();
                    appContext = Interlocked.CompareExchange(ref s_appContext, appContext, null) ?? newAppContext;
                }
                return appContext;
            }
        }


        #region Implementation of IValueConverter
        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
        #endregion
    }
}