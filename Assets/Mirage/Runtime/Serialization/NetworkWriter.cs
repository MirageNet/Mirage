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
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Mirage.Serialization
{
    /// <summary>
    /// Bit writer, writes values to a buffer on a bit level
    /// <para>Use <see cref="NetworkWriterPool.GetWriter"/> to reduce memory allocation</para>
    /// </summary>
    public unsafe class NetworkWriter
    {
        /// <summary>
        /// Max buffer size = 0.5MB
        /// </summary>
        const int MaxBufferSize = 524_288;

        byte[] managedBuffer;
        int bitCapacity;
        /// <summary>Allow internal buffer to resize if capcity is reached</summary>
        readonly bool allowResize;

        GCHandle handle;
        ulong* longPtr;
        bool needsDisposing;

        int bitPosition;

        /// <summary>
        /// Size limit of buffer
        /// </summary>
        public int ByteCapacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            // see ByteLength for comment on math
            get => (bitCapacity + 0b111) >> 3;
        }

        /// <summary>
        /// Current <see cref="BitPosition"/> rounded up to nearest multiple of 8
        /// <para>To set byte position use <see cref="MoveBitPosition"/> multiple by 8</para>
        /// </summary>
        public int ByteLength
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            // rounds up to nearest 8
            // add to 3 last bits,
            //   if any are 1 then it will roll over 4th bit.
            //   if all are 0, then nothing happens 
            get => (bitPosition + 0b111) >> 3;
        }

        /// <summary>
        /// Current bit position for writing to buffer
        /// <para>To set bit position use <see cref="MoveBitPosition"/></para>
        /// </summary>
        public int BitPosition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => bitPosition;
        }

        public NetworkWriter(int minByteCapacity) : this(minByteCapacity, true) { }
        public NetworkWriter(int minByteCapacity, bool allowResize)
        {
            this.allowResize = allowResize;

            // ensure capacity is multiple of 8
            int ulongCapacity = Mathf.CeilToInt(minByteCapacity / (float)sizeof(ulong));
            int byteCapacity = ulongCapacity * sizeof(ulong);

            bitCapacity = byteCapacity * 8;
            managedBuffer = new byte[byteCapacity];

            CreateHandle();
        }


        ~NetworkWriter()
        {
            FreeHandle();
        }


        void ResizeBuffer(int minBitCapacity)
        {
            int minByteCapacity = minBitCapacity / 8;
            int size = managedBuffer.Length;
            while (size < minByteCapacity)
            {
                size *= 2;
                if (size > MaxBufferSize)
                {
                    throw new InvalidOperationException($"Can not resize buffer to {size} bytes because it is above max value of {MaxBufferSize}");
                }
            }

            Debug.LogWarning($"Resizing buffer, new size:{size} bytes");

            FreeHandle();

            Array.Resize(ref managedBuffer, size);
            bitCapacity = size * 8;

            CreateHandle();
        }
        void CreateHandle()
        {
            if (needsDisposing) FreeHandle();

            handle = GCHandle.Alloc(managedBuffer, GCHandleType.Pinned);
            longPtr = (ulong*)handle.AddrOfPinnedObject();
            needsDisposing = true;
        }
        /// <summary>
        /// Frees the handle for the buffer
        /// <para>In order for <see cref="PooledNetworkWriter"/> to work This class can not have <see cref="IDisposable"/>. Instead we call this method from finalize</para>
        /// </summary>
        void FreeHandle()
        {
            if (!needsDisposing) return;

            handle.Free();
            longPtr = null;
            needsDisposing = false;
        }

        public void Reset()
        {
            bitPosition = 0;
        }

        /// <summary>
        /// Copies internal buffer to new Array
        /// <para>To reduce Allocations use <see cref="ToArraySegment"/> instead</para>
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            byte[] data = new byte[ByteLength];
            Buffer.BlockCopy(managedBuffer, 0, data, 0, ByteLength);
            return data;
        }
        public ArraySegment<byte> ToArraySegment()
        {
            return new ArraySegment<byte>(managedBuffer, 0, ByteLength);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CheckCapacity(int newLength)
        {
            if (newLength > bitCapacity)
            {
                if (allowResize)
                {
                    ResizeBuffer(newLength);
                }
                else
                {
                    ThrowLengthOverCapacity(newLength);
                }
            }
        }
        void ThrowLengthOverCapacity(int newLength)
        {
            throw new InvalidOperationException($"Can not write over end of buffer, new length {newLength}, capacity {bitCapacity}");
        }

        private void PadToByte()
        {
            bitPosition = ByteLength << 3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBoolean(bool value)
        {
            WriteBoolean(value ? 1UL : 0UL);
        }
        /// <summary>
        /// Writes first bit of <paramref name="value"/> to buffer
        /// </summary>
        /// <param name="value"></param>
        public void WriteBoolean(ulong value)
        {
            int newPosition = bitPosition + 1;
            CheckCapacity(newPosition);

            int bitsInLong = bitPosition & 0b11_1111;

            ulong* ptr = longPtr + (bitPosition >> 6);
            *ptr = (
                *ptr & (
                    // start with 0000_0001
                    // shift by number in bit, eg 5 => 0010_0000
                    // then not 1101_1111
                    ~(1UL << bitsInLong)
                )
            ) | ((value & 0b1) << bitsInLong);

            bitPosition = newPosition;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSByte(sbyte value) => WriteByte((byte)value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(byte value) => WriterUnmasked(value, 8);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt16(short value) => WriteUInt16((ushort)value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt16(ushort value) => WriterUnmasked(value, 16);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt32(int value) => WriteUInt32((uint)value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt32(uint value) => WriterUnmasked(value, 32);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt64(long value) => WriteUInt64((ulong)value);
        public void WriteUInt64(ulong value)
        {
            int newPosition = bitPosition + 64;
            CheckCapacity(newPosition);

            int bitsInLong = bitPosition & 0b11_1111;

            if (bitsInLong == 0)
            {
                ulong* ptr1 = longPtr + (bitPosition >> 6);
                *ptr1 = value;
            }
            else
            {
                int bitsLeft = 64 - bitsInLong;

                ulong* ptr1 = longPtr + (bitPosition >> 6);
                ulong* ptr2 = ptr1 + 1;

                *ptr1 = (*ptr1 & (ulong.MaxValue >> bitsLeft)) | (value << bitsInLong);
                *ptr2 = (*ptr2 & (ulong.MaxValue << newPosition)) | (value >> bitsLeft);
            }
            bitPosition = newPosition;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSingle(float value) => WriteUInt32(*(uint*)&value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteDouble(double value) => WriteUInt64(*(ulong*)&value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ulong value, int bits)
        {
            if (bits == 0) return;
            // mask so we dont overwrite
            WriterUnmasked(value & BitMask.Mask(bits), bits);
        }

        private void WriterUnmasked(ulong value, int bits)
        {
            int newPosition = bitPosition + bits;
            CheckCapacity(newPosition);

            int bitsInLong = bitPosition & 0b11_1111;
            int bitsLeft = 64 - bitsInLong;

            if (bitsLeft >= bits)
            {
                ulong* ptr = longPtr + (bitPosition >> 6);

                *ptr = (*ptr & BitMask.OuterMask(bitPosition, newPosition)) | (value << bitsInLong);
            }
            else
            {
                ulong* ptr1 = longPtr + (bitPosition >> 6);
                ulong* ptr2 = ptr1 + 1;

                *ptr1 = (*ptr1 & (ulong.MaxValue >> bitsLeft)) | (value << bitsInLong);
                *ptr2 = (*ptr2 & (ulong.MaxValue << newPosition)) | (value >> bitsLeft);
            }
            bitPosition = newPosition;
        }

        /// <summary>
        /// Same as <see cref="WriteAtPosition"/> expect position given is in bytes instead of bits
        /// <para>WARNING: When writing to bytes instead of bits make sure you are able to read at the right position when deserializing as it might cause data to be misaligned</para>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bits"></param>
        /// <param name="bytePosition"></param>
        public void WriteAtBytePosition(ulong value, int bits, int bytePosition)
        {
            WriteAtPosition(value, bits, bytePosition * 8);
        }

        /// <summary>
        /// Writes n <paramref name="bits"/> from <paramref name="value"/> to <paramref name="bitPosition"/>
        /// <para>This methods can be used to go back to a previous position to write length or other flags to the buffer after other data has been written</para>
        /// <para>WARNING: This method does not change the internal position so will not change the overall length if writing past internal position</para>
        /// </summary>
        /// <param name="value">value to write</param>
        /// <param name="bits">number of bits in value to write</param>
        /// <param name="bitPosition">where to write bits</param>
        public void WriteAtPosition(ulong value, int bits, int bitPosition)
        {
            // check length here so this methods throws instead of the write below
            // this is so that it is more obvious that the position arg for this method is invalid
            CheckCapacity(bitPosition + bits);

            // moves position to arg, then write, then reset position
            int currentPosition = this.bitPosition;
            this.bitPosition = bitPosition;
            Write(value, bits);
            this.bitPosition = currentPosition;
        }


        /// <summary>
        /// Moves the internal bit position
        /// <para>For most usecases it is safer to use <see cref="WriteAtPosition"/></para>
        /// <para>WARNING: When writing to earlier position make sure to move position back to end of buffer after writing because position is also used as length</para>
        /// </summary>
        /// <param name="newPosition"></param>
        public void MoveBitPosition(int newPosition)
        {
            CheckCapacity(newPosition);
            bitPosition = newPosition;
        }

        /// <summary>
        /// <para>
        ///    Moves position to nearest byte then copies struct to that position
        /// </para>
        /// See <see href="https://docs.unity3d.com/ScriptReference/Unity.Collections.LowLevel.Unsafe.UnsafeUtility.CopyStructureToPtr.html">UnsafeUtility.CopyStructureToPtr</see>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="byteSize">size of struct, in bytes</param>
        public void PadAndCopy<T>(ref T value, int byteSize) where T : struct
        {
            PadToByte();
            int newPosition = bitPosition + (8 * byteSize);
            CheckCapacity(newPosition);

            byte* startPtr = ((byte*)longPtr) + (bitPosition >> 3);

            UnsafeUtility.CopyStructureToPtr(ref value, startPtr);
            bitPosition = newPosition;
        }

        /// <summary>
        /// <para>
        ///    Moves position to nearest byte then writes bytes to that position
        /// </para>
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void WriteBytes(byte[] array, int offset, int length)
        {
            PadToByte();
            int newPosition = bitPosition + (8 * length);
            CheckCapacity(newPosition);

            // todo benchmark this vs Marshal.Copy or for loop
            Buffer.BlockCopy(array, offset, managedBuffer, ByteLength, length);
            bitPosition = newPosition;
        }

        public void CopyFromWriter(NetworkWriter other, int otherBitPosition, int bitLength)
        {
            int newBit = bitPosition + bitLength;
            CheckCapacity(newBit);

            int ulongPos = otherBitPosition >> 6;
            ulong* otherPtr = other.longPtr + ulongPos;


            int firstBitOffset = otherBitPosition & 0b11_1111;

            // first align other
            if (firstBitOffset != 0)
            {
                int bitsToCopyFromFirst = Math.Min(64 - firstBitOffset, bitLength);

                // if offset is 10, then we want to shift value by 10 to remove un-needed bits
                ulong firstValue = *otherPtr >> firstBitOffset;

                Write(firstValue, bitsToCopyFromFirst);

                bitLength -= bitsToCopyFromFirst;
                otherPtr++;
            }

            // write aligned with other
            while (bitLength > 64)
            {
                WriteUInt64(*otherPtr);

                bitLength -= 64;
                otherPtr++;
            }

            // write left over others
            //      if bitlength == 0 then write will return
            Write(*otherPtr, bitLength);

            Debug.Assert(bitPosition == newBit, "bitPosition should already be equal to newBit because it would have incremented each WriteUInt64");
            bitPosition = newBit;
        }
    }
}
