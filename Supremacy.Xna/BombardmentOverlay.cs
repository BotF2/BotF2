using System;
using System.Collections.Generic;
using System.Windows;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Color = Microsoft.Xna.Framework.Graphics.Color;
using Matrix = Microsoft.Xna.Framework.Matrix;

namespace Supremacy.Xna
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class BombardmentOverlay : XnaComponent
    {
        // This sample uses five different particle systems.
        private ParticleSystem _explosionParticles;

        // The explosions effect works by firing projectiles up into the
        // air, so we need to keep track of all the active projectiles.
        private readonly List<Explosion> _projectiles = new List<Explosion>();

        private TimeSpan _timeToNextProjectile = TimeSpan.Zero;

        public BombardmentOverlay()
            : base(true, false, false) { }

        protected internal TimeSpan TimeToNextProjectile
        {
            get { return _timeToNextProjectile; }
            set { _timeToNextProjectile = value; }
        }

        protected override void Initialize()
        {
            Graphics.MinimumPixelShaderProfile = ShaderProfile.PS_2_0;
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _explosionParticles = new ExplosionParticleSystem(Graphics.GraphicsDevice, Content);
            _explosionParticles.Initialize();
            _explosionParticles.LoadContent();

            _timeToNextProjectile = TimeSpan.Zero;
        }

        protected override void UnloadContent()
        {
            if (_explosionParticles != null)
            {
                _explosionParticles.Dispose();
                _explosionParticles = null;
            }

            base.UnloadContent();
        }

        protected override void Update(XnaTime gameTime)
        {
            UpdateExplosions(gameTime);
            UpdateProjectiles(gameTime);

            _explosionParticles.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Present(XnaTime time)
        {
            var device = Graphics.GraphicsDevice;

            Clear();

            // Compute camera matrices.
            var aspectRatio = (float)device.Viewport.Width / device.Viewport.Height;

            var view = Matrix.CreateLookAt(
                new Vector3(0, 0, -200),
                new Vector3(0, 0, 0),
                Vector3.Up);

            var projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                aspectRatio,
                1,
                10000);

            // Pass camera matrices through to the particle system components.
            _explosionParticles.SetCamera(view, projection);
            _explosionParticles.Draw(time);
        }

        private void Clear()
        {
            var device = Graphics.GraphicsDevice;
            if (device == null)
                return;

            device.Clear(
                options: ClearOptions.Target | ClearOptions.DepthBuffer,
                color: Color.TransparentBlack,
                depth: 1.0f,
                stencil: 0);
        }

        #region GenerateExplosions Property

        private bool _generateExplosions;

        public bool GenerateExplosions
        {
            get { return _generateExplosions; }
            set
            {
                if (value == _generateExplosions)
                    return;

                _generateExplosions = value;
                _timeToNextProjectile = TimeSpan.Zero;
            }
        }

        #endregion

        #region ExplosionInterval Property

        public TimeSpan? ExplosionInterval { get; set; }

        #endregion

        /// <summary>
        /// Helper for updating the explosions effect.
        /// </summary>
        void UpdateExplosions(XnaTime gameTime)
        {
            var explosionInterval = ExplosionInterval;
            if (explosionInterval == null)
                return;

            _timeToNextProjectile -= gameTime.ElapsedGameTime;

            if (_timeToNextProjectile <= TimeSpan.Zero)
            {
                // Create a new projectile once per second. The real work of moving
                // and creating particles is handled inside the Projectile class.
                _projectiles.Add(new Explosion(_explosionParticles));

                _timeToNextProjectile += explosionInterval.Value;
            }
        }


        /// <summary>
        /// Helper for updating the list of active projectiles.
        /// </summary>
        void UpdateProjectiles(XnaTime gameTime)
        {
            int i = 0;

            while (i < _projectiles.Count)
            {
                if (!_projectiles[i].Update(gameTime))
                {
                    // Remove projectiles at the end of their life.
                    _projectiles.RemoveAt(i);
                }
                else
                {
                    // Advance to the next projectile.
                    i++;
                }
            }
        }
    }

    public sealed class BombardmentOverlayRenderer : XnaComponentRenderer
    {
        public BombardmentOverlayRenderer()
            : base(new BombardmentOverlay()) { }

        public new BombardmentOverlay Component
        {
            get { return base.Component as BombardmentOverlay; }
            set { base.Component = value; }
        }

        #region GenerateExplosions Property

        public static readonly DependencyProperty GenerateExplosionsProperty = DependencyProperty.Register(
            "GenerateExplosions",
            typeof(bool),
            typeof(BombardmentOverlayRenderer),
            new PropertyMetadata((d, e) => ((BombardmentOverlayRenderer)d).Component.GenerateExplosions = (bool)e.NewValue));

        public bool GenerateExplosions
        {
            get { return (bool)GetValue(GenerateExplosionsProperty); }
            set { SetValue(GenerateExplosionsProperty, value); }
        }

        #endregion

        #region ExplosionInterval Property

        public static readonly DependencyProperty ExplosionIntervalProperty = DependencyProperty.Register(
            "ExplosionInterval",
            typeof(TimeSpan?),
            typeof(BombardmentOverlayRenderer),
            new PropertyMetadata((d, e) => ((BombardmentOverlayRenderer)d).Component.ExplosionInterval = (TimeSpan?)e.NewValue));

        public TimeSpan? ExplosionInterval
        {
            get { return (TimeSpan?)GetValue(ExplosionIntervalProperty); }
            set { SetValue(ExplosionIntervalProperty, value); }
        }

        #endregion
    }
}
