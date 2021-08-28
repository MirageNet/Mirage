using Mirage;
using Mirage.Serialization;

namespace GeneratedReaderWriter.MultipleMethodsForSameType
{
    public static class MyExtensions 
    {
        public static void WriteMyInt10(this NetworkWriter writer, int value) 
        {
            writer.Write((ulong)value, 10);
        }
        public static void WriteMyInt20(this NetworkWriter writer, int value) 
        {
            writer.Write((ulong)value, 10);
        }

        public static int ReadMyInt10(this NetworkReader reader) 
        {
            return (int)reader.Read(10);
        }

        public static int ReadMyInt20(this NetworkReader reader) 
        {
            return (int)reader.Read(10);
        }
    }
}
