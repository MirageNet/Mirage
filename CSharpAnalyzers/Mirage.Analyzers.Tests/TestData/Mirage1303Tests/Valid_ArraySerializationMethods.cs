using Mirage;
using Mirage.Serialization;

public static class ArraySerializationExtensions
{
    public static T[] ReadArray<T>(this NetworkReader reader)
    {
        return default;
    } 

    public static void WriteArray<T>(this NetworkWriter writer, T[] array)
    {
    }
}
