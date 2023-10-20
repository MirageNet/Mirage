using System.Security.Cryptography;
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

            var bytes = Encoding.ASCII.GetBytes(key);
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(bytes);
                return hash;
            }
        }
        public ConnectKeyValidator(string key) : this(GetKeyBytes(key))
        {
        }

        public bool Validate(byte[] buffer, int length)
        {
            // buffer is pooled, so might contain old data,
            // check the length so we only process that new data (if it is correct length)
            if (length != OFFSET + KeyLength)
                return false;

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
