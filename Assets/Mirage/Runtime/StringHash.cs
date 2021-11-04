namespace Mirage
{
    public static class StringHash
    {
        public const int EmptyString = 23;

        /// <summary>
        /// Gets a hash for a string. This hash will be the same on all platforms 
        /// </summary>
        /// <remarks>
        /// <see cref="string.GetHashCode"/> is not guaranteed to be the same on all platforms
        /// </remarks>
        public static int GetStableHashCode(this string text)
        {
            unchecked
            {
                int hash = EmptyString;
                foreach (char c in text)
                    hash = hash * 31 + c;
                return hash;
            }
        }
    }
}
