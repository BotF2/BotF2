using System;
using Microsoft.Xna.Framework;

using Supremacy.Utility;

namespace Supremacy.Xna
{
    /// <summary>
    /// This class demonstrates how to combine several different particle systems
    /// to build up a more sophisticated composite effect. It implements a rocket
    /// projectile, which arcs up into the sky using a ParticleEmitter to leave a
    /// steady stream of trail particles behind it. After a while it explodes,
    /// creating a sudden burst of explosion and smoke particles.
    /// </summary>
    class Explosion
    {
        #region Constants

        private const int numExplosionParticles = 30;
        private const float projectileLifespan = 0f;

        #endregion

        #region Fields

        private readonly ParticleSystem _explosionParticles;

        private Vector3 _position;
        private readonly Vector3 _velocity = new Vector3();
        private float _age;

        #endregion


        /// <summary>
        /// Constructs a new projectile.
        /// </summary>
        public Explosion(ParticleSystem explosionParticles)
        {
            _explosionParticles = explosionParticles;

            // Start at the origin, firing in a random (but roughly upward) direction.
            _position = Vector3.Zero;

            double r = RandomProvider.Shared.NextDouble() * 100;
            double theta = RandomProvider.Shared.NextDouble() * MathHelper.TwoPi;

            _position.X = (float)(r * Math.Cos(theta));
            _position.Y = (float)(r * Math.Sin(theta));
            _position.Z = (float)r;
        }


        /// <summary>
        /// Updates the projectile.
        /// </summary>
        public bool Update(XnaTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Simple projectile physics.
            _position += _velocity * elapsedTime;
            _age += elapsedTime;

            // If enough time has passed, explode! Note how we pass our velocity
            // in to the AddParticle method: this lets the explosion be influenced
            // by the speed and direction of the projectile which created it.
            if (_age > projectileLifespan)
            {
                for (int i = 0; i < numExplosionParticles; i++)
                {
                    _explosionParticles.AddParticle(_position, Vector3.Zero);
                }

                return false;
            }
                
            return true;
        }
    }
}
