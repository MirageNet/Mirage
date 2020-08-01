using UnityEngine;
using NUnit.Framework;
using Object = UnityEngine.Object;
using UnityEditor.VersionControl;

namespace Mirror.Tests
{
    [TestFixture]
    public class HeadlessAutoStartTest : MonoBehaviour
    {
        protected GameObject gameObject;
        protected HeadlessAutoStart comp;

        [SetUp]
        public void Setup()
        {
            gameObject = new GameObject();
            comp = gameObject.AddComponent<HeadlessAutoStart>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void StartOnHeadlessValue()
        {
            Assert.That(comp.startOnHeadless, Is.True);
        }
    }
}
