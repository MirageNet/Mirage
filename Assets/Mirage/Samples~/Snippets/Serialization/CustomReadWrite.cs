using Mirage.Serialization;

namespace Mirage.Snippets.Serialization
{
    public struct MyType
    {
        public int value;
    }

    public static class CustomReadWrite
    {
        // CodeEmbed-Start: custom-read-write
        public static void WriteMyType(this NetworkWriter writer, MyType value)
        {
            // write MyType data here
        }

        public static MyType ReadMyType(this NetworkReader reader)
        {
            // read MyType data here
            return default;
        }
        // CodeEmbed-End: custom-read-write
    }
}
