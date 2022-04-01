using System;
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
    public sealed class AddLateEvent : AddLateEventBase, IAddLateEvent
    {
        event Action _event;

        public void AddListener(Action handler)
        {
            // invoke handler if event has been invoked atleast once
            if (hasInvoked)
            {
                handler.Invoke();
            }

            // add handler to inner event so that it can be invoked again
            _event += handler;
        }

        public void RemoveListener(Action handler)
        {
            _event -= handler;
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
    public sealed class AddLateEvent<T0> : AddLateEventBase, IAddLateEvent<T0>
    {
        event Action<T0> _event;

        T0 arg0;

        public void AddListener(Action<T0> handler)
        {
            // invoke handler if event has been invoked atleast once
            if (hasInvoked)
            {
                handler.Invoke(arg0);
            }

            // add handler to inner event so that it can be invoked again
            _event += handler;
        }

        public void RemoveListener(Action<T0> handler)
        {
            _event -= handler;
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
    public sealed class AddLateEvent<T0, T1> : AddLateEventBase, IAddLateEvent<T0, T1>
    {
        event Action<T0, T1> _event;

        T0 arg0;
        T1 arg1;

        public void AddListener(Action<T0, T1> handler)
        {
            // invoke handler if event has been invoked atleast once
            if (hasInvoked)
            {
                handler.Invoke(arg0, arg1);
            }

            // add handler to inner event so that it can be invoked again
            _event += handler;
        }

        public void RemoveListener(Action<T0, T1> handler)
        {
            _event -= handler;
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
