using Mirage.Serialization;

namespace SerializeExtensionAttributeTest.NoWarningWhenDifferentPriority
{
    public struct MyPriorityType 
    {
        public int value;
    }
    public static class MyPriorityTypeExtension
    {
        [SerializeExtension(Priority = 1)]
        public static void MyPriorityTypeWrite1(this NetworkWriter writer, MyPriorityType value) 
        {
            writer.WriteInt32(value.value);
        }

        [SerializeExtension(Priority = 8)]
        public static void MyPriorityTypeWrite2(this NetworkWriter writer, MyPriorityType value) 
        {
            writer.WriteInt32(value.value - 10);
        }

        [SerializeExtension(Priority = 5)]
        public static MyPriorityType MyPriorityTypeRead2(this NetworkReader reader) 
        {
            return new MyPriorityType { value = reader.ReadInt32() + 10 };
        }

        // this is defined after to test both define orders
        [SerializeExtension(Priority = 2)]
        public static MyPriorityType MyPriorityTypeRead1(this NetworkReader reader) 
        {
            return new MyPriorityType { value = reader.ReadInt32() };
        }
    }
}
