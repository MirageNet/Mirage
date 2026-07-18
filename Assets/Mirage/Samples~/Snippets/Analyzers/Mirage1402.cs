using Mirage;
using Mirage.Serialization;

namespace Mirage.Snippets.Analyzers
{
    namespace M1402.Triggering
    {
        // CodeEmbed-Start: mirage1402-triggering
        public class BasePlayer : NetworkBehaviour
        {
            [SyncVar]
            public string PlayerName;
        }

        public class HeroPlayer : BasePlayer
        {
            [SyncVar]
            public int HeroId;

            // Warning: Missing base.OnSerialize call
            public override bool OnSerialize(NetworkWriter writer, bool initialState)
            {
                writer.WritePackedInt32(HeroId);
                return true;
            }
        }
        // CodeEmbed-End: mirage1402-triggering
    }

    namespace M1402.Resolved
    {
        // CodeEmbed-Start: mirage1402-resolved
        public class BasePlayer : NetworkBehaviour
        {
            [SyncVar]
            public string PlayerName;
        }

        public class HeroPlayer : BasePlayer
        {
            [SyncVar]
            public int HeroId;

            // Correct: Calls base.OnSerialize and combines results
            public override bool OnSerialize(NetworkWriter writer, bool initialState)
            {
                bool baseDirty = base.OnSerialize(writer, initialState);
                writer.WritePackedInt32(HeroId);
                return baseDirty || true;
            }
        }
        // CodeEmbed-End: mirage1402-resolved
    }
}
