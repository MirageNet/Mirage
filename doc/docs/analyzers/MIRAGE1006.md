# MIRAGE1006: Prefer Sync collection instead of collection SyncVar

## The Problem
A field marked with `[SyncVar]` is declared as an array or collection type that has a Sync collection version:

| SyncVar type | Use instead |
| --- | --- |
| `T[]` (such as `byte[]`) | `SyncList<T>` |
| `List<T>` | `SyncList<T>` |
| `HashSet<T>` | `SyncHashSet<T>` |
| `Dictionary<TKey, TValue>` | `SyncDictionary<TKey, TValue>` |

These types are usually a poor fit for SyncVars that change during play:
* **Content changes are not synced:** Changing the contents (`values[0] = 5`, `list.Add(x)`, `dict[key] = x`) does not call the SyncVar setter, so the field is never marked dirty and the change is never sent.
* **Full resend:** When the field is reassigned, the whole collection is sent again, even if only one element changed.
* **Allocations:** A new collection is allocated every time the SyncVar is deserialized.

Sync collections track individual changes and only send what changed, so they are the better choice for collections that are modified at runtime.

This rule does not trigger when the SyncVar uses `initialOnly = true`. Initial only SyncVars are only sent once when the object is spawned, so none of the problems above apply, and using a plain collection is likely an intentional design choice.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1006.cs' Name:'mirage1006-triggering' }}}

---

## How to Resolve

### Recommended Fix: Use a Sync collection
Replace the field with a `readonly` Sync collection field (without `[SyncVar]`). Changes are tracked and only the changes are sent.
{{{ Path:'Snippets/Analyzers/Mirage1006.cs' Name:'mirage1006-recommended' }}}

---

### Alternative: Use initialOnly
If the collection is only set before the object is spawned and never changes afterwards, set `initialOnly = true` on the SyncVar. The collection is sent once in the spawn message, and the warning is not shown.
{{{ Path:'Snippets/Analyzers/Mirage1006.cs' Name:'mirage1006-initial-only' }}}
