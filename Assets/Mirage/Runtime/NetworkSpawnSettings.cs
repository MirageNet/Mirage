namespace Mirage
{
    /// <summary>
    /// Spawn Settings for <see cref="NetworkIdentity"/>
    /// </summary>
    [System.Serializable]
    public struct NetworkSpawnSettings
    {
        public bool SendPosition;
        public bool SendRotation;
        public bool SendScale;
        public bool SendName;
        public SyncActiveOption SendActive;

        public NetworkSpawnSettings(bool sendPosition, bool sendRotation, bool sendScale, bool sendName, SyncActiveOption sendActive) : this()
        {
            SendPosition = sendPosition;
            SendRotation = sendRotation;
            SendScale = sendScale;
            SendName = sendName;
            SendActive = sendActive;
        }
        public NetworkSpawnSettings(bool sendPosition, bool sendRotation, bool sendScale) : this()
        {
            SendPosition = sendPosition;
            SendRotation = sendRotation;
            SendScale = sendScale;
        }

        public static NetworkSpawnSettings Default => new NetworkSpawnSettings(
            sendPosition: true,
            sendRotation: true,
            sendScale: false,
            sendName: false,
            sendActive: SyncActiveOption.ForceEnable);
    }


    public enum SyncActiveOption
    {
        /// <summary>
        /// Do nothing - leave the game object in its current state.
        /// </summary>
        DoNothing,

        /// <summary>
        /// Synchronize the active state of the game object with the server's state.
        /// </summary>
        SyncWithServer,

        /// <summary>
        /// Force-enable the game object, even if the server's version is disabled.
        /// </summary>
        ForceEnable,
    }
}
