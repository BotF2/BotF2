using System;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Supremacy.Xna
{
    /// <summary>
    /// Custom particle system for creating the fiery part of the explosions.
    /// </summary>
    class ExplosionParticleSystem : ParticleSystem
    {
        public ExplosionParticleSystem(GraphicsDevice graphicsDevice, ContentManager content)
            : base(graphicsDevice, content) { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = @"Resources\Textures\explosion";

            settings.MaxParticles = 2400;

            settings.Duration = TimeSpan.FromSeconds(2);
            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = 2;
            settings.MaxHorizontalVelocity = 2;

            settings.MinVerticalVelocity = -2;
            settings.MaxVerticalVelocity = 2;

            settings.EndVelocity = 0;

            settings.MinColor = Color.DarkGray;
            settings.MaxColor = Color.Gray;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 10;
            settings.MaxStartSize = 10;

            settings.MinEndSize = 20;
            settings.MaxEndSize = 40;

            // Use additive blending.
            settings.SourceBlend = Blend.SourceAlpha;
            settings.DestinationBlend = Blend.One;
        }
    }
}
