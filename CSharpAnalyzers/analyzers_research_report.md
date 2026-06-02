# Mirage Roslyn Analyzer Proposal: Future Rules & Serialization Size Estimation

This report compiles the static analysis opportunities identified by five concurrent research subagents. It includes baseline overhead calculations, size estimation rules, and proposed analyzer rules.

---

## 1. Baseline Overhead & Size Estimation (Message/RPC)

Mirage uses `NetworkWriter` (a bit-level stream writer) to pack messages. 

### Message ID Overhead
Every `[NetworkMessage]` writes a **2-byte (16-bit) Message ID** at the start of the payload. The ID is computed as:
```csharp
var id = GetId<T>();
writer.WriteUInt16((ushort)id); // 2 bytes
```

### RPC Envelope Overheads
RPCs package arguments into a custom struct payload, which is then wrapped in a standard `[NetworkMessage]` defined in `Assets/Mirage/Runtime/RemoteCalls/RpcMessages.cs`. Under typical gameplay conditions (`NetId` < 240, `FunctionIndex` < 240, `ReplyId` < 240, occupying **1 byte** each), the baseline headers are:

1. **`RpcMessage` (Standard RPCs)**
   * Message ID: 2 bytes
   * `NetId` (packed uint): 1 byte (typical)
   * `FunctionIndex` (packed int): 1 byte (typical)
   * `Payload` Length Prefix (packed uint): 1 byte (typical, for payloads < 240 bytes)
   * **Total Baseline Overhead:** **5 bytes (40 bits)**.
   *(Note: Since all preceding fields write exactly 8-bit or 16-bit blocks, the bit position remains byte-aligned when writing payload bytes).*

2. **`RpcWithReplyMessage` (RPCs returning `UniTask<T>`)**
   * Message ID: 2 bytes
   * `NetId` (packed uint): 1 byte
   * `FunctionIndex` (packed int): 1 byte
   * `ReplyId` (packed int): 1 byte
   * `Payload` Length Prefix: 1 byte
   * **Total Baseline Overhead:** **6 bytes (48 bits)**.

3. **`RpcReply` (Async RPC Response)**
   * Message ID: 2 bytes
   * `ReplyId` (packed int): 1 byte
   * `Success` (bool): 1 bit
   * `Payload` Length Prefix: 1 byte
   * **Total Baseline Overhead:** **5 bytes (40 bits)**.
   *(Note: The 1-bit `Success` bool causes the writer to insert 7 bits of alignment padding before writing the raw span, making the header exactly 5 bytes).*

---

## 2. Serialization Size Estimation Rules

To statically calculate or estimate the serialized size of a message or RPC, walk the declared field/parameter types and sum their sizes:

### Fixed-Size Types
* **`bool`:** 1 bit.
* **`byte` / `sbyte`:** 8 bits (1 byte).
* **`char` / `ushort` / `short`:** 16 bits (2 bytes).
* **`float`:** 32 bits (4 bytes).
* **`double`:** 64 bits (8 bytes).
* **`decimal`:** 128 bits (16 bytes).
* **`Vector2`:** 64 bits (8 bytes) — 2 floats.
* **`Vector3`:** 96 bits (12 bytes) — 3 floats.
* **`Vector4` / `Color`:** 128 bits (16 bytes) — 4 floats.
* **`Color32`:** 32 bits (4 bytes).
* **`Rect` / `Plane`:** 128 bits (16 bytes).
* **`Ray`:** 192 bits (24 bytes).
* **`Matrix4x4`:** 512 bits (64 bytes).
* **`Quaternion`:** **29 bits** (packed using `QuaternionPacker.Default9`: 9 bits per element + 2 index bits, unless packing attributes override it).

### Packed & Variable-Length Types
* **`uint` / `int` / `ulong` / `long` (SQLite varint):**
  * $\le 240$: 1 byte (8 bits)
  * $\le 2287$: 2 bytes (16 bits)
  * $\le 67823$: 3 bytes (24 bits)
  * *Estimation Strategy:* Assume **1 byte** for indexes/small IDs, or **2 bytes** for typical numeric quantities.
* **`NetworkIdentity`:** 1 packed uint (Estimate: 1–2 bytes).
* **`NetworkBehaviour`:** 1 packed uint + 1 byte component index (Estimate: 2–3 bytes).
* **`GameObject`:** 1 packed uint (Estimate: 1–2 bytes).

### Weaver Packing Attributes
* **`[BitCount(N)]`:** exactly $N$ bits.
* **`[BitCountFromRange(min, max)]`:** exactly $\lfloor\log_2(\text{max} - \text{min})\rfloor + 1$ bits.
* **`[FloatPack(max, bitCount)]`:** exactly $\text{bitCount}$ bits.
* **`[FloatPack(max, precision)]`:** exactly $\lfloor\log_2(2 \times \text{max} / \text{precision})\rfloor + 1$ bits.
* **`[Vector2Pack / Vector3Pack / QuaternionPack]`:** uses specified bit sizes.

---

## 3. Compiled Proposed Rules

