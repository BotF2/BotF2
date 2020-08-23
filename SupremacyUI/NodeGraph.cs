// NodeGraph.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;

using Supremacy.Utility;

namespace Supremacy.UI
{
    public interface INodeGraphPenSelector
    {
        Pen GetPen(object parentNode, object childNode);
    }

    public class NodeGraph : FrameworkElement, INodeGraphPenSelector
    {
        static NodeGraph()
        {
            ClipToBoundsProperty.OverrideMetadata(
                typeof(NodeGraph),
                new FrameworkPropertyMetadata(true));

            FocusVisualStyleProperty.OverrideMetadata(
                typeof(NodeGraph),
                new FrameworkPropertyMetadata(
                    null,
                    (DependencyObject d, object baseValue) => null));
        }

        public NodeGraph()
        {
            Loaded += Graph_Loaded;
            Unloaded += Graph_Unloaded;
            IsVisibleChanged += HandleIsVisibleChanged;
            _compositionTarget_RenderingCallback = CompositionTarget_Rendering;

            _fadingGCPs = new Dictionary<int, GraphContentPresenter>();
            _fadingGCPsNextKey = int.MinValue;

            _nodeTemplateBinding = new Binding(NodeTemplateProperty.Name)
            {
                Source = this
            };

            _nodeTemplateSelectorBinding = new Binding(NodeTemplateSelectorProperty.Name)
            {
                Source = this
            };

            _nodesChangedHandler = NodesCollectionChanged;

            _frameTickWired = false;

            _nodePresenters = new List<GraphContentPresenter>();

            _penSelector = this;
        }

