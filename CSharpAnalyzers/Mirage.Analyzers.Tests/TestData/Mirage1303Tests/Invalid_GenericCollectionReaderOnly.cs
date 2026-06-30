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
    public static MyGenericStruct<T> {|#0:ReadMyGenericStruct|}<T>(this NetworkReader reader)
    {
        var value = new MyGenericStruct<T>();
        value.OtherValue = reader.ReadInt32();
        return value;
    }
}
