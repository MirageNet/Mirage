using Mirage.Serialization;

namespace SerializeExtensionAttributeTest.WarningFor2WithoutAttribute
{
    public struct MyPriorityType 
    {
        public int value;
    }
    public static class MyPriorityTypeExtension
    {
        public static void MyPriorityTypeWrite1(this NetworkWriter writer, MyPriorityType value) 
        {
            writer.WriteInt32(value.value);
        }

        public static void MyPriorityTypeWrite2(this NetworkWriter writer, MyPriorityType value) 
        {
            writer.WriteInt32(value.value - 10);
        }


        public static MyPriorityType MyPriorityTypeRead1(this NetworkReader reader) 
        {
            return new MyPriorityType { value = reader.ReadInt32() };
        }

        public static MyPriorityType MyPriorityTypeRead2(this NetworkReader reader) 
        {
            return new MyPriorityType { value = reader.ReadInt32() + 10 };
        }
    }
}
