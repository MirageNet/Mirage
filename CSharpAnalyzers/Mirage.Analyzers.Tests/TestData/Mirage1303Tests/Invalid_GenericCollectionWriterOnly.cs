using Mirage;
using Mirage.Serialization;

public struct MyGenericStruct<T>
{
    public T Value;
    public int OtherValue;
}

public static class GenericCollectionSerialization
{
    [WeaverSerializeCollection]
    public static void {|#0:WriteMyGenericStruct|}<T>(this NetworkWriter writer, MyGenericStruct<T> id)
    {
        writer.WriteInt32(id.OtherValue);
    }
}
