---
sidebar_position: 5
---
# Session Id Authenticator

Session Id Authenticator is a built-in Authenticator that will allow clients to reconnect to a server without requiring them to fully authenticate again.

Session ID is only valid for a set amount of time, which can be configured in the inspector and defaults to 1 day (1440 minutes).

To use the Session ID Authenticator, you can manually control it using `ClientIdStore` and `CreateOrRefreshSession()`.


