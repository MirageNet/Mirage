# MIRAGE1201: NetworkMessage/RPC Class Warning

## When this appears

This advisory flags a class used directly as a payload type. Classes can serialize correctly; review their allocation and reference behavior.

The rule checks the declared type of:

- Eligible fields, including inherited fields, in a `[NetworkMessage]` using generated serialization.
- RPC payload parameters and the result `T` of `UniTask<T>`.

Properties, ignored fields, and `INetworkPlayer` connection context are excluded. So are `string`, arrays, `List<T>`, `Dictionary<TKey, TValue>`, and supported `NetworkIdentity`, `NetworkBehaviour` (including subclasses), and networked `GameObject` references.

It does not inspect collection elements, nested members, or custom serializer bodies. Exempt types still need [serialization support](./MIRAGE1301.md).

Generated serialization allocates non-null class values and sends the declared type's eligible fields, including inherited fields. It does not preserve arbitrary object identity or automatically include runtime subclass fields. Custom serializers can change this.

{{{ Path:'Snippets/Analyzers/Mirage1201.cs' Name:'mirage1201-triggering' }}}

## How to fix

A small struct can avoid the outer class allocation. Reference fields can still allocate, and copying the struct copies references rather than their objects. The string here can still allocate.

{{{ Path:'Snippets/Analyzers/Mirage1201.cs' Name:'mirage1201-recommended' }}}

### Keep an intentional class payload

A custom writer/reader pair controls the format. This example preserves nulls but still allocates non-null instances. Polymorphism and shared reference identity require an explicit format.

{{{ Path:'Snippets/Analyzers/Mirage1201.cs' Name:'mirage1201-alternative-custom' }}}

After reviewing allocation and wire format, suppress this advisory with `[WeaverSafeClass]` on the payload class, serialized field, or RPC parameter. For RPC results, annotate the class or use normal diagnostic suppression; method and return-value annotations are unsupported.

Custom serialization alone does not establish allocation or reference safety. The annotation does not generate or validate serializers, change runtime behavior, or make properties serializable.

{{{ Path:'Snippets/Analyzers/Mirage1201.cs' Name:'mirage1201-alternative-suppress' }}}
