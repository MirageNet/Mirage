using NUnit.Framework;

namespace Mirage.Weaver
{
    public class SerializeExtensionAttributeTest : TestsBuildFromTestName
    {
        const string type = "MyPriorityType";
        const string write1 = "MyPriorityTypeExtension1";
        const string write2 = "MyPriorityTypeExtension2";

        [Test]
        public void WarningForSamePriority()
        {
            IsSuccess();
            HasWarning($"Registering a Write method for {type} when one already exists\n  old:{write1}\n  new:{write2}", write2);
            HasWarning($"Registering a Read method for {type} when one already exists\n  old:{write1}\n  new:{write2}", write2);
        }
        [Test]
        public void WarningFor2WithoutAttribute()
        {
            IsSuccess();
            HasWarning($"Registering a Write method for {type} when one already exists\n  old:{write1}\n  new:{write2}", write2);
            HasWarning($"Registering a Read method for {type} when one already exists\n  old:{write1}\n  new:{write2}", write2);
        }
        [Test]
        public void WarningForZeroPriorityAndWithoutAttribute()
        {
            IsSuccess();
            HasWarning($"Registering a Write method for {type} when one already exists\n  old:{write1}\n  new:{write2}", write2);
            HasWarning($"Registering a Read method for {type} when one already exists\n  old:{write1}\n  new:{write2}", write2);
        }
    }
}
