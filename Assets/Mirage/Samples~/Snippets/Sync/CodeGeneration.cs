using Mirage;
using Mirage.Serialization;

namespace Mirage.Snippets.Sync
{
    // CodeEmbed-Start: CodeGenerationExample
    public class Data : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnInt1Changed))]
        public int int1 { get; set; } = 66;

        [SyncVar]
        public int int2 { get; set; } = 23487;

        [SyncVar]
        public string MyString { get; set; } = "Example string";

        void OnInt1Changed(int oldValue, int newValue)
        {
            // do something here
        }
    }
    // CodeEmbed-End: CodeGenerationExample

    public class GeneratedCodeExample : NetworkBehaviour
    {
        public int int1;
        public int int2;
        public string MyString;

        public void OnInt1Changed(int oldValue, int newValue)
        {
        }

        // CodeEmbed-Start: SerializeSyncVarsExample
        public override bool SerializeSyncVars(NetworkWriter writer, bool initialState)
        {
            // Write any SyncVars in base class
            bool written = base.SerializeSyncVars(writer, initialState);

            if (initialState)
            {
                // The first time a game object is sent to a client, send all the data (and no dirty bits)
                writer.WritePackedUInt32((uint)this.int1);
                writer.WritePackedUInt32((uint)this.int2);
                writer.Write(this.MyString);
                return true;
            }
            else 
            {
                // Writes which SyncVars have changed
                writer.WritePackedUInt64(base.SyncVarDirtyBits);

                if ((base.SyncVarDirtyBits & 1u) != 0u)
                {
                    writer.WritePackedUInt32((uint)this.int1);
                    written = true;
                }

                if ((base.SyncVarDirtyBits & 2u) != 0u)
                {
                    writer.WritePackedUInt32((uint)this.int2);
                    written = true;  
                }

                if ((base.SyncVarDirtyBits & 4u) != 0u)
                {
                    writer.Write(this.MyString);
                    written = true;     
                }

                return written;
            }
        }
        // CodeEmbed-End: SerializeSyncVarsExample

        // CodeEmbed-Start: DeserializeSyncVarsExample
        public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
        {
            // Read any SyncVars in base class
            base.DeserializeSyncVars(reader, initialState);

            if (initialState)
            {
                // The first time a game object is sent to a client, read all the data (and no dirty bits)
                int oldInt1 = this.int1;
                this.int1 = (int)reader.ReadPackedUInt32();
                // if old and new values are not equal, call hook
                if (!base.SyncVarEqual<int>(oldInt1, this.int1))
                {
                    this.OnInt1Changed(oldInt1, this.int1);
                }

                this.int2 = (int)reader.ReadPackedUInt32();
                this.MyString = reader.ReadString();
                return;
            }

            int dirtySyncVars = (int)reader.ReadPackedUInt32();
            // is 1st SyncVar dirty
            if ((dirtySyncVars & 1) != 0)
            {
                int oldInt1 = this.int1;
                this.int1 = (int)reader.ReadPackedUInt32();
                // if old and new values are not equal, call hook
                if (!base.SyncVarEqual<int>(oldInt1, this.int1))
                {
                    this.OnInt1Changed(oldInt1, this.int1);
                }
            }

            // is 2nd SyncVar dirty
            if ((dirtySyncVars & 2) != 0)
            {
                this.int2 = (int)reader.ReadPackedUInt32();
            }

            // is 3rd SyncVar dirty
            if ((dirtySyncVars & 4) != 0)
            {
                this.MyString = reader.ReadString();
            }
        }
        // CodeEmbed-End: DeserializeSyncVarsExample
    }
}
