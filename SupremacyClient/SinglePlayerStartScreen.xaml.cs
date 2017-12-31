// SinglePlayerStartScreen.xaml.cs
//
// Copyright (c) 2007-2008 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Collections.Generic;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Client.Audio;
using Supremacy.Annotations;
using System;
using Supremacy.Utility;
using Supremacy.Client.Dialogs;


namespace Supremacy.Client
{
    /// <summary>
    /// Interaction logic for SinglePlayerStartScreen.xaml
    /// </summary>
    public partial class SinglePlayerStartScreen
    {
        #region Field
        ISoundPlayer _soundPlayer = null;
        
        Boolean _startAudio = false;
        #endregion

        #region Constructors and Finalizers
        public SinglePlayerStartScreen([NotNull] ISoundPlayer soundPlayer)
        {
            if (soundPlayer == null)
                throw new ArgumentNullException("soundPlayer");
            _soundPlayer = soundPlayer;

            InitializeComponent();

            //lstFederationPlayable.ItemsSource = EnumHelper.GetValues<FederationPlayable>();
            //lstRomulanPlayable.ItemsSource = EnumHelper.GetValues<RomulanPlayable>();
            //lstKlingonPlayable.ItemsSource = EnumHelper.GetValues<KlingonPlayable>();
            //lstCardassianPlayable.ItemsSource = EnumHelper.GetValues<CardassianPlayable>();
            //lstDominionPlayable.ItemsSource = EnumHelper.GetValues<DominionPlayable>();
            //lstBorgPlayable.ItemsSource = EnumHelper.GetValues<BorgPlayable>();
            //lstTerranEmpirePlayable.ItemsSource = EnumHelper.GetValues<TerranEmpirePlayable>();

            //lstFederationPlayable.SelectionChanged += (sender, args) => {  UpdateFederationPlayable(); };
            //lstRomulanPlayable.SelectionChanged += (sender, args) => {  UpdateRomulanPlayable(); };
            //lstKlingonPlayable.SelectionChanged += (sender, args) => {  UpdateKlingonPlayable(); };
            //lstCardassianPlayable.SelectionChanged += (sender, args) => {  UpdateCardassianPlayable(); };
            //lstDominionPlayable.SelectionChanged += (sender, args) => {  UpdateDominionPlayable(); };
            //lstBorgPlayable.SelectionChanged += (sender, args) => {  UpdateBorgPlayable(); };
            //lstTerranEmpirePlayable.SelectionChanged += (sender, args) => {  UpdateTerranEmpirePlayable(); };

            //lstFederationPlayable.SelectionChanged += (sender, args) => { OnOptionsChanged(); UpdateFederationPlayable(); };
            //lstRomulanPlayable.SelectionChanged += (sender, args) => { OnOptionsChanged(); UpdateRomulanPlayable(); };
            //lstKlingonPlayable.SelectionChanged += (sender, args) => { OnOptionsChanged(); UpdateKlingonPlayable(); };
            //lstCardassianPlayable.SelectionChanged += (sender, args) => { OnOptionsChanged(); UpdateCardassianPlayable(); };
            //lstDominionPlayable.SelectionChanged += (sender, args) => { OnOptionsChanged(); UpdateDominionPlayable(); };
            //lstBorgPlayable.SelectionChanged += (sender, args) => { OnOptionsChanged(); UpdateBorgPlayable(); };
            //lstTerranEmpirePlayable.SelectionChanged += (sender, args) => { OnOptionsChanged(); UpdateTerranEmpirePlayable(); };

            GameContext.PushThreadContext(new GameContext());

            try
            {
                DataContext = CivDatabase.Load()
                    .Where(civ => civ.CivilizationType == CivilizationType.Empire || civ.Key == "BORG" || civ.Key == "TERRANEMPIRE" || 
                            civ.Key == "FEDERATION" || civ.Key == "ROMULANS" || civ.Key == "KLINGONS" || civ.Key == "CARDASSIANS" || civ.Key == "DOMINION")  
                    .ToList();
                CivSelector.SelectionChanged += CivSelector_SelectionChanged;

                // Pre-Setting in Options would be fine, just for coding issues
                CivSelector.SelectedIndex = 1;  // jumps over first selection ("Intro" race) and jumps to Federation  
            }
            finally
            {
                GameContext.PopThreadContext();
            }
        }

