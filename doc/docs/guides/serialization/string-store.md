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

{{{ Path:'Snippets/Serialization/StringStoreSnippets.cs' Name:'mission-example' }}}


## Brotli Compression (Advanced)

For extremely large sets of strings, Mirage provides a `StringStoreBrotliEncoder` helper. This is primarily intended for **Server -> Client** messages where you might be sending a large "World State" or "Manifest" containing hundreds of unique strings. For strings that are very similar (like file paths or long identifiers), this can achieve 90% or more compression.

### Implementation Logic

The `StringStoreBrotliEncoder` handles the compression and prepares raw payloads. Because these payloads can be large, they are sent as two separate reliable messages: one for metadata/lengths and one for the compressed content.

### Usage Example 

Because Brotli compression is CPU-intensive, you should **Encode once and reuse** the encoder for all players, including those who join the session late.

{{{ Path:'Snippets/Serialization/StringStoreSnippets.cs' Name:'brotli-example' }}}
