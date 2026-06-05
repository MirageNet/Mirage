using NUnit.Framework;

namespace Mirage.Tests.Weaver
{
    public class MaxLengthAttributeTests : WeaverTestBase
    {
        [Test, BatchSafe(BatchType.Success)]
        public void MaxLength()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Fail)]
        public void MaxLengthInvalid()
        {
            // Weaver should report failure when attribute is used on non-collection and non-string types
            HasErrorCount(8);

            HasError("Cannot generate write function with length for Int32. Limit attributes can only be used on types with a registered length-limited serializer.",
                "System.Int32");

            HasError("Could not process Rpc because one or more of its parameter were invalid",
                "System.Void MaxLengthAttributeTests.MaxLengthInvalid.MyBehaviour::SendInvalid(System.Int32)");

            HasError("Cannot generate write function with length for MyCustomStruct. Limit attributes can only be used on types with a registered length-limited serializer.",
                "MaxLengthAttributeTests.MaxLengthInvalid.MyCustomStruct");

            HasError("Could not process Rpc because one or more of its parameter were invalid",
                "System.Void MaxLengthAttributeTests.MaxLengthInvalid.MyBehaviour::SendInvalidCustom(MaxLengthAttributeTests.MaxLengthInvalid.MyCustomStruct)");

            HasError("MaxLength must be greater than 0.",
                "System.Void MaxLengthAttributeTests.MaxLengthInvalid.MyBehaviour::SendInvalidZero(System.String)");

            HasError("Could not process Rpc because one or more of its parameter were invalid",
                "System.Void MaxLengthAttributeTests.MaxLengthInvalid.MyBehaviour::SendInvalidZero(System.String)");

            HasError("MaxLength must be greater than 0.",
                "System.Void MaxLengthAttributeTests.MaxLengthInvalid.MyBehaviour::SendInvalidNegative(System.String)");

            HasError("Could not process Rpc because one or more of its parameter were invalid",
                "System.Void MaxLengthAttributeTests.MaxLengthInvalid.MyBehaviour::SendInvalidNegative(System.String)");
        }
    }
}
