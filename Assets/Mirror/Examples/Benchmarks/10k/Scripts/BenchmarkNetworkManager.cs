using System;

namespace Mirror.Examples
{
    public class BenchmarkNetworkManager : NetworkHost
    {
        /// <summary>
        /// hook for benchmarking
        /// </summary>
        public Action BeforeLateUpdate;
        /// <summary>
        /// hook for benchmarking
        /// </summary>
        public Action AfterLateUpdate;


        public void LateUpdate()
        {
            BeforeLateUpdate?.Invoke();
            sceneManager.LateUpdate();
            AfterLateUpdate?.Invoke();
        }
    }
}
