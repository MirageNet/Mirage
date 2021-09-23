using Mirage;
using Mirage.Serialization;

namespace GeneratedReaderWriter.CanUseCustomReadWriteForAbstractClassUsedInMessage
{
    [NetworkMessage]
    public struct FooMessage
    {
        public Foo Foo;
    }

    public abstract class Foo {}
    public class FooBar : Foo {}

    public static class FooReaderWriter
    {
        public static Foo ReadFoo(this NetworkReader reader)
        {
            var fooValue = reader.ReadInt32();
            return new FooBar();
        }

        public static void WriteFoo(this NetworkWriter writer, Foo foo)
        {
            writer.WriteInt32(0);
        }
    }
}