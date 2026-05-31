# MIRAGE1501: Network Message Exceeds Safe MTU

## The Problem
A `[NetworkMessage]` struct/class has a static or maximum serialized size that exceeds the safe Maximum Transmission Unit (MTU) of the transport layer (typically 1200 - 1400 bytes).

If a single message size exceeds the MTU, it must be fragmented at the transport or IP layer. IP fragmentation increases packet loss rates, latency, and connection instability. Designing messages that stay within the safe MTU boundary improves network reliability and performance.

---

## Example of Triggering Code
```csharp
using Mirage;

[NetworkMessage]
public struct HugeMessage
{
    // Warning: Array size and primitives exceed the safe MTU threshold
    public byte[] largeBuffer; // e.g. filled with 2048 bytes of data
}
```

---

## How to Resolve

Break large messages down into smaller chunks, use compression, or send raw bulk data using a streaming/chunking API instead of a single massive NetworkMessage.

```csharp
using Mirage;

[NetworkMessage]
public struct ChunkMessage
{
    public int chunkIndex;
    // Correct: Small buffer sizes that fit comfortably within a single MTU packet
    public byte[] smallBuffer; // e.g. limited to 512 bytes per chunk
}
```
