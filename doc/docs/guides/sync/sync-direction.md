---
sidebar_position: 3
---

## Sync To Owner

It is often the case when you don't want some player data visible to other players. In the inspector change the "Network Sync Mode" from "Observers" (default) to "Owner" to let Mirage know to synchronize the data only with the owning client.

For example, suppose you are making an inventory system. Suppose players A, B, and C are in the same area. There will be a total of 12 objects in the entire network:

* Client A has Player A (himself), Player B, and Player C
* Client B has Player A, Player B (himself), and Player C
* Client C has Player A, Player B, and Player C (himself)
* The server has Player A, Player B, Player C

each one of them would have an Inventory component

Suppose Player A picks up some loot.  The server adds the loot to Player's A inventory,  which would have a [SyncLists](/docs/guides/sync/sync-list) of Items. 

By default,  Mirage now has to synchronize player A's inventory everywhere, which means sending an update message to client A, client B, and client C, because they all have a copy of Player A. This is wasteful, Client B and Client C do not need to know about Player's A inventory, they never see it on screen. It is also a security problem, someone could hack the client and display other people's inventory and use it to their advantage.

If you set the "Network Sync Mode" in the Inventory component to "Owner", then Player A's inventory will only be synchronized with Client A.  

Now, suppose instead of 3 people you have 50 people in an area and one of them picks up loot.  It means that instead of sending 50 messages to 50 different clients,  you would only send 1.  This can have a big impact on the bandwidth in your game.

Other typical use cases include quests,  player's hand in a card game, skills, experience, or any other data you don't need to share with other players.
