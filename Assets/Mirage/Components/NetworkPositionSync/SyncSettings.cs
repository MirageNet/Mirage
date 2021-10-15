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
using Mirage.Serialization;
using UnityEngine;

namespace JamesFrowen.PositionSync
{
    [Serializable]
    public class SyncSettings
    {
        [Header("timer Compression")]
        public float maxTime = 60 * 60 * 24;
        public float timePrecision = 1 / 1000f;

        [Header("Int Compression")]
        public int blockSize = 5;

        [Header("Position Compression")]
        public Vector3 max = Vector3.one * 100;
        public Vector3 precision = Vector3.one * 0.01f;

        [Header("Rotation Compression")]
        //public bool syncRotation = true;
        public int bitCount = 9;



        public FloatPacker CreateTimePacker()
        {
            return new FloatPacker(maxTime, timePrecision);
        }
        public Vector3Packer CreatePositionPacker()
        {
            return new Vector3Packer(max, precision);
        }
        public QuaternionPacker CreateRotationPacker()
        {
            return new QuaternionPacker(bitCount);
        }
    }

    public class SyncPacker
    {
        // packers
        readonly FloatPacker timePacker;
        readonly Vector3Packer positionPacker;
        readonly QuaternionPacker rotationPacker;
        readonly int blockSize;

        public SyncPacker(SyncSettings settings)
        {
            timePacker = settings.CreateTimePacker();
            positionPacker = settings.CreatePositionPacker();
            rotationPacker = settings.CreateRotationPacker();
            blockSize = settings.blockSize;
        }

        public void PackTime(NetworkWriter writer, float time)
        {
            timePacker.Pack(writer, time);
        }

        public void PackNext(NetworkWriter writer, SyncPositionBehaviour behaviour)
        {
            uint id = behaviour.NetId;
            TransformState state = behaviour.TransformState;

            VarIntBlocksPacker.Pack(writer, id, blockSize);
            positionPacker.Pack(writer, state.position);
            rotationPacker.Pack(writer, state.rotation);
        }


        public float UnpackTime(NetworkReader reader)
        {
            return timePacker.Unpack(reader);
        }

        public void UnpackNext(NetworkReader reader, out uint id, out Vector3 pos, out Quaternion rot)
        {
            id = (uint)VarIntBlocksPacker.Unpack(reader, blockSize);
            pos = positionPacker.Unpack(reader);
            rot = rotationPacker.Unpack(reader);
        }

        internal bool TryUnpackNext(PooledNetworkReader reader, out uint id, out Vector3 pos, out Quaternion rot)
        {
            // assume 1 state is atleast 3 bytes
            // (it should be more, but there shouldn't be random left over bits in reader so 3 is enough for check)
            const int minSize = 3;
            if (reader.CanReadBytes(minSize))
            {
                UnpackNext(reader, out id, out pos, out rot);
                return true;
            }
            else
            {
                id = default;
                pos = default;
                rot = default;
                return false;
            }
        }
    }
    //[Serializable]
    //public class SyncSettingsDebug
    //{
    //    // todo replace these serialized fields with custom editor
    //    public bool drawGizmo;
    //    public Color gizmoColor;
    //    [Tooltip("readonly")]
    //    public int _posBitCount;
    //    [Tooltip("readonly")]
    //    public Vector3Int _posBitCountAxis;
    //    [Tooltip("readonly")]
    //    public int _posByteCount;

    //    public int _totalBitCountMin;
    //    public int _totalBitCountMax;
    //    public int _totalByteCountMin;
    //    public int _totalByteCountMax;

    //    internal void SetValues(SyncSettings settings)
    //    {
    //        var positionPacker = new Vector3Packer(settings.max, settings.precision);
    //        _posBitCount = positionPacker.bitCount;
    //        _posBitCountAxis = positionPacker.BitCountAxis;
    //        _posByteCount = Mathf.CeilToInt(_posBitCount / 8f);

    //        var timePacker = new FloatPacker(0, settings.maxTime, settings.timePrecision);
    //        var idPacker = new UIntVariablePacker(settings.smallBitCount, settings.mediumBitCount, settings.largeBitCount);
    //        UIntVariablePacker parentPacker = idPacker;
    //        var rotationPacker = new QuaternionPacker(settings.bitCount);


    //        _totalBitCountMin = idPacker.minBitCount + (settings.syncRotation ? rotationPacker.bitCount : 0) + positionPacker.bitCount;
    //        _totalBitCountMax = idPacker.maxBitCount + (settings.syncRotation ? rotationPacker.bitCount : 0) + positionPacker.bitCount;
    //        _totalByteCountMin = Mathf.CeilToInt(_totalBitCountMin / 8f);
    //        _totalByteCountMax = Mathf.CeilToInt(_totalBitCountMax / 8f);
    //    }
    //}
}
