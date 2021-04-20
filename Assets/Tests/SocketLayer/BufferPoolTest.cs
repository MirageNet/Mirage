using NUnit.Framework;

namespace Mirage.SocketLayer.Tests
{
    [Category("SocketLayer")]
    public class BufferPoolTest
    {
        [Test] public void CreateBuffersOfCorrectSize() { Assert.Ignore("not implemented"); }
        [Test] public void TakeReturnsANewBufferEachCall() { Assert.Ignore("not implemented"); }
        [Test] public void PutThenTakeReturnsSameBuffer() { Assert.Ignore("not implemented"); }
        [Test] public void TakingMoreBuffersThanMaxLogsWarning() { Assert.Ignore("not implemented"); }
        [Test] public void PutingMoreBuffersThanMaxLogsWarning() { Assert.Ignore("not implemented"); }
    }
}
