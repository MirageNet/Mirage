namespace Mirage
{
    public static class Version
    {
        public static readonly string Current = typeof(NetworkIdentity).Assembly.GetName().Version.ToString();
    }

    /// <summary>
    /// An object that implements this interface can find objects by their net id
    /// This is used by readers when trying to deserialize gameobjects
    /// </summary>
    public interface IObjectLocator
    {
        /// <summary>
        /// Finds a network identity by id
        /// </summary>
        /// <param name="netId">the id of the object to find</param>
        /// <returns>The NetworkIdentity matching the netid or null if none is found</returns>
        NetworkIdentity this[uint netId] { get; }
    }

}
