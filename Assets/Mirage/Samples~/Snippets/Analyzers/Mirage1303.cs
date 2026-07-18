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
            // Error: Missing matching custom reader
            public static void WriteCustomType(this NetworkWriter writer, CustomType value)
            {
                writer.WritePackedInt32(value.value);
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
            // Correct: Writer and reader signatures match
            public static void WriteCustomType(this NetworkWriter writer, CustomType value)
            {
                writer.WritePackedInt32(value.value);
            }

            public static CustomType ReadCustomType(this NetworkReader reader)
            {
                return new CustomType { value = reader.ReadPackedInt32() };
            }
        }

        public struct LengthCustomType
        {
            public byte[] data;
        }

        public static class LengthCustomSerialization
        {
            // Correct: Length-based signatures match
            public static void WriteLengthCustomType(this NetworkWriter writer, LengthCustomType value, int length)
            {
                writer.WriteBytes(value.data, 0, length);
            }

            public static LengthCustomType ReadLengthCustomType(this NetworkReader reader, int length)
            {
                return new LengthCustomType { data = reader.ReadBytes(length) };
            }
        }
        // CodeEmbed-End: mirage1303-resolved
    }
}