        void CivSelector_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!_startAudio)
            {
                _startAudio = true;
                return;
            }
            var civlist = DataContext as List<Civilization>;
            if(CivSelector.SelectedIndex >= 0)
            {
                switch(civlist[CivSelector.SelectedIndex].Name)
                {
                    case "Federation":
                        _soundPlayer.Play("Menu", "FedSelection");
                        //GameLog.Client.GameData.DebugFormat("SPStartScreen.cs: CivID={0}, Name={1}", civlist[CivSelector.SelectedIndex].CivID, civlist[CivSelector.SelectedIndex].Name);
                        break;
                    case "Romulans":
                        _soundPlayer.Play("Menu", "RomSelection");
                        break;
                    case "Klingons":
                        _soundPlayer.Play("Menu", "KlingSelection");
                        //GameLog.Client.GameData.DebugFormat("SPStartScreen.cs: CivID={0}, Name={1}", civlist[CivSelector.SelectedIndex].CivID, civlist[CivSelector.SelectedIndex].Name);
                        break;
                    case "Cardassians":
                        _soundPlayer.Play("Menu", "CardSelection");
                        break;
                    case "Dominion":
                        _soundPlayer.Play("Menu", "DomSelection");
                        break;
                    case "Borg":
                        _soundPlayer.Play("Menu", "BorgSelection");
                        break;
                    case "Terran Empire":
                        _soundPlayer.Play("Menu", "TerranSelection");
                        //GameLog.Client.GameData.DebugFormat("SPStartScreen.cs: CivID={0}, Name={1}", civlist[CivSelector.SelectedIndex].CivID, civlist[CivSelector.SelectedIndex].Name);
                        break;
                }
            }
        }
        #endregion

        #region Properties and Indexers
        public int EmpireID
        {
            get { return CivSelector.SelectedIndex; }
        }

        public GameOptions Options
        {
            get { return OptionsPanel.Options; }
        }
        #endregion

        #region Private Methods

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // first check for races available

            // aim is to have this variable, also for translation: 
            // Just usage of civ and stuff in en.txt must fit manually, in en.txt Terran Empire can be translated into french "Empire"
            // only INSIDE CODE we use names like "RomulanPlayable" instead of "Civ_3_Playable"

            var civlist = DataContext as List<Civilization>;
            if (CivSelector.SelectedIndex >= 0)
            {
                //switch (CivSelector.SelectedIndex)
                //{
                if (CivSelector.SelectedIndex == 0)  // 
                {
                    if (Options.IntroPlayable == EmpirePlayable.No)
                    {
                        var result0 = MessageDialog.Show(Environment.NewLine +
                        Supremacy.Resources.ResourceManager.GetString("CIV_0_NOT_IN GAME"), MessageDialogButtons.Ok);
                        return;
                    }
                }
                if (CivSelector.SelectedIndex == 1)  // 
                {
                    if (Options.FederationPlayable == EmpirePlayable.No)
                    {
                        var result1 = MessageDialog.Show(Environment.NewLine +
                        Supremacy.Resources.ResourceManager.GetString("CIV_1_NOT_IN GAME"), MessageDialogButtons.Ok);
                        return;
                    }
                }
                //return;
                if (CivSelector.SelectedIndex == 2)  // 
                {
                    if (Options.TerranEmpirePlayable == EmpirePlayable.No)
                    {
                        var result2 = MessageDialog.Show(Environment.NewLine +
                           Supremacy.Resources.ResourceManager.GetString("CIV_2_NOT_IN GAME"), MessageDialogButtons.Ok);
                        return;
                    }
                }
                //return;
                if (CivSelector.SelectedIndex == 3)  // 
                {
                    if (Options.RomulanPlayable == EmpirePlayable.No)
                    {
                        var result3 = MessageDialog.Show(Environment.NewLine +
                       Supremacy.Resources.ResourceManager.GetString("CIV_3_NOT_IN GAME"), MessageDialogButtons.Ok);
                        return;
                    }
                }
                //return;
                if (CivSelector.SelectedIndex == 4)  // 
                {
                    if (Options.KlingonPlayable == EmpirePlayable.No)
                    {
                        var result4 = MessageDialog.Show(Environment.NewLine +
                        Supremacy.Resources.ResourceManager.GetString("CIV_4_NOT_IN GAME"), MessageDialogButtons.Ok);
                        return;
                    }
                }
                //return;
                if (CivSelector.SelectedIndex == 5)  // 
                {
                    if (Options.CardassianPlayable == EmpirePlayable.No)
                    {
                        var result5 = MessageDialog.Show(Environment.NewLine +
                        Supremacy.Resources.ResourceManager.GetString("CIV_5_NOT_IN GAME"), MessageDialogButtons.Ok);
                        return;
                    }
                }
                //return;
                if (CivSelector.SelectedIndex == 6)  // 
                {
                    if (Options.DominionPlayable == EmpirePlayable.No)
                    {
                        var result6 = MessageDialog.Show(Environment.NewLine +
                        Supremacy.Resources.ResourceManager.GetString("CIV_6_NOT_IN GAME"), MessageDialogButtons.Ok);
                        return;
                    }
                }
                //return;
                if (CivSelector.SelectedIndex == 7)  // Borg
                {
                    if (Options.BorgPlayable == EmpirePlayable.No)
                    {
                        var result7 = MessageDialog.Show(Environment.NewLine +
                       Supremacy.Resources.ResourceManager.GetString("CIV_7_NOT_IN GAME"), MessageDialogButtons.Ok);
                        return;
                    }
                }
                //return;

            }

            // ## old code ##
            //if (CivSelector.SelectedIndex == 7)  // Borg
            //{
            //    if (Options.BorgPlayable == EmpirePlayable.No)
            //    {
            //        var result = MessageDialog.Show(Environment.NewLine + "Borg Empire is set to NOT playable on the right",
            //                                MessageDialogButtons.Ok);
            //        //CivSelector.SelectedIndex = 1;
            //        return;   // return = not starting the game
            //    }
            //}

            _soundPlayer.Play("Menu", "LoadingGame");
            GameOptionsManager.SaveDefaults(OptionsPanel.Options);
            DialogResult = true;
        }

        private void UpdateFederationPlayable()
        {
            try
            {
                // at the moment no idea 
                // we don't want to disallow Fed + Terran being inside the game - this can or better must decided each player on his own

                //var imageSource = new BitmapImage(
                //    new Uri(
                //        "vfs:///Resources/Images/Galaxies/" + this.lstGalaxyShape.SelectedItem + ".png",
                //        UriKind.Absolute));
                //GalaxyImage.Source = imageSource;
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
            }
        }

        private void UpdateRomulanPlayable()
        {
            try
            {
                // at least if "no" on the right side then grey out the left side
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
            }
        }

        private void UpdateKlingonPlayable()
        {
            try
            {
                // at least if "no" on the right side then grey out the left side
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
            }
        }

        private void UpdateCardassianPlayable()
        {
            try
            {
                // at least if "no" on the right side then grey out the left side
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
            }
        }

        private void UpdateDominionPlayable()
        {
            try
            {
                // at least if "no" on the right side then grey out the left side
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
            }
        }

        private void UpdateBorgPlayable()
        {
            try
            {
                // at the moment no idea
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
            }
        }

        private void UpdateTerranEmpirePlayable()
        {
            try
            {
                // at the moment no idea
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
            }
        }

        #endregion
    }

    public static class BindableExtender
    {
        #region Constants
        public static readonly DependencyProperty BindableTextProperty =
            DependencyProperty.RegisterAttached(
                "BindableText",
                typeof(string),
                typeof(BindableExtender),
                new UIPropertyMetadata(
                    null,
                    BindableTextProperty_PropertyChanged));
        #endregion

        #region Public and Protected Methods
        public static string GetBindableText(DependencyObject obj)
        {
            return (string)obj.GetValue(BindableTextProperty);
        }

        public static void SetBindableText(
            DependencyObject obj,
            string value)
        {
            obj.SetValue(BindableTextProperty, value);
        }
        #endregion

        #region Private Methods
        private static void BindableTextProperty_PropertyChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is Run)
            {
                ((Run)dependencyObject).Text = (string)e.NewValue;
            }
        }
        #endregion
    }
}

