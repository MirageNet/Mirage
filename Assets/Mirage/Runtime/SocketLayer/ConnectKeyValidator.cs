using System.Text;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// Validates key that client sends in order to connect
    /// <para>This is a simple method that should be enough to stop random packets to the server creating connections</para>
    /// </summary>
    internal class ConnectKeyValidator
    {
        readonly byte[] key;
        public readonly int KeyLength;
        const int OFFSET = 2;

        public ConnectKeyValidator(byte[] key)
        {
            this.key = key;
            KeyLength = key.Length;
        }

        static byte[] GetKeyBytes(string key)
        {
            // default to mirage version
            if (string.IsNullOrEmpty(key))
            {
                string version = typeof(ConnectKeyValidator).Assembly.GetName().Version.Major.ToString();
                key = $"Mirage V{version}";
            }

            return Encoding.ASCII.GetBytes(key);
        }
        public ConnectKeyValidator(string key) : this(GetKeyBytes(key))
        {
        }

        public bool Validate(byte[] buffer)
        {
            for (int i = 0; i < KeyLength; i++)
            {
                byte keyByte = buffer[i + OFFSET];
                if (keyByte != key[i])
                    return false;
            }

            return true;
        }

        public void CopyTo(byte[] buffer)
        {
            for (int i = 0; i < KeyLength; i++)
            {
                buffer[i + OFFSET] = key[i];
            }
        }
    }
}
