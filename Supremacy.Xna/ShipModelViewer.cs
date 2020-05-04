using System;
using System.Concurrency;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Supremacy.Resources;

using Color = Microsoft.Xna.Framework.Graphics.Color;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Point = System.Windows.Point;

namespace Supremacy.Xna
{
    public sealed class ShipModelViewer : XnaComponentRenderer
    {
        private const string ModelDirectory = @"Resources\Models";
        private const string XnaContentExtension = ".xnb";

        // ReSharper disable InconsistentNaming
        
        private readonly object _modelLock = new object();
        private readonly string _workingDirectory;

        private volatile Model _model;

        private Matrix _worldMatrix;
        private Matrix _viewMatrix;
        private Matrix _projectionMatrix;
        private bool _isDragging;
        private Point _lastMousePosition;
        private IDisposable _modelLoadRequest;

        // TODO: Localize these status messages.
        // ReSharper disable ConvertToConstant.Local
        private readonly string _loadFailureMessage = "Error Loading Model";
        private readonly string _modelUnavailableMessage = "No Model Available";
        private readonly string _loadingMessage = "Loading...";
        // ReSharper restore ConvertToConstant.Local

        // ReSharper restore InconsistentNaming

        public ShipModelViewer()
            : base(new XnaComponent(true, true, true))
        {
            _workingDirectory = PathHelper.GetWorkingDirectory();

            MouseLeftButtonDown += OnTargetImageMouseLeftButtonDown;
            MouseLeftButtonUp += OnTargetImageMouseLeftButtonUp;
            MouseMove += OnTargetImageMouseMove;
            LostMouseCapture += OnTargetImageLostMouseCapture;

            Component.LoadedContent += OnLoadedContent;
            Component.UnloadingContent += OnUnloadingContent;
            Component.CustomPresent += OnCustomPresent;
        }

        #region ModelFile Property
        public static readonly DependencyProperty ModelFileProperty = DependencyProperty.Register(
            "ModelFile",
            typeof(string),
            typeof(ShipModelViewer),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender,
                (d, e) => ((ShipModelViewer)d).LoadModel()));

        public string ModelFile
        {
            get => (string)GetValue(ModelFileProperty);
            set => SetValue(ModelFileProperty, value);
        }
        #endregion

        #region StatusMessage Property
        private static readonly DependencyPropertyKey StatusMessagePropertyKey = DependencyProperty.RegisterReadOnly(
            "StatusMessage",
            typeof(string),
            typeof(ShipModelViewer),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StatusMessageProperty = StatusMessagePropertyKey.DependencyProperty;

        public string StatusMessage
        {
            get => (string)GetValue(StatusMessageProperty);
            private set => SetValue(StatusMessagePropertyKey, value);
        }
        #endregion

        #region CameraDistanceMultiplier Property
        public static readonly DependencyProperty CameraDistanceMultiplierProperty = DependencyProperty.Register(
            "CameraDistanceMultiplier",
            typeof(float),
            typeof(ShipModelViewer),
            new FrameworkPropertyMetadata(
                3f,
                FrameworkPropertyMetadataOptions.None));

        public float CameraDistanceMultiplier
        {
            get => (float)GetValue(CameraDistanceMultiplierProperty);
            set => SetValue(CameraDistanceMultiplierProperty, value);
        }
        #endregion

        private void OnLoadedContent(object sender, EventArgs e)
        {
            Vector3 eye = new Vector3(0.0f, 0.0f, -35.0f);
            Vector3 at = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 up = new Vector3(0, 1.0f, 0.0f);

            _worldMatrix = Matrix.Identity;
            _viewMatrix = Matrix.CreateLookAt(eye, at, up);
            _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                (float)Math.PI * 0.25f,
                (float)Component.TargetSize.Width / Component.TargetSize.Height,
                1f,
                1000f);

            LoadModel();
        }

        private void OnUnloadingContent(object sender, EventArgs e)
        {
            lock (_modelLock)
            {
                if (_modelLoadRequest != null)
                {
                    _modelLoadRequest.Dispose();
                    _modelLoadRequest = null;
                }
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            string statusMessage = StatusMessage;

            if (string.IsNullOrEmpty(statusMessage))
            {
                base.OnRender(drawingContext);
                return;
            }

            Int32Rect targetSize = Component.TargetSize;

            FormattedText message = new FormattedText(
                statusMessage,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                24d * 96d / 72d,
                Brushes.White,
                VisualTreeHelper.GetDpi(this).PixelsPerDip)
                          {
                              MaxTextWidth = targetSize.Width,
                              TextAlignment = TextAlignment.Center
                          };

            drawingContext.DrawText(
                message,
                new Point(
                    0,
                    (targetSize.Height - message.Height) / 2d));
        }

        private void OnCustomPresent(object sender, CustomPresentEventArgs e)
        {
            XnaTime time = e.Time;

            if (!_isDragging)
            {
                _worldMatrix *= Matrix.CreateRotationY(-time.ElapsedGameTime.Milliseconds / 1000f);
            }

            Clear();

            Model model;

            lock (_modelLock)
            {
                model = _model;
            }

            if (model != null)
            {
                // Draw the model. A model can have multiple meshes, so loop.
                foreach (ModelMesh mesh in model.Meshes)
                {
                    // This is where the mesh orientation is set, as well 
                    // as our camera and projection.
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        effect.World = _worldMatrix;
                        effect.View = _viewMatrix;
                        effect.Projection = _projectionMatrix;
                    }

                    // Draw the mesh, using the effects set above.
                    mesh.Draw();
                }
            }

            e.Handled = true;
        }

