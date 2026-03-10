# String Store

`StringStore` is an optional optimization that can be used when you are serializing a lot of duplicate strings, It can greatly reduce bandwidth, how ever requires some manual setup to get working.

## When to Use

- **Repeated Strings**: Best for messages that contain many instances of the same strings.
- **Fixed Sets**: Ideal for sending a set of strings that are known at the time of sending and won't change frequently during the session.
- **Large Payloads**: Useful when the memory overhead of the `StringStore` is smaller than the total size of duplicated raw strings in the packet.

## How it Works

1. You create or obtain a `StringStore` object.
2. You assign it to `writer.StringStore` or `reader.StringStore`.
3. When `writer.WriteString(value)` is called:
    - If the string is encountered for the first time, it's added to the store and assigned an index.
    - Subsequent calls with the same string write only the integer index.
4. **Crucially**, the `StringStore` contents must be sent to the receiver *before* any strings that depend on it can be read.


## Implementation Pattern

To make best use of `StringStore`, Network message should have `ArraySegment<byte> payload` field, and use `NetworkWriter` and `NetworkReader` where you can assign the `StringStore` before you start. 

You will then need to serialize the `StringStore` separately from your message and make sure it is sent before your message


### Example Using Pools

Using `NetworkWriterPool` and `NetworkReaderPool` is the most efficient way to handle the temporary buffers required for `StringStore`.

```csharp
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
```


## Brotli Compression (Advanced)

For extremely large sets of strings, Mirage provides a `StringStoreBrotliEncoder` helper. This is primarily intended for **Server -> Client** messages where you might be sending a large "World State" or "Manifest" containing hundreds of unique strings. For strings that are very similar (like file paths or long identifiers), this can achieve 90% or more compression.

### Implementation Logic

The `StringStoreBrotliEncoder` handles the compression and prepares raw payloads. Because these payloads can be large, they are sent as two separate reliable messages: one for metadata/lengths and one for the compressed content.

### Usage Example 

Because Brotli compression is CPU-intensive, you should **Encode once and reuse** the encoder for all players, including those who join the session late.

```csharp
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
```
