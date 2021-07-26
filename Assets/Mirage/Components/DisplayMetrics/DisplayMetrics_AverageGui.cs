using Mirage.SocketLayer;
using UnityEngine;

namespace JamesFrowen.NetworkingBenchmark
{
    public class DisplayMetrics_AverageGui : MonoBehaviour
    {
        public Metrics Metrics { get; set; }

        public Rect offset = new Rect(10, 10, 400, 800);
        public Color background;
        GUIStyle style;


        private void Start()
        {
            style = new GUIStyle();
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, background);
            tex.Apply();
            style.normal.background = tex;
        }

        private void OnGUI()
        {
            if (Metrics == null) { return; }

            using (new GUILayout.AreaScope(offset, GUIContent.none, style))
            {
                DrawAverage();
            }
        }
        void DrawAverage()
        {
            double connectionCount = 0;

            double sendCount = 0;
            double sendBytes = 0;

            double sendUnconnectedCount = 0;
            double sendUnconnectedBytes = 0;

            double resendCount = 0;
            double resendBytes = 0;

            double receiveCount = 0;
            double receiveBytes = 0;

            double receiveUnconnectedBytes = 0;
            double receiveUnconnectedCount = 0;

            Metrics.Frame[] array = Metrics.buffer;
            for (int i = 0; i < array.Length; i++)
            {
                connectionCount += array[i].connectionCount;

                sendCount += array[i].sendCount;
                sendBytes += array[i].sendBytes;

                sendUnconnectedCount += array[i].sendUnconnectedCount;
                sendUnconnectedBytes += array[i].sendUnconnectedBytes;

                resendCount += array[i].resendCount;
                resendBytes += array[i].resendBytes;

                receiveCount += array[i].receiveCount;
                receiveBytes += array[i].receiveBytes;

                receiveUnconnectedBytes += array[i].receiveUnconnectedBytes;
                receiveUnconnectedCount += array[i].receiveUnconnectedCount;
            }

            GUILayout.Label($"connectionCount: {connectionCount / array.Length:0.0}");
            GUILayout.Space(8);
            GUILayout.Label($"sendCount: {sendCount / array.Length:0.0}");
            GUILayout.Label($"sendBytes: {sendBytes / array.Length:0.00}");
            GUILayout.Space(8);
            GUILayout.Label($"sendUnconnectedCount: {sendUnconnectedCount / array.Length:0.0}");
            GUILayout.Label($"sendUnconnectedBytes: {sendUnconnectedBytes / array.Length:0.00}");
            GUILayout.Space(8);
            GUILayout.Label($"resendCount: {resendCount / array.Length:0.0}");
            GUILayout.Label($"resendBytes: {resendBytes / array.Length:0.00}");
            GUILayout.Space(8);
            GUILayout.Label($"receiveCount: {receiveCount / array.Length:0.0}");
            GUILayout.Label($"receiveBytes: {receiveBytes / array.Length:0.00}");
            GUILayout.Space(8);
            GUILayout.Label($"receiveUnconnectedCount: {receiveUnconnectedCount / array.Length:0.0}");
            GUILayout.Label($"receiveUnconnectedBytes: {receiveUnconnectedBytes / array.Length:0.00}");

        }
    }
}
