# Authentication

Add `AuthenticatorSettings` too your `NetworkManager`, and assign reference to `NetworkServer`.

[image of Settings]

AuthenticatorSettings will let you configure the Timeout and which Authenticators are available.

### Server side

The list of authenticators on the server is which ones the client can use to. The client can use any of them to become authenticated.

You can find out which one the player used by checking `NetworkPlayer.Authentication.Authenticator`

### Client side

On the client you need to tell the authenticator to send a message to the server, this is because most authenticators will need extra information, like player login information. 

The exception to this is authenticator which are able to automatically find that information themselves and send it to server. One example for this is `SessionIdAuthenticator` which will use a session token given by the server to automatically reconnect. It is best to only use 1 authenticator like this because the server will only process 1 authentication per player.

#### Session Id Authenticator

`SessionIdAuthenticator` will only send a message to server when it has a session Id is valid. session id is only valid for a set amount of time, this can be set in the inspector and defaults to 1 day (1440 minutes).

<!-- If you want to use Session ID without any additional Authenticator you will have to check the "Allow Unauthenticated" -->

