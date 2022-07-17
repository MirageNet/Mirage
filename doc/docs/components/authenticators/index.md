# Authentication

For some games you may want to limit who can join or uniquely identity a user in order to save stats or communicate with friends. Authentication is checking if a user is valid and is who they say they are. There are several methods available, some examples include:
- Ask the user for username and password
- Use a third party OAuth2 or OpenID identity provider, such as Facebook, Twitter, Google
- Use a third party service such as PlayFab, GameLift or Steam
- Use the device id, very popular method in mobile
- Use Google Play in Android
- Use Game Center in IOS
- Use a web service in your website


## Encryption Notice

By default Mirage is not encrypted, so if you want to do authentication through Mirage, we highly recommend you use a transport that supports encryption.

## Basic Authenticator

[Basic Authenticator](/docs/components/authenticators/basic-authenticator)  

Mirage includes a Basic Authenticator in the `Mirage/Authenticators` folder which just uses a simple password. This will only allow people with the password to join the server. For example, a password on a hosted game so that only friends can join.


## Custom Authenticators

To Create a custom authenticator implement the `NetworkAuthenticator` abstract class and override the `ServerAuthenticated` and `ClientAuthenticated` methods.

After authenticating a player call either `ServerAccept`, `ServerReject`, `ClientAccept` or `ClientReject` depending if running on Server or Client and if you it was successful or not.

Calling the `Accept` method will cause mirage to invoke the `OnServerAuthenticated` or `OnClientAuthenticated` events. Subscribe to OnServerAuthenticated and OnClientAuthenticated events if you wish to perform additional steps after authentication.

Calling the `Reject` method will cause the player to be disconnected after a short delay.

When Rejecting, It is a good idea to send a message to the client to tell them that authentication failed, for example: "Server password invalid" or "Login failed".


## Check if a player is authenticated

After a player has been accepted `IsAuthenticated` will be set to true. The bool can be used alongside `AuthenticationData` to check if a user is allowed to do certain actions.


## Storing Authentication data

The `NetworkPlayer` object has an `AuthenticationData` property that can be used to store any data related to authentication, such as account id, tokens, or players username. 

This property is of type `object` so can be set to any object and can be cast back to that object when you need to read the data.

```cs
if (player.IsAuthenticated)
{
    var loginData = (MyLogInData)player.AuthenticationData;
    var username = loginData.Username;
    // do something with username :)
}
```


Now that you have the foundation of a custom Authenticator component, the rest is up to you. You can exchange any number of custom messages between the server and client as necessary to complete your authentication process before approving the client.

Authentication can also be extended to character selection and customization, just by crafting additional messages and exchanging them with the client before completing the authentication process.  This means this process takes place before the client player actually enters the game or changes to the Online scene.

If you write a good authenticator, consider sharing it with other users or contributing it to the Mirage project.
