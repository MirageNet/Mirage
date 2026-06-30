using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Mirage;
using Mirage.Serialization;

public static class GenericTypesSerializationExtensions
{
    [WeaverIgnore]
    public static void Write<T>(this NetworkWriter writer, T value)
    {
    }

    [WeaverIgnore]
    public static void WriteWithLength<T>(this NetworkWriter writer, T value, int maxLength)
    {
    }

    [WeaverIgnore]
    public static T Read<T>(this NetworkReader reader)
    {
        return default;
    }

    [WeaverIgnore]
    public static T ReadWithLength<T>(this NetworkReader reader, int maxLength)
    {
        return default;
    }
}
