---
id: FloatPacker
title: FloatPacker
---

# Class FloatPacker


Helps compresses a float into a reduced number of bits



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
public sealed class FloatPacker
```

### Constructors

#### FloatPacker(Single, Single)



##### Declaration

```cs
public FloatPacker(float max, float lowestPrecision)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Single | max |  |
| System.Single | lowestPrecision | lowest precision, actual precision will be caculated from number of bits used |

#### FloatPacker(Single, Int32)



##### Declaration

```cs
public FloatPacker(float max, int bitCount)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Single | max |  |
| System.Int32 | bitCount |  |

#### FloatPacker(Single, Single, Boolean)



##### Declaration

```cs
public FloatPacker(float max, float lowestPrecision, bool signed)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Single | max |  |
| System.Single | lowestPrecision | lowest precision, actual precision will be caculated from number of bits used |
| System.Boolean | signed | if negative values will be allowed or not |

#### FloatPacker(Single, Int32, Boolean)



##### Declaration

```cs
public FloatPacker(float max, int bitCount, bool signed)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Single | max |  |
| System.Int32 | bitCount |  |
| System.Boolean | signed | if negative values will be allowed or not |
### Methods
#### Pack(Single)


Packs a float value into a uint
Clamps the value within min/max range



##### Declaration

```cs
public uint Pack(float value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Single | value |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt32 |  |

#### Pack(NetworkWriter, Single)


Packs and Writes a float value
Clamps the value within min/max range



##### Declaration

```cs
public void Pack(NetworkWriter writer, float value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.Single | value |  |


#### PackNoClamp(Single)


Packs a float value into a uint without clamping it in range

WARNING: only use this method if value is always in range. Out of range values may not be unpacked correctly




##### Declaration

```cs
public uint PackNoClamp(float value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Single | value |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt32 |  |

#### PackNoClamp(NetworkWriter, Single)



##### Declaration

```cs
public void PackNoClamp(NetworkWriter writer, float value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.Single | value |  |


#### Unpack(UInt32)


Unpacks uint value to float



##### Declaration

```cs
public float Unpack(uint value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt32 | value |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Single |  |

#### Unpack(NetworkReader)


Reads and unpacks float value



##### Declaration

```cs
public float Unpack(NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Single |  |

