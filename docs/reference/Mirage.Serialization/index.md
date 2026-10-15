---
id: Mirage.Serialization
title: Mirage.Serialization
---

# Mirage.Serialization

## Classes

#### [AnglePacker](./AnglePacker)
#### [BitCountAttribute](./BitCountAttribute)
> 
Tells weaver how many bits to sue for field
Only works with integer fields (byte, int, ulong, enums etc)

NOTE: bits are truncated when using this, so signed values will lose their sign. Use  as well if value might be negative

Also See: Bit Packing Documentation

#### [BitCountFromRangeAttribute](./BitCountFromRangeAttribute)
> 
Calculates bitcount from then given min/max values and then packs using 
Also See: Bit Packing Documentation

#### [BitHelper](./BitHelper)
#### [BitMask](./BitMask)
#### [CollectionExtensions](./CollectionExtensions)
#### [CompressedExtensions](./CompressedExtensions)
#### [FloatPackAttribute](./FloatPackAttribute)
> 
Packs a float field, clamped from -max to +max, with
Also See: Bit Packing Documentation

#### [FloatPacker](./FloatPacker)
> 
Helps compresses a float into a reduced number of bits

#### [FromBitCount](./FromBitCount)
> 
The max value for N number of bits

#### [GenericTypesSerializationExtensions](./GenericTypesSerializationExtensions)
#### [MessageIdCache&lt;T&gt;](./MessageIdCache-1)
> 
Class that will cache the ID for type T
avoids needing to calculate the stable hash of the full name each time a message is sent

#### [MessagePacker](./MessagePacker)
#### [MirageNetworkReader](./MirageNetworkReader)
> 
NetworkReader but has a ObjectLocator field that can be used by Reader functions to fetch NetworkIdentity

#### [MirageTypesExtensions](./MirageTypesExtensions)
#### [NetworkReader](./NetworkReader)
> 
Bit writer, writes values to a buffer on a bit level
Use  to reduce memory allocation

#### [NetworkReaderPool](./NetworkReaderPool)
> 
Holds static reference to  of 

#### [NetworkWriter](./NetworkWriter)
> 
Bit writer, writes values to a buffer on a bit level
Use  to reduce memory allocation

#### [NetworkWriterPool](./NetworkWriterPool)
#### [PackedExtensions](./PackedExtensions)
#### [PooledNetworkReader](./PooledNetworkReader)
> 
NetworkReader to be used with 

#### [PooledNetworkWriter](./PooledNetworkWriter)
> 
NetworkWriter to be used with 

#### [QuaternionPackAttribute](./QuaternionPackAttribute)
#### [QuaternionPacker](./QuaternionPacker)
#### [Reader&lt;T&gt;](./Reader-1)
> 
a class that holds readers for the different types
Note that c# creates a different static variable for each
type
This will be populated by the weaver

#### [SerializationLimitException](./SerializationLimitException)
#### [StringExtensions](./StringExtensions)
#### [StringStore](./StringStore)
#### [StringStoreExtensions](./StringStoreExtensions)
> Default write/read methods for , using 
#### [SystemTypesExtensions](./SystemTypesExtensions)
#### [UnityTypesExtensions](./UnityTypesExtensions)
#### [VarDoublePacker](./VarDoublePacker)
> 
Packs a double using  and 

#### [VarFloatPacker](./VarFloatPacker)
> 
Packs a float using  and 

#### [VarIntAttribute](./VarIntAttribute)
> 
Tells weaver the max range for small, medium and large values.
Allows small values to be sent using less bits
Only works with integer fields (byte, int, ulong, enums etc)

#### [VarIntBlocksAttribute](./VarIntBlocksAttribute)
> 
Tells weaver the block size to use for packing int values
Allows small values to be sent using less bits
Only works with integer fields (byte, int, ulong, enums etc)

#### [VarIntBlocksPacker](./VarIntBlocksPacker)
#### [VarIntPacker](./VarIntPacker)
#### [VarVector2Packer](./VarVector2Packer)
> 
Packs a vector3 using  and 

#### [VarVector3Packer](./VarVector3Packer)
> 
Packs a vector3 using  and 

#### [Vector2PackAttribute](./Vector2PackAttribute)
#### [Vector2Packer](./Vector2Packer)
#### [Vector3PackAttribute](./Vector3PackAttribute)
#### [Vector3Packer](./Vector3Packer)
#### [WeaverIgnoreAttribute](./WeaverIgnoreAttribute)
> 
Tells Weaver to ignore a field or Method

#### [WeaverSerializeCollectionAttribute](./WeaverSerializeCollectionAttribute)
> 
Tells weaver to use this method to write a generic type or collection
Can also be used for other generic types like Nullable

#### [WeaverWriteAsGenericAttribute](./WeaverWriteAsGenericAttribute)
> 
Tells Weaver to serialize a type as generic instead of creating a custom functions.

Use this when you have created and assigned your own Read/Write functions 
or when you can&apos;t use extension methods for types and need to manually assign them.


This will cause Weaver to use the  and  generic functions instead of creating new ones.
You must set these functions manually when using this attribute.


#### [Writer&lt;T&gt;](./Writer-1)
> 
a class that holds writers for the different types
Note that c# creates a different static variable for each
type
This will be populated by the weaver

#### [ZigZag](./ZigZag)
> 
See zigzag encoding

#### [ZigZagEncodeAttribute](./ZigZagEncodeAttribute)
> 
Used along size  to encodes a integer value using  so that both positive and negative values can be sent
Also See: Bit Packing Documentation

