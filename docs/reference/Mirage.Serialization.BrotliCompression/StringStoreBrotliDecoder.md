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

#### StringStoreBrotliDecoder(IMessageReceiver)



##### Declaration

```cs
public StringStoreBrotliDecoder(IMessageReceiver receiver)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.IMessageReceiver | receiver |  |

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


