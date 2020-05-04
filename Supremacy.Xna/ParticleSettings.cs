using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Supremacy.Xna
{
    /// <summary>
    /// Settings class describes all the tweakable options used
    /// to control the appearance of a particle system.
    /// </summary>
    public class ParticleSettings
    {
        // How long these particles will last.
        private TimeSpan duration = TimeSpan.FromSeconds(1);

        // Direction and strength of the gravity effect. Note that this can point in any
        // direction, not just down! The fire effect points it upward to make the flames
        // rise, and the smoke plume points it sideways to simulate wind.
        private Vector3 gravity = Vector3.Zero;

        // Range of values controlling the particle color and alpha. Values for
        // individual particles are randomly chosen from somewhere between these limits.
        private Color minColor = Color.White;
        private Color maxColor = Color.White;

        public string TextureName { get; set; } = null;
        public int MaxParticles { get; set; } = 100;
        public TimeSpan Duration { get => duration; set => duration = value; }
        public float DurationRandomness { get; set; } = 0;
        public float EmitterVelocitySensitivity { get; set; } = 1;
        public float MinHorizontalVelocity { get; set; } = 0;
        public float MaxHorizontalVelocity { get; set; } = 0;
        public float MinVerticalVelocity { get; set; } = 0;
        public float MaxVerticalVelocity { get; set; } = 0;
        public Vector3 Gravity { get => gravity; set => gravity = value; }
        public float EndVelocity { get; set; } = 1;
        public Color MinColor { get => minColor; set => minColor = value; }
        public Color MaxColor { get => maxColor; set => maxColor = value; }
        public float MinRotateSpeed { get; set; } = 0;
        public float MaxRotateSpeed { get; set; } = 0;
        public float MinStartSize { get; set; } = 100;
        public float MaxStartSize { get; set; } = 100;
        public float MinEndSize { get; set; } = 100;
        public float MaxEndSize { get; set; } = 100;
        public Blend SourceBlend { get; set; } = Blend.SourceAlpha;
        public Blend DestinationBlend { get; set; } = Blend.InverseSourceAlpha;
    }
}
