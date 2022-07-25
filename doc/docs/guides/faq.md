---
sidebar_position: 13
---
# FAQ

:::caution Work In Progress
This page is a work in progress
:::

### How do I use this feature?

<details>
  <summary>How to send/sync custom data types?</summary>
  
  Mirage can automatically create serialization functions for many custom data types when your scripts are compiled.

  For example, Mirage will automatically create a function for `MyCustomStruct` so that it can be sent without any extra work.

  ```cs
  [ClientRpc]
  public void RpcDoSomething(MyCustomStruct data)
  {
      // do stuff here
  }

  struct MyCustomStruct
  {
      int someNumber;
      Vector3 somePosition;
  }
  ```

  For More details 
  - [Data Types](/docs/guides/data-types)
  - [Serialization](/docs/guides/serialization)

</details>


### How to Connect

<details>
  <summary>How to connect to games on same PC</summary>

  Make sure the Network Address field on NetworkManager or the Hud is set up `localHost`
</details>

<details>
  <summary>How to connect to a different PC/Device on same network</summary>

  Set the Network Address field to the LAN IP of the host `192.168.x.x`

  *In some cases, you may need additional steps, check below*

  To check IP on Windows you can open PowerShell and use the `ipconfig` command, then under your current adapter (ethernet/wifi/etc) look for `IPv4 Address`

  ` IPv4 Address. . . . . . . . . . . : 192.168.x.x `
</details>

<details>
  <summary>How to connect to a different PC/Device over the internet</summary>

  Set the Network Address field to be the IP address of the host (google 'whats my IP')

  > This section does not cover relays/dedicated vps/headless features

  For this to work, you will need to do **some** of the following, most of these depend on your set-up and router

  - **Port forward**:  
  You'll have to log in to your router.
    - Forward your game port (default is 7777) for your PC's local IP. (192.168.1.20 for example) 

  - **PC Firewalls**: 
    - You can turn it off for a quick test (And turn it back on later)
    - manually allow the editor and any builds you create in firewalls settings.

  - Try from a build rather than the Unity Editor
  
  - Some anti-virus/phones may have additional blocking.
    - You can turn it off for a quick test (And turn it back on later)
  
  - In rare cases ISPs or companies/schools block ports and connections, this is harder to adjust yourself.
    If you need more help it is best to google for a guide for your setup and router.
    An alternative to the above is to use a dedicated server (VPS) or use a relay.


</details>
