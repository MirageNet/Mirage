using System.Reflection;

namespace Mirage.Tests.Runtime
{
    public static class NetworkIdentityTestExtensions
    {
        public static void SetSceneId(this NetworkIdentity identity, int id, int hash = 0)
        {
            FieldInfo fieldInfo = typeof(NetworkIdentity).GetField("_sceneId", BindingFlags.Instance | BindingFlags.NonPublic);
            fieldInfo.SetValue(identity, (ulong)((((long)hash) << 32) | (long)id));
        }
    }
}

