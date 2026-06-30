using Mirage;
using Mirage.Serialization;

namespace Mirage.Snippets.Analyzers
{
    namespace M1201.Triggering
    {
        // CodeEmbed-Start: mirage1201-triggering
        [NetworkMessage]
        public struct UpdateUserMessage
        {
            // Warning: Class types cause GC allocations during deserialization and lack change tracking.
            public UserData data;
        }

        public class UserData
        {
            public string name;
        }
        // CodeEmbed-End: mirage1201-triggering
    }

    namespace M1201.Resolved
    {
        // CodeEmbed-Start: mirage1201-struct-option
        [NetworkMessage]
        public struct UpdateUserMessage
        {
            public UserDataStruct data;
        }

        // Correct: Structs avoid memory allocation issues on deserialization.
        public struct UserDataStruct
        {
            public string name;
        }
        // CodeEmbed-End: mirage1201-struct-option

        // CodeEmbed-Start: mirage1201-class-option
        // Correct: WeaverSafeClass confirms custom serialization handles allocations safely.
        [WeaverSafeClass]
        public class UserDataClass
        {
            public string name;
        }

        public static class UserDataSerializer
        {
            public static void WriteUserData(this NetworkWriter writer, UserDataClass data)
            {
                writer.WriteString(data.name);
            }

            public static UserDataClass ReadUserData(this NetworkReader reader)
            {
                return new UserDataClass { name = reader.ReadString() };
            }
        }
        // CodeEmbed-End: mirage1201-class-option

        // CodeEmbed-Start: mirage1201-suppress-option
        [NetworkMessage]
        public struct UpdateUserMessageWithSuppressed
        {
            // Correct: WeaverSafeClass on individual members overrides validation warnings.
            [WeaverSafeClass]
            public UserDataClassWithoutAttribute data;
        }

        public class UserDataClassWithoutAttribute
        {
            public string name;
        }
        // CodeEmbed-End: mirage1201-suppress-option
    }
}
