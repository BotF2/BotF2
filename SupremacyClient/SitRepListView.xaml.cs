// SitRepListView.xaml.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.Windows.Controls;
using System.Windows.Input;

using Microsoft.Practices.ServiceLocation;
using Supremacy.Client.Context;
using Supremacy.Utility;

namespace Supremacy.Client
{
    /// <summary>
    /// Interaction logic for SitRepListView.xaml
    /// </summary>
    /// 

    // TODO: THIS FILE IS APPARENTLY NOT BEING USED!!!!!  Delete? (and accompanying .xaml file)

    public partial class SitRepListView : UserControl
    {
        private readonly IAppContext _appContext;

        public SitRepListView()
        {
            InitializeComponent();
            _appContext = ServiceLocator.Current.GetInstance<IAppContext>();
            GameLog.Print("called !  (when is it used ?? - see line 29");
        }

        public IAppContext AppContext
        {
            get { return _appContext; }
        }

        void SitRepListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
        }
    }
}