        private void Clear()
        {
            GraphicsDevice device = Component.Graphics.GraphicsDevice;
            if (device == null)
            {
                return;
            }

            device.Clear(
                options: ClearOptions.Target | ClearOptions.DepthBuffer,
                color: Color.TransparentBlack,
                depth: 1.0f,
                stencil: 0);
        }

        private void LoadModel()
        {
            string modelFile = ModelFile;

            lock (_modelLock)
            {
                _model = null;

                if (_modelLoadRequest != null)
                {
                    _modelLoadRequest.Dispose();
                    _modelLoadRequest = null;
                }
            }

            if (string.IsNullOrWhiteSpace(modelFile))
            {
                StatusMessage = _modelUnavailableMessage;
                return;
            }

            try
            {
                modelFile = Path.Combine(ModelDirectory, modelFile);

                if (!Path.HasExtension(modelFile))
                {
                    modelFile += XnaContentExtension;
                }

                modelFile = Path.Combine(
                    _workingDirectory,
                    ResourceManager.GetResourcePath(modelFile));
            }
            catch
            {
                modelFile = null;
            }

            if (modelFile == null ||
                !File.Exists(modelFile))
            {
                StatusMessage = _modelUnavailableMessage;
                return;
            }

            modelFile = modelFile.Substring(
                0,
                modelFile.Length - XnaContentExtension.Length);

            StatusMessage = _loadingMessage;

            Microsoft.Xna.Framework.Content.ContentManager contentManager = Component.Content;
            if (contentManager == null)
            {
                return;
            }

            lock (_modelLock)
            {
                _modelLoadRequest = ((Func<string, Model>)contentManager.Load<Model>)
                    .ToAsync(Scheduler.ThreadPool)(modelFile)
                    .ObserveOnDispatcher()
                    .Subscribe(
                        model =>
                        {
                            float radius = model.Meshes.Max(o => (o.BoundingSphere.Center - Vector3.Zero).Length() + o.BoundingSphere.Radius);
                            float cameraDistance = radius * CameraDistanceMultiplier;

                            Vector3 eye = new Vector3(0.0f, 0.0f, cameraDistance);
                            Vector3 at = new Vector3(0.0f, 0.0f, 0.0f);
                            Vector3 up = new Vector3(0, 1.0f, 0.0f);

                            ClearValue(StatusMessagePropertyKey);

                            lock (_modelLock)
                            {
                                if (_modelLoadRequest == null)
                                {
                                    return;
                                }

                                _modelLoadRequest = null;
                                _model = model;
                                _viewMatrix = Matrix.CreateLookAt(eye, at, up);
                            }
                        },
                        e =>
                        {
                            lock (_modelLock)
                            {
                                if (_modelLoadRequest == null)
                                {
                                    return;
                                }

                                _modelLoadRequest = null;

                                StatusMessage = _loadFailureMessage;
                            }
                        });
            }
        }

        private void OnTargetImageLostMouseCapture(object sender, MouseEventArgs e)
        {
            EndDrag();
        }

        private void OnTargetImageMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging)
            {
                return;
            }

            Point mousePosition = e.GetPosition(this);

            double dX = mousePosition.X - _lastMousePosition.X;
            double dY = mousePosition.Y - _lastMousePosition.Y;

            _lastMousePosition = mousePosition;

            _worldMatrix *= Matrix.CreateRotationX(0.01f * (float)dY);
            _worldMatrix *= Matrix.CreateRotationY(0.01f * (float)dX);
        }

        private void OnTargetImageMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDragging)
            {
                return;
            }

            EndDrag();

            e.Handled = true;
        }

        private void EndDrag()
        {
            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
            }

            _isDragging = false;
            _worldMatrix = Matrix.Identity;
        }

        private void OnTargetImageMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!CaptureMouse())
            {
                return;
            }

            _isDragging = true;
            _lastMousePosition = e.GetPosition(this);

            e.Handled = true;
        }
    }
}