using System;

namespace Mirage
{
    /// <summary>
    /// Exception thrown when spawning fails
    /// </summary>
    [Serializable]
    public class SpawnObjectException : Exception
    {
        public SpawnObjectException(string message) : base(message)
        {
        }
    }
}
