# MIRAGE1201: NetworkMessage/RPC Class Warning

## The Problem
This advisory rule asks you to review a class used as a network payload. Classes can be valid Mirage payloads; this is not a Weaver serialization error.

The rule checks the declared type of an eligible serialized field in a `[NetworkMessage]` using generated serialization, an RPC payload parameter, or the result `T` of an RPC returning `UniTask<T>`. Ordinary properties and fields ignored by generated serialization are outside this scope. Inherited serialized fields are included.

The policy excludes `string`, arrays, `List<T>`, `Dictionary<TKey, TValue>`, and Mirage's supported network references (`NetworkIdentity`, `NetworkBehaviour` and its subclasses, and networked `GameObject`). RPC sender/target `INetworkPlayer` parameters are connection context, not serialized payloads. This rule checks these declared payload types directly; it does not recursively inspect collection elements, nested payload members, or custom serializer bodies. Exemption from this warning does not prove that a type or its elements are serializable; see [MIRAGE1301](./MIRAGE1301.md).

Generated serialization normally constructs a new instance for each non-null class value and serializes the eligible fields of the declared type, including inherited fields. It does not preserve arbitrary object identity or automatically send fields added by a runtime subclass. Custom serializers can change this behavior.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1201.cs' Name:'mirage1201-triggering' }}}

---

## How to Resolve

### Recommended Fix: Use a struct
A small struct can avoid the outer class allocation. Its reference fields can still allocate during deserialization, and copying the struct copies those references rather than cloning their objects. The string in this example can still allocate.
{{{ Path:'Snippets/Analyzers/Mirage1201.cs' Name:'mirage1201-recommended' }}}

---

### Alternative Solutions
If the class representation is intentional, review its allocation and wire format before suppressing the warning.

#### 1. Implement Custom Serialization
Use a matching custom writer/reader pair when you need control over the wire format. The example preserves nulls and still allocates a class for a non-null value. It does not implement polymorphism or shared reference identity; those require an explicit format of your own.

After reviewing that tradeoff, `[WeaverSafeClass]` on the payload type suppresses this advisory at its use sites. The attribute is an analyzer annotation: it does not generate serializers, validate custom code, or change runtime behavior. A custom serializer alone is not proof that allocation or reference semantics are safe.
{{{ Path:'Snippets/Analyzers/Mirage1201.cs' Name:'mirage1201-alternative-custom' }}}

#### 2. Suppress the warning at a use site
For an intentionally class-based payload, apply `[WeaverSafeClass]` to the serialized field or RPC parameter. For an RPC result, annotate the payload class or use a normal diagnostic suppression; this attribute cannot be applied to the method or return value. Marking a property does not make it part of generated serialization.
{{{ Path:'Snippets/Analyzers/Mirage1201.cs' Name:'mirage1201-alternative-suppress' }}}
