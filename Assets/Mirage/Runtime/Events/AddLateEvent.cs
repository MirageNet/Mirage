using System;
using UnityEngine;
using UnityEngine.Events;

namespace Mirage.Events
{
    /// <summary>
    /// An event that will invoke handlers immediately if they are added after <see cref="Invoke"/> has been called
    /// </summary>
    /// <remarks>
    /// <para>
    /// AddLateEvent should be used for time sensitive events where Invoke might be called before the user has chance to add a handler. 
    /// For example Server Started event.
    /// </para>
    /// <para>
    /// Events that are invoked multiple times, like AuthorityChanged, will have the most recent <see cref="Invoke"/> argument sent to new handler. 
    /// </para>
    /// </remarks>
    /// <example>
    /// This Example shows uses of Event
    /// <code>
    /// 
    /// public class Server : MonoBehaviour
    /// {
    ///     // shows in inspector
    ///     [SerializeField]
    ///     private AddLateEvent _started;
    ///
    ///     // expose interface so others can add handlers, but does not let them invoke
    ///     public IAddLateEvent Started => customEvent;
    ///
    ///     public void StartServer()
    ///     {
    ///         // ...
    ///
    ///         // invoke using field
    ///         _started.Invoke();
    ///     }
    ///
    ///     public void StopServer()
    ///     {
    ///         // ...
    ///
    ///         // reset event, resets the hasInvoked flag
    ///         _started.Reset();
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// This is an example to show how to create events with arguments:
    /// <code>
    /// // Serializable so that it can be used in inspector
    /// [Serializable]
    /// public class IntUnityEvent : UnityEvent&lt;int&gt; { }
    /// [Serializable]
    /// public class IntAddLateEvent : AddLateEvent&lt;int, IntUnityEvent&gt; { }
    /// 
    /// public class MyClass : MonoBehaviour
    /// {
    ///     [SerializeField]
    ///     private IntAddLateEvent customEvent;
    /// 
    ///     public IAddLateEvent&lt;int&gt; CustomEvent => customEvent;
    /// }
    /// </code>
    /// </example>
    [Serializable]
    public sealed class AddLateEvent : AddLateEventBase, IAddLateEvent
    {
        [SerializeField] UnityEvent _event = new UnityEvent();

        protected override UnityEventBase baseEvent => _event;

        public void AddListener(UnityAction handler)
        {
            // invoke handler if event has been invoked atleast once
            if (hasInvoked)
            {
                handler.Invoke();
            }

            // add handler to inner event so that it can be invoked again
            _event.AddListener(handler);
        }

        public void RemoveListener(UnityAction handler)
        {
            _event.RemoveListener(handler);
        }

        public void Invoke()
        {
            MarkInvoked();

            _event.Invoke();
        }
    }

    /// <summary>
    /// Version of <see cref="AddLateEvent"/> with 1 argument
    /// <para>Create a non-generic class inheriting from this to use in inspector. Same rules as <see cref="UnityEvent"/></para>
    /// </summary>
    /// <typeparam name="T0">argument 0</typeparam>
    /// <typeparam name="TEvent">UnityEvent</typeparam>
    [Serializable]
    public abstract class AddLateEvent<T0, TEvent> : AddLateEventBase, IAddLateEvent<T0>
        where TEvent : UnityEvent<T0>, new()
    {
        [SerializeField] TEvent _event = new TEvent();
        protected override UnityEventBase baseEvent => _event;

        T0 arg0;

        public void AddListener(UnityAction<T0> handler)
        {
            // invoke handler if event has been invoked atleast once
            if (hasInvoked)
            {
                handler.Invoke(arg0);
            }

            // add handler to inner event so that it can be invoked again
            _event.AddListener(handler);
        }

        public void RemoveListener(UnityAction<T0> handler)
        {
            _event.RemoveListener(handler);
        }

        public void Invoke(T0 arg0)
        {
            MarkInvoked();

            this.arg0 = arg0;
            _event.Invoke(arg0);
        }
    }

    /// <summary>
    /// Version of <see cref="AddLateEvent"/> with 2 arguments
    /// <para>Create a non-generic class inheriting from this to use in inspector. Same rules as <see cref="UnityEvent"/></para>
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    [Serializable]
    public abstract class AddLateEvent<T0, T1, TEvent> : AddLateEventBase, IAddLateEvent<T0, T1>
        where TEvent : UnityEvent<T0, T1>, new()
    {
        [SerializeField] TEvent _event = new TEvent();
        protected override UnityEventBase baseEvent => _event;

        T0 arg0;
        T1 arg1;

        public void AddListener(UnityAction<T0, T1> handler)
        {
            // invoke handler if event has been invoked atleast once
            if (hasInvoked)
            {
                handler.Invoke(arg0, arg1);
            }

            // add handler to inner event so that it can be invoked again
            _event.AddListener(handler);
        }

        public void RemoveListener(UnityAction<T0, T1> handler)
        {
            _event.RemoveListener(handler);
        }

        public void Invoke(T0 arg0, T1 arg1)
        {
            MarkInvoked();

            this.arg0 = arg0;
            this.arg1 = arg1;
            _event.Invoke(arg0, arg1);
        }
    }
}
