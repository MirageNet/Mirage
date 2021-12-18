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
        byte[] managedBuffer;
        GCHandle handle;
        ulong* longPtr;
        bool needsDisposing;

        /// <summary>Current read position</summary>
        int bitPosition;
        /// <summary>Offset of given buffer</summary>
        int bitOffset;
        /// <summary>Length of given buffer</summary>
        int bitLength;

        /// <summary>
        /// Size of buffer that is being read from
        /// </summary>
        public int BitLength
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => bitLength;
        }

        /// <summary>
        /// Current bit position for reading from buffer
        /// </summary>
        public int BitPosition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => bitPosition;
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
            get => (bitPosition + 0b111) >> 3;
        }

        /// <summary>
        /// some service object that can find objects by net id
        /// </summary>
        // todo try move this somewhere else
        public IObjectLocator ObjectLocator { get; internal set; }


        public NetworkReader() { }

        ~NetworkReader()
        {
            Dispose(false);
        }
        /// <param name="disposing">true if called from IDisposable</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!needsDisposing) return;

            handle.Free();
            longPtr = null;
            needsDisposing = false;

            if (disposing)
            {
                // clear manged stuff here because we no longer want reader to keep reference to buffer
                bitLength = 0;
                managedBuffer = null;
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
            if (needsDisposing)
            {
                // dispose old handler first
                // false here so we dont release reader back to pool
                Dispose(false);
            }

            // reset disposed bool, as it can be disposed again after reset
            needsDisposing = true;

            bitPosition = position * 8;
            bitOffset = position * 8;
            bitLength = bitPosition + (length * 8);
            managedBuffer = array;
            handle = GCHandle.Alloc(managedBuffer, GCHandleType.Pinned);
            longPtr = (ulong*)handle.AddrOfPinnedObject();
        }

        /// <summary>
        /// Can read atleast 1 bit
        /// </summary>
        /// <returns></returns>
        public bool CanRead()
        {
            return bitPosition < bitLength;
        }

        /// <summary>
        /// Can atleast <paramref name="byteLength"/> bytes
        /// </summary>
        /// <param name="byteLength"></param>
        /// <returns></returns>
        public bool CanReadBytes(int byteLength)
        {
            return (bitPosition + (byteLength * 8)) <= bitLength;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CheckNewLength(int newPosition)
        {
            if (newPosition > bitLength)
            {
                ThrowPositionOverLength(newPosition);
            }
        }
        void ThrowPositionOverLength(int newPosition)
        {
            throw new EndOfStreamException($"Can not read over end of buffer, new position {newPosition}, length {bitLength} bits");
        }

        private void PadToByte()
        {
            bitPosition = BytePosition << 3;
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
            int newPosition = bitPosition + 1;
            CheckNewLength(newPosition);

            ulong* ptr = (longPtr + (bitPosition >> 6));
            ulong result = ((*ptr) >> bitPosition) & 0b1;

            bitPosition = newPosition;
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
            int newPosition = bitPosition + 64;
            CheckNewLength(newPosition);

            int bitsInLong = bitPosition & 0b11_1111;
            ulong result;
            if (bitsInLong == 0)
            {
                ulong* ptr1 = (longPtr + (bitPosition >> 6));
                result = *ptr1;
            }
            else
            {
                int bitsLeft = 64 - bitsInLong;

                ulong* ptr1 = (longPtr + (bitPosition >> 6));
                ulong* ptr2 = (ptr1 + 1);

                // eg use byte, read 6  =>bitPosition=5, bitsLeft=3, newPos=1
                // r1 = aaab_bbbb => 0000_0aaa
                // r2 = cccc_caaa => ccaa_a000
                // r = r1|r2 => ccaa_aaaa
                // we mask this result later

                ulong r1 = (*ptr1) >> bitPosition;
                ulong r2 = (*ptr2) << bitsLeft;
                result = r1 | r2;
            }

            bitPosition = newPosition;

            // dont need to mask this result because should be reading all 64 bits
            return result;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadSingle()
        {
            uint uValue = ReadUInt32();
            return *(float*)&uValue;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadDouble()
        {
            ulong uValue = ReadUInt64();
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
            int newPosition = bitPosition + bits;
            CheckNewLength(newPosition);

            int bitsInLong = bitPosition & 0b11_1111;
            int bitsLeft = 64 - bitsInLong;

            ulong result;
            if (bitsLeft >= bits)
            {
                ulong* ptr = longPtr + (bitPosition >> 6);
                result = (*ptr) >> bitsInLong;
            }
            else
            {
                ulong* ptr1 = longPtr + (bitPosition >> 6);
                ulong* ptr2 = ptr1 + 1;

                // eg use byte, read 6  =>bitPosition=5, bitsLeft=3, newPos=1
                // r1 = aaab_bbbb => 0000_0aaa
                // r2 = cccc_caaa => ccaa_a000
                // r = r1|r2 => ccaa_aaaa
                // we mask this result later

                ulong r1 = (*ptr1) >> bitsInLong;
                ulong r2 = (*ptr2) << bitsLeft;
                result = r1 | r2;
            }
            bitPosition = newPosition;

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

            int currentPosition = this.bitPosition;
            this.bitPosition = bitPosition;
            ulong result = Read(bits);
            this.bitPosition = currentPosition;

            return result;
        }


        /// <summary>
        /// Moves the internal bit position
        /// <para>For most usecases it is safer to use <see cref="ReadAtPosition"/></para>
        /// <para>WARNING: When reading from earlier position make sure to move position back to end of buffer after reading</para>
        /// </summary>
        /// <param name="newPosition"></param>
        /// <exception cref="ArgumentOutOfRangeException">throws when <paramref name="newPosition"/> is less than <see cref="bitOffset"/></exception>
        public void MoveBitPosition(int newPosition)
        {
            if (newPosition < bitOffset)
            {
                throw new ArgumentOutOfRangeException(nameof(newPosition), newPosition, $"New position can not be less than buffer offset, Buffer offset: {bitOffset}");
            }
            CheckNewLength(newPosition);
            bitPosition = newPosition;
        }


        /// <summary>
        /// <para>
        ///    Moves position to nearest byte then copies struct from that position
        /// </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="byteSize"></param>
        public void PadAndCopy<T>(int byteSize, out T value) where T : unmanaged
        {
            PadToByte();
            int newPosition = bitPosition + (64 * byteSize);
            CheckNewLength(newPosition);

            byte* startPtr = ((byte*)longPtr) + (bitPosition >> 3);

            value = *(T*)startPtr;
            bitPosition = newPosition;
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
            PadToByte();
            int newPosition = bitPosition + (8 * length);
            CheckNewLength(newPosition);

            // todo benchmark this vs Marshal.Copy or for loop
            Buffer.BlockCopy(managedBuffer, BytePosition, array, offset, length);
            bitPosition = newPosition;
        }

        public ArraySegment<byte> ReadBytesSegment(int count)
        {
            PadToByte();
            int newPosition = bitPosition + (8 * count);
            CheckNewLength(newPosition);

            var result = new ArraySegment<byte>(managedBuffer, BytePosition, count);
            bitPosition = newPosition;
            return result;
        }
    }
}
