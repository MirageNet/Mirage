using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// CrossAssemblyGenericBase<T> is in Mirage.Tests.Common.
// The Weaver used to import base method references into the base type's module, giving this assembly a reference to
// itself, which Unity refuses to load after an editor restart
namespace Mirage.Tests.Runtime.ClientServer.GenericBehaviours.CrossAssembly
{
    public struct CrossAssemblyArg
    {
        public int value;
    }

    public class CrossAssemblyDerived : CrossAssemblyGenericBase<CrossAssemblyArg>
    {
        [SyncVar] public int derivedValue;
        public int derivedRpcValue;

        [ClientRpc]
        public void RpcDerived(int value)
        {
            derivedRpcValue = value;
        }
    }

    public class CrossAssemblyGenericBehaviourTest : ClientServerSetup<CrossAssemblyDerived>
    {
        [Test]
        public void AssemblyDoesNotReferenceItselfOrCoreLib()
        {
            var assembly = typeof(CrossAssemblyDerived).Assembly;
            var selfName = assembly.GetName().Name;

            foreach (var reference in assembly.GetReferencedAssemblies())
            {
                Assert.That(reference.Name, Is.Not.EqualTo(selfName));
                // Weaver runs on .NET core, so a bad import can point at its corlib instead of netstandard
                Assert.That(reference.Name, Is.Not.EqualTo("System.Private.CoreLib"));
            }
        }

        [UnityTest]
        public IEnumerator CanSyncValues()
        {
            serverComponent.baseValue = 10;
            serverComponent.derivedValue = 20;
            yield return new WaitForSeconds(0.2f);

            Assert.That(clientComponent.baseValue, Is.EqualTo(10));
            Assert.That(clientComponent.derivedValue, Is.EqualTo(20));
        }

        [UnityTest]
        public IEnumerator CanCallRpcs()
        {
            serverComponent.RpcBase(30);
            serverComponent.RpcDerived(40);
            yield return new WaitForSeconds(0.2f);

            Assert.That(clientComponent.baseRpcValue, Is.EqualTo(30));
            Assert.That(clientComponent.derivedRpcValue, Is.EqualTo(40));
        }
    }

    public class CrossAssemblyNonGenericDerived : CrossAssemblyBase
    {
        [SyncVar] public int derivedValue;
    }

    /// <summary>
    /// Sets SyncVar fields declared in another assembly. The Weaver swaps those field accesses for the base's generated
    /// property methods, which must be imported into this assembly or writing it fails.
    /// Only checks the server side, syncing a SyncVar field set from another assembly is not supported yet
    /// </summary>
    public class CrossAssemblySyncVarFieldTest : ClientServerSetup<CrossAssemblyNonGenericDerived>
    {
        [Test]
        public void CanSetAndGetBaseSyncVar()
        {
            serverComponent.baseValue = 10;

            Assert.That(serverComponent.baseValue, Is.EqualTo(10));
        }

        [Test]
        public void CanSetBaseStructSyncVarToDefault()
        {
            serverComponent.baseVector = Vector3.one;
            serverComponent.baseVector = default;

            Assert.That(serverComponent.baseVector, Is.EqualTo(Vector3.zero));
        }
    }

    // goes through CrossAssemblyGenericMiddle<T>, so the Weaver has to match CrossAssemblyArg to the base's T
    public class CrossAssemblyDerivedThroughMiddle : CrossAssemblyGenericMiddle<CrossAssemblyArg>
    {
        [SyncVar] public int derivedValue;
        public int derivedRpcValue;

        [ClientRpc]
        public void RpcDerived(int value)
        {
            derivedRpcValue = value;
        }
    }

    public class CrossAssemblyGenericMiddleBehaviourTest : ClientServerSetup<CrossAssemblyDerivedThroughMiddle>
    {
        [UnityTest]
        public IEnumerator CanSyncValues()
        {
            serverComponent.baseValue = 10;
            serverComponent.derivedValue = 20;
            yield return new WaitForSeconds(0.2f);

            Assert.That(clientComponent.baseValue, Is.EqualTo(10));
            Assert.That(clientComponent.derivedValue, Is.EqualTo(20));
        }

        [UnityTest]
        public IEnumerator CanCallRpcs()
        {
            serverComponent.RpcBase(30);
            serverComponent.RpcDerived(40);
            yield return new WaitForSeconds(0.2f);

            Assert.That(clientComponent.baseRpcValue, Is.EqualTo(30));
            Assert.That(clientComponent.derivedRpcValue, Is.EqualTo(40));
        }
    }
}
