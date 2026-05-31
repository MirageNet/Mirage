# MIRAGE1303: Mismatched Custom Serialization Methods

## The Problem
A custom serializer signature does not match the expected pattern, or a custom reader is missing for a custom writer (or vice versa).

When writing custom serialization for a type, Mirage requires both extension methods to be defined with specific signatures:
- **Writer:** `public static void WriteMyType(this NetworkWriter writer, MyType value)`
- **Reader:** `public static MyType ReadMyType(this NetworkReader reader)`

If only one of the methods is defined, or if the parameter/return types do not exactly match the type, Mirage cannot pair them up, causing serialization to fail at compile-time.

---

## Example of Triggering Code
```csharp
using Mirage;
using Mirage.Serialization;

public struct CustomType
{
    public int value;
}

public static class CustomSerialization
{
    // Error: Custom writer defined but matching custom reader is missing
    public static void WriteCustomType(this NetworkWriter writer, CustomType value)
    {
        writer.WritePackedInt32(value.value);
    }
}
```

---

## How to Resolve

Provide a matching reader or writer method with the correct signature. Ensure that the type being read and written is exactly the same.

```csharp
using Mirage;
using Mirage.Serialization;

public struct CustomType
{
    public int value;
}

public static class CustomSerialization
{
    // Correct: Both writer and reader are defined with matching signatures
    public static void WriteCustomType(this NetworkWriter writer, CustomType value)
    {
        writer.WritePackedInt32(value.value);
    }

    public static CustomType ReadCustomType(this NetworkReader reader)
    {
        return new CustomType { value = reader.ReadPackedInt32() };
    }
}
```
