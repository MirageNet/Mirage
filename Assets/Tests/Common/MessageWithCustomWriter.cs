using Mirage.Serialization;

namespace Mirage.Tests
{
    public struct MessageWithCustomWriter
    {
        public int type;
        public float value;
    }

    public static class MessageWithCustomWriterExtesions
    {
        public const int WriteSize = 16;

        // flaged used by test to check methods are called
        public static int WriterCalled;
        public static int ReaderCalled;

        public static void Write(this NetworkWriter writer, MessageWithCustomWriter value)
        {
            WriterCalled++;
            writer.Write((ulong)value.type, 4);
            writer.Write((ulong)(value.value * 100), 12);
        }
        public static MessageWithCustomWriter Read(this NetworkReader reader)
        {
            ReaderCalled++;
            MessageWithCustomWriter value = default;
            value.type = (int)reader.Read(4);
            value.value = reader.Read(12) / 100f;
            return value;
        }
    }

    [NetworkMessage]
    public struct MessageWitAutoWriter
    {
        public int type;
        public float value;
    }

    // no NetworkMessage, this type wont have extension methods in this assembly
    public struct MessageWithNoWriter
    {
        public int type;
        public float value;
    }
}
