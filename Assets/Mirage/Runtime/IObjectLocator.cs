namespace Mirage
{
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
        /// <param name="identity">The NetworkIdentity matching the netId or null if none is found</param>
        /// <returns>true if identity is found and is not null</returns>
        bool TryGetIdentity(uint netId, out NetworkIdentity identity);
    }
}
