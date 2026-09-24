---
id: UnityTypesExtensions
title: UnityTypesExtensions
---

# Class UnityTypesExtensions



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
public static class UnityTypesExtensions
```

### Methods
#### WriteVector2(NetworkWriter, Vector2)



##### Declaration

```cs
public static void WriteVector2(this NetworkWriter writer, Vector2 value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Vector2 | value |  |


#### WriteVector3(NetworkWriter, Vector3)



##### Declaration

```cs
public static void WriteVector3(this NetworkWriter writer, Vector3 value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Vector3 | value |  |


#### WriteVector4(NetworkWriter, Vector4)



##### Declaration

```cs
public static void WriteVector4(this NetworkWriter writer, Vector4 value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Vector4 | value |  |


#### WriteVector2Int(NetworkWriter, Vector2Int)



##### Declaration

```cs
public static void WriteVector2Int(this NetworkWriter writer, Vector2Int value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Vector2Int | value |  |


#### WriteVector3Int(NetworkWriter, Vector3Int)



##### Declaration

```cs
public static void WriteVector3Int(this NetworkWriter writer, Vector3Int value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Vector3Int | value |  |


#### WriteColor(NetworkWriter, Color)



##### Declaration

```cs
public static void WriteColor(this NetworkWriter writer, Color value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Color | value |  |


#### WriteColor32(NetworkWriter, Color32)



##### Declaration

```cs
public static void WriteColor32(this NetworkWriter writer, Color32 value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Color32 | value |  |


#### WriteRect(NetworkWriter, Rect)



##### Declaration

```cs
public static void WriteRect(this NetworkWriter writer, Rect value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Rect | value |  |


#### WritePlane(NetworkWriter, Plane)



##### Declaration

```cs
public static void WritePlane(this NetworkWriter writer, Plane value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Plane | value |  |


#### WriteRay(NetworkWriter, Ray)



##### Declaration

```cs
public static void WriteRay(this NetworkWriter writer, Ray value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Ray | value |  |


#### WriteMatrix4X4(NetworkWriter, Matrix4x4)



##### Declaration

```cs
public static void WriteMatrix4X4(this NetworkWriter writer, Matrix4x4 value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Matrix4x4 | value |  |


#### ReadVector2(NetworkReader)



##### Declaration

```cs
public static Vector2 ReadVector2(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Vector2 |  |

#### ReadVector3(NetworkReader)



##### Declaration

```cs
public static Vector3 ReadVector3(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Vector3 |  |

#### ReadVector4(NetworkReader)



##### Declaration

```cs
public static Vector4 ReadVector4(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Vector4 |  |

#### ReadVector2Int(NetworkReader)



##### Declaration

```cs
public static Vector2Int ReadVector2Int(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Vector2Int |  |

#### ReadVector3Int(NetworkReader)



##### Declaration

```cs
public static Vector3Int ReadVector3Int(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Vector3Int |  |

#### ReadColor(NetworkReader)



##### Declaration

```cs
public static Color ReadColor(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Color |  |

#### ReadColor32(NetworkReader)



##### Declaration

```cs
public static Color32 ReadColor32(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Color32 |  |

#### ReadRect(NetworkReader)



##### Declaration

```cs
public static Rect ReadRect(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Rect |  |

#### ReadPlane(NetworkReader)



##### Declaration

```cs
public static Plane ReadPlane(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Plane |  |

#### ReadRay(NetworkReader)



##### Declaration

```cs
public static Ray ReadRay(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Ray |  |

#### ReadMatrix4x4(NetworkReader)



##### Declaration

```cs
public static Matrix4x4 ReadMatrix4x4(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Matrix4x4 |  |

