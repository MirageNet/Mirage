using System;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// <summary>
    /// Object returned from <see cref="NotifySystem.Send(byte[])"/> with events for when packet is Lost or Delivered
    /// </summary>
    public interface INotifyToken
    {
        event Action Delivered;
        event Action Lost;
    }


    /// <summary>
    /// Object returned from <see cref="NotifySystem.Send(byte[])"/> with events for when packet is Lost or Delivered
    /// </summary>
    public class NotifyToken : INotifyToken
    {
        public event Action Delivered;
        public event Action Lost;

        bool notified;

        internal void Notify(bool delivered)
        {
            if (notified) throw new InvalidOperationException("this token as already been notified");
            notified = true;

            if (delivered)
                Delivered?.Invoke();
            else
                Lost?.Invoke();
        }
    }
}
