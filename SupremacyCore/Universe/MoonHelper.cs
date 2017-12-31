// MoonHelper.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

namespace Supremacy.Universe
{
    /// <summary>
    /// Helper class for converting between a <see cref="MoonType"/> and the corresponding
    /// <see cref="MoonSize"/> and <see cref="MoonShape"/> values.
    /// </summary>
    public static class MoonHelper
    {
        /// <summary>
        /// Gets the <see cref="MoonSize"/> from a <see cref="MoonType"/>.
        /// </summary>
        /// <param name="moonType">The <see cref="MoonType"/>.</param>
        /// <returns>The <see cref="MoonSize"/></returns>
        public static MoonSize GetSize(this MoonType moonType)
        {
            return (MoonSize)((byte)moonType & 0x3);
        }

        /// <summary>
        /// Gets the <see cref="MoonShape"/> from a <see cref="MoonType"/>.
        /// </summary>
        /// <param name="moonType">The <see cref="MoonType"/>.</param>
        /// <returns>The <see cref="MoonShape"/></returns>
        public static MoonShape GetShape(this MoonType moonType)
        {
            return (MoonShape)(((byte)moonType & 0xC) >> 2);
        }

        /// <summary>
        /// Gets the <see cref="MoonType"/> from a <see cref="MoonSize"/> and <see cref="MoonShape"/>.
        /// </summary>
        /// <param name="size">The <see cref="MoonSize"/>.</param>
        /// <param name="shape">The <see cref="MoonShape"/>.</param>
        /// <returns>The <see cref="MoonType"/>.</returns>
        public static MoonType GetType(this MoonSize size, MoonShape shape)
        {
            return (MoonType)((byte)size | ((byte)shape << 2));
        }
    }
}
