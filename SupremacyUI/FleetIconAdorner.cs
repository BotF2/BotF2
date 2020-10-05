using Supremacy.Annotations;
using Supremacy.Client;
using Supremacy.Client.Views;
using Supremacy.Collections;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Resources;
using Supremacy.Universe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Supremacy.UI
{
    public class FleetIconAdorner : UIElement, IAnimationsHost, IDisposable
    {
        private readonly Civilization _playerCiv;
        private readonly MapLocation _location;
        private readonly Civilization[] _owners;
        private readonly Lazy<bool> _drawDistressIndicator;
        private readonly Pen _distressPen;
        private readonly BitmapImage _fleetIcon;

        private IList<FleetView> _fleets;
        private bool _isDisposed;
        private ClockGroup _clockGroup;

        public FleetIconAdorner([NotNull] Civilization playerCiv, MapLocation location, [NotNull] Civilization[] owners)
        {
            _playerCiv = playerCiv ?? throw new ArgumentNullException("playerCiv");
            _location = location;
            _owners = owners ?? throw new ArgumentNullException("owners");
            _drawDistressIndicator = new Lazy<bool>(CheckDistressIndicatorVisibility);
            _distressPen = new Pen(new SolidColorBrush(Colors.White), 2.0);

            _fleetIcon = (owners.Length == 1)
                               ? GalaxyGridPanel.GetFleetIcon(owners[0])
                               : GalaxyGridPanel.GetMultiFleetIcon();
            _fleetIcon = GalaxyGridPanel.GetMultiFleetIcon();
            if (owners.Length == 1)
            {
                _fleetIcon = playerCiv == owners[0]
                    ? GalaxyGridPanel.GetFleetIcon(playerCiv)
                    : DiplomacyHelper.IsContactMade(playerCiv, owners[0]) ? GalaxyGridPanel.GetFleetIcon(owners[0]) : GalaxyGridPanel.GetUnknownFleetIcon();
            }

            UpdateDistressIndicator();
            if (owners.Contains(playerCiv))
                UpdateToolTip();
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            const double iconSize = GalaxyGridPanel.FleetIconSize;

            return hitTestParameters.HitPoint.X >= 0 &&
                hitTestParameters.HitPoint.X < iconSize &&
                hitTestParameters.HitPoint.Y >= 0 &&
                hitTestParameters.HitPoint.Y < iconSize
                ? new PointHitTestResult(this, hitTestParameters.HitPoint)
                : null;
        }

        private void RefreshFleets()
        {
            IEnumerable<Fleet> fleetViews = GameContext.Current.Universe.FindAt<Fleet>(_location)
                .Where(o => _owners.Contains(o.Owner));

            _fleets = new List<FleetView>(fleetViews.Count());

            fleetViews.Select(o => FleetView.Create(_playerCiv, o)).CopyTo(_fleets);
        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            GalaxyScreenView galaxyScreen = this.FindVisualAncestorByType<GalaxyScreenView>();
            if (galaxyScreen != null)
            {
                GalaxyScreenPresentationModel galaxyScreenModel = galaxyScreen.Model;
                if (galaxyScreenModel != null)
                {
                    galaxyScreenModel.SelectedSector = GameContext.Current.Universe.Map[_location];

                    FleetViewWrapper taskForce = galaxyScreenModel.TaskForces.FirstOrDefault(o => o.View.IsOwned);
                    if (taskForce != null)
                    {
                        galaxyScreenModel.SelectedTaskForce = taskForce;
                    }

                    e.Handled = true;

                    return;
                }
            }
            base.OnMouseLeftButtonDown(e);
        }

        private void UpdateDistressIndicator()
        {
            if (!_drawDistressIndicator.Value)
            {
                return;
            }

            ResumeAnimations();
            InvalidateVisual();
        }

        private void UpdateToolTip()
        {
            if (_fleets == null)
            {
                RefreshFleets();
            }

            string toolTip = string.Format(
                ResourceManager.GetString("MAP_FLEET_INDICATOR_TOOLTIP_NORMAL_TEXT"),
                _fleets.Count,
                _fleets.Sum(o => o.Ships.Count));

            if (_drawDistressIndicator.Value)
            {
                toolTip += "\n" + ResourceManager.GetString("MAP_FLEET_INDICATOR_TOOLTIP_DISTRESS_TEXT");
            }

            ToolTipService.SetToolTip(this, toolTip);
            ToolTipService.SetIsEnabled(this, true);
            ToolTipService.SetShowOnDisabled(this, true);
        }

        private bool CheckDistressIndicatorVisibility()
        {
            if (_fleets == null)
            {
                RefreshFleets();
            }

            IEnumerable<FleetView> fleetsInDistress =
                                       from fleet in _fleets
                                       where fleet.IsPresenceKnown && fleet.Source.IsInDistress() && fleet.Ships[0].Source.Owner == _playerCiv
                                       select fleet;
                                   

            if (!fleetsInDistress.Any())
            {
                return false;
            }

            Duration duration = new Duration(TimeSpan.FromSeconds(1));

            DoubleAnimation widthAnimation = new DoubleAnimation(
                GalaxyGridPanel.SectorSize,
                duration);

            DoubleAnimation heightAnimation = new DoubleAnimation(
                GalaxyGridPanel.SectorSize,
                duration);

            ColorAnimation opacityAnimation = new ColorAnimation(
                Color.FromArgb(0, 255, 255, 255),
                duration);

            ParallelTimeline timeline = new ParallelTimeline(
                null,
                new Duration(TimeSpan.FromSeconds(3)),
                RepeatBehavior.Forever)
                           {
                               Children =
                                   {
                                       widthAnimation,
                                       heightAnimation,
                                       opacityAnimation
                                   }
                           };

            _clockGroup = timeline.CreateClock();

            _distressPen.Brush.ApplyAnimationClock(
                SolidColorBrush.ColorProperty,
                (AnimationClock)_clockGroup.Children[2]);

            return true;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawImage(
                _fleetIcon,
                new Rect(new Size(GalaxyGridPanel.FleetIconSize, GalaxyGridPanel.FleetIconSize)));

            if (!_drawDistressIndicator.Value)
            {
                return;
            }

            drawingContext.DrawEllipse(
                null,
                _distressPen,
                new Point(GalaxyGridPanel.FleetIconSize / 2, GalaxyGridPanel.FleetIconSize / 2),
                null,
                0,
                (AnimationClock)_clockGroup.Children[0],
                0,
                (AnimationClock)_clockGroup.Children[1]);
        }

        public void PauseAnimations()
        {
            if (_isDisposed || !_drawDistressIndicator.Value)
            {
                return;
            }

            if (!_clockGroup.IsPaused && _clockGroup.Controller != null)
            {
                _clockGroup.Controller.Pause();
            }
        }

        public void ResumeAnimations()
        {
            if (_isDisposed || !_drawDistressIndicator.Value)
            {
                return;
            }

            switch (_clockGroup.CurrentState)
            {
                case ClockState.Stopped:
                    if (_clockGroup.Controller != null)
                    {
                        _clockGroup.Controller.Begin();
                    }

                    break;
                default:
                    if (_clockGroup.IsPaused && (_clockGroup.Controller != null))
                    {
                        _clockGroup.Controller.Resume();
                    }

                    break;
            }
        }

        public void StopAnimations()
        {
            if (!_drawDistressIndicator.Value || _clockGroup.Controller == null)
            {
                return;
            }

            _clockGroup.Controller.Stop();
            _clockGroup.Controller.Remove();
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            try
            {
                StopAnimations();
            }
            finally
            {
                _isDisposed = true;
            }
        }
    }
}