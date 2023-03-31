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
    /// Packs a vector3 using <see cref="ZigZag"/> and <see cref="VarIntBlocksPacker"/>
    /// </summary>
    public sealed class VarVector2Packer
    {
        private readonly VarFloatPacker _xPacker;
        private readonly VarFloatPacker _yPacker;

        public VarVector2Packer(Vector2 precision, int blocksize)
        {
            _xPacker = new VarFloatPacker(precision.x, blocksize);
            _yPacker = new VarFloatPacker(precision.y, blocksize);
        }

        public void Pack(NetworkWriter writer, Vector2 position)
        {
            _xPacker.Pack(writer, position.x);
            _yPacker.Pack(writer, position.y);
        }

        public Vector2 Unpack(NetworkReader reader)
        {
            Vector2 value = default;
            value.x = _xPacker.Unpack(reader);
            value.y = _yPacker.Unpack(reader);
            return value;
        }
    }
}
