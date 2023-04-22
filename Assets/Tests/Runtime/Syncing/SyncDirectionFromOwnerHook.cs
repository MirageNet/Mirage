using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Syncing
{
    public class SyncDirectionFromOwnerHook : SyncDirectionTestBase<MockPlayerHook>
    {
        [UnityTest]
        [Ignore("Needs new SyncVar feature")]
        public IEnumerator HookInvokedOnOwnerWhenInvokeOnServerTrue()
        {
            SetDirection(SyncFrom.Owner, SyncTo.Server);

            var serverHookInvoked = new List<int>();
            var ownerHookInvoked = new List<int>();

            ServerComponent.MyNumberEventChanged += (n) => serverHookInvoked.Add(n);
            OwnerComponent.MyNumberEventChanged += (n) => ownerHookInvoked.Add(n);

            const int Value1 = 10;

            // set on owner
            OwnerComponent.myNumberEvent = Value1;

            Assert.That(serverHookInvoked, Has.Count.EqualTo(1), "Hook should be called on owner right away");
            Assert.That(serverHookInvoked[0], Is.EqualTo(Value1));

            yield return null;
            yield return null;


            Assert.That(ownerHookInvoked, Has.Count.EqualTo(1), "Hook should be called on server when it is received");
            Assert.That(ownerHookInvoked[0], Is.EqualTo(Value1));

            Assert.That(ServerComponent.myNumberEvent, Is.EqualTo(Value1));
            Assert.That(OwnerComponent.myNumberEvent, Is.EqualTo(Value1));
        }

        [UnityTest]
        [Ignore("Needs new SyncVar feature")]
        public IEnumerator HookMethodInvokedOnOwnerWhenInvokeOnServerTrue()
        {
            SetDirection(SyncFrom.Owner, SyncTo.Server);

            var serverHookInvoked = new List<int>();
            var ownerHookInvoked = new List<int>();

            ServerComponent.MyNumberMethodChangedCalled += (n) => serverHookInvoked.Add(n);
            OwnerComponent.MyNumberMethodChangedCalled += (n) => ownerHookInvoked.Add(n);

            const int Value1 = 10;

            // set on owner
            OwnerComponent.myNumberMethod = Value1;

            Assert.That(serverHookInvoked, Has.Count.EqualTo(1), "Hook should be called on owner right away");
            Assert.That(serverHookInvoked[0], Is.EqualTo(Value1));

            yield return null;
            yield return null;


            Assert.That(ownerHookInvoked, Has.Count.EqualTo(1), "Hook should be called on server when it is received");
            Assert.That(ownerHookInvoked[0], Is.EqualTo(Value1));

            Assert.That(ServerComponent.myNumberMethod, Is.EqualTo(Value1));
            Assert.That(OwnerComponent.myNumberMethod, Is.EqualTo(Value1));
        }
    }
}
