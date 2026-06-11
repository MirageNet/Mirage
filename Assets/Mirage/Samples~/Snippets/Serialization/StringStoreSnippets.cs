using System;
using UnityEngine;
using Mirage;
using Mirage.Serialization;
using Mirage.Serialization.BrotliCompression;

namespace Mirage.Snippets.Serialization.StringStoreSnippets
{
    // CodeEmbed-Start: mission-example
    public class Mission
    {
        public void OnSerialize(NetworkWriter writer)
        {
            // serialize mission here
        }

        public void OnDeserialize(NetworkReader reader)
        {
            // deserialize mission here
        }
    }

    public static class MissionExtensions
    {
        public static void WriteMission(this NetworkWriter finalWriter, Mission mission)
        {
            // Get a temporary writer from the pool
            using (PooledNetworkWriter innerWriter = NetworkWriterPool.GetWriter())
            {
                // Create a new store and attach it to the temporary writer
                StringStore stringStore = new StringStore();
                innerWriter.StringStore = stringStore;

                // Write the mission data. 
                // Any repeated strings (like Objective titles or NPC names) 
                // will be indexed in 'stringStore'.
                mission.OnSerialize(innerWriter);

                // Write the populated store to the REAL writer first
                finalWriter.WriteStringStore(stringStore);
                
                // Write the actual message data as a segment
                finalWriter.WriteBytesAndSizeSegment(innerWriter.ToArraySegment());
            }
        }

        public static Mission ReadMission(this NetworkReader finalReader)
        {
            // Read the StringStore that was sent first
            StringStore stringStore = finalReader.ReadStringStore();

            // Read the data segment containing the mission
            ArraySegment<byte> segment = finalReader.ReadBytesAndSizeSegment();

            // Get a pooled reader for the segment and attach the store
            using (PooledNetworkReader innerReader = NetworkReaderPool.GetReader(segment, null))
            {
                innerReader.StringStore = stringStore;

                // 4. Deserialize the mission. 
                // ReadString calls will now correctly resolve indices using the store.
                var mission = new Mission();
                mission.OnDeserialize(innerReader);
                return mission;
            }
        }
    }
    // CodeEmbed-End: mission-example

    // CodeEmbed-Start: brotli-example
    // SERVER: Compressing and caching
    public class WorldServer : MonoBehaviour
    {
        // keep the StringStoreBrotliEncoder (and its results) so that it can be sent to new players
        // this is to avoid heavy cpu encoding every time a new player joins
        private StringStoreBrotliEncoder _worldEncoder;

        public void InitializeWorld(WorldData world)
        {
            StringStore store = new StringStore();
            // ... populate store by writing world data to a temp writer ...

            // Create the encoder once. This performs the heavy compression logic.
            _worldEncoder = StringStoreBrotliEncoder.Encode(store);
        }

        public void OnPlayerJoin(INetworkPlayer player)
        {
            // Send the pre-compressed payloads to the player.
            // This is very fast as it just sends cached byte segments.
            _worldEncoder.Send(player);
        }
    }

    // CLIENT: Receiving
    public class MissionManager : MonoBehaviour
    {
        private StringStoreBrotliDecoder _decoder;

        public void Start()
        {
            // Initialize the decoder with the Network Client.
            // NOTE: StringStoreBrotliDecoder will only receive 1 set of messages, it will unregister the message handlers after it as received one
            _decoder = new StringStoreBrotliDecoder(Client.Instance);
            
            // Subscribe to the completion event
            _decoder.OnReceived += () => 
            {
                Debug.Log("Strings Received! Ready to deserialize mission.");
                ProcessMission(_decoder.StringStore);
            };
        }

        private void ProcessMission(StringStore store)
        {
            // Use the store in your Readers as shown in the previous example
        }
    }

    // Dummy classes to make the example compile
    public class WorldData {}
    public static class Client
    {
        public static IMessageReceiver Instance => null;
    }
    // CodeEmbed-End: brotli-example
}
