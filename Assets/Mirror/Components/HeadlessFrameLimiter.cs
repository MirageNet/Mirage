using UnityEngine;

namespace Mirror
{
    public class HeadlessFrameLimiter : MonoBehaviour
    {
        static readonly ILogger logger = LogFactory.GetLogger<NetworkManager>();

        /// <summary>
        /// Server Update frequency, per second. Use around 60Hz for fast paced games like Counter-Strike to minimize latency. Use around 30Hz for games like WoW to minimize computations. Use around 1-10Hz for slow paced games like EVE.
        /// </summary>
        [Tooltip("Server Update frequency, per second. Use around 60Hz for fast paced games like Counter-Strike to minimize latency. Use around 30Hz for games like WoW to minimize computations. Use around 1-10Hz for slow paced games like EVE.")]
        public int serverTickRate = 30;

        /// <summary>
        /// Set the frame rate for a headless server.
        /// <para>Override if you wish to disable the behavior or set your own tick rate.</para>
        /// </summary>
        public void Start()
        {
            // set a fixed tick rate instead of updating as often as possible
            // * if not in Editor (it doesn't work in the Editor)
            // DO NOT ATTACHED THIS TO A NON HEADLESS BUILD
#if !UNITY_EDITOR
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                Application.targetFrameRate = serverTickRate;
                if (logger.logEnabled) logger.Log("Server Tick Rate set to: " + Application.targetFrameRate + " Hz.");
            }
#endif
        }
    }
}
