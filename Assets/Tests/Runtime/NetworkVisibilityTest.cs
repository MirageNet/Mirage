using Mirage.Tests.Runtime.ClientServer;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests.Runtime
{
    public class NetworkVisibilityTest : ClientServerSetup<MockVisibility>
    {
        public MockVisibility ServerVisibility => (MockVisibility)serverIdentity.Visibility;

        [Test]
        public void PlayerCanSeeObjectsTheyOwn()
        {
            Debug.Assert(!ServerVisibility.Visible);

            CollectionAssert.Contains(serverIdentity.observers, serverPlayer);
            CollectionAssert.Contains(serverPlayer.VisList, serverIdentity);
        }

        [Test]
        public void ObjectCanBecomeVisible()
        {
            var otherObj = CreateBehaviour<MockVisibility>();
            serverObjectManager.Spawn(otherObj.Identity);

            CollectionAssert.IsEmpty(otherObj.Identity.observers);
            CollectionAssert.DoesNotContain(serverPlayer.VisList, otherObj.Identity);

            Debug.Assert(!ServerVisibility.Visible);

            // Visible will call RebuildObservers
            otherObj.Visible = true;

            CollectionAssert.Contains(otherObj.Identity.observers, serverPlayer);
            CollectionAssert.Contains(serverPlayer.VisList, otherObj.Identity);
        }

        [Test]
        public void EventCalledWhenVisiblityChanges()
        {
            var otherObj = CreateBehaviour<MockVisibility>();
            var sub = Substitute.For<NetworkVisibility.VisibilityChanged>();
            otherObj.OnVisibilityChanged += sub;

            serverObjectManager.Spawn(otherObj.Identity);
            // not called when spawning when not visible
            sub.DidNotReceiveWithAnyArgs().Invoke(default, default);

            otherObj.Visible = true;
            sub.Received(1).Invoke(serverPlayer, true);
            sub.ClearReceivedCalls();

            otherObj.Visible = false;
            sub.Received(1).Invoke(serverPlayer, false);
        }


        [Test]
        public void EventCalledWhenSpawning()
        {
            var otherObj = CreateBehaviour<MockVisibility>();
            otherObj.Visible = true;
            var sub = Substitute.For<NetworkVisibility.VisibilityChanged>();
            otherObj.OnVisibilityChanged += sub;

            serverObjectManager.Spawn(otherObj.Identity);
            sub.Received(1).Invoke(serverPlayer, true);
        }

        [Test]
        public void EventNotCalledWhenDestroying()
        {
            var otherObj = CreateBehaviour<MockVisibility>();
            otherObj.Visible = true;

            serverObjectManager.Spawn(otherObj.Identity);

            var sub = Substitute.For<NetworkVisibility.VisibilityChanged>();
            otherObj.OnVisibilityChanged += sub;

            serverObjectManager.Destroy(otherObj.gameObject);
            sub.DidNotReceiveWithAnyArgs().Invoke(default, default);
        }
    }
}
