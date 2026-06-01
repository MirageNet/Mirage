---
sidebar_position: 9
---
# Pickups, Drops, and Child Objects

Frequently the question comes up about how to handle objects that are attached as children of the player prefab that all clients need to know about and synchronize, such as which weapon is equipped, picking up networked scene objects, and players dropping objects into the scene.

:::caution
Mirage cannot support multiple Network Identity components within an object hierarchy. Since the character object must have a Network Identity, none of its descendant objects can have one.
:::

## Child Objects

Let's start with the simple case of a single attachment point that is somewhere down the hierarchy of our Player, such as a hand at the end of an arm. In a script that inherits from NetworkBehaviour on the Player Prefab, we'd have a `GameObject` reference where the attachment point can be assigned in the inspector, a SyncVar enum with various choices of what the player is holding, and a Hook for the SyncVar to swap out the art of the held item based on the new value.

In the image below, Kyle has an empty game object, `RightHand`, added to the wrist, and some prefabs to be equipped (Ball, Box, Cylinder), and a Player Equip script to handle them.

The inspector shows `RightHand` assigned in 2 places, the Player Equip script, as well as the target of the Network Transform Child component, so we could adjust the relative position of the attachment point (not the art) for all clients as needed.

![Screenshot of Player with Equip Script](/img/guides/game-objects/child-objects1.png)

Below is the Player Equip script to handle the changing of the equipped item, and some notes for consideration:
-   While we could just have all the art items attached at design time and just enable/disable them based on the enum, this doesn't scale well to a lot of items and if they have scripts on them for how they behave in the game, such as animations, special effects, etc. it could get ugly pretty fast, so this example locally instantiates and destroys instead as a design choice.
-   The example makes no effort to deal with position offset between the item and the attach point, e.g. having the grip or handle of an item aligns with the hand. This is best dealt with in a MonoBehaviour script on the item that has public fields for the local position and rotation that can be set in the designer and a bit of code in Start to apply those values in local coordinates relative to the parent attach point.

{{{ Path:'Snippets/GameObjects/PlayerEquipInitial.cs' Name:'player-equip-initial' }}}

## Dropping Items

Now that we can equip the items, we need a way to drop the current item into the world as a networked item. Remember that, as child art, the item prefabs have no networking components on them at all.

First, let's add one more Input to the Update method above and a `CmdDropItem` method:

{{{ Path:'Snippets/GameObjects/PlayerEquip.cs' Name:'player-equip-update' }}}

{{{ Path:'Snippets/GameObjects/PlayerEquip.cs' Name:'player-equip-drop' }}}

In the image above, there's a `sceneObjectPrefab` field that is assigned to a prefab that will act as a container for our item prefabs. The SceneObject prefab has a SceneObject script with a SyncVar like the Player Equip script, and a SetEquippedItem method that takes the shared enum value as a parameter.

{{{ Path:'Snippets/GameObjects/SceneObject.cs' Name:'scene-object-class' }}}

In the run-time image below, the Ball(Clone) is attached to the `RightHand` object, and the Box(Clone) is attached to the SceneObject(Clone), which is shown in the inspector.

The art prefabs have simple colliders on them (sphere, box, capsule).  If your art item has a mesh collider, it must be marked as Convex to work with the RigidBody on the SceneObject container.

![Screenshot of Kyle with equipped item and scene object](/img/guides/game-objects/child-objects2.png)

## Pickup Items

Now that we have a box dropped in the scene, we need to pick it up again. To do that, a `CmdPickupItem` method is added to the Player Equip script:

{{{ Path:'Snippets/GameObjects/PlayerEquip.cs' Name:'player-equip-pickup' }}}

This method is simply called from `OnMouseDown` in the Scene Object script:

{{{ Path:'Snippets/GameObjects/SceneObject.cs' Name:'scene-object-mousedown' }}}

Since the SceneObject(Clone) is networked, we can pass it directly through to `CmdPickupItem` on the character object to set the equipped item SyncVar and destroy the scene object.

For this entire example, the only prefab that needs to be registered with Network Manager besides the Player is the SceneObject prefab.

![Screenshot of inspector](/img/guides/game-objects/child-objects3.png)
