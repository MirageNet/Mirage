using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Syncing
{
    public class SyncVarFieldReplaceTest : ServerSetup
    {
        private NetworkIdentity _target1;
        private NetworkIdentity _target2;
        private MockPlayer component;

        protected override UniTask LateSetup()
        {
            _target1 = CreateNetworkIdentity();
            serverObjectManager.Spawn(_target1, 1);

            _target2 = CreateNetworkIdentity();
            serverObjectManager.Spawn(_target2, 1);

            component = CreateBehaviour<MockPlayer>();
            component.target = _target1;
            serverObjectManager.Spawn(component.Identity, 2);

            return base.LateSetup();
        }

        [Test]
        public void CanGetSyncVarFromMethod()
        {
            var mock = new MockCSharpClass();
            mock.GetTarget(component);
            Assert.That(mock.netid, Is.Not.Zero.And.EqualTo(component.target.NetId));
        }

        [Test]
        public void CanSetSyncVarFromMethod()
        {
            var mock = new MockCSharpClass();
            mock.SetTarget(component, _target2);
            Assert.That(component.target, Is.EqualTo(_target2));
        }
        [Test]
        public void CanGetSyncVarFromConstructor()
        {
            var mock = new MockCSharpClass(component);
            Assert.That(mock.netid, Is.Not.Zero.And.EqualTo(component.target.NetId));
        }

        [Test]
        public void CanSetSyncVarFromConstructor()
        {
            var mock = new MockCSharpClass(component, _target2);
            Assert.That(component.target, Is.EqualTo(_target2));
        }


        public class MockCSharpClass
        {
            public uint netid;
            private MockPlayer player;

            public MockCSharpClass() { }
            public MockCSharpClass(MockPlayer myPlayer)
            {
                // tests getting syncvar from constructor of c# only class
                if (myPlayer.target != null)
                    netid = myPlayer.target.NetId;

                player = myPlayer;
            }

            public MockCSharpClass(MockPlayer myPlayer, NetworkIdentity target)
            {
                // tests setting syncvar from constructor of c# only class
                myPlayer.target = target;
            }

            public void GetTarget(MockPlayer myPlayer)
            {
                if (myPlayer.target != null)
                {
                    netid = myPlayer.target.NetId;
                }
            }

            public void SetTarget(MockPlayer myPlayer, NetworkIdentity target)
            {
                myPlayer.target = target;
            }
        }
    }
}
