# MIRAGE1404: AutoPool collection is not cleared on return

## The Problem
`AutoPool<T>` clears `List<T>` and `Dictionary<TKey, TValue>` when the wrapper is disposed, because they implement the non-generic `IList` and `IDictionary` interfaces. Other collections, such as `HashSet<T>`, `Queue<T>` and `Stack<T>`, have no non-generic interface with a `Clear` method, so they are only cleared if `AutoPool<T>.ClearAction` has been set.

Taking one of these collections directly with `AutoPool<T>.Take()`, `AutoPool<T>.Take(out T)` or `AutoPool<T>.Pool.Take()` can return a collection that still contains items from the last time it was used. This leads to bugs that are hard to track down, such as objects being skipped because a `HashSet` already contains them.

This rule triggers when `T` is a collection (implements `IEnumerable`) but does not implement `IList` or `IDictionary`.

| Collection | Use instead |
| --- | --- |
| `HashSet<T>` | `AutoPool.TakeHashSet<T>(out var set)` |
| `Queue<T>` | `AutoPool.TakeQueue<T>(out var queue)` |
| `Stack<T>` | `AutoPool.TakeStack<T>(out var stack)` |

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1404.cs' Name:'mirage1404-triggering' }}}

---

## How to Resolve

### Recommended Fix: Use the AutoPool helper
Use the matching helper on the non-generic `AutoPool` class. The helper sets `ClearAction` for that collection type, so the collection is cleared when it is returned to the pool.
{{{ Path:'Snippets/Analyzers/Mirage1404.cs' Name:'mirage1404-recommended' }}}

---

### Alternative: Set ClearAction for other collections
For collections that have no helper (such as `SortedSet<T>` or `LinkedList<T>`), set `AutoPool<T>.ClearAction` before the first time the collection is taken. The analyzer can not see where `ClearAction` is set, so suppress the warning at the call site.
{{{ Path:'Snippets/Analyzers/Mirage1404.cs' Name:'mirage1404-clear-action' }}}
