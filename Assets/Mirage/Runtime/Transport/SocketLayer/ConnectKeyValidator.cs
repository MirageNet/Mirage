namespace Mirage.SocketLayer
{
    /// <summary>
    /// Validates key that client sends in order to connect
    /// <para>This is a simple method that should be enough to stop random packets to the server creating connections</para>
    /// </summary>
    internal class ConnectKeyValidator
    {
        // todo pass in key instead of having constant
        readonly byte[] key = new[] { (byte)'H' };

        public bool Validate(Packet packet)
        {
            byte keyByte = packet.data[2];

            return keyByte == key[0];
        }
    }
}
