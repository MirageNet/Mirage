using Mirage;
using Mirage.Serialization;

namespace Mirage.Snippets.Sync
{
    public class CustomSerializationExample
    {
        // CodeEmbed-Start: OnSerializeSignature
        public virtual bool OnSerialize(NetworkWriter writer, bool initialState) => false;
        // CodeEmbed-End: OnSerializeSignature

        // CodeEmbed-Start: OnDeserializeSignature
        public virtual void OnDeserialize(NetworkReader reader, bool initialState) { }
        // CodeEmbed-End: OnDeserializeSignature
    }
}
