using NUnit.Framework;
using UnityEngine.Events;

namespace Mirage.Events.Tests
{
    public abstract class AddLateEventTestsBase
    {
        int listenerCallCount;
        protected void TestListener() => listenerCallCount++;

        protected abstract void Init();
        protected abstract void Invoke();
        protected abstract void AddListener();
        protected abstract void RemoveListener();
        protected abstract void Reset();
        protected abstract void RemoveAllListeners();


        [SetUp]
        public void Setup()
        {
            listenerCallCount = 0;
            Init();
        }

        [Test]
        public void EventCanBeInvokedOnce()
        {
            AddListener();
            Invoke();
            Assert.That(listenerCallCount, Is.EqualTo(1));
        }

        [Test]
        public void EventCanBeInvokedTwice()
        {
            AddListener();

            Invoke();
            Assert.That(listenerCallCount, Is.EqualTo(1));

            Invoke();
            Assert.That(listenerCallCount, Is.EqualTo(2));
        }

        [Test]
        public void EventCanBeInvokedEmpty()
        {
            Assert.DoesNotThrow(() =>
            {
                Invoke();
            });
        }

        [Test]
        public void AddingListenerLateInvokesListener()
        {
            Invoke();
            Assert.That(listenerCallCount, Is.EqualTo(0));
            AddListener();
            Assert.That(listenerCallCount, Is.EqualTo(1));
        }

        [Test]
        public void AddingListenerLateInvokesListenerOnce()
        {
            Invoke();
            Invoke();
            Invoke();
            Assert.That(listenerCallCount, Is.EqualTo(0));
            AddListener();
            Assert.That(listenerCallCount, Is.EqualTo(1));
        }

        [Test]
        public void AddingListenerLateCanBeInvokedMultipleTimes()
        {
            Invoke();
            Assert.That(listenerCallCount, Is.EqualTo(0));

            AddListener();
            Assert.That(listenerCallCount, Is.EqualTo(1));

            Invoke();
            Assert.That(listenerCallCount, Is.EqualTo(2));
        }

        [Test]
        public void ResetThenAddListenerDoesntInvokeRightAway()
        {
            AddListener();

            Invoke();
            Assert.That(listenerCallCount, Is.EqualTo(1));

            Reset();

            AddListener();
            Assert.That(listenerCallCount, Is.EqualTo(1), "Event should not auto invoke after reset");

            Invoke();
            Assert.That(listenerCallCount, Is.EqualTo(3), "old and new listeners should have been invoked");
        }

        [Test]
        public void RemoveListenersShouldRemove1Listner()
        {
            AddListener();

            Invoke();
            Assert.That(listenerCallCount, Is.EqualTo(1));

            RemoveListener();
            Invoke();
            // listener removed so no increase to count
            Assert.That(listenerCallCount, Is.EqualTo(1));
        }

        [Test]
        public void RemoveAllRemovesListeners()
        {
            AddListener();

            Invoke();
            Assert.That(listenerCallCount, Is.EqualTo(1));

            RemoveAllListeners();

            Assert.DoesNotThrow(() =>
            {
                Invoke();
            });
            // listener removed so no increase to count
            Assert.That(listenerCallCount, Is.EqualTo(1));
        }
    }


    public class AddLateEvent0ArgTest : AddLateEventTestsBase
    {
        AddLateEvent allLate;
        protected override void Init()
        {
            allLate = new AddLateEvent();
        }

        protected override void Invoke()
        {
            allLate.Invoke();
        }

        protected override void AddListener()
        {
            allLate.AddListener(TestListener);
        }

        protected override void RemoveListener()
        {
            allLate.RemoveListener(TestListener);
        }

        protected override void Reset()
        {
            allLate.Reset();
        }

        protected override void RemoveAllListeners()
        {
            allLate.RemoveAllListeners();
        }
    }


    public class IntUnityEvent : UnityEvent<int> { }
    public class IntAddLateEvent : AddLateEvent<int, IntUnityEvent> { }
    public class AddLateEvent1ArgTest : AddLateEventTestsBase
    {
        IntAddLateEvent allLate;

        void TestListener1Arg(int a)
        {
            TestListener();
        }

        protected override void Init()
        {
            allLate = new IntAddLateEvent();
        }

        protected override void Invoke()
        {
            allLate.Invoke(default);
        }

        protected override void AddListener()
        {
            allLate.AddListener(TestListener1Arg);
        }

        protected override void RemoveListener()
        {
            allLate.RemoveListener(TestListener1Arg);
        }

        protected override void Reset()
        {
            allLate.Reset();
        }

        protected override void RemoveAllListeners()
        {
            allLate.RemoveAllListeners();
        }

        [Test]
        public void ListenerIsInvokedWithCorrectArgs()
        {
            const int arg0 = 10;

            int callCount = 0;

            allLate.AddListener((a0) =>
            {
                callCount++;
                Assert.That(a0, Is.EqualTo(arg0));
            });


            allLate.Invoke(arg0);
            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public void ListenerIsInvokedLateWithCorrectArgs()
        {
            const int arg0 = 10;

            int callCount = 0;

            // invoke before adding handler
            allLate.Invoke(arg0);

            allLate.AddListener((a0) =>
            {
                callCount++;
                Assert.That(a0, Is.EqualTo(arg0));
            });

            Assert.That(callCount, Is.EqualTo(1));
        }
    }


    public class IntStringUnityEvent : UnityEvent<int, string> { }
    public class IntStringAddLateEvent : AddLateEvent<int, string, IntStringUnityEvent> { }
    public class AddLateEvent2ArgTest : AddLateEventTestsBase
    {
        IntStringAddLateEvent allLate;
        void TestListener2Arg(int a, string b)
        {
            TestListener();
        }

        protected override void Init()
        {
            allLate = new IntStringAddLateEvent();
        }

        protected override void Invoke()
        {
            allLate.Invoke(default, default);
        }

        protected override void AddListener()
        {
            allLate.AddListener(TestListener2Arg);
        }

        protected override void RemoveListener()
        {
            allLate.RemoveListener(TestListener2Arg);
        }

        protected override void Reset()
        {
            allLate.Reset();
        }

        protected override void RemoveAllListeners()
        {
            allLate.RemoveAllListeners();
        }

        [Test]
        public void ListenerIsInvokedWithCorrectArgs()
        {
            const int arg0 = 10;
            const string arg1 = "hell world";

            int callCount = 0;

            allLate.AddListener((a0, a1) =>
            {
                callCount++;
                Assert.That(a0, Is.EqualTo(arg0));
                Assert.That(a1, Is.EqualTo(arg1));
            });


            allLate.Invoke(arg0, arg1);
            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public void ListenerIsInvokedLateWithCorrectArgs()
        {
            const int arg0 = 10;
            const string arg1 = "hell world";

            int callCount = 0;

            // invoke before adding handler
            allLate.Invoke(arg0, arg1);

            allLate.AddListener((a0, a1) =>
            {
                callCount++;
                Assert.That(a0, Is.EqualTo(arg0));
                Assert.That(a1, Is.EqualTo(arg1));
            });

            Assert.That(callCount, Is.EqualTo(1));
        }
    }
}
