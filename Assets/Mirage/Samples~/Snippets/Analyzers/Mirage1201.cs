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
            // Warning: Class types cause garbage collection allocations during deserialization.
            public UserData data;
        }

        public class UserData
        {
            public string name;
        }
        // CodeEmbed-End: mirage1201-triggering
    }

    namespace M1201.Recommended
    {
        // CodeEmbed-Start: mirage1201-recommended
        [NetworkMessage]
        public struct UpdateUserMessage
        {
            public UserDataStruct data;
        }
        public struct UserDataStruct
        {
            public string name;
        }
        // CodeEmbed-End: mirage1201-recommended
    }

    namespace M1201.AlternativeCustom
    {
        // CodeEmbed-Start: mirage1201-alternative-custom
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
        // CodeEmbed-End: mirage1201-alternative-custom
    }

    namespace M1201.AlternativeSuppress
    {
        // CodeEmbed-Start: mirage1201-alternative-suppress
        [NetworkMessage]
        public struct UpdateUserMessageWithSuppressed
        {
            [WeaverSafeClass]
            public UserDataClassWithoutAttribute data;
        }
        public class UserDataClassWithoutAttribute
        {
            public string name;
        }
        // CodeEmbed-End: mirage1201-alternative-suppress
    }

}
