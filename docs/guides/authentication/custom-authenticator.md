---
sidebar_position: 3
---
# Custom Authenticator

To create a custom Authenticator, follow these steps:

1. Inherit from the `NetworkAuthenticatorBase<T>` class.
2. Create a network message that your authenticator will receive from the client.
    - Clients should use the `SendAuthentication(NetworkClient client, T msg)` method provided by the authenticator to correctly send the authentication message.
4. Implement your authenticator to process this message and return a success or failure result.
5. Optionally, your authenticator can return additional data that you want to set on `INetworkPlayer.Authentication`.
6. Use the `GetData<T>()` method to retrieve the custom data on the server-side.


**Step 1: Inherit from `NetworkAuthenticatorBase<T>`**
```cs
public class CustomAuthenticator : NetworkAuthenticator<CustomAuthMessage>
 {
```

**Step 2: Create a Network Message**
```cs
[NetworkMessage]
 public struct CustomAuthMessage
 {
     // token used to validate user
     public string token;
 }
```

Clients should use the `SendAuthentication(NetworkClient client, T msg)` method to correctly send the authentication message.

:::note
Using `player.Send` directly will not work because the authenticator message is wrapped in an internal `AuthMessage` message.
:::

**Step 3: Implement the Authenticator**
```cs
    public class CustomAuthenticator : NetworkAuthenticator<CustomAuthMessage>
    {
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
```

**Step 4: Return Additional Data (Optional)**
```cs
public class CustomAuthenticationData
 {
     public string UserId;
     public string Username;
 }
```

**Step 5: Retrieve Custom Data**
```cs
public string GetPlayerName(INetworkPlayer player)
 {
     // get the data and cast it to customAuth type
     var data = player.Authentication.GetData<CustomAuthenticationData>();

     // use the data to get the value you want from it
     return data.Username;
 }
```
