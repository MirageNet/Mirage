using System;
using System.Collections.Generic;
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
    public class AddLateEvent : AddLateEventBase, IAddLateEvent
    {
        private readonly List<Action> tmp = new List<Action>();
        private readonly List<Action> _listeners = new List<Action>();

        public void AddListener(Action handler)
        {
            // invoke handler if event has been invoked atleast once
            if (HasInvoked)
            {
                handler.Invoke();
            }

            // add handler to inner event so that it can be invoked again
            _listeners.Add(handler);
        }
        public void RemoveListener(Action handler)
        {
            _listeners.Remove(handler);
        }

        public virtual void Invoke()
        {
            MarkInvoked();

            // tmp incase RemoveListener is called inside loop
            tmp.Clear();
            tmp.AddRange(_listeners);
            foreach (var handler in tmp)
                handler.Invoke();
            tmp.Clear();
        }
    }

    /// <summary>
    /// Version of <see cref="AddLateEvent"/> with 1 argument
    /// <para>Create a non-generic class inheriting from this to use in inspector. Same rules as <see cref="UnityEvent"/></para>
    /// </summary>
    /// <typeparam name="T0">argument 0</typeparam>
    /// <typeparam name="TEvent">UnityEvent</typeparam>
    [Serializable]
    public class AddLateEvent<T0> : AddLateEventBase, IAddLateEvent<T0>
    {
        private readonly List<Action<T0>> tmp = new List<Action<T0>>();
        private readonly List<Action<T0>> _listeners = new List<Action<T0>>();

        protected T0 _arg0;

        public void AddListener(Action<T0> handler)
        {
            // invoke handler if event has been invoked atleast once
            if (HasInvoked)
            {
                handler.Invoke(_arg0);
            }

            // add handler to inner event so that it can be invoked again
            _listeners.Add(handler);
        }

        public void RemoveListener(Action<T0> handler)
        {
            _listeners.Remove(handler);
        }

        public virtual void Invoke(T0 arg0)
        {
            MarkInvoked();

            _arg0 = arg0;
            // tmp incase RemoveListener is called inside loop
            tmp.Clear();
            tmp.AddRange(_listeners);
            foreach (var handler in tmp)
                handler.Invoke(arg0);
            tmp.Clear();
        }
    }

    /// <summary>
    /// Version of <see cref="AddLateEvent"/> with 2 arguments
    /// <para>Create a non-generic class inheriting from this to use in inspector. Same rules as <see cref="UnityEvent"/></para>
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    [Serializable]
    public class AddLateEvent<T0, T1> : AddLateEventBase, IAddLateEvent<T0, T1>
    {
        private readonly List<Action<T0, T1>> tmp = new List<Action<T0, T1>>();
        private readonly List<Action<T0, T1>> _listeners = new List<Action<T0, T1>>();

        protected T0 _arg0;
        protected T1 _arg1;

        public void AddListener(Action<T0, T1> handler)
        {
            // invoke handler if event has been invoked atleast once
            if (HasInvoked)
            {
                handler.Invoke(_arg0, _arg1);
            }

            // add handler to inner event so that it can be invoked again
            _listeners.Add(handler);
        }

        public void RemoveListener(Action<T0, T1> handler)
        {
            _listeners.Remove(handler);
        }

        public virtual void Invoke(T0 arg0, T1 arg1)
        {
            MarkInvoked();

            _arg0 = arg0;
            _arg1 = arg1;
            // tmp incase RemoveListener is called inside loop
            tmp.Clear();
            tmp.AddRange(_listeners);
            foreach (var handler in tmp)
                handler.Invoke(arg0, arg1);
            tmp.Clear();
        }
    }
}
