using NUnit.Framework;

namespace Mirror.Weaver.Tests
{
    public class WeaverObserverRpcTests : WeaverTestsBuildFromTestName
    {
        [Test]
        public void ObserverRpcValid()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void ObserverRpcStartsWithRpc()
        {
            Assert.That(weaverErrors, Contains.Item("DoesntStartWithRpc must start with Rpc.  Consider renaming it to RpcDoesntStartWithRpc (at System.Void WeaverObserverRpcTests.ObserverRpcStartsWithRpc.ObserverRpcStartsWithRpc::DoesntStartWithRpc())"));
        }

        [Test]
        public void ObserverRpcCantBeStatic()
        {
            Assert.That(weaverErrors, Contains.Item("RpcCantBeStatic must not be static (at System.Void WeaverObserverRpcTests.ObserverRpcCantBeStatic.ObserverRpcCantBeStatic::RpcCantBeStatic())"));
        }

        [Test]
        public void VirtualObserverRpc()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void OverrideVirtualObserverRpc()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void AbstractObserverRpc()
        {
            Assert.That(weaverErrors, Contains.Item("Abstract ObserverRpc are currently not supported, use virtual method instead (at System.Void WeaverObserverRpcTests.AbstractObserverRpc.AbstractObserverRpc::RpcDoSomething())"));
        }

        [Test]
        public void OverrideAbstractObserverRpc()
        {
            Assert.That(weaverErrors, Contains.Item("Abstract ObserverRpc are currently not supported, use virtual method instead (at System.Void WeaverObserverRpcTests.OverrideAbstractObserverRpc.BaseBehaviour::RpcDoSomething())"));
        }

        [Test]
        public void ObserverRpcThatExcludesOwner()
        {
            Assert.That(weaverErrors, Is.Empty);
        }
    }
}
