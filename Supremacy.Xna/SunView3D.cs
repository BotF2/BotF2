using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Supremacy.Universe;

using Color = Microsoft.Xna.Framework.Graphics.Color;
using Matrix = Microsoft.Xna.Framework.Matrix;

namespace Supremacy.Xna
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class SunView3D : XnaComponent
    {

        private Matrix _projectionMatrix;
        private RenderTarget2D _tempBuffer;
        private float _dt;
        private PostProcessor _postProcessor;
        private bool _usePostProcessor;
        private Sun _sun;



        public SunView3D(StarType starType)
            : base(false, false, false, new Int32Rect(0, 0, 128, 128), ExitRunScopeBehavior.Suspend)
        {
            StarType = starType;
        }

        public StarType StarType { get; }

        protected override void CreateBuffers()
        {
            base.CreateBuffers();

            GraphicsDevice device = Graphics.GraphicsDevice;

            _tempBuffer?.Dispose();

            _tempBuffer = new RenderTarget2D(
                device,
                TargetSize.Width,
                TargetSize.Height,
                1,
                device.PresentationParameters.BackBufferFormat,
                device.PresentationParameters.MultiSampleType,
                device.PresentationParameters.MultiSampleQuality);
        }

        protected override void DisposeBuffers()
        {
            base.DisposeBuffers();

            _tempBuffer?.Dispose();

            _tempBuffer = null;
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                (float)Math.PI * 0.25f,
                (float)TargetSize.Width / TargetSize.Height,
                0.05f,
                1000f);

            Camera camera = new Camera(_projectionMatrix) { ViewPosition = new Vector3(-.0f, .0f, 1.2f) };
            GraphicsDevice device = Graphics.GraphicsDevice;

            _sun = new Sun(StarType);
            _sun.Create(Graphics, Content, camera);

            _usePostProcessor = device.GraphicsDeviceCapabilities.DeviceType == DeviceType.Hardware &&
                                device.GraphicsDeviceCapabilities.MaxPixelShaderProfile >= ShaderProfile.PS_3_0;

            if (_usePostProcessor)
            {
                _postProcessor = new PostProcessor(device, Content);
            }
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();

            _sun?.Dispose();

            _sun = null;

            _postProcessor?.Dispose();

            _postProcessor = null;
        }

        protected override void Present(XnaTime time)
        {
            int milliseconds = time.ElapsedGameTime.Milliseconds;

            _dt = milliseconds / 1000.0f;
            _sun.Update(time.ElapsedGameTime);

            GraphicsDevice device = Graphics.GraphicsDevice;

            if (_usePostProcessor)
            {
                device.SetRenderTarget(0, _tempBuffer);

                device.Clear(
                    options: ClearOptions.Target,
                    color: Color.TransparentBlack,
                    depth: 1.0f,
                    stencil: 0);

                _sun.Render();
                _postProcessor.ToneMap(_tempBuffer, BackBuffer, _dt, false, true);
                device.SetRenderTarget(0, BackBuffer);
            }
            else
            {
                device.Clear(
                    options: ClearOptions.Target,
                    color: Color.TransparentBlack,
                    depth: 1.0f,
                    stencil: 0);

                _sun.Render();
            }
        }
    }

    public static class SunViews
    {
        private static readonly Dictionary<StarType, SunView3D> _views = new Dictionary<StarType, SunView3D>();

        public static SunView3D GetView(StarType starType)
        {
            if (starType >= StarType.Nebula)
            {
                return null;
            }

            if (!_views.TryGetValue(starType, out SunView3D sunView))
            {
                sunView = new SunView3D(starType);
                _views[starType] = sunView;
            }

            return sunView;
        }
    }

    public sealed class SunView3DRenderer : XnaComponentRenderer
    {
        public SunView3DRenderer()
        {
            Width = 128;
            Height = 128;
        }

        public new SunView3D Component
        {
            get => base.Component as SunView3D;
            set => base.Component = value;
        }

        protected override bool ManageComponentTargetSize => false;

        #region StarType Property

        public static readonly DependencyProperty StarTypeProperty = DependencyProperty.Register(
            "StarType",
            typeof(StarType?),
            typeof(SunView3DRenderer),
            new PropertyMetadata(
                null,
                (o, args) => ((SunView3DRenderer)o).OnStarTypeChanged((StarType?)args.NewValue)));

        private void OnStarTypeChanged(StarType? newValue)
        {
            Component = newValue.HasValue ? SunViews.GetView(newValue.Value) : null;
        }

        public StarType? StarType
        {
            get => (StarType?)GetValue(StarTypeProperty);
            set => SetValue(StarTypeProperty, value);
        }

        #endregion

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.PushClip(new EllipseGeometry(new Rect(RenderSize)));
            base.OnRender(drawingContext);
            drawingContext.Pop();
        }
    }
}
