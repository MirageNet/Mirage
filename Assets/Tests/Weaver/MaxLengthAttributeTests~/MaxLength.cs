using Mirage;
using Mirage.Serialization;
using System.Collections.Generic;

namespace MaxLengthAttributeTests.MaxLength
{
    public class MyBehaviour : NetworkBehaviour
    {
        [ServerRpc]
        public void SendString([MaxLength(10)] string message)
        {
        }

        [ServerRpc]
        public void SendArray([MaxLength(5)] int[] array)
        {
        }

        [ServerRpc]
        public void SendList([MaxLength(8)] List<float> list)
        {
        }

        [ServerRpc]
        public void SendCustomStruct([MaxLength(10)] MyCustomStructWithWriter custom)
        {
        }
    }

    public struct MyCustomStructWithWriter
    {
        public string Name;
        public int Value;
    }

    public static class MyCustomStructWithWriterExtensions
    {
        // Extension methods must explain why they are defined (to register length-limited serialization)
        public static void WriteMyStruct(this NetworkWriter writer, MyCustomStructWithWriter value, int maxLength)
        {
            writer.WriteString(value.Name, maxLength);
            writer.WriteInt32(value.Value);
        }

        public static MyCustomStructWithWriter ReadMyStruct(this NetworkReader reader, int maxLength)
        {
            return new MyCustomStructWithWriter
            {
                Name = reader.ReadString(maxLength),
                Value = reader.ReadInt32()
            };
        }
    }
}
