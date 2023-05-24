using System;
using System.Collections.Generic;

namespace Mirage.Authentication
{
    public class PlayerAuthentication
    {
        /// <summary>
        /// What Authenticator was used to accept this player
        /// <para>Null if no Authenticator existed on Server</para>
        /// </summary>
        public readonly INetworkAuthenticator Authenticator;

        /// <summary>
        /// Authentication data set by Authenticator when player is accepted
        /// </summary>
        public readonly object Data;

        public PlayerAuthentication(INetworkAuthenticator authenticator, object data)
        {
            Authenticator = authenticator;
            Data = data;
        }

        /// <summary>
        /// Helper method to cast <see cref="Data"/> to type set by NetworkAuthenticatorBase
        /// <para>WARNING: this function is <b>NOT thread safe</b> when data is <see cref="IAuthenticationDataWrapper"/> rather than <typeparamref name="T"/> directly</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetData<T>()
        {
            // short cut if data is type T
            if (Data is T tdata)
                return tdata;

            // otherwise check if it is wrapper
            antiInfiniteLoop.Clear();
            return GetDataRecursive<T>(Data);
        }

        private static readonly HashSet<IAuthenticationDataWrapper> antiInfiniteLoop = new HashSet<IAuthenticationDataWrapper>();

        private static T GetDataRecursive<T>(object data)
        {
            // if data is T then return it
            if (data is T tdata)
                return tdata;

            // else if data is a wrapper, then set it as data, and check if that is T
            // we need to do this in a while loop because wrapper.inner might also be a wrapper
            if (data is IAuthenticationDataWrapper wrapper)
            {
                if (antiInfiniteLoop.Contains(wrapper))
                    throw new InvalidOperationException($"Infinite loop detected, wrappers: {string.Join(",", antiInfiniteLoop)}");

                antiInfiniteLoop.Add(wrapper);
                return GetDataRecursive<T>(wrapper.Inner);
            }

            throw new InvalidCastException($"Unable to cast data to type {typeof(T)}");
        }
    }

    /// <summary>
    /// Auth data might be a wrapper around another Authenticator's data.
    /// In that case <see cref="PlayerAuthentication.GetData{T}"/> should check if data is T or if it is IDataWrapper
    /// </summary>
    public interface IAuthenticationDataWrapper
    {
        object Inner { get; }
    }
}

