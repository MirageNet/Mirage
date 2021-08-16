using NUnit.Framework;

namespace Mirage.Weaver
{
    public class SerializeExtensionAttributeTest : TestsBuildFromTestName
    {
        static string getNameSpace(string testName) => $"SerializeExtensionAttributeTest.{testName}";
        const string type = "MyPriorityType";
        static string write(int i) => $"MyPriorityTypeExtension::MyPriorityTypeWrite{i}";
        static string read(int i) => $"MyPriorityTypeExtension::MyPriorityTypeRead{i}";

        [Test]
        public void NoWarningWhenDifferentPriority()
        {
            IsSuccess();
        }

        [Test]
        public void WarningForSamePriority()
        {
            string @namespace = getNameSpace("WarningForSamePriority");
            NoErrors();

            string write1 = $"System.Void {@namespace}.{write(1)}(Mirage.Serialization.NetworkWriter,{@namespace}.{type})";
            string write2 = $"System.Void {@namespace}.{write(2)}(Mirage.Serialization.NetworkWriter,{@namespace}.{type})";
            HasWarning($"Registering a Write method for {@namespace}.{type} when one already exists\n" +
                $"  old:{write1}\n" +
                $"  new:{write2}",
                write2);

            string read1 = $"{@namespace}.{type} {@namespace}.{read(1)}(Mirage.Serialization.NetworkReader)";
            string read2 = $"{@namespace}.{type} {@namespace}.{read(2)}(Mirage.Serialization.NetworkReader)";
            HasWarning($"Registering a Read method for {@namespace}.{type} when one already exists\n" +
                $"  old:{read1}\n" +
                $"  new:{read2}",
                read2);
        }

        [Test]
        public void WarningFor2WithoutAttribute()
        {
            string @namespace = getNameSpace("WarningFor2WithoutAttribute");
            NoErrors();

            string write1 = $"System.Void {@namespace}.{write(1)}(Mirage.Serialization.NetworkWriter,{@namespace}.{type})";
            string write2 = $"System.Void {@namespace}.{write(2)}(Mirage.Serialization.NetworkWriter,{@namespace}.{type})";
            HasWarning($"Registering a Write method for {@namespace}.{type} when one already exists\n" +
                $"  old:{write1}\n" +
                $"  new:{write2}",
                write2);

            string read1 = $"{@namespace}.{type} {@namespace}.{read(1)}(Mirage.Serialization.NetworkReader)";
            string read2 = $"{@namespace}.{type} {@namespace}.{read(2)}(Mirage.Serialization.NetworkReader)";
            HasWarning($"Registering a Read method for {@namespace}.{type} when one already exists\n" +
                $"  old:{read1}\n" +
                $"  new:{read2}",
                read2);
        }

        [Test]
        public void WarningForZeroPriorityAndWithoutAttribute()
        {
            string @namespace = getNameSpace("WarningForZeroPriorityAndWithoutAttribute");
            NoErrors();

            string write1 = $"System.Void {@namespace}.{write(1)}(Mirage.Serialization.NetworkWriter,{@namespace}.{type})";
            string write2 = $"System.Void {@namespace}.{write(2)}(Mirage.Serialization.NetworkWriter,{@namespace}.{type})";
            HasWarning($"Registering a Write method for {@namespace}.{type} when one already exists\n" +
                $"  old:{write1}\n" +
                $"  new:{write2}",
                write2);

            string read1 = $"{@namespace}.{type} {@namespace}.{read(1)}(Mirage.Serialization.NetworkReader)";
            string read2 = $"{@namespace}.{type} {@namespace}.{read(2)}(Mirage.Serialization.NetworkReader)";
            HasWarning($"Registering a Read method for {@namespace}.{type} when one already exists\n" +
                $"  old:{read1}\n" +
                $"  new:{read2}",
                read2);
        }
    }
}
