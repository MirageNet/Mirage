using UnityEngine.Events;

namespace Mirage.Events
{
    /// <summary>
    /// Event that can only run once, adding handler late will it invoke right away
    /// </summary>
    /// <remarks>
    /// Interface only contains AddHandler method because Invoke should only be called from the owner of the event
    /// </remarks>
    public interface IAddLateEventUnity : IAddLateEvent
    {
        void AddListener(UnityAction handler);
        void RemoveListener(UnityAction handler);
    }


    /// <summary>
    /// Version of <see cref="IAddLateEventUnity"/> with 1 argument
    /// </summary>
    public interface IAddLateEventUnity<T0> : IAddLateEvent<T0>
    {
        void AddListener(UnityAction<T0> handler);
        void RemoveListener(UnityAction<T0> handler);
    }


    /// <summary>
    /// Version of <see cref="IAddLateEventUnity"/> with 2 arguments
    /// </summary>
    public interface IAddLateEventUnity<T0, T1> : IAddLateEvent<T0, T1>
    {
        void AddListener(UnityAction<T0, T1> handler);
        void RemoveListener(UnityAction<T0, T1> handler);
    }
}
