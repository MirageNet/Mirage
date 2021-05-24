namespace Mirage.SocketLayer
{
    /// <summary>
    /// Validates key that client sends in order to connect
    /// <para>This is a simple method that should be enough to stop random packets to the server creating connections</para>
    /// </summary>
    internal class ConnectKeyValidator
    {
        // todo pass in key instead of having constant
        readonly byte key = (byte)'H';

        public int KeyLength => 1;

        public bool Validate(byte[] buffer)
        {
            byte keyByte = buffer[2];

            return keyByte == key;
        }

        public void CopyTo(byte[] buffer)
        {
            buffer[2] = key;
        }
        public byte GetKey() => key;
    }
}
