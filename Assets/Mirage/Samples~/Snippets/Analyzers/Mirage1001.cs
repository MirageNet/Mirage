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
        }
        // CodeEmbed-End: mirage1001-recommended
    }

    namespace M1001.AlternativeCustom
    {
        // CodeEmbed-Start: mirage1001-alternative-custom
        [WeaverSafeClass]
        public class PlayerData
        {
            public int health;
            public string name;
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
