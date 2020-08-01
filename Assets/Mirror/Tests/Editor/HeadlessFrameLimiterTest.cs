using UnityEngine;
using NUnit.Framework;
using Object = UnityEngine.Object;

namespace Mirror.Tests
{
    [TestFixture]
    public class HeadlessFrameLimiterTest : MonoBehaviour
    {
        protected GameObject gameObject;
        protected HeadlessFrameLimiter comp;

        [SetUp]
        public void Setup()
        {
            gameObject = new GameObject();
            comp = gameObject.AddComponent<HeadlessFrameLimiter>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void StartOnHeadlessValue()
        {
            Assert.That(comp.serverTickRate, Is.EqualTo(30));
        }
    }
}
