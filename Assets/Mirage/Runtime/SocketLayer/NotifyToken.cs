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

    public static class INotifyCallBackExtensions
    {
        public static void Notify(this INotifyCallBack callBack, bool delivered)
        {
            if (delivered)
                callBack.OnDelivered();
            else
                callBack.OnLost();
        }
    }

    /// <summary>
    /// Can be passed into <see cref="AckSystem.SendNotify(byte[], int, int, INotifyCallBack)"/> and methods will be invoked when notify is delivered or lost
    /// <para>
    /// See the Notify Example on how to use this interface
    /// </para>
    /// </summary>
    public interface INotifyCallBack
    {
        void OnDelivered();
        void OnLost();
    }
}
