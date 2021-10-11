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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Mirage.Logging;
using UnityEngine;

namespace JamesFrowen.PositionSync
{
    public interface ISnapshotInterpolator<T>
    {
        T Lerp(T a, T b, float alpha);
    }
    public class SnapshotBuffer<T>
    {
        static readonly ILogger logger = LogFactory.GetLogger<SnapshotBuffer<T>>();

        struct Snapshot
        {
            /// <summary>
            /// Server Time
            /// </summary>
            public readonly double time;
            public readonly T state;

            public Snapshot(T state, double time) : this()
            {
                this.state = state;
                this.time = time;
            }
        }

        readonly List<Snapshot> buffer = new List<Snapshot>();
        readonly ISnapshotInterpolator<T> interpolator;

        public SnapshotBuffer(ISnapshotInterpolator<T> interpolator)
        {
            this.interpolator = interpolator;
        }

        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => buffer.Count == 0;
        }
        public int SnapshotCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => buffer.Count;
        }

        Snapshot First
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => buffer[0];
        }
        Snapshot Last
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => buffer[buffer.Count - 1];
        }

        public void AddSnapShot(T state, double serverTime)
        {
            if (!IsEmpty && serverTime < Last.time)
            {
                throw new ArgumentException($"Can not add Snapshot to buffer out of order, last t={Last.time:0.000}, new t={serverTime:0.000}");
            }

            buffer.Add(new Snapshot(state, serverTime));
        }

        /// <summary>
        /// Gets snapshot to use for interpolation
        /// <para>this method should not be called when there are no snapshots in buffer</para>
        /// </summary>
        /// <param name="now"></param>
        /// <returns></returns>
        public T GetLinearInterpolation(double now)
        {
            if (buffer.Count == 0)
            {
                throw new InvalidOperationException("No snapshots in buffer");
            }

            // first snapshot
            if (buffer.Count == 1)
            {
                if (logger.LogEnabled()) logger.Log("First snapshot");

                return First.state;
            }

            // if first snapshot is after now, there is no "from", so return same as first snapshot
            if (First.time > now)
            {
                if (logger.LogEnabled()) logger.Log($"No snapshots for t={now:0.000}, using earliest t={buffer[0].time:0.000}");

                return First.state;
            }

            // if last snapshot is before now, there is no "to", so return last snapshot
            // this can happen if server hasn't sent new data
            // there could be no new data from either lag or because object hasn't moved
            if (Last.time < now)
            {
                if (logger.WarnEnabled()) logger.LogWarning($"No snapshots for t={now:0.000}, using first t={buffer[0].time:0.000} last t={Last.time:0.000}");
                return Last.state;
            }

            // edge cases are returned about, if code gets to this for loop then a valid from/to should exist
            for (int i = 0; i < buffer.Count - 1; i++)
            {
                Snapshot from = buffer[i];
                Snapshot to = buffer[i + 1];
                double fromTime = buffer[i].time;
                double toTime = buffer[i + 1].time;

                // if between times, then use from/to
                if (fromTime <= now && now <= toTime)
                {
                    float alpha = (float)Clamp01((now - fromTime) / (toTime - fromTime));
                    // todo add trace log
                    if (logger.LogEnabled()) logger.Log($"alpha:{alpha:0.000}");

                    return interpolator.Lerp(from.state, to.state, alpha);
                }
            }

            logger.LogError("Should never be here! Code should have return from if or for loop above.");
            return Last.state;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double Clamp01(double v)
        {
            if (v < 0) { return 0; }
            if (v > 1) { return 1; }
            else { return v; }
        }

        /// <summary>
        /// removes snapshots older than <paramref name="oldTime"/>, but keeps atleast <paramref name="keepCount"/> snapshots in buffer that are older than oldTime
        /// <para>
        /// Keep atleast 1 snapshot older than old time so there is something to interoplate from
        /// </para>
        /// </summary>
        /// <param name="oldTime"></param>
        /// <param name="keepCount">minium number of snapshots to keep in buffer</param>
        public void RemoveOldSnapshots(float oldTime)
        {
            // loop from newest to oldest
            for (int i = buffer.Count - 1; i >= 0; i--)
            {
                // older than oldTime
                if (buffer[i].time < oldTime)
                {
                    buffer.RemoveAt(i);
                }
            }
        }


        public string ToDebugString()
        {
            if (buffer.Count == 0) { return "Buffer Empty"; }

            var builder = new StringBuilder();
            builder.AppendLine($"count:{buffer.Count}, minTime:{buffer[0].time:0.000}, maxTime:{buffer[buffer.Count - 1].time:0.000}");
            for (int i = 0; i < buffer.Count; i++)
            {
                builder.AppendLine($"  {i}: {buffer[i].time:0.000}");
            }
            return builder.ToString();
        }
    }
}
