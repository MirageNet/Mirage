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
    public static class VarIntBlocksPacker
    {
        // todo needs doc comments
        // todo neeeds tests

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pack(NetworkWriter writer, ulong value, int blockSize)
        {
            // always writes atleast 1 block
            int count = 1;
            ulong checkValue = value >> blockSize;
            while (checkValue != 0)
            {
                count++;
                checkValue >>= blockSize;
            }
            // count = 1, write = b0, (1<<(1-1) -1 => 1<<0 -1) => 1 -1 => 0)
            // count = 2, write = b01
            // count = 3, write = b011, (1<<(3-1) -1 => 1<<2 -1) => 100 - 1 => 011)
            writer.Write((1ul << (count - 1)) - 1, count);
            writer.Write(value, Math.Min(64, blockSize * count));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Unpack(NetworkReader reader, int blockSize)
        {
            int blocks = 1;
            // read bits till we see a zero
            while (reader.ReadBoolean())
            {
                blocks++;
            }

            return reader.Read(Math.Min(64, blocks * blockSize));
        }
    }
}
