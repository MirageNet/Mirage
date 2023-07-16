using System;
using System.IO;
using ConditionalAttribute = System.Diagnostics.ConditionalAttribute;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Mirage.CodeGen
{
    /// <summary>
    /// Timer used to see how long different parts of weaver take
    /// Call Start when starting all code. Use <see cref="Sample"/> and using scope to record part of the code.
    /// <para>
    /// WEAVER_DEBUG_TIMER must be added to Compile defines for this class to do anything
    /// </para>
    /// </summary>
    public class WeaverDiagnosticsTimer
    {
        public bool writeToFile;
        private StreamWriter writer;
        private Stopwatch stopwatch;
        private string name;

        public long ElapsedMilliseconds => stopwatch?.ElapsedMilliseconds ?? 0;

        ~WeaverDiagnosticsTimer()
        {
            writer?.Dispose();
            writer = null;
        }

        private static bool _checkDirectory = false;

        private static void CheckDirectory()
        {
            if (_checkDirectory)
                return;
            _checkDirectory = true;
            if (!Directory.Exists("./Logs/WeaverLogs"))
            {
                Directory.CreateDirectory("./Logs/WeaverLogs");
            }
        }

        [Conditional("WEAVER_DEBUG_TIMER")]
        public void Start(string name)
        {
            this.name = name;

            if (writeToFile)
            {
                CheckDirectory();
                var path = $"./Logs/WeaverLogs/Timer_{name}.log";
                try
                {
                    writer = new StreamWriter(path)
                    {
                        AutoFlush = true,
                    };
                }
                catch (Exception e)
                {
                    writer?.Dispose();
                    writeToFile = false;
                    WriteLine($"Failed to open {path}: {e}");
                }
            }

            stopwatch = Stopwatch.StartNew();

            WriteLine($"Weave Started - {name}");
            WriteLine($"Time: {DateTime.Now}");
#if WEAVER_DEBUG_LOGS
            WriteLine($"Debug logs enabled");
#else
            WriteLine($"Debug logs disabled");
#endif 
        }

        [Conditional("WEAVER_DEBUG_TIMER")]
        private void WriteLine(string msg)
        {
            Console.WriteLine($"[WeaverDiagnostics] {msg}");
            if (writeToFile)
            {
                writer.WriteLine(msg);
            }
        }

        public long End()
        {
            WriteLine($"Weave Finished: {ElapsedMilliseconds}ms - {name}");
            stopwatch?.Stop();
            writer?.Close();
            return ElapsedMilliseconds;
        }

        public SampleScope Sample(string label)
        {
            return new SampleScope(this, label);
        }

        public struct SampleScope : IDisposable
        {
            private readonly WeaverDiagnosticsTimer timer;
            private readonly long start;
            private readonly string label;

            public SampleScope(WeaverDiagnosticsTimer timer, string label)
            {
                this.timer = timer;
                start = timer.ElapsedMilliseconds;
                this.label = label;
            }

            public void Dispose()
            {
                timer.WriteLine($"{label}: {timer.ElapsedMilliseconds - start}ms - {timer.name}");
            }
        }
    }
}
