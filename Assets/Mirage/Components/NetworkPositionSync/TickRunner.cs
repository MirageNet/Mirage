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
using Mirage.SocketLayer;

namespace JamesFrowen.PositionSync
{
    public class TickRunner
    {
        public event Action<TickRunner> OnTick;

        private readonly float tickInterval;
        private readonly Sequencer sequencer;

        float _time;
        float _tickTime;
        uint _tick;

        public TickRunner(float tickRate, int sequencerBits = 32)
        {
            tickInterval = 1 / tickRate;
            sequencer = new Sequencer(sequencerBits);
        }

        public float Time
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _time;
        }
        public float FixedTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _tickTime;
        }
        public float FixedDeltaTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => tickInterval;
        }
        public ulong Tick
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _tick;
        }

        public void OnUpdate(float deltaTime)
        {
            _time += deltaTime;
            while (_tickTime < _time)
            {
                _tickTime += tickInterval;
                _tick = (uint)sequencer.NextAfter(_tick);
                OnTick?.Invoke(this);
            }
        }
    }
}
