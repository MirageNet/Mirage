using System.Threading;
using Cysharp.Threading.Tasks;
using Mirage.Authentication;
using UnityEngine;
using UnityEngine.Networking;

namespace Mirage.Snippets.Authentication
{
    // CodeEmbed-Start: auth-data
    public class CustomAuthenticationData
    {
        public string UserId;
        public string Username;
    }
    // CodeEmbed-End: auth-data

    // CodeEmbed-Start: auth-message
    [NetworkMessage]
    public struct CustomAuthMessage
    {
        // token used to validate user
        public string token;
    }
    // CodeEmbed-End: auth-message

    // CodeEmbed-Start: authenticator
    // CodeEmbed-Start: authenticator-def
    public class CustomAuthenticator : NetworkAuthenticator<CustomAuthMessage>
    {
        // CodeEmbed-End: authenticator-def
        protected override async UniTask<AuthenticationResult> AuthenticateAsync(INetworkPlayer player, CustomAuthMessage msg, CancellationToken cancellationToken)
        {
            // check user sent token, if they didn't then return fail
            if (string.IsNullOrEmpty(msg.token))
                return AuthenticationResult.CreateFail("No token");

            // send token to api to validate it
            var result = await ValidateToken(msg.token);

            // return success or fail
            if (result.Success)
            {
                // create auth data, this will be set on NetworkPlayer.Authentication.Data
                var data = new CustomAuthenticationData
                {
                    UserId = result.UserId,
                    Username = result.UserName,
                };
                return AuthenticationResult.CreateSuccess(this, data);
            }
            else
            {
                return AuthenticationResult.CreateFail("Validate failed");
            }
        }

        private static async UniTask<ValidateResultJson> ValidateToken(string token)
        {
            var sendJson = JsonUtility.ToJson(new ValidateTokenJson { token = token });

            // make sure to send token over https
#if UNITY_2022_3_OR_NEWER
            var webRequest = UnityWebRequest.PostWwwForm("https://example.com/api/validate", sendJson);
#else
            var webRequest = UnityWebRequest.Post("https://example.com/api/validate", sendJson);
#endif

            // wait for result
            var op = await webRequest.SendWebRequest();
            var text = op.downloadHandler.text;
            var result = JsonUtility.FromJson<ValidateResultJson>(text);
            return result;
        }

        private struct ValidateTokenJson
        {
            public string token;
        }

        private struct ValidateResultJson
        {
            public bool Success;
            public string UserId;
            public string UserName;
        }
    }
    // CodeEmbed-End: authenticator

    public class UserData
    {
        // CodeEmbed-Start: use-data
        public string GetPlayerName(INetworkPlayer player)
        {
            // get the data and cast it to customAuth type
            var data = player.Authentication.GetData<CustomAuthenticationData>();

            // use the data to get the value you want from it
            return data.Username;
        }
        // CodeEmbed-End: use-data
    }
}
