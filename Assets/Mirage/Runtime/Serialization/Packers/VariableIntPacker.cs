/*
MIT License

Copyright (c) 2021 James Frowen

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Runtime.CompilerServices;

namespace Mirage.Serialization
{
    public sealed class VariableIntPacker
    {
        // todo needs doc comments
        // todo neeeds tests

        readonly int smallBitCount;
        readonly int mediumBitsCount;
        readonly int largeBitsCount;

        readonly ulong smallValue;
        readonly ulong mediumValue;
        readonly ulong largeValue;

        readonly bool throwIfOverLarge;

        public VariableIntPacker(ulong smallValue, ulong mediumValue)
            : this(smallValue, mediumValue, ulong.MaxValue, false) { }
        public VariableIntPacker(ulong smallValue, ulong mediumValue, ulong largeValue, bool throwIfOverLarge = true)
            : this(BitHelper.BitCount(smallValue), BitHelper.BitCount(mediumValue), BitHelper.BitCount(largeValue), throwIfOverLarge) { }

        public static VariableIntPacker FromBitCount(int smallBits, int mediumBits)
            => FromBitCount(smallBits, mediumBits, 64, false);
        public static VariableIntPacker FromBitCount(int smallBits, int mediumBits, int largeBits, bool throwIfOverLarge)
            => new VariableIntPacker(smallBits, mediumBits, largeBits, throwIfOverLarge);

        private VariableIntPacker(int smallBits, int mediumBits, int largeBits, bool throwIfOverLarge)
        {
            this.throwIfOverLarge = throwIfOverLarge;
            if (smallBits == 0) throw new ArgumentException();
            if (smallBits >= mediumBits) throw new ArgumentException();
            if (mediumBits >= largeBits) throw new ArgumentException();
            if (largeBits > 64) throw new ArgumentException();
            // force medium to also be 62 or less so we can use 1 write call (2 bits to say its medium + 62 value bits
            if (mediumBits > 62) throw new ArgumentException();
            if (smallBits > 62) throw new ArgumentException();

            smallBitCount = smallBits;
            mediumBitsCount = mediumBits;
            largeBitsCount = largeBits;

            // mask is also max value for n bits
            smallValue = BitMask.Mask(smallBits);
            mediumValue = BitMask.Mask(mediumBits);
            largeValue = BitMask.Mask(largeBits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PackUlong(NetworkWriter writer, ulong value)
        {
            if (value <= smallValue)
            {
                // start with b0 to say small, then value
                writer.Write(value << 1, smallBitCount + 1);
            }
            else if (value <= mediumValue)
            {
                // start with b01 to say medium, then value
                writer.Write(value << 2 | 0b01, mediumBitsCount + 2);
            }
            else if (value <= largeValue)
            {
                // start with b11 to say large, then value
                // use 2 write calls here because bitCount could be 64
                writer.Write(0b11, 2);
                writer.Write(value, largeBitsCount);
            }
            else
            {
                if (throwIfOverLarge)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, $"Value is over max of {largeValue}");
                }
                else
                {
                    // if no throw write MaxValue
                    // we dont want to write value here because it will be masked and lose some high bits
                    // need 2 write calls here because max is 64+2 bits
                    writer.Write(0b11, 2);
                    writer.Write(ulong.MaxValue, largeBitsCount);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong UnpackUlong(NetworkReader reader)
        {
            if (!reader.ReadBoolean())
            {
                return reader.Read(smallBitCount);
            }
            else
            {
                if (!reader.ReadBoolean())
                {
                    return reader.Read(mediumBitsCount);
                }
                else
                {
                    return reader.Read(largeBitsCount);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PackUint(NetworkWriter writer, uint value)
        {
            // todo do we need to check that bitcount are less than 32, because uint has max of 32 (maybe we can validate this with attribute?)
            if (value <= smallValue)
            {
                // start with b0 to say small, then value
                writer.Write(value << 1, smallBitCount + 1);
            }
            else if (value <= mediumValue)
            {
                // start with b01 to say medium, then value
                writer.Write(value << 2 | 0b01, mediumBitsCount + 2);
            }
            else if (value <= largeValue)
            {
                // start with b11 to say large, then value
                // can use 1 write call here because value less be at most 32 bits
                writer.Write(((ulong)value) << 1 | 0b11, largeBitsCount);
            }
            else
            {
                if (throwIfOverLarge)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, $"Value is over max of {largeValue}");
                }
                else
                {
                    // if no throw write MaxValue
                    // we dont want to write value here because it will be masked and lose some high bits
                    // can do 1 write call here because max is 32+2 bits
                    writer.Write(uint.MaxValue, largeBitsCount + 2);
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint UnpackUint(NetworkReader reader)
        {
            return (uint)UnpackUlong(reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PackUshort(NetworkWriter writer, ushort value)
        {
            // todo do we need to check bitcount here?, comment same as uint
            PackUint(writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort UnpackUshort(NetworkReader reader)
        {
            return (ushort)UnpackUlong(reader);
        }
    }
}