        private void HandleIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                WireFrameTick();
            }
            else
            {
                UnwireFrameTick();
            }
        }

        #region overrides
        protected override int VisualChildrenCount
        {
            get
            {
                if (!_isChildCountValid)
                {
                    _childCount = 0;
                    if (_centerObjectPresenter != null)
                    {
                        _childCount++;
                    }
                    if (_nodePresenters != null)
                    {
                        _childCount += _nodePresenters.Count;
                    }
                    if (_fadingGCPs != null)
                    {
                        if (!_fadingGCPListValid)
                        {
                            _fadingGCPList.Clear();
                            Dictionary<int, GraphContentPresenter>.ValueCollection values = _fadingGCPs.Values;
                            foreach (GraphContentPresenter gcp in values)
                            {
                                _fadingGCPList.Add(gcp);
                            }
                            _fadingGCPListValid = true;
                        }
                        Debug.Assert(_fadingGCPList.Count == _fadingGCPs.Count);
                        _childCount += _fadingGCPList.Count;
                    }
                    _isChildCountValid = true;
                }

                return _childCount;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            HandleChanges();
            _measureInvalidated = true;
            WireFrameTick();

            for (int i = 0; i < _needsMeasure.Count; i++)
            {
                _needsMeasure[i].Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            }
            _needsMeasure.Clear();

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _controlCenter.X = finalSize.Width / 2;
            _controlCenter.Y = finalSize.Height / 2;

            for (int i = 0; i < _needsArrange.Count; i++)
            {
                _needsArrange[i].Arrange(EmptyRect);
            }
            _needsArrange.Clear();

            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < _fadingGCPs.Count)
            {
                Debug.Assert(_fadingGCPListValid);
                return _fadingGCPList[index];
            }
            else
            {
                index -= _fadingGCPs.Count;
            }

            if (_nodePresenters != null)
            {
                if (index < _nodePresenters.Count)
                {
                    return _nodePresenters[index];
                }
                else
                {
                    index -= _nodePresenters.Count;
                }
            }

            if (index == 0)
            {
                return _centerObjectPresenter;
            }
            else
            {
                throw new Exception("not a valid index");
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (PenSelector != null)
            {
                if (_nodePresenters != null && _centerObjectPresenter != null)
                {
                    for (int i = 0; i < _nodePresenters.Count; i++)
                    {
                        Pen pen = PenSelector.GetPen(_centerObjectPresenter.Content, _nodePresenters[i].Content);
                        drawingContext.DrawLine(
                            pen, _centerObjectPresenter.ActualLocation, _nodePresenters[i].ActualLocation);
                    }
                }
            }
        }
        #endregion

        #region Properties

        #region CenterObject
        public static readonly DependencyProperty CenterObjectProperty = DependencyProperty.Register(
            "CenterObject",
            typeof(object),
            typeof(NodeGraph),
            GetCenterObjectPropertyMetadata());

        #region CenterObject Implementation
        private static PropertyMetadata GetCenterObjectPropertyMetadata()
        {
            FrameworkPropertyMetadata fpm = new FrameworkPropertyMetadata
            {
                AffectsMeasure = true,
                PropertyChangedCallback = CenterObjectPropertyChanged
            };
            return fpm;
        }

        private static void CenterObjectPropertyChanged(
            DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            ((NodeGraph)element).CenterObjectPropertyChanged();
        }

        private void CenterObjectPropertyChanged()
        {
            _centerChanged = true;
            ResetNodesBinding();
        }
        #endregion

        public object CenterObject
        {
            get => GetValue(CenterObjectProperty);
            set => SetValue(CenterObjectProperty, value);
        }
        #endregion

        #region NodesBindingPath
        public static readonly DependencyProperty NodesBindingPathProperty =
            DependencyProperty.Register(
                "NodesBindingPath",
                typeof(string),
                typeof(NodeGraph),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(NodesBindingPathPropertyChanged)));

        public string NodesBindingPath
        {
            get => (string)GetValue(NodesBindingPathProperty);
            set => SetValue(NodesBindingPathProperty, value);
        }

        private static void NodesBindingPathPropertyChanged(
            DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            NodeGraph g = (NodeGraph)element;
            g.ResetNodesBinding();
        }
        #endregion

        #region NodeTemplate
        public static readonly DependencyProperty NodeTemplateProperty = DependencyProperty.Register(
            "NodeTemplate", typeof(DataTemplate), typeof(NodeGraph), new FrameworkPropertyMetadata(null));

        public DataTemplate NodeTemplate
        {
            get => (DataTemplate)GetValue(NodeTemplateProperty);
            set => SetValue(NodeTemplateProperty, value);
        }
        #endregion

        #region NodeTemplateSelector
        public static readonly DependencyProperty NodeTemplateSelectorProperty = DependencyProperty.Register(
            "NodeTemplateSelector", typeof(DataTemplateSelector), typeof(NodeGraph), new FrameworkPropertyMetadata(null));

        public DataTemplateSelector NodeTemplateSelector
        {
            get => (DataTemplateSelector)GetValue(NodeTemplateSelectorProperty);
            set => SetValue(NodeTemplateSelectorProperty, value);
        }
        #endregion

        #region CoefficientOfDampening
        public static readonly DependencyProperty CoefficientOfDampeningProperty = DependencyProperty.Register(
            "CoefficientOfDampening",
            typeof(double),
            typeof(NodeGraph),
            new FrameworkPropertyMetadata(
                0.9,
                null,
                CoerceCoefficientOfDampeningPropertyCallback));

        public double CoefficientOfDampening
        {
            get => (double)GetValue(CoefficientOfDampeningProperty);
            set => SetValue(CoefficientOfDampeningProperty, value);
        }

        private static object CoerceCoefficientOfDampeningPropertyCallback(DependencyObject element, object baseValue)
        {
            return CoerceCoefficientOfDampeningPropertyCallback((double)baseValue);
        }

        private static double CoerceCoefficientOfDampeningPropertyCallback(double baseValue)
        {
            return baseValue <= MinCOD ? MinCOD : baseValue >= MaxCOD ? MaxCOD : baseValue;
        }
        #endregion

        #region FrameRate
        public static readonly DependencyProperty FrameRateProperty = DependencyProperty.Register(
            "FrameRate",
            typeof(double),
            typeof(NodeGraph),
            new FrameworkPropertyMetadata(
                0.4,
                null,
                CoerceFrameRatePropertyCallback));

        public double FrameRate
        {
            get => (double)GetValue(FrameRateProperty);
            set => SetValue(FrameRateProperty, value);
        }

        private static object CoerceFrameRatePropertyCallback(DependencyObject element, object baseValue)
        {
            return CoerceFrameRatePropertyCallback((double)baseValue);
        }

        private static double CoerceFrameRatePropertyCallback(double baseValue)
        {
            return baseValue <= MinCOD ? MinCOD : baseValue >= MaxCOD ? MaxCOD : baseValue;
        }
        #endregion

        #region Line brush
        public static readonly DependencyProperty LinePenProperty =
            DependencyProperty.Register("LinePen", typeof(Pen), typeof(NodeGraph), new PropertyMetadata(GetPen()));

        private static Pen DefaultPen;

        public Pen LinePen
        {
            get => (Pen)GetValue(LinePenProperty);
            set => SetValue(LinePenProperty, value);
        }

        public INodeGraphPenSelector PenSelector
        {
            get => _penSelector;
            set => _penSelector = value ?? this;
        }

        // Using a DependencyProperty as the backing store for LinePen.  This enables animation, styling, binding, etc...

        private static Pen GetPen()
        {
            if (DefaultPen == null)
            {
                DefaultPen = new Pen(Brushes.Gray, 1);
                DefaultPen.Freeze();
            }
            return DefaultPen;
        }
        #endregion

        #endregion

        #region Implementation
        private readonly EventHandler _compositionTarget_RenderingCallback;
        private readonly List<GraphContentPresenter> _fadingGCPList = new List<GraphContentPresenter>();
        private readonly Dictionary<int, GraphContentPresenter> _fadingGCPs;
        private readonly List<GraphContentPresenter> _needsArrange = new List<GraphContentPresenter>();
        private readonly List<GraphContentPresenter> _needsMeasure = new List<GraphContentPresenter>();
        private readonly List<GraphContentPresenter> _nodePresenters;
        private readonly NotifyCollectionChangedEventHandler _nodesChangedHandler;
        private readonly Binding _nodeTemplateBinding;
        private readonly Binding _nodeTemplateSelectorBinding;
        private bool _centerChanged;
        private object _centerDataInUse;
        private GraphContentPresenter _centerObjectPresenter;
        private int _childCount;
        private Point _controlCenter;
        private bool _fadingGCPListValid = false;
        private int _fadingGCPsNextKey;
        private bool _frameTickWired;
        private bool _isChildCountValid;
        private bool _measureInvalidated = false;
        private bool _nodeCollectionChanged;
        private bool _nodesChanged;
        private IList _nodesInUse;
        private INodeGraphPenSelector _penSelector;
        private Vector[,] _springForces;
        private bool _stillMoving = false;
        private long _ticksOfLastMeasureUpdate = long.MinValue;

        private void ResetNodesBinding()
        {
            if (NodesBindingPath == null)
            {
                BindingOperations.ClearBinding(this, NodesProperty);
            }
            else
            {
                Binding theBinding = GetBinding(NodesBindingPath, CenterObject);
                if (theBinding == null)
                {
                    BindingOperations.ClearBinding(this, NodesProperty);
                }
                else
                {
                    _ = BindingOperations.SetBinding(this, NodesProperty, theBinding);
                }
            }
        }

        private void WireFrameTick()
        {
            if (!_frameTickWired)
            {
                Debug.Assert(CheckAccess());
                CompositionTarget.Rendering += _compositionTarget_RenderingCallback;
                _frameTickWired = true;
            }
        }

        private void UnwireFrameTick()
        {
            if (_frameTickWired)
            {
                Debug.Assert(CheckAccess());
                CompositionTarget.Rendering -= _compositionTarget_RenderingCallback;
                _frameTickWired = false;
            }
        }

        private void Graph_Unloaded(object sender, RoutedEventArgs e)
        {
            UnwireFrameTick();
        }

        private void Graph_Loaded(object sender, RoutedEventArgs e)
        {
            WireFrameTick();
        }

        private void CompositionTarget_Rendering(object sender, EventArgs args)
        {
            Debug.Assert(_nodePresenters != null);

            if (_springForces == null)
            {
                _springForces = SetupForceVertors(_nodePresenters.Count);
            }
            else if (_springForces.GetLowerBound(0) != _nodePresenters.Count)
            {
                _springForces = SetupForceVertors(_nodePresenters.Count);
            }

            bool _somethingInvalid = false;
            if (_measureInvalidated || _stillMoving)
            {
                if (_measureInvalidated)
                {
                    _ticksOfLastMeasureUpdate = DateTime.Now.Ticks;
                }

                #region CenterObject
                if (_centerObjectPresenter != null)
                {
                    if (_centerObjectPresenter.New)
                    {
                        _centerObjectPresenter.ParentCenter = _controlCenter;
                        _centerObjectPresenter.New = false;
                        _somethingInvalid = true;
                    }
                    else
                    {
                        Vector forceVector = GetAttractionForce(
                            EnsureNonzeroVector((Vector)_centerObjectPresenter.Location));

                        if (
                            UpdateGraphCP(
                                _centerObjectPresenter, forceVector, CoefficientOfDampening, FrameRate, _controlCenter))
                        {
                            _somethingInvalid = true;
                        }
                    }
                }
                #endregion

                for (int i = 0; i < _nodePresenters.Count; i++)
                {
                    GraphContentPresenter gcp = _nodePresenters[i];

                    if (gcp.New)
                    {
                        gcp.New = false;
                        _somethingInvalid = true;
                    }

                    for (int j = i + 1; j < _nodePresenters.Count; j++)
                    {
                        Vector distance = EnsureNonzeroVector(gcp.Location - _nodePresenters[j].Location);

                        Vector repulsiveForce = GetRepulsiveForce(distance);
                        _springForces[i, j] = repulsiveForce;
                    }
                }

                Point centerLocationToUse = (_centerObjectPresenter != null)
                                                ? _centerObjectPresenter.Location
                                                : new Point();

                for (int i = 0; i < _nodePresenters.Count; i++)
                {
                    Vector forceVector = new Vector();
                    forceVector += GetVectorSum(i, _nodePresenters.Count, _springForces);
                    forceVector +=
                        GetSpringForce(EnsureNonzeroVector(_nodePresenters[i].Location - centerLocationToUse));
                    forceVector += GetWallForce(RenderSize, _nodePresenters[i].Location);

                    if (
                        UpdateGraphCP(
                            _nodePresenters[i], forceVector, CoefficientOfDampening, FrameRate, _controlCenter))
                    {
                        _somethingInvalid = true;
                    }
                }

                #region animate all of the fading ones away
                for (int i = 0; i < _fadingGCPList.Count; i++)
                {
                    if (!_fadingGCPList[i].WasCenter)
                    {
                        Vector centerDiff = EnsureNonzeroVector(_fadingGCPList[i].Location - centerLocationToUse);
                        centerDiff.Normalize();
                        centerDiff *= 20;
                        if (
                            UpdateGraphCP(
                                _fadingGCPList[i], centerDiff, CoefficientOfDampening, FrameRate, _controlCenter))
                        {
                            _somethingInvalid = true;
                        }
                    }
                }
                #endregion

                if (_somethingInvalid && BelowMaxSettleTime())
                {
                    _stillMoving = true;
                    InvalidateVisual();
                }
                else
                {
                    _stillMoving = false;
                    UnwireFrameTick();
                }
                _measureInvalidated = false;
            }
        }

        private bool BelowMaxSettleTime()
        {
            Debug.Assert(_ticksOfLastMeasureUpdate != long.MinValue);
            return MaxSettleTime > new TimeSpan(DateTime.Now.Ticks - _ticksOfLastMeasureUpdate);
        }

        private static Vector EnsureNonzeroVector(Vector vector)
        {
            return vector.Length > 0 ? vector : new Vector(Rnd.NextDouble() - .5, Rnd.NextDouble() - .5);
        }

        private static bool UpdateGraphCP(
            GraphContentPresenter graphContentPresenter,
            Vector forceVector,
            double coefficientOfDampening,
            double frameRate,
            Point parentCenter)
        {
            bool parentCenterChanged = graphContentPresenter.ParentCenter != parentCenter;
            if (parentCenterChanged)
            {
                graphContentPresenter.ParentCenter = parentCenter;
            }

            //add system drag
            Debug.Assert(coefficientOfDampening > 0);
            Debug.Assert(coefficientOfDampening < 1);
            graphContentPresenter.Velocity *= 1 - coefficientOfDampening*frameRate;

            //add force
            graphContentPresenter.Velocity += forceVector * frameRate;

            //apply terminalVelocity
            if (graphContentPresenter.Velocity.Length > TerminalVelocity)
            {
                graphContentPresenter.Velocity *= TerminalVelocity / graphContentPresenter.Velocity.Length;
            }

            if (graphContentPresenter.Velocity.Length > MinVelocity && forceVector.Length > MinVelocity)
            {
                graphContentPresenter.Location += graphContentPresenter.Velocity * frameRate;
                return true;
            }
            else
            {
                graphContentPresenter.Velocity = new Vector();
                return false || parentCenterChanged;
            }
        }

        private static Vector[,] SetupForceVertors(int count)
        {
            return new Vector[count, count];
        }

        private void KillGCP(GraphContentPresenter gcp, bool isCenter)
        {
            Debug.Assert(VisualTreeHelper.GetParent(gcp) == this);

            _fadingGCPs.Add(_fadingGCPsNextKey, gcp);
            _fadingGCPListValid = false;
            _isChildCountValid = false;

            int theKey = _fadingGCPsNextKey;

            gcp.IsHitTestVisible = false;
            if (isCenter)
            {
                gcp.WasCenter = true;
            }

            ScaleTransform st = gcp.ScaleTransform;

            DoubleAnimation da = GetNewHideAnimation(this, theKey);
            st.BeginAnimation(ScaleTransform.ScaleXProperty, da);
            st.BeginAnimation(ScaleTransform.ScaleYProperty, da);
            gcp.BeginAnimation(OpacityProperty, da);

            if (_fadingGCPsNextKey == int.MaxValue)
            {
                _fadingGCPsNextKey = int.MinValue;
            }
            else
            {
                _fadingGCPsNextKey++;
            }
        }

        private void CleanUpGCP(int key)
        {
            Debug.Assert(CheckAccess());
            if (_fadingGCPs.ContainsKey(key))
            {
                GraphContentPresenter gcp = _fadingGCPs[key];
                Debug.Assert(gcp != null);
                Debug.Assert(VisualTreeHelper.GetParent(gcp) == this);
                RemoveVisualChild(gcp);
                _fadingGCPListValid = false;
                _isChildCountValid = false;
                _fadingGCPs.Remove(key);
            }
        }

        private static DoubleAnimation GetNewHideAnimation(NodeGraph owner, int key)
        {
            DoubleAnimation da = new DoubleAnimation(0, HideDuration)
            {
                FillBehavior = FillBehavior.Stop
            };
            HideAnimationManager ham = new HideAnimationManager(owner, key);
            da.Completed += ham.CompletedHandler;
            da.Freeze();
            return da;
        }

        private void HandleChanges()
        {
            HandleNodesChangedWiring();

            if (_centerChanged && _nodeCollectionChanged && _centerObjectPresenter != null && _nodePresenters != null &&
                CenterObject != null && !CenterObject.Equals(_centerDataInUse))
            {
                Debug.Assert(!CenterObject.Equals(_centerDataInUse));
                Debug.Assert(
                    _centerObjectPresenter.Content == null || _centerObjectPresenter.Content.Equals(_centerDataInUse));

                _centerDataInUse = CenterObject;

                //figure out if we can re-cycle one of the existing children as the center Node
                //if we can, newCenter != null
                GraphContentPresenter newCenterPresenter = null;
                for (int i = 0; i < _nodePresenters.Count; i++)
                {
                    if (_nodePresenters[i].Content.Equals(CenterObject))
                    {
                        //we should re-use this 
                        newCenterPresenter = _nodePresenters[i];
                        _nodePresenters[i] = null;
                        break;
                    }
                }

                //figure out if we can re-cycle the exsting center as one of the new child nodes
                //if we can, newChild != null && newChildIndex == indexOf(data in Nodes)
                int newChildIndex = -1;
                GraphContentPresenter newChildPresnter = null;
                for (int i = 0; i < _nodesInUse.Count; i++)
                {
                    if (_nodesInUse[i] != null && _centerObjectPresenter.Content != null &&
                        _nodesInUse[i].Equals(_centerObjectPresenter.Content))
                    {
                        newChildIndex = i;
                        newChildPresnter = _centerObjectPresenter;
                        _centerObjectPresenter = null;
                        break;
                    }
                }

                //now we potentially have a center (or not) and one edge(or not)
                GraphContentPresenter[] newChildren = new GraphContentPresenter[_nodesInUse.Count];
                if (newChildPresnter != null)
                {
                    newChildren[newChildIndex] = newChildPresnter;
                }

                for (int i = 0; i < _nodesInUse.Count; i++)
                {
                    if (newChildren[i] == null)
                    {
                        for (int j = 0; j < _nodePresenters.Count; j++)
                        {
                            if (_nodePresenters[j] != null)
                            {
                                if (_nodesInUse[i].Equals(_nodePresenters[j].Content))
                                {
                                    Debug.Assert(newChildren[i] == null);
                                    newChildren[i] = _nodePresenters[j];
                                    _nodePresenters[j] = null;
                                    break;
                                }
                            }
                        }
                    }
                }

                //we've no reused everything we can
                if (_centerObjectPresenter == null)
                {
                    if (newCenterPresenter == null)
                    {
                        _centerObjectPresenter = GetGraphContentPresenter(
                            CenterObject,
                            _nodeTemplateBinding,
                            _nodeTemplateSelectorBinding
                            );
                        AddVisualChild(_centerObjectPresenter);
                    }
                    else
                    {
                        _centerObjectPresenter = newCenterPresenter;
                        Debug.Assert(VisualTreeHelper.GetParent(newCenterPresenter) == this);
                    }
                }
                else
                {
                    if (newCenterPresenter == null)
                    {
                        _centerObjectPresenter.Content = CenterObject;
                    }
                    else
                    {
                        KillGCP(_centerObjectPresenter, true);
                        _centerObjectPresenter = newCenterPresenter;
                        Debug.Assert(VisualTreeHelper.GetParent(newCenterPresenter) == this);
                    }
                }

                //go through all of the old CPs that are not being used and remove them
                for (int i = 0; i < _nodePresenters.Count; i++)
                {
                    if (_nodePresenters[i] != null)
                    {
                        KillGCP(_nodePresenters[i], false);
                    }
                }

                //go throug and "fill in" all the new CPs
                for (int i = 0; i < _nodesInUse.Count; i++)
                {
                    if (newChildren[i] == null)
                    {
                        GraphContentPresenter gcp = GetGraphContentPresenter(
                            _nodesInUse[i],
                            _nodeTemplateBinding,
                            _nodeTemplateSelectorBinding);
                        AddVisualChild(gcp);
                        newChildren[i] = gcp;
                    }
                }

                _nodePresenters.Clear();
                _nodePresenters.AddRange(newChildren);

                _isChildCountValid = false;

                _centerChanged = false;
                _nodeCollectionChanged = false;
            }

            if (_centerChanged)
            {
                _centerDataInUse = CenterObject;
                if (_centerObjectPresenter != null)
                {
                    KillGCP(_centerObjectPresenter, true);
                    _centerObjectPresenter = null;
                }
                if (_centerDataInUse != null)
                {
                    SetUpCleanCenter(_centerDataInUse);
                }
                _centerChanged = false;
            }

            if (_nodeCollectionChanged)
            {
                SetupNodes(Nodes);

                _nodesInUse = Nodes;

                _nodeCollectionChanged = false;
            }

#if DEBUG
            if (CenterObject != null)
            {
                CenterObject.Equals(_centerDataInUse);
                Debug.Assert(_centerObjectPresenter != null);
            }
            else
            {
                Debug.Assert(_centerDataInUse == null);
            }
            if (Nodes != null)
            {
                Debug.Assert(_nodePresenters != null);
                Debug.Assert(Nodes.Count == _nodePresenters.Count);
                Debug.Assert(_nodesInUse == Nodes);
            }
            else
            {
                Debug.Assert(_nodesInUse == null);
                if (_nodePresenters != null)
                {
                    Debug.Assert(_nodePresenters.Count == 0);
                }
            }
#endif
        }

        private void HandleNodesChangedWiring()
        {
            if (_nodesChanged)
            {
                if (_nodesInUse is INotifyCollectionChanged oldList)
                {
                    oldList.CollectionChanged -= _nodesChangedHandler;
                }

                if (Nodes is INotifyCollectionChanged newList)
                {
                    newList.CollectionChanged += _nodesChangedHandler;
                }

                _nodesInUse = Nodes;
                _nodesChanged = false;
            }
        }

        private void SetupNodes(IList nodes)
        {
#if DEBUG
            for (int i = 0; i < _nodePresenters.Count; i++)
            {
                Debug.Assert(_nodePresenters[i] != null);
                Debug.Assert(VisualTreeHelper.GetParent(_nodePresenters[i]) == this);
            }
#endif

            int nodesCount = (nodes == null) ? 0 : nodes.Count;

            GraphContentPresenter[] newNodes = new GraphContentPresenter[nodesCount];

            if (nodes != null)
            {
                for (int i = 0; i < nodesCount; i++)
                {
                    for (int j = 0; j < _nodePresenters.Count; j++)
                    {
                        if (_nodePresenters[j] != null)
                        {
                            if (nodes[i] == _nodePresenters[j].Content)
                            {
                                newNodes[i] = _nodePresenters[j];
                                _nodePresenters[j] = null;
                                break;
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < _nodePresenters.Count; i++)
            {
                if (_nodePresenters[i] != null)
                {
                    KillGCP(_nodePresenters[i], false);
                    _nodePresenters[i] = null;
                }
            }

            if (nodes != null)
            {
                for (int i = 0; i < newNodes.Length; i++)
                {
                    if (newNodes[i] == null)
                    {
                        newNodes[i] = GetGraphContentPresenter(
                            nodes[i],
                            _nodeTemplateBinding,
                            _nodeTemplateSelectorBinding);
                        AddVisualChild(newNodes[i]);
                    }
                }
            }

#if DEBUG
            for (int i = 0; i < _nodePresenters.Count; i++)
            {
                Debug.Assert(_nodePresenters[i] == null);
            }

            if (nodes != null)
            {
                for (int i = 0; i < newNodes.Length; i++)
                {
                    Debug.Assert(newNodes[i] != null);
                    Debug.Assert(VisualTreeHelper.GetParent(newNodes[i]) == this);
                    Debug.Assert(newNodes[i].Content == nodes[i]);
                }
            }
#endif

            _nodePresenters.Clear();
            _nodePresenters.AddRange(newNodes);
            _isChildCountValid = false;
        }

        private void SetUpCleanCenter(object newCenter)
        {
            Debug.Assert(_centerObjectPresenter == null);

            _centerObjectPresenter =
                GetGraphContentPresenter(newCenter, _nodeTemplateBinding, _nodeTemplateSelectorBinding);
            AddVisualChild(_centerObjectPresenter);

            _isChildCountValid = false;
        }

        private void NodesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            VerifyAccess();
            InvalidateMeasure();
            _nodeCollectionChanged = true;
        }

        private GraphContentPresenter GetGraphContentPresenter(
            object content, BindingBase nodeTemplateBinding, BindingBase nodeTemplateSelectorBinding)
        {
            GraphContentPresenter gcp =
                new GraphContentPresenter(content, nodeTemplateBinding, nodeTemplateSelectorBinding);

            _needsMeasure.Add(gcp);
            _needsArrange.Add(gcp);

            return gcp;
        }

        #region private Nodes property
        private static readonly DependencyProperty NodesProperty = DependencyProperty.Register(
            "Nodes",
            typeof(IList),
            typeof(NodeGraph),
            GetNodesPropertyMetadata());

        private IList Nodes => (IList)GetValue(NodesProperty);

        private static PropertyMetadata GetNodesPropertyMetadata()
        {
            FrameworkPropertyMetadata fpm = new FrameworkPropertyMetadata
            {
                AffectsMeasure = true,
                PropertyChangedCallback = NodesPropertyChanged
            };
            return fpm;
        }

        private static void NodesPropertyChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            ((NodeGraph)element).NodesPropertyChanged();
        }

        private void NodesPropertyChanged()
        {
            _nodeCollectionChanged = true;
            _nodesChanged = true;
        }
        #endregion

        #endregion

        #region INodeGraphPenSelector Members
        public Pen GetPen(object parentNode, object childNode)
        {
            return LinePen;
        }
        #endregion

        #region Nested type: GraphContentPresenter
        private class GraphContentPresenter : ContentPresenter
        {
            private readonly TranslateTransform _translateTransform;
            public readonly ScaleTransform ScaleTransform;

            private Size _actualDesiredSize;
            private Size _actualRenderSize;

            private Vector _centerVector;
            private Point _location;
            private Point _parentCenter;
            public bool New = true;
            public Vector Velocity;
            public bool WasCenter = false;

            public GraphContentPresenter(
                object content,
                BindingBase nodeTemplateBinding,
                BindingBase nodeTemplateSelectorBinding)
            {
                Content = content;

                _ = SetBinding(ContentTemplateProperty, nodeTemplateBinding);
                _ = SetBinding(ContentTemplateSelectorProperty, nodeTemplateSelectorBinding);

                ScaleTransform = new ScaleTransform();
                _translateTransform = new TranslateTransform();

                TransformGroup tg = new TransformGroup();
                tg.Children.Add(ScaleTransform);
                tg.Children.Add(_translateTransform);

                RenderTransform = tg;

                DoubleAnimation da = new DoubleAnimation(.5, 1, ShowDuration);
                BeginAnimation(OpacityProperty, da);
                ScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, da);
                ScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, da);
            }

            public Point Location
            {
                get => _location;
                set
                {
                    if (_location != value)
                    {
                        _location = value;
                        UpdateTransform();
                    }
                }
            }

            public Point ParentCenter
            {
                get => _parentCenter;
                set
                {
                    if (_parentCenter != value)
                    {
                        _parentCenter = value;
                        UpdateTransform();
                    }
                }
            }

            public Point ActualLocation => new Point(_location.X + _parentCenter.X, _location.Y + _parentCenter.Y);

            protected override Size MeasureOverride(Size constraint)
            {
                _actualDesiredSize = base.MeasureOverride(new Size(double.PositiveInfinity, double.PositiveInfinity));
                return new Size();
            }

            protected override Size ArrangeOverride(Size arrangeSize)
            {
                _actualRenderSize = base.ArrangeOverride(_actualDesiredSize);

                ScaleTransform.CenterX = _actualRenderSize.Width / 2;
                ScaleTransform.CenterY = _actualRenderSize.Height / 2;

                _centerVector.X = -_actualRenderSize.Width / 2;
                _centerVector.Y = -_actualRenderSize.Height / 2;

                UpdateTransform();

                return new Size();
            }

            private void UpdateTransform()
            {
                _translateTransform.X = _centerVector.X + _location.X + _parentCenter.X;
                _translateTransform.Y = _centerVector.Y + _location.Y + _parentCenter.Y;
            }
        }
        #endregion

        #region Nested type: HideAnimationManager
        private class HideAnimationManager
        {
            private readonly int _key;
            private readonly NodeGraph _owner;

            public HideAnimationManager(NodeGraph owner, int key)
            {
                _owner = owner;
                _key = key;
            }

            public void CompletedHandler(object sender, EventArgs args)
            {
#if DEBUG
                _owner.VerifyAccess();
#endif
                _owner.CleanUpGCP(_key);
            }
        }

        #region Static Stuff
        private const double MaxCOD = .999;
        private const double MinCOD = .001;
        private const double MinVelocity = .05;
        private const double TerminalVelocity = 150;

        private static readonly Rect EmptyRect = new Rect();
        private static readonly Duration HideDuration = new Duration(new TimeSpan(0, 0, 1));
        private static readonly Vector HorizontalVector = new Vector(1, 0);
        private static readonly TimeSpan MaxSettleTime = new TimeSpan(0, 0, 8);
        private static readonly Duration ShowDuration = new Duration(new TimeSpan(0, 0, 0, 0, 500));
        private static readonly Vector VerticalVector = new Vector(0, 1);

        private static Binding GetBinding(string bindingPath, object source)
        {
            Binding newBinding = null;
            try
            {
                newBinding = new Binding(bindingPath)
                {
                    Source = source,
                    Mode = BindingMode.OneWay
                };
            }
            catch (InvalidOperationException)
            {
            }
            return newBinding;
        }

        #region static helpers
        private static readonly Random Rnd = new MersenneTwister();

        private static Vector GetVectorSum(int itemIndex, int itemCount, Vector[,] vectors)
        {
            Debug.Assert(itemIndex >= 0);
            Debug.Assert(itemIndex < itemCount);

            Vector vector = new Vector();

            for (int i = 0; i < itemCount; i++)
            {
                if (i != itemIndex)
                {
                    vector += GetVector(itemIndex, i, vectors);
                }
            }

            return vector;
        }

        private static Vector GetVector(int a, int b, Vector[,] vectors)
        {
            Debug.Assert(a != b);
            return a < b ? vectors[a, b] : -vectors[b, a];
        }

        private static Vector GetSpringForce(Vector x)
        {
            Vector force = new Vector();
            //negative is attraction
            force += GetAttractionForce(x);
            //positive is repulsion
            force += GetRepulsiveForce(x);

            Debug.Assert(IsGoodVector(force));

            return force;
        }

        private static Vector GetAttractionForce(Vector x)
        {
            Vector force = -.2 * Normalize(x) * x.Length;
            Debug.Assert(IsGoodVector(force));
            return force;
        }

        private static Vector GetRepulsiveForce(Vector x)
        {
            Vector force = .1 * Normalize(x) / Math.Pow(x.Length / 1000, 2);
            Debug.Assert(IsGoodVector(force));
            return force;
        }

        private static Vector Normalize(Vector v)
        {
            v.Normalize();
            Debug.Assert(IsGoodVector(v));
            return v;
        }

        private static Vector GetWallForce(Size area, Point location)
        {
            Vector force = new Vector();
            force += VerticalVector * GetForce(-location.Y - (area.Height / 2));
            force += -VerticalVector * GetForce(location.Y - (area.Height / 2));

            force += HorizontalVector * GetForce(-location.X - (area.Width / 2));
            force += -HorizontalVector * GetForce(location.X - (area.Width / 2));

            force *= 1000;
            return force;
        }

        private static double GetForce(double x)
        {
            return GetSCurve((x + 100) / 200);
        }

        private static bool IsGoodDouble(double d)
        {
            return !double.IsNaN(d) && !double.IsInfinity(d);
        }

        private static bool IsGoodVector(Vector v)
        {
            return IsGoodDouble(v.X) && IsGoodDouble(v.Y);
        }

        #region math
        private static double GetSCurve(double x)
        {
            return 0.5 + (Math.Sin(Math.Abs(x * (Math.PI / 2)) - Math.Abs((x * (Math.PI / 2)) - (Math.PI / 2))) / 2);
        }
        #endregion

        #endregion

        #endregion

        #endregion
    }
}