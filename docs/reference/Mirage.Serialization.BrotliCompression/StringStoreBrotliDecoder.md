---
id: StringStoreBrotliDecoder
title: StringStoreBrotliDecoder
---

# Class StringStoreBrotliDecoder


Used to receive the next StringStore sent.

Will unregister message handlers after receiving StringStore




<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>
</details>

##### Syntax

```cs
public class StringStoreBrotliDecoder
```

### Constructors

#### StringStoreBrotliDecoder(IMessageReceiver, Int32)



##### Declaration

```cs
public StringStoreBrotliDecoder(IMessageReceiver receiver, int maxUncompressedBytes = 67108864)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.IMessageReceiver | receiver |  |
| System.Int32 | maxUncompressedBytes | Max total size of all strings (in UTF-8 bytes) after decompression, see <xref href="Mirage.Serialization.BrotliCompression.StringStoreBrotliDecoder.DefaultMaxUncompressedBytes" data-throw-if-not-resolved="false"></xref> for why the limit exists |

### Fields

#### DefaultMaxUncompressedBytes

Default max total size of all strings (in UTF-8 bytes) after decompression.

The sizes come from the remote peer, so without a limit a tiny message could make us allocate gigabytes.
Peak memory is higher than this limit, because the bytes are also copied into managed UTF-16 strings (up to about 2x more for ASCII).



##### Declaration

```cs
public const int DefaultMaxUncompressedBytes = 67108864
```

### Properties

#### StringStore

##### Declaration

```cs
public StringStore StringStore { get; }
```
### Methods
#### HandleStringStoreLengthsMessage(StringStoreLengthsMessage)



##### Declaration

```cs
public void HandleStringStoreLengthsMessage(StringStoreLengthsMessage message)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.BrotliCompression.StringStoreLengthsMessage | message |  |


#### HandleStringStoreStringsMessage(StringStoreStringsMessage)



##### Declaration

```cs
public void HandleStringStoreStringsMessage(StringStoreStringsMessage message)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.BrotliCompression.StringStoreStringsMessage | message |  |


