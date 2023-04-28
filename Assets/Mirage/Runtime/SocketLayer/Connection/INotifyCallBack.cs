namespace Mirage.SocketLayer
{
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
}
