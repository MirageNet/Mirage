# MIRAGE1501: Network Message Serialized Size Estimation

**Status: Proposed.**

## When this appears

Shows how much data a `[NetworkMessage]` can send.

Use this to spot large messages and decide where sending less data would save bandwidth. The result covers message data; the **2-byte message ID** and transport overhead are extra.

## Reading the result

| Result | Meaning |
| --- | --- |
| Exact | Same size for every value; shown in bits and rounded bytes. |
| Bounded | Proven lower and upper bounds, marked when conservative. This is not an average. |
| Variable | Size depends on the value; no useful total bound is available. |
| Unknown | Size cannot be determined for this serializer; the reason is shown. |

Unknown data never counts as zero, and a known part is never shown as the full message size.

## Example

{{{ Path:'Snippets/Analyzers/Mirage1501.cs' Name:'mirage1501-example' }}}

The packed `int` uses 1–5 bytes and `Vector3` uses 12: **13–17 bytes before the message ID**, or **15–19 with it**.

## What affects size

| Encoding | Size or behavior |
| --- | --- |
| Boolean / Quaternion | 1 bit / 29 bits by default. |
| Generated classes / nullable values | One presence bit, then contents if present; no extra flag per base class. |
| Normal UTF-8 string | 16-bit prefix; non-null data aligns to a byte. `[MaxLength]` counts UTF-16 code units, not bytes. |
| Collections | Variable packed count-plus-one; zero means null. Raw byte-array data aligns to a byte. |

Sizes follow the chosen serializers, packing attributes, and [fields in serialization order](./MIRAGE1301.md), including inheritance and exclusions. Scalars share bits; raw string, byte-array and Guid data can add alignment. Only the final total is rounded to bytes.

## Limits

- Estimates start on a byte boundary and assume known default serializers with no runtime writer reassignment. Only values that serialize successfully are counted.
- Strings and collections without a known limit may report Variable. Mutable string limits and `StringStore` need an explicit supported configuration.
- `[MaxLength]` gives a bound only when its serializer is understood and each element has a known maximum size. It is not a fixed length or a size guarantee for custom formats.
- Custom/manual serializers, unresolved or ambiguous registrations, unknown field order, recursive types, and other formats without size support report Unknown. Their fields cannot stand in for their actual wire format.
