using System;

namespace Mirage.Events
{
    /// <summary>
    /// Event that can only run once, adding handler late will it invoke right away
    /// </summary>
    /// <remarks>
    /// Interface only contains AddHandler method because Invoke should only be called from the owner of the event
    /// </remarks>
    public interface IAddLateEvent_new
    {
        void AddListener(Action handler);
        void RemoveListener(Action handler);
    }


    /// <summary>
    /// Version of <see cref="IAddLateEventUnity"/> with 1 argument
    /// </summary>
    public interface IAddLateEvent_new<T0>
    {
        void AddListener(Action<T0> handler);
        void RemoveListener(Action<T0> handler);
    }


    /// <summary>
    /// Version of <see cref="IAddLateEventUnity"/> with 2 arguments
    /// </summary>
    public interface IAddLateEvent_new<T0, T1>
    {
        void AddListener(Action<T0, T1> handler);
        void RemoveListener(Action<T0, T1> handler);
    }
}
