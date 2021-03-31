using NUnit.Framework;
using NUnit.Framework.Internal;
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
        protected abstract void Reset();


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
            Assert.That(listenerCallCount, Is.EqualTo(2));
        }

        [Test]
        public void ResetEventRemovesOldListners()
        {
            AddListener();

            Invoke();
            Assert.That(listenerCallCount, Is.EqualTo(1));

            Reset();

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
        AddLateEvent onceEvent;
        protected override void Init()
        {
            onceEvent = new AddLateEvent();
        }

        protected override void Invoke()
        {
            onceEvent.Invoke();
        }

        protected override void AddListener()
        {
            onceEvent.AddListener(TestListener);
        }

        protected override void Reset()
        {
            onceEvent.Reset();
        }
    }


    public class IntUnityEvent : UnityEvent<int> { }
    public class IntAddLateEvent : AddLateEvent<int, IntUnityEvent> { }
    public class AddLateEvent1ArgTest : AddLateEventTestsBase
    {
        IntAddLateEvent onceEvent;

        protected override void Init()
        {
            onceEvent = new IntAddLateEvent();
        }

        protected override void Invoke()
        {
            onceEvent.Invoke(default);
        }

        protected override void AddListener()
        {
            onceEvent.AddListener((_) => TestListener());
        }

        protected override void Reset()
        {
            onceEvent.Reset();
        }

        [Test]
        public void ListenerIsInvokedWithCorrectArgs()
        {
            const int arg0 = 10;

            int callCount = 0;

            onceEvent.AddListener((a0) =>
            {
                callCount++;
                Assert.That(a0, Is.EqualTo(arg0));
            });


            onceEvent.Invoke(arg0);
            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public void ListenerIsInvokedLateWithCorrectArgs()
        {
            const int arg0 = 10;

            int callCount = 0;

            // invoke before adding handler
            onceEvent.Invoke(arg0);

            onceEvent.AddListener((a0) =>
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
        IntStringAddLateEvent onceEvent;

        protected override void Init()
        {
            onceEvent = new IntStringAddLateEvent();
        }

        protected override void Invoke()
        {
            onceEvent.Invoke(default, default);
        }

        protected override void AddListener()
        {
            onceEvent.AddListener((_, __) => TestListener());
        }

        protected override void Reset()
        {
            onceEvent.Reset();
        }

        [Test]
        public void ListenerIsInvokedWithCorrectArgs()
        {
            const int arg0 = 10;
            const string arg1 = "hell world";

            int callCount = 0;

            onceEvent.AddListener((a0, a1) =>
            {
                callCount++;
                Assert.That(a0, Is.EqualTo(arg0));
                Assert.That(a1, Is.EqualTo(arg1));
            });


            onceEvent.Invoke(arg0, arg1);
            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public void ListenerIsInvokedLateWithCorrectArgs()
        {
            const int arg0 = 10;
            const string arg1 = "hell world";

            int callCount = 0;

            // invoke before adding handler
            onceEvent.Invoke(arg0, arg1);

            onceEvent.AddListener((a0, a1) =>
            {
                callCount++;
                Assert.That(a0, Is.EqualTo(arg0));
                Assert.That(a1, Is.EqualTo(arg1));
            });

            Assert.That(callCount, Is.EqualTo(1));
        }
    }
}
