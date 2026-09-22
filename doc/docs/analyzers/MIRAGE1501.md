# MIRAGE1501: Network Message Serialized Size Estimation

**Status: Proposed.**

Reports `[NetworkMessage]` payload sizes with known default serializers, no runtime writer reassignment, and a fresh byte-aligned payload.

## Reading the result

| Result | Meaning |
| --- | --- |
| Exact | Fixed bit count for all successfully serialized values; also shows rounded bytes. |
| Bounded | Proven lower/upper bounds; labels conservative bounds. Not an average. |
| Variable | Known format without a useful total bound. |
| Unknown | Unmodeled serialization, with a reason; never counted as zero. |

Payload sizes exclude the **2-byte message ID** and transport overhead. A known fixed portion is never presented as the total.

## Example

{{{ Path:'Snippets/Analyzers/Mirage1501.cs' Name:'mirage1501-example' }}}

The packed `int` occupies 1–5 bytes and `Vector3` occupies 12: **13–17 payload bytes**, or **15–19 with the message ID**.

## What affects size

| Encoding | Size or behavior |
| --- | --- |
| Boolean / Quaternion | 1 bit / 29 bits by default. |
| Generated classes / nullable values | One presence bit, then contents if present; no extra flag per base class. |
| Normal UTF-8 string | 16-bit prefix; non-null data aligns to a byte. `[MaxLength]` counts UTF-16 code units, not bytes. |
| Collections | Variable packed count-plus-one; zero means null. Raw byte-array data aligns to a byte. |

Sizes follow selected serializers, packing attributes, and [serialized fields in order](./MIRAGE1301.md), including inheritance and exclusions. Scalars share bits; raw string, byte-array and Guid data can add alignment. Only the final total is rounded to bytes.

## Limits

- Known unconstrained strings and collections may report Variable. Mutable string limits and `StringStore` require an explicit supported configuration.
- `[MaxLength]` gives a bound only for understood limit serializers with bounded elements; it is not a fixed length or a custom-format size guarantee.
- Custom/manual serializers, unresolved or ambiguous registrations, unknown field order, recursive types and other unmodeled formats report Unknown. Declared fields cannot substitute for a custom wire format.
