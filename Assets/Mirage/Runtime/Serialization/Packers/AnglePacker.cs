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

using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mirage.Serialization
{
    public sealed class AnglePacker
    {
        private FloatPacker _floatPacker;

        /// <param name="lowestPrecision">lowest precision, actual precision will be caculated from number of bits used</param>
        public AnglePacker(float lowestPrecision)
        {
            _floatPacker = new FloatPacker(180, lowestPrecision, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Pack(float value)
        {
            value = Mathf.DeltaAngle(0, value);
            return _floatPacker.PackNoClamp(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(NetworkWriter writer, float value)
        {
            value = Mathf.DeltaAngle(value, 0);
            _floatPacker.PackNoClamp(writer, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Unpack(uint value)
        {
            return _floatPacker.Unpack(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Unpack(NetworkReader reader)
        {
            return _floatPacker.Unpack(reader);
        }
    }
}
