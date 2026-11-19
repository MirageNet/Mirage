# MIRAGE1006: Prefer SyncList instead of array for SyncVar

## The Problem
A field marked with `[SyncVar]` is declared as an array type (such as `byte[]` or `int[]`).

Arrays are usually a poor fit for SyncVars that change during play:
* **Element changes are not synced:** Setting an element (`values[0] = 5`) does not call the SyncVar setter, so the field is never marked dirty and the change is never sent.
* **Full resend:** When the array is reassigned, the whole array is sent again, even if only one element changed.
* **Allocations:** A new array is allocated every time the SyncVar is deserialized.

`SyncList<T>` tracks individual changes and only sends what changed, so it is the better choice for collections that are modified at runtime.

This rule does not trigger when the SyncVar uses `initialOnly = true`. Initial only SyncVars are only sent once when the object is spawned, so none of the problems above apply, and using an array is likely an intentional design choice.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1006.cs' Name:'mirage1006-triggering' }}}

---

## How to Resolve

### Recommended Fix: Use a SyncList
Replace the array with a `readonly` `SyncList<T>` field (without `[SyncVar]`). Element changes are tracked and only the changes are sent.
{{{ Path:'Snippets/Analyzers/Mirage1006.cs' Name:'mirage1006-recommended' }}}

---

### Alternative: Use initialOnly
If the array is only set before the object is spawned and never changes afterwards, set `initialOnly = true` on the SyncVar. The array is sent once in the spawn message, and the warning is not shown.
{{{ Path:'Snippets/Analyzers/Mirage1006.cs' Name:'mirage1006-initial-only' }}}
