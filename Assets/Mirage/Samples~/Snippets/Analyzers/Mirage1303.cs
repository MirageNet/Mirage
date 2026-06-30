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
            // Error: Custom writer defined but matching custom reader is missing
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
            // Correct: Both writer and reader are defined with matching signatures
            public static void WriteCustomType(this NetworkWriter writer, CustomType value)
            {
                writer.WritePackedInt32(value.value);
            }

            public static CustomType ReadCustomType(this NetworkReader reader)
            {
                return new CustomType { value = reader.ReadPackedInt32() };
            }
        }
        // CodeEmbed-End: mirage1303-resolved
    }
}
