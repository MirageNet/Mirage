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

using UnityEngine;

namespace Mirage.Serialization
{
    /// <summary>
    /// Packs a float using <see cref="ZigZag"/> and <see cref="VarIntBlocksPacker"/>
    /// </summary>
    public sealed class VarFloatPacker
    {
        private readonly int _blockSize;
        private readonly float _precision;
        private readonly float _inversePrecision;

        public VarFloatPacker(float precision, int blockSize)
        {
            _precision = precision;
            _blockSize = blockSize;
            _inversePrecision = 1 / precision;
        }

        public void Pack(NetworkWriter writer, float value)
        {
            var scaled = Mathf.RoundToInt(value * _inversePrecision);
            var zig = ZigZag.Encode(scaled);
            VarIntBlocksPacker.Pack(writer, zig, _blockSize);
        }

        public float Unpack(NetworkReader reader)
        {
            var zig = (uint)VarIntBlocksPacker.Unpack(reader, _blockSize);
            var scaled = ZigZag.Decode(zig);
            return scaled * _precision;
        }
    }
}
