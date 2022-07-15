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
    public sealed class Vector3Packer
    {
        private readonly FloatPacker _xPacker;
        private readonly FloatPacker _yPacker;
        private readonly FloatPacker _zPacker;

        public Vector3Packer(float xMax, float yMax, float zMax, int xBitCount, int yBitCount, int zBitCount)
        {
            _xPacker = new FloatPacker(xMax, xBitCount);
            _yPacker = new FloatPacker(yMax, yBitCount);
            _zPacker = new FloatPacker(zMax, zBitCount);
        }
        public Vector3Packer(float xMax, float yMax, float zMax, float xPrecision, float yPrecision, float zPrecision)
        {
            _xPacker = new FloatPacker(xMax, xPrecision);
            _yPacker = new FloatPacker(yMax, yPrecision);
            _zPacker = new FloatPacker(zMax, zPrecision);
        }
        public Vector3Packer(Vector3 max, Vector3 precision)
        {
            _xPacker = new FloatPacker(max.x, precision.x);
            _yPacker = new FloatPacker(max.y, precision.y);
            _zPacker = new FloatPacker(max.z, precision.z);
        }

        public void Pack(NetworkWriter writer, Vector3 value)
        {
            _xPacker.Pack(writer, value.x);
            _yPacker.Pack(writer, value.y);
            _zPacker.Pack(writer, value.z);
        }

        public Vector3 Unpack(NetworkReader reader)
        {
            Vector3 value = default;
            value.x = _xPacker.Unpack(reader);
            value.y = _yPacker.Unpack(reader);
            value.z = _zPacker.Unpack(reader);
            return value;
        }
    }
}
