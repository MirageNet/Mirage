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
    /// <remarks>
    /// This class will speed up or slow down Client time scale based on if it is ahead of behind the lastest server time
    /// <para>
    /// Every Update we add DeltaTime * TimeScale to client time
    /// </para>
    /// <para>
    /// Every Update server sends message with its time<br/>
    /// When client receives message it calculates difference between server time and local time<br/>
    /// This difference is stored in a moving average so it is smoothed out
    /// </para>
    /// <para>
    /// If this difference is greater or less than a threashold then we speed up or slow down Client time scale<br/>
    /// If difference is between threshold time is set back to normal scale
    /// </para>
    /// <para>
    /// This Client time can then be used to snapshot interpolation using <c>InterpolationTime = ClientTime - Offset</c>
    /// </para>
    /// <para>
    /// Some other implementaions include the offset in the time scale calculations itself,
    /// So that Client time is always (2) intervals behind the recieved server time. <br/>
    /// Moving that offset to outside this class should still give the same results.
    /// We are just trying to make the difference equal to 0 instead of negative offset.
    /// Then subtracking offset from the ClientTime before we do the interpolation
    /// </para>
    /// </remarks>
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

        /// <param name="diffThreshold">how far off client time can be before changing its speed, Good value is half SyncInterval</param>
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
        public void OnUpdate(float deltaTime)
        {
            _clientTime += deltaTime * clientScaleTime;
        }

        /// <summary>
        /// Updates <see cref="clientScaleTime"/> to keep <see cref="ClientTime"/> in line with <see cref="LatestServerTime"/>
        /// </summary>
        /// <param name="serverTime"></param>
        public void OnMessage(float serverTime)
        {
            logger.Assert(serverTime > _latestServerTime, $"Received message out of order");
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
