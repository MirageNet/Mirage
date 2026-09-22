# MIRAGE1501: Network Message Serialized Size Estimation

## Description
**Proposed analyzer contract:** report a provable payload size or bound for `[NetworkMessage]` types when their serialization can be modeled. This page specifies intended behavior, not a claim that the current analyzer already implements it.

The result describes the selected serializer starting at bit offset zero, under the stated default-serializer configuration without runtime delegate reassignment. It excludes the message ID and transport framing. `MessagePacker` adds a fixed **2-byte message ID**; socket batching, packet headers, reliability and fragmentation add separate costs. A payload result is not a total bandwidth prediction.

## Result Categories

| Result | Meaning |
|---|---|
| Exact | Every successfully serialized value has the same bit count under the stated assumptions. Report bits and the final rounded byte count. |
| Bounded | A proven minimum and maximum; identify conservative bounds rather than present a minimum as an average. |
| Variable | A known value-dependent format for which the supported analysis cannot provide a useful total bound. |
| Unknown | Custom, manual, unresolved or otherwise unmodeled serialization prevents a reliable result. Include the reason. |

Never count an unknown member as zero, or display a known fixed portion as the whole message size.

---

## Example
{{{ Path:'Snippets/Analyzers/Mirage1501.cs' Name:'mirage1501-example' }}}

The default packed `int` uses 1–5 bytes, while `Vector3` uses 12 bytes. Thus this message has a **13–17 byte payload**, or **15–19 bytes including the message ID**. For example, `id = 0` uses one byte; `id = int.MaxValue` uses five. Neither value establishes a typical message size.

---

## Size Estimation Details

The model must follow the selected serializer, not the type's in-memory size:

* Follow generated field inclusion, inherited fields and serialization order, including the [current field-filter caveat](./MIRAGE1301.md). Skip properties, static fields and explicitly ignored fields. A custom serializer replaces that field-layout assumption.
* Count in bits. Booleans use 1 bit; byte/sbyte use 8; short/ushort/char use 16; float/double/decimal use 32/64/128. Default int/uint and long/ulong use variable packed encodings of 1–5 and 1–9 bytes. Enums use their underlying type's selected serializer.
* `Vector3` uses 96 bits. `Quaternion` is already compressed to **29 bits** by default. Explicit fixed packing attributes select their validated bit widths; variable packing also has discriminator or block-prefix costs.
* Do not round every field to a byte. Ordinary scalar writes continue at the current bit position. Raw byte copies, including string data, byte-array data and Guid data, align to the next byte. Round the final payload once, while retaining any internal alignment costs.
* Generated classes and nullable values write a presence bit, then their contents only when present. Class inheritance does not add another presence bit for every base class.
* In normal UTF-8 string mode, null is a 16-bit zero prefix. A non-null string writes a 16-bit encoded-byte-count-plus-one, then aligns and copies its UTF-8 bytes. `[MaxLength(n)]` limits UTF-16 string length, not encoded byte count. The global string-byte limit is mutable. `StringStore` uses a different, state-dependent representation and must be modeled separately or reported as unknown.
* Lists, arrays and dictionaries use a packed count-plus-one, with zero representing null. This prefix is not always one byte. Specialized byte-array serializers also align their raw data; generic element serialization need not align it.
* `[MaxLength]` supplies a maximum, not a fixed element count. It yields a useful bound only when the selected limit serializer is understood and every element's contribution is bounded. For custom serializers, merely receiving this argument does not establish any wire-size bound.

For example, two Boolean fields occupy 2 bits and round to 1 payload byte. A Quaternion followed by a Boolean occupies 30 bits and rounds to 4 bytes. With no `StringStore`, `bool; string; bool` occupies 18 bits when the string is null but 25 bits when it is empty: the non-null path aligns its data even at length zero.

## Implementable Initial Scope

Start with generated layouts, known primitive and Unity serializers, enums, class/nullable presence flags, explicit fixed packing and default packed-integer bounds. Compose sizes with bit offsets and alignment; if serialization order or a selected serializer cannot be established, fall back conservatively.

Known unconstrained strings and collections can report variable until useful bounds are supported. A first bounded collection case is `[MaxLength(240)] byte[]`: at offset zero its default payload is 1–242 bytes, including its variable count prefix. Add other bounded collections only when their element serializers are understood.

Custom extension bodies, manual `Writer<T>` dispatch, unresolved or ambiguous registrations and recursive type graphs should initially report unknown with a reason. Do not analyze their public fields as a substitute for their actual wire format. A bounded outer collection does not bound recursive elements. Future support for these cases requires an explicit, verifiable serialization contract or runtime measurement.
