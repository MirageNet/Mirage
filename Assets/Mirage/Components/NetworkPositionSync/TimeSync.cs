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

using System.Runtime.CompilerServices;
using Mirage;
using Mirage.Logging;
using UnityEngine;

namespace JamesFrowen.PositionSync
{
    /// <summary>
    /// Syncs time between server and client be receving regular message from server
    /// <para>Can be used for snapshot interpolation</para>
    /// </summary>
    public class TimeSync
    {
        static readonly ILogger logger = LogFactory.GetLogger<TimeSync>();

        /// <summary>
        /// if new time and previous time are this far apart then reset client time
        /// </summary>
        const float SKIP_TIME_DIFF = 1f;

        bool intialized;
        /// <summary>
        /// time client uses to interpolate
        /// </summary>
        float _clientTime;
        /// <summary>
        /// Multiples deltaTime by this scale each frame
        /// </summary>
        float clientScaleTime;

        readonly ExponentialMovingAverage diffAvg;

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
        float _latestServerTime;


        [System.Obsolete("Use InterpolationTime insteads", true)]
        public float ClientTime_old { get; }
        /// <summary>
        /// Timer that follows server time
        /// </summary>
        public float ClientTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _clientTime;
        }
        /// <summary>
        /// Last time Received by server
        /// </summary>
        public float LatestServerTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _latestServerTime;
        }

        [System.Obsolete("", true)]
        public TimeSync(float clientDelay, float rangeFromGoal = 4, int movingAverageCount = 30) { }

        /// <param name="diffThreshold">how far off client time can be before changing its speed</param>
        /// <param name="movingAverageCount">how many ticks used in average, increase or decrease with framerate</param>
        public TimeSync(float diffThreshold, int movingAverageCount = 30)
        {
            // todo do we need tick rate here?
            positiveThreshold = diffThreshold;
            negativeThreshold = -diffThreshold;

            diffAvg = new ExponentialMovingAverage(movingAverageCount);

            // start at normal time scale
            clientScaleTime = normalScale;
        }

        /// <summary>
        /// Updates client time
        /// </summary>
        /// <param name="deltaTime"></param>
        public void OnTick(float deltaTime)
        {
            _clientTime += deltaTime * clientScaleTime;
        }

        /// <summary>
        /// Updates <see cref="clientScaleTime"/> to keep <see cref="ClientTime"/> in line with <see cref="LatestServerTime"/>
        /// </summary>
        /// <param name="serverTime"></param>
        public void OnMessage(float serverTime)
        {
            _latestServerTime = serverTime;

            // if first message set client time to server-diff
            // reset stuff if too far behind
            // todo check this is correct
            if (!intialized || (serverTime > _latestServerTime + SKIP_TIME_DIFF))
            {
                _clientTime = serverTime;
                clientScaleTime = normalScale;
                intialized = true;
                return;
            }

            logger.Assert(serverTime > _latestServerTime, "Received message out of order.");

            float diff = serverTime - _clientTime;
            diffAvg.Add(diff);

            AdjustClientTimeScale((float)diffAvg.Value);

            //todo add trace level
            if (logger.LogEnabled()) logger.Log($"st {serverTime:0.00} ct {_clientTime:0.00} diff {diff * 1000:0.0}, wanted:{diffAvg.Value * 1000:0.0}, scale:{clientScaleTime}");
        }

        private void AdjustClientTimeScale(float diff)
        {
            // diff is server-client,
            // if positive then server is ahead, => we can run client faster to catch up
            // if negative then server is behind, => we need to run client slow to not run out of spanshots

            // we want diffVsGoal to be as close to 0 as possible

            // server ahead, speed up client
            if (diff > positiveThreshold)
                clientScaleTime = fastScale;
            // server behind, slow down client
            else if (diff < negativeThreshold)
                clientScaleTime = slowScale;
            // close enough
            else
                clientScaleTime = normalScale;
        }
    }
}
