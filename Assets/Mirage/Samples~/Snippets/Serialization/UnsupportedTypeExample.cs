using UnityEngine;
using Mirage.Serialization;

namespace Mirage.Snippets.Serialization.UnsupportedType
{
    // CodeEmbed-Start: collision-example
    public struct MyCollision
    {
        public Vector3 force;
        public Rigidbody rigidbody;
    }

    public static class CustomReadWriteFunctions
    {
        public static void WriteMyCollision(this NetworkWriter writer, MyCollision value)
        {
            writer.WriteVector3(value.force);

            NetworkIdentity networkIdentity = value.rigidbody.GetComponent<NetworkIdentity>();
            writer.WriteNetworkIdentity(networkIdentity);
        }

        public static MyCollision ReadMyCollision(this NetworkReader reader)
        {
            Vector3 force = reader.ReadVector3();

            NetworkIdentity networkIdentity = reader.ReadNetworkIdentity();
            Rigidbody rigidBody = networkIdentity != null
                ? networkIdentity.GetComponent<Rigidbody>()
                : null;

            return new MyCollision
            {
                force = force,
                rigidbody = rigidBody,
            };
        }
    }
    // CodeEmbed-End: collision-example
}

namespace Mirage.Snippets.Serialization.RigidbodyExample
{
    using UnityEngine;
    using Mirage.Serialization;

    // CodeEmbed-Start: rigidbody-example
    public static class CustomReadWriteFunctions
    {
        public static void WriteRigidbody(this NetworkWriter writer, Rigidbody rigidbody)
        {
            NetworkIdentity networkIdentity = rigidbody.GetComponent<NetworkIdentity>();
            writer.WriteNetworkIdentity(networkIdentity);
        }

        public static Rigidbody ReadRigidbody(this NetworkReader reader)
        {
            NetworkIdentity networkIdentity = reader.ReadNetworkIdentity();
            Rigidbody rigidBody = networkIdentity != null
                ? networkIdentity.GetComponent<Rigidbody>()
                : null;

            return rigidBody;
        }
    }
    // CodeEmbed-End: rigidbody-example
}
