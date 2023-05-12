using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Mirage.Authentication
{
    public abstract class NetworkAuthenticatorBase : MonoBehaviour
    {
        internal abstract void Setup(MessageHandler messageHandler, Action<INetworkPlayer, AuthenticationResult> afterAuth);
    }

    public abstract class NetworkAuthenticatorBase<T> : NetworkAuthenticatorBase
    {
        private Action<INetworkPlayer, AuthenticationResult> _afterAuth;

        internal string AuthenticatorName => GetType().Name;

        internal sealed override void Setup(MessageHandler messageHandler, Action<INetworkPlayer, AuthenticationResult> afterAuth)
        {
            messageHandler.RegisterHandler<T>(HandleAuth);
            _afterAuth = afterAuth;
        }

        private async UniTaskVoid HandleAuth(INetworkPlayer player, T msg)
        {
            var result = await Authenticate(msg);
            _afterAuth.Invoke(player, result);
        }

        /// <summary>
        /// Called on server to Authenticate a message from client
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected abstract UniTask<AuthenticationResult> Authenticate(T message);

        /// <summary>
        /// Called on client to create message to send to server
        /// </summary>
        /// <returns></returns>
        protected abstract UniTask<T> CreateAuthentication();
    }
}

