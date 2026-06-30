using Mirage.Serialization;

public struct MyGenericStruct<T>
{
    public T Value;
    public int OtherValue;
}

public static class GenericCollectionSerialization
{
    [WeaverSerializeCollection]
    public static void WriteMyGenericStruct<T>(this NetworkWriter writer, MyGenericStruct<T> id)
    {
        writer.WriteInt32(id.OtherValue);
    }

    [WeaverSerializeCollection]
    public static MyGenericStruct<T> ReadMyGenericStruct<T>(this NetworkReader reader)
    {
        var value = new MyGenericStruct<T>();
        value.OtherValue = reader.ReadInt32();
        return value;
    }
}