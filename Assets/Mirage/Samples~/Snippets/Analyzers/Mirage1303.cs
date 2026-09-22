using Mirage;
using Mirage.Serialization;

namespace Mirage.Snippets.Analyzers
{
    namespace M1303.Triggering
    {
        // CodeEmbed-Start: mirage1303-triggering
        public struct CustomType
        {
            public int value;
        }

        public static class CustomSerialization
        {
            // MIRAGE1303: Add a custom reader for this fixed-width encoding.
            public static void WriteCustomType(this NetworkWriter writer, CustomType value)
            {
                writer.WriteInt32(value.value);
            }
        }
        // CodeEmbed-End: mirage1303-triggering
    }

    namespace M1303.Resolved
    {
        // CodeEmbed-Start: mirage1303-resolved
        public struct CustomType
        {
            public int value;
        }

        public static class CustomSerialization
        {
            // Correct: Both methods use the same fixed-width encoding.
            public static void WriteCustomType(this NetworkWriter writer, CustomType value)
            {
                writer.WriteInt32(value.value);
            }

            public static CustomType ReadCustomType(this NetworkReader reader)
            {
                return new CustomType { value = reader.ReadInt32() };
            }
        }

        public struct LengthCustomType
        {
            public byte[] data;
        }

        public static class LengthCustomSerialization
        {
            // Encode the actual count and enforce the supplied maximum.
            public static void WriteLengthCustomType(this NetworkWriter writer, LengthCustomType value, int maxLength)
            {
                writer.WriteBytesAndSize(value.data, maxLength);
            }

            public static LengthCustomType ReadLengthCustomType(this NetworkReader reader, int maxLength)
            {
                return new LengthCustomType { data = reader.ReadBytesAndSize(maxLength) };
            }
        }

        [NetworkMessage]
        public struct BoundedDataMessage
        {
            [MaxLength(20)] public LengthCustomType payload;
        }
        // CodeEmbed-End: mirage1303-resolved
    }
}
