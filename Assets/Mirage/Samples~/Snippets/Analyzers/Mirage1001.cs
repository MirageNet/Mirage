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
            public PlayerData data { get; set; }
        }
        // CodeEmbed-End: mirage1001-triggering
    }

    namespace M1001.StructOption
    {
        // CodeEmbed-Start: mirage1001-struct-option
        public struct PlayerData
        {
            public int health;
            public string name;
        }

        public class Player : NetworkBehaviour
        {
            [SyncVar]
            public PlayerData data { get; set; }
        }
        // CodeEmbed-End: mirage1001-struct-option
    }

    namespace M1001.ClassOption
    {
        // CodeEmbed-Start: mirage1001-class-option
        [WeaverSafeClass]
        public class PlayerData
        {
            public int health;
            public string name;
        }
        // CodeEmbed-End: mirage1001-class-option
    }

    namespace M1001.SuppressOption
    {
        // CodeEmbed-Start: mirage1001-suppress-option
        public class PlayerData
        {
            public int health;
            public string name;
        }

        public class Player : NetworkBehaviour
        {
            [SyncVar]
            [WeaverSafeClass]
            public PlayerData data { get; set; }
        }
        // CodeEmbed-End: mirage1001-suppress-option
    }
}
