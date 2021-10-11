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
using Mirage.Logging;
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
        public bool syncRotation = true;
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
    /// <summary>
    /// Settings for SyncPosition packer
    /// <para>IMPORTANT: DONT HOLD STATE HERE, might be used by multiple instances</para>
    /// </summary>
    [CreateAssetMenu(menuName = "PositionSync/Packer")]
    public class SyncPositionPacker : ScriptableObject
    {
        static readonly ILogger logger = LogFactory.GetLogger<SyncPositionPacker>();

        [Header("Compression Settings")]
        [SerializeField] SyncSettings settings = new SyncSettings();

        //[Header("Position Debug And Gizmo")]
        //[SerializeField] SyncSettingsDebug settingsDebug = new SyncSettingsDebug();

        [Header("Snapshot Interpolation")]
        [Tooltip("Delay to add to client time to make sure there is always a snapshot to interpolate towards. High delay can handle more jitter, but adds latancy to the position. Should be ~2*syncInterval")]
        [SerializeField] float _clientDelay = 0.2f;

        [Header("Sync")]
        [Tooltip("How often 1 behaviour should update")]
        public float syncInterval = 0.1f;

        [Tooltip("Check if behaviours need update every frame, If false then checks every syncInterval")]
        public bool checkEveryFrame = true;
        [Tooltip("Skips Visibility and sends position to all ready connections")]
        public bool sendToAll = true;
        [Tooltip("Create new system object if missing when first Behaviour is added")]
        public bool CreateSystemIfMissing = false;

        public float ClientDelay
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _clientDelay;
        }

        public float Time
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => UnityEngine.Time.unscaledTime;
        }

        public float DeltaTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => UnityEngine.Time.unscaledDeltaTime;
        }

        public bool SyncRotation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => settings.syncRotation;
        }
        public SyncSettings Settings => settings;

        private void OnValidate()
        {
            //settingsDebug.SetValues(settings);

            if (!sendToAll)
            {
                sendToAll = true;
                logger.LogWarning("sendToAll disabled is not implemented yet");
            }
        }

        //        [Conditional("UNITY_EDITOR")]
        //        internal void DrawGizmo()
        //        {
        //#if UNITY_EDITOR
        //            if (!settingsDebug.drawGizmo) { return; }
        //            Gizmos.color = settingsDebug.gizmoColor;
        //            Bounds bounds = default;
        //            bounds.min = settings.min;
        //            bounds.max = settings.max;
        //            Gizmos.DrawWireCube(bounds.center, bounds.size);
        //#endif  
        //        }
    }
}
