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
            public string PlayerName { get; set; }
        }

        public class HeroPlayer : BasePlayer
        {
            [SyncVar]
            public int HeroId { get; set; }

            // Warning: Overriding OnSerialize without calling base.OnSerialize
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
            public string PlayerName { get; set; }
        }

        public class HeroPlayer : BasePlayer
        {
            [SyncVar]
            public int HeroId { get; set; }

            // Correct: Calls base.OnSerialize and combines dirty states
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
