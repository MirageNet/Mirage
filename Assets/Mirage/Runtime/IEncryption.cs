namespace Mirage
{
    public interface IEncryption
    {
        void Send<T>(INetworkPlayer player, T msg) where T : struct;
    }
}
