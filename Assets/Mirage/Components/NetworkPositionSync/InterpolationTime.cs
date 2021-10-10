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

using JamesFrowen.Logging;
using Mirror;
using System.Runtime.CompilerServices;

namespace JamesFrowen.PositionSync
{
    public class InterpolationTime
    {
        /// <summary>
        /// if new time and previous time are this far apart then reset client time
        /// </summary>
        const float SKIP_TIME_DIFF = 1f;

        bool intialized;
        /// <summary>
        /// time client uses to interpolate
        /// </summary>
        float clientTime;
        /// <summary>
        /// Multiples deltaTime by this scale each frame
        /// </summary>
        float clientScaleTime;

        readonly ExponentialMovingAverage diffAvg;

        /// <summary>
        /// goal offset between serverTime and clientTime
        /// </summary>
        readonly float goalOffset;

        /// <summary>
        /// how much above goalOffset diff is allowed to go before changing timescale
        /// </summary>
        readonly float positiveThreshold;
        /// <summary>
        /// how much below goalOffset diff is allowed to go before changing timescale
        /// </summary>
        readonly float negativeThreshold;

        readonly float fastScale = 1.01f;
        readonly float normalScale = 1f;
        readonly float slowScale = 0.99f;

        // debug
        float previousServerTime;


        public float ClientTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.clientTime;
        }
        public float ServerTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.previousServerTime;
        }

        public InterpolationTime(float clientDelay, float rangeFromGoal = 4, int movingAverageCount = 30)
        {
            this.goalOffset = clientDelay;

            this.positiveThreshold = clientDelay / rangeFromGoal;
            this.negativeThreshold = -clientDelay / rangeFromGoal;

            this.diffAvg = new ExponentialMovingAverage(movingAverageCount);

            // start at normal time scale
            this.clientScaleTime = this.normalScale;
        }

        public void OnTick(float deltaTime)
        {
            this.clientTime += deltaTime * this.clientScaleTime;
        }

        public void OnMessage(float serverTime)
        {
            // if first message set client time to server-diff
            // reset stuff if too far behind
            // todo check this is correct
            if (!this.intialized || (serverTime > this.previousServerTime + SKIP_TIME_DIFF))
            {
                this.previousServerTime = serverTime;
                this.clientTime = serverTime - this.goalOffset;
                this.clientScaleTime = this.normalScale;
                this.intialized = true;
                return;
            }

            SimpleLogger.Assert(serverTime > this.previousServerTime, "Received message out of order.");

            this.previousServerTime = serverTime;

            var diff = serverTime - this.clientTime;
            this.diffAvg.Add(diff);
            // diff is server-client,
            // we want client to be 2 frames behind so that there is always snapshots to interoplate towards
            // server-client-offset
            // if positive then server is ahead, => we can run client faster to catch up
            // if negative then server is behind, => we need to run client slow to not run out of spanshots

            // we want diffVsGoal to be as close to 0 as possible
            var fromGoal = (float)this.diffAvg.Value - this.goalOffset;
            if (fromGoal > this.positiveThreshold)
                this.clientScaleTime = this.fastScale;
            else if (fromGoal < this.negativeThreshold)
                this.clientScaleTime = this.slowScale;
            else
                this.clientScaleTime = this.normalScale;

            SimpleLogger.Trace($"st {serverTime:0.00} ct {this.clientTime:0.00} diff {diff * 1000:0.0}, wanted:{fromGoal * 1000:0.0}, scale:{this.clientScaleTime}");
        }
    }
}
