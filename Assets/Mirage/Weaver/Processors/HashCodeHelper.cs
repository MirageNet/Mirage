namespace Mirage.Weaver
{
    public static class HashCodeHelper
    {
        /// <summary>
        /// Use this to get a hash of 2 objects.
        /// <para>This should be more efficient than concatenating 2 strings</para>
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static int GetCombineHash(object obj1, object obj2)
        {
            // https://stackoverflow.com/a/263416/8479976
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + obj1.GetHashCode();
                hash = hash * 31 + obj2.GetHashCode();
                return hash;
            }
        }
    }
}
