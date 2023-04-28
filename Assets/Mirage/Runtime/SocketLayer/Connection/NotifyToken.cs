using System;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// Object returned from <see cref="AckSystem.SendNotify"/> with events for when packet is Lost or Delivered
    /// </summary>
    public interface INotifyToken
    {
        event Action Delivered;
        event Action Lost;
    }

    /// <summary>
    /// Object returned from <see cref="AckSystem.SendNotify"/> with events for when packet is Lost or Delivered
    /// </summary>
    public class NotifyToken : INotifyToken, INotifyCallBack
    {
        public event Action Delivered;
        public event Action Lost;

        private bool _notified;

        public void OnDelivered()
        {
            if (_notified) throw new InvalidOperationException("this token as already been notified");
            _notified = true;

            Delivered?.Invoke();
        }

        public void OnLost()
        {
            if (_notified) throw new InvalidOperationException("this token as already been notified");
            _notified = true;

            Lost?.Invoke();
        }
    }

    /// <summary>
    /// Token that invokes <see cref="INotifyToken.Delivered"/> immediately
    /// </summary>
    public class AutoCompleteToken : INotifyToken
    {
        /// <summary>
        /// this token just invokes event instantly, so only needs 1 instance to exist
        /// </summary>
        public static AutoCompleteToken Instance = new AutoCompleteToken();
        protected AutoCompleteToken() { }

        public event Action Delivered
        {
            add
            {
                value.Invoke();
            }
            remove
            {
                // nothing
            }
        }
        public event Action Lost
        {
            add
            {
                // nothing
            }
            remove
            {
                // nothing
            }
        }
    }
}
