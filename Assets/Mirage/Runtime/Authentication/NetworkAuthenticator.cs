using System;
using Cysharp.Threading.Tasks;
using Mirage.Serialization;
using UnityEngine;

namespace Mirage.Authentication
{
    public interface INetworkAuthenticator
    {
        string AuthenticatorName { get; }
    }

    public abstract class NetworkAuthenticator : MonoBehaviour, INetworkAuthenticator
    {
        public virtual string AuthenticatorName => GetType().Name;

        internal abstract void Setup(MessageHandler messageHandler, Action<INetworkPlayer, AuthenticationResult> afterAuth);
    }

    public abstract class NetworkAuthenticator<T> : NetworkAuthenticator, INetworkAuthenticator
    {
        private Action<INetworkPlayer, AuthenticationResult> _afterAuth;

        internal sealed override void Setup(MessageHandler messageHandler, Action<INetworkPlayer, AuthenticationResult> afterAuth)
        {
            messageHandler.RegisterHandler<T>(HandleAuth, allowUnauthenticated: true);
            _afterAuth = afterAuth;
        }

        private async UniTaskVoid HandleAuth(INetworkPlayer player, T msg)
        {
            var result = await AuthenticateAsync(player, msg);
            _afterAuth.Invoke(player, result);
        }

        /// <summary>
        /// Called on server to Authenticate a message from client
        /// <para>
        /// Use <see cref="AuthenticateAsync(T)"/> OR <see cref="Authenticate(T)"/>. 
        /// By default the async version just call the normal version.
        /// </para>
        /// </summary>
        /// <param name="player">player that send message</param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected internal virtual UniTask<AuthenticationResult> AuthenticateAsync(INetworkPlayer player, T message)
        {
            return UniTask.FromResult(Authenticate(player, message));
        }

        /// <summary>
        /// Called on server to Authenticate a message from client
        /// <para>
        /// Use <see cref="AuthenticateAsync(T)"/> OR <see cref="Authenticate(T)"/>. 
        /// By default the async version just call the normal version.
        /// </para>
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected virtual AuthenticationResult Authenticate(INetworkPlayer player, T message) => throw new NotImplementedException("You must Implement Authenticate or AuthenticateAsync");

        /// <summary>
        /// Sends Authentication from client
        /// </summary>
        public void SendAuthentication(NetworkClient client, T message)
        {
            using (var writer = NetworkWriterPool.GetWriter())
            {
                MessagePacker.Pack(message, writer);
                var payload = writer.ToArraySegment();

                client.Send(new AuthMessage { Payload = payload });
            }
        }
    }
}

