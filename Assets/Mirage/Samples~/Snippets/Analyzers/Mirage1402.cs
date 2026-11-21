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
            public int HeroId;

            // Warning: Missing base.OnSerialize call
            public override bool OnSerialize(NetworkWriter writer, bool initialState)
            {
                writer.WritePackedInt32(HeroId);
                return true;
            }

            // Warning: Missing base.OnDeserialize call
            public override void OnDeserialize(NetworkReader reader, bool initialState)
            {
                HeroId = reader.ReadPackedInt32();
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
            public int HeroId;

            public void SetHeroId(int value)
            {
                HeroId = value;
                SetDirtyBit(ulong.MaxValue);
            }

            // Correct: Preserve generated state, then append custom data
            public override bool OnSerialize(NetworkWriter writer, bool initialState)
            {
                base.OnSerialize(writer, initialState);
                writer.WritePackedInt32(HeroId);
                return true; // This example always writes custom data
            }

            public override void OnDeserialize(NetworkReader reader, bool initialState)
            {
                base.OnDeserialize(reader, initialState);
                HeroId = reader.ReadPackedInt32();
            }
        }
        // CodeEmbed-End: mirage1402-resolved
    }
}
