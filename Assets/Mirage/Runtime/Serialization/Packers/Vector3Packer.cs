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
        readonly FloatPacker xPacker;
        readonly FloatPacker yPacker;
        readonly FloatPacker zPacker;

        public Vector3Packer(float xMax, float yMax, float zMax, int xBitCount, int yBitCount, int zBitCount)
        {
            xPacker = new FloatPacker(xMax, xBitCount);
            yPacker = new FloatPacker(yMax, yBitCount);
            zPacker = new FloatPacker(zMax, zBitCount);
        }
        public Vector3Packer(float xMax, float yMax, float zMax, float xPrecision, float yPrecision, float zPrecision)
        {
            xPacker = new FloatPacker(xMax, xPrecision);
            yPacker = new FloatPacker(yMax, yPrecision);
            zPacker = new FloatPacker(zMax, zPrecision);
        }
        public Vector3Packer(Vector3 max, Vector3 precision)
        {
            xPacker = new FloatPacker(max.x, precision.x);
            yPacker = new FloatPacker(max.y, precision.y);
            zPacker = new FloatPacker(max.z, precision.z);
        }

        public void Pack(NetworkWriter writer, Vector3 value)
        {
            xPacker.Pack(writer, value.x);
            yPacker.Pack(writer, value.y);
            zPacker.Pack(writer, value.z);
        }

        public Vector3 Unpack(NetworkReader reader)
        {
            Vector3 value = default;
            value.x = xPacker.Unpack(reader);
            value.y = yPacker.Unpack(reader);
            value.z = zPacker.Unpack(reader);
            return value;
        }
    }
}
