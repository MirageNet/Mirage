namespace Mirage
{
    public interface INetworkIdentityGenerator
    {
        /// <summary>
        ///     Generate your own specific net id.
        /// </summary>
        byte GenerateNetId();
    }
}
