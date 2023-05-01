namespace Mirage
{
    /// <summary>
    /// Spawn Settings for <see cref="NetworkIdentity"/>
    /// </summary>
    [System.Serializable]
    public struct TransformSpawnSettings
    {
        public bool SendPosition;
        public bool SendRotation;
        public bool SendScale;
        public bool SendName;
        public bool SendGameObjectActive;

        public TransformSpawnSettings(bool sendPosition, bool sendRotation, bool sendScale, bool sendName, bool sendActive) : this()
        {
            SendPosition = sendPosition;
            SendRotation = sendRotation;
            SendScale = sendScale;
            SendName = sendName;
            SendGameObjectActive = sendActive;
        }
        public TransformSpawnSettings(bool sendPosition, bool sendRotation, bool sendScale) : this()
        {
            SendPosition = sendPosition;
            SendRotation = sendRotation;
            SendScale = sendScale;
        }

        public static TransformSpawnSettings Default => new TransformSpawnSettings(
            sendPosition: true,
            sendRotation: true,
            sendScale: false,
            sendName: false,
            sendActive: true);
    }
}
