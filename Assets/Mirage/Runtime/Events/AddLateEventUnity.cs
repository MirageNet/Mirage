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
    public sealed class AddLateEventUnity : AddLateEvent, IAddLateEventUnity
    {
        [SerializeField] private UnityEvent _event = new UnityEvent();

        public void AddListener(UnityAction handler)
        {
            // invoke handler if event has been invoked atleast once
            if (HasInvoked)
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

        public override void Invoke()
        {
            base.Invoke();
            _event.Invoke();
        }

        /// <summary>
        /// Remove all non-persisent (ie created from script) listeners from the event.
        /// </summary>
        public void RemoveAllListeners()
        {
            _event.RemoveAllListeners();
        }
    }

    /// <summary>
    /// Version of <see cref="AddLateEventUnity"/> with 1 argument
    /// <para>Create a non-generic class inheriting from this to use in inspector. Same rules as <see cref="UnityEvent"/></para>
    /// </summary>
    /// <typeparam name="T0">argument 0</typeparam>
    /// <typeparam name="TEvent">UnityEvent</typeparam>
    [Serializable]
    public abstract class AddLateEventUnity<T0, TEvent> : AddLateEvent<T0>, IAddLateEventUnity<T0>
        where TEvent : UnityEvent<T0>, new()
    {
        [SerializeField] private TEvent _event = new TEvent();

        public void AddListener(UnityAction<T0> handler)
        {
            // invoke handler if event has been invoked atleast once
            if (HasInvoked)
            {
                handler.Invoke(_arg0);
            }

            // add handler to inner event so that it can be invoked again
            _event.AddListener(handler);
        }

        public void RemoveListener(UnityAction<T0> handler)
        {
            _event.RemoveListener(handler);
        }

        public override void Invoke(T0 arg0)
        {
            base.Invoke(arg0);
            _event.Invoke(arg0);
        }

        /// <summary>
        /// Remove all non-persisent (ie created from script) listeners from the event.
        /// </summary>
        public void RemoveAllListeners()
        {
            _event.RemoveAllListeners();
        }
    }

    /// <summary>
    /// Version of <see cref="AddLateEventUnity"/> with 2 arguments
    /// <para>Create a non-generic class inheriting from this to use in inspector. Same rules as <see cref="UnityEvent"/></para>
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    [Serializable]
    public abstract class AddLateEventUnity<T0, T1, TEvent> : AddLateEvent<T0, T1>, IAddLateEventUnity<T0, T1>
        where TEvent : UnityEvent<T0, T1>, new()
    {
        [SerializeField] private TEvent _event = new TEvent();

        public void AddListener(UnityAction<T0, T1> handler)
        {
            // invoke handler if event has been invoked atleast once
            if (HasInvoked)
            {
                handler.Invoke(_arg0, _arg1);
            }

            // add handler to inner event so that it can be invoked again
            _event.AddListener(handler);
        }

        public void RemoveListener(UnityAction<T0, T1> handler)
        {
            _event.RemoveListener(handler);
        }

        public override void Invoke(T0 arg0, T1 arg1)
        {
            base.Invoke(arg0, arg1);
            _event.Invoke(arg0, arg1);
        }

        /// <summary>
        /// Remove all non-persisent (ie created from script) listeners from the event.
        /// </summary>
        public void RemoveAllListeners()
        {
            _event.RemoveAllListeners();
        }
    }
}