//public class ClippingBorder : Border
//{
//    static ClippingBorder()
//    {
//        ClipProperty.OverrideMetadata(
//            typeof(ClippingBorder),
//            new FrameworkPropertyMetadata(
//                null,
//                CoerceClip));
//        BorderThicknessProperty.OverrideMetadata(
//            typeof(ClippingBorder),
//            new FrameworkPropertyMetadata((d, e) => d.CoerceValue(ClipProperty)));
//        CornerRadiusProperty.OverrideMetadata(
//            typeof(ClippingBorder),
//            new FrameworkPropertyMetadata((d, e) => d.CoerceValue(ClipProperty)));
//    }

//    public override UIElement Child
//    {
//        get
//        {
//            return base.Child;
//        }
//        set
//        {
//            ClearValue(ClipProperty);
//            base.Child = value;
//            CoerceValue(ClipProperty);
//        }
//    }

//    protected override Size ArrangeOverride(Size finalSize)
//    {
//        CoerceValue(ClipProperty);
//        return base.ArrangeOverride(finalSize);
//    }

//    private static object CoerceClip(DependencyObject d, object value)
//    {
//        if (DependencyPropertyHelper.GetValueSource(d, ClipProperty).BaseValueSource == BaseValueSource.Local)
//            return value;
//        var clippingBorder = d as ClippingBorder;
//        if ((clippingBorder == null) || (clippingBorder.Child == null) || clippingBorder.RenderSize.IsEmpty)
//            return value;
//        clippingBorder.InvalidateVisual();
//        return CreateClipGeometry(clippingBorder);
//    }

