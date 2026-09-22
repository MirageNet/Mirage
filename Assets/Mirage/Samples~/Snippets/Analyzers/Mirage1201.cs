using Mirage;
using Mirage.Serialization;

namespace Mirage.Snippets.Analyzers
{
    namespace M1201.Triggering
    {
        // CodeEmbed-Start: mirage1201-triggering
        using Mirage;

        [NetworkMessage]
        public struct UpdateUserMessage
        {
            // Warning: Reading a non-null UserData value creates a new object.
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
        using Mirage;

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
        using Mirage;
        using Mirage.Serialization;

        [WeaverSafeClass]
        public sealed class UserDataClass
        {
            public string name;
        }
        public static class UserDataSerializer
        {
            public static void WriteUserData(this NetworkWriter writer, UserDataClass data)
            {
                writer.WriteBoolean(data != null);
                if (data == null)
                    return;

                writer.WriteString(data.name);
            }
            public static UserDataClass ReadUserData(this NetworkReader reader)
            {
                if (!reader.ReadBoolean())
                    return null;

                return new UserDataClass { name = reader.ReadString() };
            }
        }
        // CodeEmbed-End: mirage1201-alternative-custom
    }

    namespace M1201.AlternativeSuppress
    {
        // CodeEmbed-Start: mirage1201-alternative-suppress
        using Mirage;

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

        public class Player : NetworkBehaviour
        {
            [ServerRpc]
            public void CmdUpdateUser([WeaverSafeClass] UserDataClassWithoutAttribute data)
            {
                // ...
            }
        }
        // CodeEmbed-End: mirage1201-alternative-suppress
    }

}
