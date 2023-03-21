// windows, linux or standalone c#, unless EXCLUDE_NANOSOCKETS is defined
#if !EXCLUDE_NANOSOCKETS && (UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || NETCOREAPP || NET_5_0_OR_GREATER)
using NanoSockets;
using UnityEngine;

namespace Mirage.Sockets.Udp
{
    public static class InitUDP
    {
        private static int initCount;

        /// <summary>
        /// Initializes the NanoSockets native library. If it fails, it resorts to C# Managed Sockets.
        /// </summary>
        public static void Init()
        {
            if (initCount == 0)
            {
                var status = UDP.Initialize();
                if (status == Status.Error)
                    Debug.LogError("Error calling UDP.Initialize");
            }

            initCount++;
        }

        public static void Deinit()
        {
            initCount--;

            if (initCount == 0) UDP.Deinitialize();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ClearCounter()
        {
            // todo do we need to call Deinitialize here?
            initCount = 0;
        }
    }
}
#endif
