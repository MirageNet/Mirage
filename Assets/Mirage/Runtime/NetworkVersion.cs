using System;
using System.Linq;
using System.Reflection;

namespace Mirage
{
    public static class NetworkVersion
    {
        public const string HashFieldName = "NetworkHash";

        private static int? _hash;

        /// <summary>
        /// Gets the hash of all network methods in all assemblies
        /// <para>This can be used to check if client and server are using the same build</para>
        /// </summary>
        /// <returns>hash, or null if it could not be found</returns>
        public static int? GetHash()
        {
            if (_hash.HasValue)
                return _hash;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var hash = 0;
            var found = false;
            foreach (var assembly in assemblies)
            {
                var generated = assembly.GetType("Mirage.GeneratedNetworkCode");
                if (generated != null)
                {
                    var field = generated.GetField(HashFieldName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                    if (field != null && field.IsLiteral)
                    {
                        var value = (int)field.GetValue(null);
                        hash ^= value;
                        found = true;
                    }
                }
            }
            
            if (found)
            {
                _hash = hash;
                return hash;
            }
            else
            {
                return null;
            }
        }
    }
}
