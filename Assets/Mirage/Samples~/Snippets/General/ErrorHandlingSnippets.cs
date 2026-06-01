using UnityEngine;

namespace Mirage.Snippets.General
{

    // CodeEmbed-Start: error-handling-custom-flags
    public static class MyErrorFlags
    {
        public static readonly PlayerErrorFlags InvalidTrade = Custom(0);
        public static readonly PlayerErrorFlags AnotherCustom = Custom(1);

        private static PlayerErrorFlags Custom(int index)
        {
            return (PlayerErrorFlags)(((int)PlayerErrorFlags.CustomError) << index);
        }
    }
    // CodeEmbed-End: error-handling-custom-flags
}

namespace Mirage.Snippets.General.CustomError
{
    // CodeEmbed-Start: error-handling-custom-error-class
    public static class MyErrorFlags
    {
        public static readonly PlayerErrorFlags InvalidAction = Custom(0);

        private static PlayerErrorFlags Custom(int index)
        {
            return (PlayerErrorFlags)(((int)PlayerErrorFlags.CustomError) << index);
        }
    }
    // CodeEmbed-End: error-handling-custom-error-class

    public class CustomErrorBehaviour : NetworkBehaviour
    {
        private bool IsActionValid(int data) => true;

        // CodeEmbed-Start: error-handling-custom-error-method
        [ServerRpc]
        private void CmdDoSomething(int data)
        {
            // The IsActionValid method would contain your custom validation logic.
            if (!IsActionValid(data))
            {
                // Penalize the player with a moderate cost for sending invalid data.
                Owner.SetError(10, MyErrorFlags.InvalidAction);
                return;
            }

            // ... process valid data
        }
        // CodeEmbed-End: error-handling-custom-error-method
    }
}

namespace Mirage.Snippets.General.AdminAction
{
    public class AdminActionBehaviour : NetworkBehaviour
    {
        private bool IsAdmin(INetworkPlayer player) => false;

        // CodeEmbed-Start: error-handling-admin-action
        [ServerRpc]
        private void CmdTryAdminAction(string command)
        {
            // The IsAdmin method would check if the player has admin privileges.
            if (!IsAdmin(Owner))
            {
                // A non-admin tried to use an admin command.
                // Set cost higher than MaxTokens (default 200) to trigger the limit immediately.
                Owner.SetError(10000, PlayerErrorFlags.Critical);
                return;
            }

            // ... execute admin command
        }
        // CodeEmbed-End: error-handling-admin-action
    }
}

namespace Mirage.Snippets.General.PublicMessage
{
    public class PublicMessageBehaviour : NetworkBehaviour
    {
        private bool CheckMessageRateLimit(INetworkPlayer sender) => false;

        // CodeEmbed-Start: error-handling-public-message
        [Client]
        public void SendPublicMessage(string message)
        {
            // client side check before sending message
            if (string.IsNullOrWhiteSpace(message) || message.Length > 100)
                return;

            CmdSendPublicMessage(message);
        }

        [ServerRpc(requireAuthority = false)]
        private void CmdSendPublicMessage(string message, INetworkPlayer sender = null)
        {
            if (string.IsNullOrWhiteSpace(message) || message.Length > 100)
            {
                // Invalid message length. this is very likely a cheat because message length is checked on client before
                // how ever this is just chat message nothing not critical gameplay
                // for example could be from chat mod with higher size that they left on after playing on a modded server
                sender.SetError(50, PlayerErrorFlags.LikelyCheater);
                return;
            }

            if (CheckMessageRateLimit(sender))
            {
                // player sent more message than chat rate limit, just use low cost
                sender.SetError(1, PlayerErrorFlags.None);
                return;
            }

            // ...
        }
        // CodeEmbed-End: error-handling-public-message
    }
}

namespace Mirage.Snippets.General.CustomErrorHandler
{
    public static class MyErrorFlags
    {
        public static readonly PlayerErrorFlags InvalidAction = Custom(0);

        private static PlayerErrorFlags Custom(int index)
        {
            return (PlayerErrorFlags)(((int)PlayerErrorFlags.CustomError) << index);
        }
    }

    // CodeEmbed-Start: error-handling-custom-handler
    public class MyGameServer : MonoBehaviour
    {
        public NetworkServer server;

        private void Start()
        {
            server.SetErrorRateLimitReachedCallback(OnPlayerErrorLimitReached);
        }

        private void OnPlayerErrorLimitReached(INetworkPlayer player)
        {
            Debug.LogWarning($"Player {player} reached error limit with flags: {player.ErrorFlags}");

            if ((player.ErrorFlags & PlayerErrorFlags.Critical) != 0)
            {
                // For critical errors, always disconnect.
                player.Disconnect();

                // ... add player to ban or timeout list here so they can't reconnect

                return;
            }
            else if ((player.ErrorFlags & MyErrorFlags.InvalidAction) != 0)
            {
                // For our custom action, maybe just send a warning.
                // Note: You would need to implement the ChatMessage struct and its handler.
                // player.Send(new ChatMessage("You are performing too many invalid actions."));
            }
            // ... other custom logic

            // Reset flags after handling
            player.ResetErrorFlag();
        }
    }
    // CodeEmbed-End: error-handling-custom-handler
}
