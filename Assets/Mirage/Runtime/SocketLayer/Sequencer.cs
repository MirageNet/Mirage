/*
The MIT License (MIT)

Copyright (c) 2020 Fredrik Holmstrom
Copyright (c) 2020 Paul Pacheco
Copyright (c) 2021 James Frowen

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// A sequence generator that can wrap.
    /// For example a 2 bit sequencer would generate
    /// the following numbers:
    /// <code>
    ///     0,1,2,3,0,1,2,3,0,1,2,3...
    /// </code>
    /// </summary>
    /// <example>
    /// <code>
    /// // create a 8 bit sequence generator
    /// Sequencer sequencer = new Sequencer(8);
    ///
    /// ulong zero = sequencer.Next();
    /// ulong one = sequencer.Next();
    /// ...
    /// ulong n = sequencer.Next();
    /// 
    /// // you can determine the distance between 2 sequences
    /// // as long as they are withing 1/2 of the sequence space
    ///
    /// // this is equivalent to a - b adjusted for wrapping
    /// int d = sequencer.Distance(a, b);
    /// </code>
    /// </example>
    public class Sequencer
    {
        private readonly int _shift;
        private readonly int _bits;
        private readonly ulong _mask;
        private ulong _sequence;

        /// <summary>
        /// Number of bits used for the sequence generator up to 64
        /// </summary>
        public int Bits => _bits;

        /// <param name="bits">amount of bits for the sequence</param>
        public Sequencer(int bits)
        {
            if (bits <= 0 || bits > 63) throw new ArgumentOutOfRangeException(nameof(bits), bits, "Bits should be between 1 and 63");

            // todo add context to the below comment

            // 1 byte
            // (1 << 8) = 256
            // - 1      = 255
            //          = 1111 1111

            _bits = bits;
            _sequence = 0;
            _mask = (1UL << bits) - 1UL;
            _shift = (sizeof(ulong) * 8) - bits;
        }

        /// <summary>
        /// Generates the next value in the sequence
        /// <para>starts with 0</para>
        /// </summary>
        /// <returns>0, 1, 2, ..., (2^n)-1, 0, 1, 2, ...</returns>
        public ulong Next()
        {
            var current = _sequence;
            _sequence = NextAfter(_sequence);
            return current;
        }

        /// <summary>
        /// Gets the next sequence value after a given sequence
        /// wraps if necessary
        /// </summary>
        /// <param name="sequence">current sequence value</param>
        /// <returns>the next sequence value</returns>
        public ulong NextAfter(ulong sequence)
        {
            return (sequence + 1UL) & _mask;
        }

        /// <summary>
        /// returns a sequence value from the given value
        /// wraps if necessary
        /// </summary>
        /// <param name="sequence">current sequence value</param>
        /// <returns>the next sequence value</returns>
        public ulong MoveInBounds(ulong sequence)
        {
            return (sequence) & _mask;
        }

        /// <summary>
        /// Calculates the distance between 2 sequences, taking into account
        /// wrapping
        /// </summary>
        /// <param name="from">current sequence value</param>
        /// <param name="to">previous sequence value</param>
        /// <returns>from - to, adjusted for wrapping</returns>
        public long Distance(ulong from, ulong to)
        {
            to <<= _shift;
            from <<= _shift;
            return ((long)(from - to)) >> _shift;
        }
    }
}
