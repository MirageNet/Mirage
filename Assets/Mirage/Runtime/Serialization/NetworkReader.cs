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
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mirage.Serialization
{
    /// <summary>
    /// Bit writer, writes values to a buffer on a bit level
    /// <para>Use <see cref="NetworkReaderPool.GetReader"/> to reduce memory allocation</para>
    /// </summary>
    public unsafe class NetworkReader : IDisposable
    {
        private byte[] _managedBuffer;
        private GCHandle _handle;
        private ulong* _longPtr;
        private bool _needsDisposing;

        /// <summary>Current read position</summary>
        private int _bitPosition;

        /// <summary>Offset of given buffer</summary>
        private int _bitOffset;

        /// <summary>Length of given buffer</summary>
        private int _bitLength;

        /// <summary>
        /// Pointer to the managed buffer being used internally
        /// </summary>
        internal void* BufferPointer => _longPtr;

        /// <summary>
        /// Size of buffer that is being read from
        /// </summary>
        public int BitLength
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bitLength;
        }

        /// <summary>
        /// Current bit position for reading from buffer
        /// </summary>
        public int BitPosition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bitPosition;
        }
        /// <summary>
        /// Current <see cref="BitPosition"/> rounded up to nearest multiple of 8
        /// </summary>
        public int BytePosition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            // rounds up to nearest 8
            // add to 3 last bits,
            //   if any are 1 then it will roll over 4th bit.
            //   if all are 0, then nothing happens 
            get => (_bitPosition + 0b111) >> 3;
        }

        public NetworkReader() { }

        ~NetworkReader()
        {
            Dispose(false);
        }
        /// <param name="disposing">true if called from IDisposable</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_needsDisposing) return;

            _handle.Free();
            _longPtr = null;
            _needsDisposing = false;

            if (disposing)
            {
                // clear manged stuff here because we no longer want reader to keep reference to buffer
                _bitLength = 0;
                _managedBuffer = null;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }

        public void Reset(ArraySegment<byte> segment)
        {
            Reset(segment.Array, segment.Offset, segment.Count);
        }
        public void Reset(byte[] array)
        {
            Reset(array, 0, array.Length);
        }
        public void Reset(byte[] array, int position, int length)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array), "Cant use null array in Reader");

            if (_needsDisposing)
            {
                // dispose old handler first
                // false here so we dont release reader back to pool
                Dispose(false);
            }

            // reset disposed bool, as it can be disposed again after reset
            _needsDisposing = true;

            _bitPosition = position * 8;
            _bitOffset = position * 8;
            _bitLength = _bitPosition + (length * 8);
            _managedBuffer = array;
            _handle = GCHandle.Alloc(_managedBuffer, GCHandleType.Pinned);
            _longPtr = (ulong*)_handle.AddrOfPinnedObject();
        }

        /// <summary>
        /// Can read atleast 1 bit
        /// </summary>
        /// <returns></returns>
        public bool CanRead()
        {
            return _bitPosition < _bitLength;
        }

        /// <summary>
        /// Can atleast <paramref name="readCount"/> bits
        /// </summary>
        /// <returns></returns>
        public bool CanReadBits(int readCount)
        {
            return (_bitPosition + readCount) <= _bitLength;
        }

        /// <summary>
        /// Can atleast <paramref name="readCount"/> bytes
        /// </summary>
        /// <param name="readCount"></param>
        /// <returns></returns>
        public bool CanReadBytes(int readCount)
        {
            return (_bitPosition + (readCount * 8)) <= _bitLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckNewLength(int newPosition)
        {
            if (newPosition > _bitLength)
            {
                ThrowPositionOverLength(newPosition);
            }
        }

        private void ThrowPositionOverLength(int newPosition)
        {
            throw new EndOfStreamException($"Can not read over end of buffer, new position {newPosition}, length {_bitLength} bits");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBoolean()
        {
            return ReadBooleanAsUlong() == 1UL;
        }

        /// <summary>
        /// Writes first bit of <paramref name="value"/> to buffer
        /// </summary>
        /// <param name="value"></param>
        public ulong ReadBooleanAsUlong()
        {
            var newPosition = _bitPosition + 1;
            CheckNewLength(newPosition);

            var ptr = _longPtr + (_bitPosition >> 6);
            var result = ((*ptr) >> _bitPosition) & 0b1;

            _bitPosition = newPosition;
            return result;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte ReadSByte() => (sbyte)ReadByte();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte() => (byte)ReadUnmasked(8);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadInt16() => (short)ReadUInt16();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUInt16() => (ushort)ReadUnmasked(16);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32() => (int)ReadUInt32();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt32() => (uint)ReadUnmasked(32);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadInt64() => (long)ReadUInt64();
        public ulong ReadUInt64()
        {
            var newPosition = _bitPosition + 64;
            CheckNewLength(newPosition);

            var bitsInLong = _bitPosition & 0b11_1111;
            ulong result;
            if (bitsInLong == 0)
            {
                var ptr1 = _longPtr + (_bitPosition >> 6);
                result = *ptr1;
            }
            else
            {
                var bitsLeft = 64 - bitsInLong;

                var ptr1 = _longPtr + (_bitPosition >> 6);
                var ptr2 = ptr1 + 1;

                // eg use byte, read 6  =>bitPosition=5, bitsLeft=3, newPos=1
                // r1 = aaab_bbbb => 0000_0aaa
                // r2 = cccc_caaa => ccaa_a000
                // r = r1|r2 => ccaa_aaaa
                // we mask this result later

                var r1 = (*ptr1) >> _bitPosition;
                var r2 = (*ptr2) << bitsLeft;
                result = r1 | r2;
            }

            _bitPosition = newPosition;

            // dont need to mask this result because should be reading all 64 bits
            return result;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadSingle()
        {
            var uValue = ReadUInt32();
            return *(float*)&uValue;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadDouble()
        {
            var uValue = ReadUInt64();
            return *(double*)&uValue;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong Read(int bits)
        {
            if (bits == 0) return 0;
            // mask so we dont returns extra bits
            return ReadUnmasked(bits) & (ulong.MaxValue >> (64 - bits));
        }

        private ulong ReadUnmasked(int bits)
        {
            var newPosition = _bitPosition + bits;
            CheckNewLength(newPosition);

            var bitsInLong = _bitPosition & 0b11_1111;
            var bitsLeft = 64 - bitsInLong;

            ulong result;
            if (bitsLeft >= bits)
            {
                var ptr = _longPtr + (_bitPosition >> 6);
                result = (*ptr) >> bitsInLong;
            }
            else
            {
                var ptr1 = _longPtr + (_bitPosition >> 6);
                var ptr2 = ptr1 + 1;

                // eg use byte, read 6  =>bitPosition=5, bitsLeft=3, newPos=1
                // r1 = aaab_bbbb => 0000_0aaa
                // r2 = cccc_caaa => ccaa_a000
                // r = r1|r2 => ccaa_aaaa
                // we mask this result later

                var r1 = (*ptr1) >> bitsInLong;
                var r2 = (*ptr2) << bitsLeft;
                result = r1 | r2;
            }
            _bitPosition = newPosition;

            return result;
        }

        /// <summary>
        /// Reads n <paramref name="bits"/> from buffer at <paramref name="bitPosition"/>
        /// </summary>
        /// <param name="bits">number of bits in value to write</param>
        /// <param name="bitPosition">where to write bits</param>
        public ulong ReadAtPosition(int bits, int bitPosition)
        {
            // check length here so this methods throws instead of the read below
            CheckNewLength(bitPosition + bits);

            var currentPosition = _bitPosition;
            _bitPosition = bitPosition;
            var result = Read(bits);
            _bitPosition = currentPosition;

            return result;
        }

        public void Skip(int bits)
        {
            MoveBitPosition(_bitPosition + bits);
        }

        /// <summary>
        /// Moves the internal bit position
        /// <para>For most usecases it is safer to use <see cref="ReadAtPosition"/></para>
        /// <para>WARNING: When reading from earlier position make sure to move position back to end of buffer after reading</para>
        /// </summary>
        /// <param name="newPosition"></param>
        /// <exception cref="ArgumentOutOfRangeException">throws when <paramref name="newPosition"/> is less than <see cref="_bitOffset"/></exception>
        public void MoveBitPosition(int newPosition)
        {
            if (newPosition < _bitOffset)
            {
                throw new ArgumentOutOfRangeException(nameof(newPosition), newPosition, $"New position can not be less than buffer offset, Buffer offset: {_bitOffset}");
            }
            CheckNewLength(newPosition);
            _bitPosition = newPosition;
        }


        /// <summary>
        /// <para>
        ///    Moves position to nearest byte then copies struct from that position
        /// </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        public void PadAndCopy<T>(out T value) where T : unmanaged
        {
            fixed (T* ptr = &value)
            {
                var bitLength = 8 * sizeof(T);

                if (bitLength <= 64) // shortcut for small values
                {
                    var v = Read(bitLength);
                    *ptr = *(T*)&v;
                }
                else
                {
                    CopyToPointer(ptr, 0, bitLength);
                }
            }
        }

        public void CopyToPointer(void* ptr, int otherBitPosition, int bitLength)
        {
            var newPosition = _bitPosition + bitLength;
            CheckNewLength(newPosition);

            var ulongPos = otherBitPosition >> 6;
            var otherPtr = (ulong*)ptr + ulongPos;

            throw new NotImplementedException();

            // TODO use NetworkWriter to write to ptr

            var firstBitOffset = otherBitPosition & 0b11_1111;

            //            // first align other
            //            if (firstBitOffset != 0)
            //            {
            //                var bitsToCopyToFirst = Math.Min(64 - firstBitOffset, bitLength);

            //                var firstValue =Read(bitsToCopyToFirst);
            //                *otherPtr = firstValue

            //                // if offset is 10, then we want to shift value by 10 to remove un-needed bits
            //                var firstValue = *otherPtr >> firstBitOffset;

            //                Write(firstValue, bitsToCopyToFirst);

            //                bitLength -= bitsToCopyToFirst;
            //                otherPtr++;
            //            }

            //            // write aligned with other
            //            while (bitLength > 64)
            //            {
            //                WriteUInt64(*otherPtr);

            //                bitLength -= 64;
            //                otherPtr++;
            //            }

            //            // write left over others
            //            //      if bitlength == 0 then write will return
            //            Write(*otherPtr, bitLength);

            //            // define to allow debug.log to be skipped when running outside of unity
            //#if !BIT_PACKING_NO_DEBUG
            //            Debug.Assert(_bitPosition == newBit, "bitPosition should already be equal to newBit because it would have incremented each WriteUInt64");
            //#endif

            //            _bitPosition = newBit;
        }

        /// <summary>
        /// <para>
        ///    Moves position to nearest byte then copies bytes from that position
        /// </para>
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void ReadBytes(byte[] array, int offset, int length)
        {
            fixed (byte* ptr = &array[offset])
            {
                var bitLength = 8 * length;
                CopyToPointer(ptr, 0, bitLength);
            }
        }

        public ArraySegment<byte> ReadBytesSegment(int count)
        {
            // is aligned
            if (_bitPosition % 8 == 0)
            {
                var newPosition = _bitPosition + (8 * count);
                CheckNewLength(newPosition);

                var result = new ArraySegment<byte>(_managedBuffer, BytePosition, count);
                _bitPosition = newPosition;
                return result;
            }
            else
            {
                // TODO remove alloc
                var a = new byte[count];
                fixed (byte* ptr = &a[0])
                {
                    var bitLength = 8 * count;
                    CopyToPointer(ptr, 0, bitLength);
                }
                return new ArraySegment<byte>(a);
            }
        }

        public BitSegment ReadBitSegment(int byteCount)
        {
            return new BitSegment
            {
                ptr = _longPtr,
                bitOffset = _bitPosition,
                bitLength = byteCount * 8,
            };
        }
    }

    public unsafe struct BitSegment
    {
        public void* ptr;
        public int bitOffset;
        public int bitLength;
    }
}
