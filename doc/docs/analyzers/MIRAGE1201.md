# MIRAGE1201: NetworkMessage/RPC Class Warning

## When this appears

A `[NetworkMessage]` field, RPC parameter, or RPC result uses a class type.

Generated serialization creates a new object for each non-null class value. Frequent messages can increase garbage collection work. It also does not preserve shared object identity or fields found only on a runtime subclass.

{{{ Path:'Snippets/Analyzers/Mirage1201.cs' Name:'mirage1201-triggering' }}}

## How to fix

Use a small struct to avoid allocating the outer class object. Reference fields can still allocate, and copying the struct copies references rather than their objects. The string in this example can still allocate.

{{{ Path:'Snippets/Analyzers/Mirage1201.cs' Name:'mirage1201-recommended' }}}

### Keep an intentional class payload

Classes can serialize correctly. Generated serialization sends the declared type's serializable fields, including inherited fields. A custom writer/reader pair can change the format, but you must explicitly handle subclass data or shared references if you need them.

This custom serializer preserves nulls but still allocates non-null instances:

{{{ Path:'Snippets/Analyzers/Mirage1201.cs' Name:'mirage1201-alternative-custom' }}}

After reviewing allocation and the data sent, suppress this warning with `[WeaverSafeClass]` on the payload class, serialized field, or RPC parameter.

For RPC results, annotate the class or use normal diagnostic suppression. The attribute cannot be placed on a method or return value.

Custom serialization alone does not establish allocation or reference safety. The annotation does not generate or validate serializers, change runtime behavior, or make properties serializable.

{{{ Path:'Snippets/Analyzers/Mirage1201.cs' Name:'mirage1201-alternative-suppress' }}}

### Which types are checked

The rule checks the declared type of:

- Serialized fields, including inherited fields, in a `[NetworkMessage]` using generated serialization.
- RPC payload parameters and the result `T` of `UniTask<T>`.

Properties, ignored fields, and `INetworkPlayer` connection parameters are excluded. So are `string`, arrays, `List<T>`, `Dictionary<TKey, TValue>`, and supported `NetworkIdentity`, `NetworkBehaviour` (including subclasses), and networked `GameObject` references.

The rule does not inspect collection elements, nested members, or custom serializer bodies. Excluded types still need [serialization support](./MIRAGE1301.md).
