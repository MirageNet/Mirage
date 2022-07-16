using System.Text;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// Validates key that client sends in order to connect
    /// <para>This is a simple method that should be enough to stop random packets to the server creating connections</para>
    /// </summary>
    internal class ConnectKeyValidator
    {
        private readonly byte[] _key;
        public readonly int KeyLength;
        private const int OFFSET = 2;

        public ConnectKeyValidator(byte[] key)
        {
            _key = key;
            KeyLength = key.Length;
        }

        private static byte[] GetKeyBytes(string key)
        {
            // default to mirage version
            if (string.IsNullOrEmpty(key))
            {
                var version = typeof(ConnectKeyValidator).Assembly.GetName().Version.Major.ToString();
                key = $"Mirage V{version}";
            }

            return Encoding.ASCII.GetBytes(key);
        }
        public ConnectKeyValidator(string key) : this(GetKeyBytes(key))
        {
        }

        public bool Validate(byte[] buffer)
        {
            for (var i = 0; i < KeyLength; i++)
            {
                var keyByte = buffer[i + OFFSET];
                if (keyByte != _key[i])
                    return false;
            }

            return true;
        }

        public void CopyTo(byte[] buffer)
        {
            for (var i = 0; i < KeyLength; i++)
            {
                buffer[i + OFFSET] = _key[i];
            }
        }
    }
}
