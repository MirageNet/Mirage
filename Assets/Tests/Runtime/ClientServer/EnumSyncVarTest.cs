using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class SampleBehaviorWithEnum : NetworkBehaviour
    {
        public const Colors DefaultValue = Colors.Red;

        [SyncVar]
        public Colors myColor = DefaultValue;
    }

    public enum Colors {
        Blue,
        Green,
        Red,
        Yellow,
    }
    public class EnumSyncvarTest : ClientServerSetup<SampleBehaviorWithEnum>
    {
        [Test]
        public void UsesDefaultValue()
        {
            Assert.That(serverComponent.myColor, Is.EqualTo(SampleBehaviorWithEnum.DefaultValue));
            Assert.That(clientComponent.myColor, Is.EqualTo(SampleBehaviorWithEnum.DefaultValue));
        }
    }
}