//    private static Geometry CreateClipGeometry(Border clippingBorder)
//    {
//        var clipGeometry = new StreamGeometry();
//        using (var geometryContext = clipGeometry.Open())
//        {
//            var cornerRadius = clippingBorder.CornerRadius;
//            var borderThickness = clippingBorder.BorderThickness;
//            var clipWidth = clippingBorder.ActualWidth - borderThickness.Left - borderThickness.Right;
//            var clipHeight = clippingBorder.ActualHeight - borderThickness.Top - borderThickness.Bottom;
//            clipGeometry.Transform = new TranslateTransform(borderThickness.Left, borderThickness.Top);
//            geometryContext.BeginFigure(
//                new Point(cornerRadius.TopLeft, 0),
//                true,
//                true);
//            geometryContext.LineTo(
//                new Point(clipWidth - cornerRadius.TopRight, 0),
//                false,
//                true);
//            geometryContext.ArcTo(
//                new Point(clipWidth, cornerRadius.TopRight),
//                new Size(cornerRadius.TopRight, cornerRadius.TopRight),
//                0,
//                false,
//                SweepDirection.Clockwise,
//                false,
//                true);
//            geometryContext.LineTo(
//                new Point(clipWidth, clipHeight - cornerRadius.BottomRight),
//                false,
//                true);
//            geometryContext.ArcTo(
//                new Point(clipWidth - cornerRadius.BottomRight, clipHeight),
//                new Size(cornerRadius.BottomRight, cornerRadius.BottomRight),
//                0,
//                false,
//                SweepDirection.Clockwise,
//                false,
//                true);
//            geometryContext.LineTo(
//                new Point(cornerRadius.BottomLeft, clipHeight),
//                false,
//                true);
//            geometryContext.ArcTo(
//                new Point(0, clipHeight - cornerRadius.BottomLeft),
//                new Size(cornerRadius.BottomLeft, cornerRadius.BottomLeft),
//                0,
//                false,
//                SweepDirection.Clockwise,
//                false,
//                true);
//            geometryContext.LineTo(
//                new Point(0, cornerRadius.TopLeft),
//                false,
//                true);
//            geometryContext.ArcTo(
//                new Point(cornerRadius.TopLeft, 0),
//                new Size(cornerRadius.TopLeft, cornerRadius.TopLeft),
//                0,
//                false,
//                SweepDirection.Clockwise,
//                false,
//                true);
//        }
//        return clipGeometry;
//        //var rectangle = new StreamGeometry();
//        //using (var dc = rectangle.Open())
//        //{
//        //    dc.BeginFigure(new Point(0, 0), true, true);
//        //    dc.LineTo(new Point(clippingBorder.ActualWidth, 0), false, true);
//        //    dc.LineTo(new Point(clippingBorder.ActualWidth, clippingBorder.ActualHeight), false, true);
//        //    dc.LineTo(new Point(0, clippingBorder.ActualHeight), false, true);
//        //    dc.Close();
//        //}
//        //return Geometry.Combine(
//        //    rectangle,
//        //    geometry,
//        //    GeometryCombineMode.Exclude,
//        //    null);
//    }
//}