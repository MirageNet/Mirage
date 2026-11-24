---
sidebar_position: 1
---
# Authentication

Authentication is the process of verifying the validity and identity of a user. It allows you to control who can join your game and enables features like saving stats and communicating with friends. Mirage supports authentication through various common methods, which can be implemented using a custom authenticator. Some examples include:

- Username and password
- Third-party OAuth2 or OpenID identity providers (e.g., Facebook, Twitter, Google)
- Third-party services like PlayFab, GameLift, or Steam
- Device ID (popular for mobile games)
- Google Play for Android
- Game Center for iOS
- Web service integration for websites

Please note that these authentication methods can be implemented using a custom authenticator in Mirage. For detailed instructions on how to create a custom authenticator, please refer to the [Custom Authenticator](./custom-authenticator.md) page.

## Built-in Authenticators

Mirage provides two built-in authenticators that you can use out of the box. These authenticators offer a convenient way to handle common authentication scenarios:

- [Basic Authenticator](./basic-authenticator.md): This authenticator uses a simple password to authenticate users. It is useful when you want to restrict access to your game to only those who know the password.

- [Session ID Authenticator](./session-id.md): This authenticator leverages a session token provided by the server to automatically reconnect clients. It is suitable for cases where you want to enable seamless reconnection for players.

For instructions on how to set up and use these built-in authenticators, please refer to their respective documentation pages.

## Encryption Notice

By default, Mirage does not provide encryption. However, if you want to secure your authentication process, you can use the WebSocket or Relay transports, which support encryption. Please refer to the transport documentation for more information.

**Note:** The default UDP transport does not support encryption.

## Authenticator Setup

To set up an authenticator, please refer to the [Authenticator Settings](./authenticator-settings.md) page, which provides detailed instructions on configuring the authenticator for your game.

Now that you have a comprehensive understanding of authentication methods, you can choose the one that best fits your requirements. If none of the built-in methods suit your needs, you can create a custom authenticator following the guidelines provided in the [Custom Authenticator](./custom-authenticator.md) page.

If you develop a robust and reusable authenticator, consider sharing it with the Mirage community or contributing it to the Mirage project.
