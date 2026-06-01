using Mirage.Serialization;

namespace Mirage.Snippets.Serialization.PropertiesExample
{
    // CodeEmbed-Start: properties-example
    public struct MyData
    {
        public int someValue { get; private set; }
        public float anotherValue { get; private set; }

        public MyData(int someValue, float anotherValue)
        {
            this.someValue = someValue;
            this.anotherValue = anotherValue;
        }
    }

    public static class CustomReadWriteFunctions 
    {
        public static void WriteMyType(this NetworkWriter writer, MyData value)
        {
            writer.WriteInt32(value.someValue);
            writer.WriteSingle(value.anotherValue);
        }

        public static MyData ReadMyType(this NetworkReader reader)
        {
            return new MyData(reader.ReadInt32(), reader.ReadSingle());
        }
    }
    // CodeEmbed-End: properties-example
}
