namespace Mirage.Authentication
{
    public class PlayerAuthentication
    {
        /// <summary>
        /// What Authenticator was used to accept this player
        /// <para>Null if no Authenticator existed on Server</para>
        /// </summary>
        public readonly NetworkAuthenticatorBase Authenticator;

        /// <summary>
        /// Authentication data set by Authenticator when player is accepted
        /// </summary>
        public readonly object Data;

        public PlayerAuthentication(NetworkAuthenticatorBase authenticator, object data)
        {
            Authenticator = authenticator;
            Data = data;
        }

        /// <summary>
        /// Helper method to cast <see cref="Data"/> to type set by NetworkAuthenticatorBase
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetData<T>() => (T)Data;
    }
}