We propose expanding Mirage's Roslyn Analyzers across the following 100-based categories:

### A. Serialization & Message Size (`MIRAGE1500` - `MIRAGE1599`)

#### `MIRAGE1501`: Message Size Exceeds Safe MTU Limit (Warning)
* **Description:** Warns if the estimated serialized size of a message or RPC exceeds typical MTU limits (e.g., 1200 bytes). Large packets face high IP-level fragmentation, resulting in latency spikes and packet loss.
* **Trigger Example:**
  ```csharp
  [NetworkMessage]
  public struct LargeTelemetryMessage
  {
      public int SequenceNumber;
      public Matrix4x4[] HistoricalTransforms; // 16 matrices = 1024 bytes!
  }
  ```
* **Solution:** Split the message, compress fields using packing attributes, or paginate the collection.

#### `MIRAGE1502`: Unbounded String or Collection (Warning)
* **Description:** Warns if a string or array field lacks size boundaries inside a `[NetworkMessage]` or RPC, as this allows malicious clients to trigger memory allocations or cause deserialization errors.
* **Solution:** Wrap them in custom serialized structures with size limits.

#### `MIRAGE1503`: High Bit-Overhead Primitive Type (Info)
* **Description:** Suggests packing floats/vectors in high-frequency messages (e.g. movement, combat state updates).
* **Solution:** Decorate floats/vectors with `[FloatPack]` or `[Vector3Pack]`.

---

### B. SyncVars and SyncObjects (`MIRAGE1000` - `MIRAGE1099`)

#### `MIRAGE1003`: Direct Mutation of SyncCollection Elements (Warning)
* **Description:** Warns when fields inside a class element stored in a `SyncList<T>` or `SyncDictionary<K, V>` are modified directly (e.g., `players[index].health -= 10`). Since the collection reference remains identical, the change isn't tracked and won't trigger synchronization.
* **Solution:** Invoke `SetItemDirtyAt(index)` manually, or switch the element type to a struct and re-assign it.

#### `MIRAGE1004`: Reassignment of SyncObject Fields (Error)
* **Description:** Triggers an error if a field implementing `ISyncObject` (like `SyncList<T>`) is reassigned or not marked `readonly`. If a SyncObject is reassigned, Mirage continues syncing the old instance while runtime code interacts with the new instance.
* **Solution:** Declare SyncObject fields as `readonly`.

#### `MIRAGE1005`: High SyncVar Count in NetworkBehaviour (Warning)
* **Description:** Warns if a component has too many SyncVars (e.g., >16), which indicates bloat.
* **Solution:** Group variables into a struct or split into multiple components.

---

### C. RPC Performance & Constraints (`MIRAGE1200` - `MIRAGE1299`)

#### `MIRAGE1202`: Pass-by-Reference Modifiers in RPCs (Error)
* **Description:** Flags parameters in RPCs that use `ref`, `in`, or `out` modifiers, since the serialization weaver cannot process references.
* **Solution:** Pass parameters by value.

#### `MIRAGE1203`: Static RPC Methods (Error)
* **Description:** Flags static methods decorated with `[ServerRpc]` or `[ClientRpc]`. RPCs require instanced object contexts to route correctly.
* **Solution:** Make the method non-static.

#### `MIRAGE1204`: Invalid ClientRpc Target Configurations (Error)
* **Description:** Ensures consistency: `target = RpcTarget.Player` must have `INetworkPlayer` as the first parameter, and `target = RpcTarget.Owner` cannot use `excludeOwner = true`.
* **Solution:** Fix parameter signature or exclusion properties.

#### `MIRAGE1205`: Invalid RateLimit Attribute Settings (Error)
* **Description:** Assures `Interval`, `Refill`, `MaxTokens`, and `Penalty` inside `[RateLimit]` are positive and greater than zero.

---

### D. General API & Lifecycle Constraints (`MIRAGE1400` - `MIRAGE1499`)

#### `MIRAGE1401`: Unity Lifecycle Method Network Guards (Warning)
* **Description:** Warns if a network guard (e.g., `[Server]`) is placed on Unity lifecycle methods (`Update`, `FixedUpdate`) without setting `error = false`. Since Unity calls these on all clients automatically, the default throwing behavior will crash client consoles.
* **Solution:** Set `[Server(error = false)]` or use an explicit check (`if (!IsServer) return`).

#### `MIRAGE1402`: Accessing Network State in Awake/Start (Warning)
* **Description:** Flags checks on network state properties (`IsServer`, `IsClient`) or RPC calls inside `Awake()` or `Start()`. The object is not spawned yet, so these evaluate to `false` and RPCs fail.
* **Solution:** Register listeners to `Identity.OnStartServer` or `Identity.OnStartClient` instead.

#### `MIRAGE1403`: Missing base Call in OnSerialize/OnDeserialize Overrides (Error)
* **Description:** Overriding `OnSerialize` or `OnDeserialize` in a component containing SyncVars/SyncObjects without calling `base.OnSerialize/OnDeserialize` stops Weaver-generated syncing from running.
* **Solution:** Add base class serialization calls.
