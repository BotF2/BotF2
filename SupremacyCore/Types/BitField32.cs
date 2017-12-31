// BitField32.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Supremacy.Types
{
    /// <summary>Provides a simple structure that stores Boolean values and small integers in 32 bits of memory.</summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct BitField32 : IEquatable<BitField32>
    {
        private uint _data;

        /// <summary>Initializes a new instance of the <see cref="BitField32"/> structure containing the data represented in an integer.</summary>
        /// <param name="data">An integer representing the data of the new <see cref="BitField32"/>.</param>
        public BitField32(int data)
        {
            _data = (uint)data;
        }

        /// <summary>Initializes a new instance of the <see cref="BitField32"/> structure containing the data represented in an existing <see cref="BitField32"/> structure.</summary>
        /// <param name="value">A <see cref="BitField32"/> structure that contains the data to copy.</param>
        public BitField32(BitField32 value)
        {
            _data = value._data;
        }

        /// <summary>Gets or sets the state of the bit flag indicated by the specified mask.</summary>
        /// <returns>true if the specified bit flag is on (1); otherwise, false.</returns>
        /// <param name="bit">A mask that indicates the bit to get or set.</param>
        public bool this[int bit]
        {
            get
            {
                return ((_data & bit) == bit);
            }
            set
            {
                if (value)
                {
                    _data |= (uint)bit;
                }
                else
                {
                    _data &= (uint)~bit;
                }
            }
        }

        /// <summary>Gets or sets the value stored in the specified <see cref="BitField32.Section"/>.</summary>
        /// <returns>The value stored in the specified <see cref="BitField32.Section"/>.</returns>
        /// <param name="section">A <see cref="BitField32.Section"/> that contains the value to get or set.</param>
        public int this[Section section]
        {
            get
            {
                return (int)((_data & (section.Mask << (section.Offset & 0x1f))) >> (section.Offset & 0x1f));
            }
            set
            {
                value = value << section.Offset;
                uint num = (uint)((0xffff & section.Mask) << section.Offset);
                _data = (_data & ~num) | ((uint)value & num);
            }
        }

        /// <summary>Gets the value of the <see cref="BitField32"/> as an integer.</summary>
        /// <returns>The value of the <see cref="BitField32"/> as an integer.</returns>
        public uint Data
        {
            get { return _data; }
            set { _data = value; }
        }

        private static short CountBitsSet(short mask)
        {
            short num = 0;
            while ((mask & 1) != 0)
            {
                num = (short)(num + 1);
                mask = (short)(mask >> 1);
            }
            return num;
        }

        /// <summary>Creates the first mask in a series of masks that can be used to retrieve individual bits in a <see cref="BitField32"/> that is set up as bit flags.</summary>
        /// <returns>A mask that isolates the first bit flag in the <see cref="BitField32"/>.</returns>
        /// <PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" /></PermissionSet>
        public static int CreateMask()
        {
            return CreateMask(0);
        }

        /// <summary>Creates an additional mask following the specified mask in a series of masks that can be used to retrieve individual bits in a <see cref="BitField32"/> that is set up as bit flags.</summary>
        /// <returns>A mask that isolates the bit flag following the one that previous points to in <see cref="BitField32"/>.</returns>
        /// <param name="previous">The mask that indicates the previous bit flag.</param>
        /// <exception cref="T:System.InvalidOperationException">previous indicates the last bit flag in the <see cref="BitField32"/>. </exception>
        public static int CreateMask(int previous)
        {
            if (previous == 0)
            {
                return 1;
            }
            if (previous == -2147483648)
            {
                throw new InvalidOperationException("BitField32 is full");
            }
            return (previous << 1);
        }

        private static short CreateMaskFromHighValue(short highValue)
        {
            short num = 0x10;
            while ((highValue & 0x8000) == 0)
            {
                num = (short)(num - 1);
                highValue = (short)(highValue << 1);
            }
            ushort num2 = 0;
            while (num > 0)
            {
                num = (short)(num - 1);
                num2 = (ushort)(num2 << 1);
                num2 = (ushort)(num2 | 1);
            }
            return (short)num2;
        }

        /// <summary>Creates the first <see cref="BitField32.Section"/> in a series of sections that contain small integers.</summary>
        /// <returns>A <see cref="BitField32.Section"/> that can hold a number from zero to <paramref name="maxValue"/>.</returns>
        /// <param name="maxValue">A 16-bit signed integer that specifies the maximum value for the new <see cref="BitField32.Section"/>.</param>
        /// <exception cref="T:System.ArgumentException"><paramref name="maxValue"/> is less than 1. </exception>
        public static Section CreateSection(short maxValue)
        {
            return CreateSectionHelper(maxValue, 0, 0);
        }

        /// <summary>Creates a new <see cref="BitField32.Section"/> following the specified <see cref="BitField32.Section"/> in a series of sections that contain small integers.</summary>
        /// <returns>A <see cref="BitField32.Section"/> that can hold a number from zero to <paramref name="maxValue"/>.</returns>
        /// <param name="maxValue">A 16-bit signed integer that specifies the maximum value for the new <see cref="BitField32.Section"/>.</param>
        /// <param name="previous">The previous <see cref="BitField32.Section"/> in the <see cref="BitField32"/>.</param>
        /// <exception cref="T:System.ArgumentException"><paramref name="maxValue"/> is less than 1. </exception>
        /// <exception cref="T:System.InvalidOperationException">previous includes the final bit in the <see cref="BitField32"/>.-or- <paramref name="maxValue"/> is greater than the highest value that can be represented by the number of bits after previous. </exception>
        public static Section CreateSection(short maxValue, Section previous)
        {
            return CreateSectionHelper(maxValue, previous.Mask, previous.Offset);
        }

        private static Section CreateSectionHelper(short maxValue, short priorMask, short priorOffset)
        {
            if (maxValue < 1)
            {
                throw new ArgumentException("invalid value", "maxValue");
            }
            short offset = (short)(priorOffset + CountBitsSet(priorMask));
            if (offset >= 0x20)
            {
                throw new InvalidOperationException("BitField32 is full");
            }
            return new Section(CreateMaskFromHighValue(maxValue), offset);
        }

        public bool Equals(BitField32 bitField32)
        {
            return _data == bitField32._data;
        }

        public override int GetHashCode()
        {
            return (int)_data;
        }

        /// <summary>Determines whether the specified object is equal to the <see cref="BitField32"/>.</summary>
        /// <returns>true if the specified object is equal to the <see cref="BitField32"/>; otherwise, false.</returns>
        /// <param name="o">The object to compare with the current <see cref="BitField32"/>.</param>
        public override bool Equals(object o)
        {
            if (o is BitField32)
            {
                return (_data == ((BitField32)o)._data);
            }
            return false;
        }

        /// <summary>Returns a string that represents the specified <see cref="BitField32"/>.</summary>
        /// <returns>A string that represents the specified <see cref="BitField32"/>.</returns>
        /// <param name="value">The <see cref="BitField32"/> to represent.</param>
        public static string ToString(BitField32 value)
        {
            StringBuilder builder = new StringBuilder(0x2d);
            builder.Append("BitField32{");
            int data = (int)value._data;
            for (int i = 0; i < 0x20; i++)
            {
                if ((data & 0x80000000) != 0)
                {
                    builder.Append("1");
                }
                else
                {
                    builder.Append("0");
                }
                data = data << 1;
            }
            builder.Append("}");
            return builder.ToString();
        }

        /// <summary>Returns a string that represents the current <see cref="BitField32"/>.</summary>
        /// <returns>A string that represents the current <see cref="BitField32"/>.</returns>
        public override string ToString()
        {
            return ToString(this);
        }

        /// <summary>Represents a section of the vector that can contain an integer number.</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Section
        {
            private readonly short _mask;
            private readonly short _offset;

            internal Section(short mask, short offset)
            {
                _mask = mask;
                _offset = offset;
            }

            /// <summary>Gets a mask that isolates this section within the <see cref="BitField32"/>.</summary>
            /// <returns>A mask that isolates this section within the <see cref="BitField32"/>.</returns>
            public short Mask
            {
                get { return _mask; }
            }

            /// <summary>Gets the offset of this section from the start of the <see cref="BitField32"/>.</summary>
            /// <returns>The offset of this section from the start of the <see cref="BitField32"/>.</returns>
            public short Offset
            {
                get { return _offset; }
            }

            public override int GetHashCode()
            {
                return (_mask << _offset);
            }

            /// <summary>Determines whether the specified object is the same as the current <see cref="BitField32.Section"/> object.</summary>
            /// <returns>true if the specified object is the same as the current <see cref="BitField32.Section"/> object; otherwise, false.</returns>
            /// <param name="o">The object to compare with the current <see cref="BitField32.Section"/>.</param>
            public override bool Equals(object o)
            {
                if (o is Section)
                {
                    return Equals((Section)o);
                }
                return false;
            }

            /// <summary>Determines whether the specified <see cref="BitField32.Section"/> object is the same as the current <see cref="BitField32.Section"/> object.</summary>
            /// <returns>true if the obj parameter is the same as the current <see cref="BitField32.Section"/> object; otherwise false.</returns>
            /// <param name="obj">The <see cref="BitField32.Section"/> object to compare with the current <see cref="BitField32.Section"/> object.</param>
            public bool Equals(Section obj)
            {
                if (obj._mask == _mask)
                {
                    return (obj._offset == _offset);
                }
                return false;
            }

            /// <summary>Determines whether two specified <see cref="BitField32.Section"/> objects are equal.</summary>
            /// <returns>true if the a and b parameters represent the same <see cref="BitField32.Section"/> object, otherwise, false.</returns>
            /// <param name="a">A <see cref="BitField32.Section"/> object.</param>
            /// <param name="b">A <see cref="BitField32.Section"/> object.</param>
            public static bool operator ==(Section a, Section b)
            {
                return a.Equals(b);
            }

            /// <summary>Determines whether two <see cref="BitField32.Section"/> objects have different values.</summary>
            /// <returns>true if the a and b parameters represent different <see cref="BitField32.Section"/> objects; otherwise, false.</returns>
            /// <param name="a">A <see cref="BitField32.Section"/> object.</param>
            /// <param name="b">A <see cref="BitField32.Section"/> object.</param>
            public static bool operator !=(Section a, Section b)
            {
                return !(a == b);
            }

            /// <summary>Returns a string that represents the specified <see cref="BitField32.Section"/>.</summary>
            /// <returns>A string that represents the specified <see cref="BitField32.Section"/>.</returns>
            /// <param name="value">The <see cref="BitField32.Section"/> to represent.</param>
            public static string ToString(Section value)
            {
                return ("Section{0x" + Convert.ToString(value.Mask, 0x10) + ", 0x" + Convert.ToString(value.Offset, 0x10) + "}");
            }

            /// <summary>Returns a string that represents the current <see cref="BitField32.Section"/>.</summary>
            /// <returns>A string that represents the current <see cref="BitField32.Section"/>.</returns>
            public override string ToString()
            {
                return ToString(this);
            }
        }
    }
}
