using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1001.Triggering
    {
        // CodeEmbed-Start: mirage1001-triggering
        public class PlayerData
        {
            public int health;
            public string name;
        }

        public class Player : NetworkBehaviour
        {
            // Warns: SyncVar 'data' is a class type 'PlayerData'.
            [SyncVar]
            public PlayerData data;
        }
        // CodeEmbed-End: mirage1001-triggering
    }

    namespace M1001.Recommended
    {
        // CodeEmbed-Start: mirage1001-recommended
        public struct PlayerData
        {
            public int health;
            public string name;
        }
        public class Player : NetworkBehaviour
        {
            [SyncVar]
            public PlayerData data;

            // Call on the sending side configured by SyncSettings.
            public void DamagePlayer(int damage)
            {
                var next = data;
                next.health -= damage;
                data = next;
            }
        }
        // CodeEmbed-End: mirage1001-recommended
    }

    namespace M1001.AlternativeCustom
    {
        // CodeEmbed-Start: mirage1001-alternative-custom
        using Mirage.Serialization;

        // Acknowledges the class-payload warning; does not change runtime behavior.
        [WeaverSafeClass]
        public sealed class PlayerData
        {
            public int Health { get; }
            public string Name { get; }

            public PlayerData(int health, string name)
            {
                Health = health;
                Name = name;
            }
        }

        public static class PlayerDataSerialization
        {
            public static void WritePlayerData(this NetworkWriter writer, PlayerData value)
            {
                writer.WriteBoolean(value != null);
                if (value == null)
                    return;

                writer.WriteInt32(value.Health);
                writer.WriteString(value.Name);
            }

            public static PlayerData ReadPlayerData(this NetworkReader reader)
            {
                if (!reader.ReadBoolean())
                    return null;

                return new PlayerData(reader.ReadInt32(), reader.ReadString());
            }
        }

        public class Player : NetworkBehaviour
        {
            [SyncVar]
            public PlayerData data;

            // Call on the sending side configured by SyncSettings.
            public void SetPlayerData(int health, string name)
            {
                data = new PlayerData(health, name);
            }
        }
        // CodeEmbed-End: mirage1001-alternative-custom
    }

    namespace M1001.AlternativeSuppress
    {
        // CodeEmbed-Start: mirage1001-alternative-suppress
        public class PlayerData
        {
            public int health;
            public string name;
        }
        public class Player : NetworkBehaviour
        {
            [SyncVar]
            [WeaverSafeClass]
            public PlayerData data;
        }
        // CodeEmbed-End: mirage1001-alternative-suppress
    }

}
